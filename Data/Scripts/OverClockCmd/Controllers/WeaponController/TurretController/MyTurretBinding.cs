using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using System.Linq;
using System;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
using System.Collections.Concurrent;

namespace SuperBlocks.Controller
{
    public sealed class MyTurretBinding
    {
        public void Init(IMyMotorStator motorAz, ref MyTurretConfig Config)
        {
            BasicInit(motorAz, Config);
            WeaponsInit(motorAz, Config);
        }
        private void BasicInit(IMyMotorStator motorAz, MyTurretConfig Config)
        {
            this.Config = Config;
            motorEvs_WTs.Clear();
            if (motorAz.CustomName.Contains(Config.TurretAzNM) && MyWeaponAndTurretApi.ActiveRotorBasicFilter(motorAz))
                this.motorAz = motorAz;
            else
                this.motorAz = null;
            var EvName = Config.TurretEzNM;
            if (Utils.Common.NullEntity(this.motorAz)) return;
            var evs = Utils.Common.GetTs<IMyMotorStator>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(this.motorAz.TopGrid), b => b.CubeGrid == this.motorAz.TopGrid && b.CustomName.Contains(EvName) && b.TopGrid != null);
            if (Utils.Common.IsNullCollection(evs)) return;
            foreach (var ev in evs) { motorEvs_WTs.AddOrUpdate(ev, new HashSet<IMyTerminalBlock>(), (key, value) => { return value; }); }
        }
        private void WeaponsInit(IMyMotorStator motorAz, MyTurretConfig Config)
        {
            var evs = motorEvs_WTs.Keys;
            Cameras.Clear();
            Weapons.Clear();
            foreach (var ev_motor in evs)
            {
                if (Utils.Common.NullEntity(ev_motor) || (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(ev_motor))) continue;
                var weapons = Utils.Common.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid && !(b is IMyLargeTurretBase) && b.CustomName.Contains(Config.weapon_tag));
                var cameras = Utils.Common.GetTs<IMyCameraBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid);
                if (!Utils.Common.IsNullCollection(weapons)) Weapons.UnionWith(weapons);
                if (!Utils.Common.IsNullCollection(cameras)) Cameras.UnionWith(cameras);
                if (!Utils.Common.IsNullCollection(weapons))
                    motorEvs_WTs.AddOrUpdate(ev_motor, new HashSet<IMyTerminalBlock>(weapons), (key, value) => { return new HashSet<IMyTerminalBlock>(weapons); });
                else if (!Utils.Common.IsNullCollection(cameras))
                    motorEvs_WTs.AddOrUpdate(ev_motor, new HashSet<IMyTerminalBlock>(weapons), (key, value) => { return new HashSet<IMyTerminalBlock>(weapons); });
            }
            HashSet<string> _WeaponsNM = new HashSet<string>();
            foreach (var Weapon in Weapons) _WeaponsNM.Add(Weapon.BlockDefinition.SubtypeId);
            WeaponsNM.AddRange(_WeaponsNM);
            ReferWeapon();
        }
        public bool HasTarget => TargetPredict.Direction != null;

        #region RunningMotors
        public void RunningDefault()
        {
            RemoveEmptyBlocks();
            MotorsRunningDefault();
        }
        public void RunningDirection(Vector3D? Direction)
        {
            RemoveEmptyBlocks();
            if (!Enabled || Direction == null) { MotorsRunningDefault(); return; }
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in motorEvs_WTs)
            {
                var data = MyWeaponAndTurretApi.Get_ArmOfForce_Point(Gun_Rotor_Group);
                TurretRotor_Torque += MyWeaponAndTurretApi.GetTorque(motorAz, data, Direction, Config.mult);
                Gun_Rotor_Group.Key.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(MyWeaponAndTurretApi.GetTorque(Gun_Rotor_Group.Key, data, Direction, Config.mult), Gun_Rotor_Group.Key.TargetVelocityRad, Config.max_ev);
            }
            TurretRotor_Torque /= motorEvs_WTs.Count;
            motorAz.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(TurretRotor_Torque, motorAz.TargetVelocityRad, Config.max_az);
        }
        public void RunningManual(Vector2? Rotation)
        {
            RemoveEmptyBlocks();
            if (!Enabled || Rotation == null) { MotorsRunningDefault(); return; }
            var Reverse = new Vector2(ReverseEv ? -1 : 1, ReverseAz ? -1 : 1);
            var finalCtrl = Vector2.Clamp(Rotation.Value * Reverse * Config.mult * Mult, -Config.Max_Speed, Config.Max_Speed);
            motorAz.TargetVelocityRad = finalCtrl.Y;
            var MD = motorAz.TopGrid.WorldMatrix.Left;
            var TurretEvs = motorEvs_WTs.Keys.ToList();
            if (Utils.Common.IsNullCollection(TurretEvs)) return;
            var MountEv = TurretEvs[0];
            MountEv.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), MountEv.TargetVelocityRad, Config.max_ev);// MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -Config.max_ev, Config.max_ev);
            for (int index = 1; index < TurretEvs.Count; index++)
                TurretEvs[index].TargetVelocityRad = MathHelper.Clamp((MathHelper.WrapAngle(Math.Sign(TurretEvs[0].WorldMatrix.Up.Dot(TurretEvs[index].WorldMatrix.Up)) * MountEv.Angle) - MathHelper.WrapAngle(TurretEvs[index].Angle)) * 35, -Config.max_ev, Config.max_ev); ;

        }
        private void MotorsRunningDefault()
        {
            Utils.MyRotorAPI.RotorSetDefault(motorAz, Config.max_az);
            Utils.MyRotorAPI.RotorsSetDefault(motorEvs_WTs?.Keys?.ToList(), Config.max_ev);
        }
        #endregion

        #region 转子控制函数


        #endregion
        #region 其他方法与函数
        public MyTurretConfig Config;
        private IMyMotorStator motorAz { get; set; }
        private ConcurrentDictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> motorEvs_WTs { get; } = new ConcurrentDictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
        public bool ReverseAz { get; set; } = false;
        public bool ReverseEv { get; set; } = false;
        private void RemoveEmptyMotorEvs()
        {
            var motors = motorEvs_WTs.Keys.Where(Utils.Common.NullEntity);
            if (Utils.Common.IsNullCollection(motors)) return;
            foreach (var motor in motors) motorEvs_WTs.Remove(motor);

        }
        private void RemoveEmptyBlocks()
        {
            RemoveEmptyMotorEvs();
            Cameras.RemoveWhere(Utils.Common.NullEntity);
            Weapons.RemoveWhere(Utils.Common.NullEntity);
        }
        #endregion



        public bool Enabled => (!(Utils.Common.NullEntity(motorAz) || Utils.Common.IsNullCollection(motorEvs_WTs)));
        public bool ManuelOnly => Utils.Common.IsNullCollection(Weapons) && CanManuel;
        public bool UnderControl => CanManuel && !Utils.Common.NullEntity(ControlledCamera);
        #region AutoAimAtTarget
        public MyTargetPredict TargetPredict { get; } = new MyTargetPredict();
        public void RunningAutoAimAt(IMyTerminalBlock Me)
        {
            ReferWeapon();
            TargetPredict.CalculateDirection(Me, CurrentWeapons);
            RunningDirection(TargetPredict.Direction);
        }
        //public void RunningAutoAimAt(IMyMotorStator Me)
        //{
        //    ReferWeapon();
        //    RunningDirection(TargetPredict.CalculateDirection_L(Me, CurrentWeapons));
        //}
        #endregion
        #region PrivateSignals
        private IMyCameraBlock ControlledCamera { get { if (Utils.Common.IsNullCollection(Cameras)) return null; IMyCameraBlock cameraBlock = Cameras.FirstOrDefault(b => b.IsActive); return cameraBlock; } }
        private HashSet<IMyCameraBlock> Cameras { get; } = new HashSet<IMyCameraBlock>();
        private HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
        private bool CanManuel => !Utils.Common.IsNullCollection(Cameras);
        private Vector2 Mult { get { if (!UnderControl) return Vector2.Zero; return (MathHelper.Clamp(MyAPIGateway.Session.Camera.FovWithZoom, 0.00001f, 0.5f)) * Vector2.One; } }
        #endregion
        #region FiringWeapons
        private bool CanFire => TargetPredict.CanFireWeapon(CurrentWeapons);
        public void RunningAutoFire()
        {
            if (Utils.Common.IsNullCollection(WeaponsToFire)) return;
            if (!CanFire) return;
            if (count > 0) { count--; return; }
            MyWeaponAndTurretApi.FireWeaponOnce(WeaponsToFire.Dequeue());
            count = Config?.firegap ?? 0;
        }
        public void SetFire(bool FireWeapons)
        {
            if (!FireWeapons) { foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, false); return; }
            if (Config == null) { foreach (var Weapon in Weapons) { MyWeaponAndTurretApi.FireWeapon(Weapon, false); } return; }
            if (Config.firegap == 0) { foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, FireWeapons && CanFire && (Config.firegap == 0)); return; }
            else { if (FireWeapons == true && Utils.Common.IsNullCollection(WeaponsToFire)) { foreach (var Weapon in CurrentWeapons) { if (Utils.Common.NullEntity(Weapon)) continue; MyWeaponAndTurretApi.FireWeapon(Weapon, false); WeaponsToFire.Enqueue(Weapon); } count = 0; } }
        }
        private int count = 0;
        private Queue<IMyTerminalBlock> WeaponsToFire { get; } = new Queue<IMyTerminalBlock>();
        #endregion
        #region GetWeaponsInfo
        public void ReferWeapon()
        {
            if (WeaponsNM.Count == 0) return;
            CurrentWeapons.Clear();
            string WeaponName = "DefaultWeapon";
            string AmmoName = "DefaultAmmo";
            WeaponName = WeaponsNM[weaponidx % WeaponsNM.Count];
            CurrentWeapons.UnionWith(Weapons.Where(b => b.BlockDefinition.SubtypeId == WeaponName) ?? new HashSet<IMyTerminalBlock>());
            SetWeaponAmmos(WeaponName, AmmoName);
        }
        public void CycleWeapon()
        {
            if (WeaponsNM.Count == 0) return;
            CurrentWeapons.Clear();
            string WeaponName = "DefaultWeapon";
            string AmmoName = "DefaultAmmo";
            WeaponName = WeaponsNM[weaponidx % WeaponsNM.Count]; weaponidx++;
            CurrentWeapons.UnionWith(Weapons.Where(b => b.BlockDefinition.SubtypeId == WeaponName) ?? new HashSet<IMyTerminalBlock>());
            SetWeaponAmmos(WeaponName, AmmoName);
        }
        private List<string> WeaponsNM { get; } = new List<string>();
        private int weaponidx = 0;
        private HashSet<IMyTerminalBlock> CurrentWeapons { get; } = new HashSet<IMyTerminalBlock>();
        public void SetWeaponAmmoConfigInfo(MyTurretConfig Config, string WeaponName, string AmmoName) { this.Config = Config; SetWeaponAmmos(WeaponName, AmmoName); }
        #endregion
        #region InternalAPIs
        private void SetWeaponAmmos(string WeaponName, string AmmoName)
        {
            if (Utils.Common.IsNullCollection(CurrentWeapons)) return;
            if (BasicInfoService.WcApi.HasCoreWeapon(CurrentWeapons.First()))
            {
                WeaponName = MyWeaponAndTurretApi.GetWeaponNM(CurrentWeapons.First());
                AmmoName = MyWeaponAndTurretApi.GetCurrentAmmo(CurrentWeapons)?.First().Value ?? "DefaultAmmo";
                foreach (var wp in CurrentWeapons) BasicInfoService.WcApi.SetActiveAmmo(wp, MyWeaponAndTurretApi.GetWeaponID(wp), AmmoName);
                if (TargetPredict.WeaponName != WeaponName || TargetPredict.AmmoName != AmmoName)
                {
                    var value = MyWeaponAndTurretApi.GetWeaponCoreDefinition(CurrentWeapons.First(), WeaponName, AmmoName);
                    if (value == null) TargetPredict.SetWeaponAmmoConfigInfo(Config, WeaponName, AmmoName);
                    else TargetPredict.SetWeaponAmmoConfigInfo(value.Value);
                }
            }
            else
            {
                if (TargetPredict.WeaponName != WeaponName || TargetPredict.AmmoName != AmmoName)
                    TargetPredict.SetWeaponAmmoConfigInfo(Config, WeaponName, AmmoName);
            }
        }


        #endregion
    }
    public class MyRotorTurretController
    {
        public IMyMotorStator MotorAz { get; private set; }
        public List<IMyMotorStator> MotorEvs { get; private set; }
        public bool Enabled => (!(Utils.Common.NullEntity(MotorAz) || Utils.Common.IsNullCollection(MotorEvs)));
        public bool ReverseAz { get; set; } = false;
        public bool ReverseEv { get; set; } = false;
        public void InitMotors(IMyMotorStator MotorAz, MyTurretConfig Config)
        {
            this.Config = Config;
            if (MotorAz.CustomName.Contains(this.Config.TurretAzNM) && MyWeaponAndTurretApi.ActiveRotorBasicFilter(MotorAz))
                this.MotorAz = MotorAz;
            else this.MotorAz = null;
            if (!Utils.Common.NullEntity(this.MotorAz))
                MotorEvs = Utils.Common.GetTs<IMyMotorStator>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(this.MotorAz.TopGrid), b => b.CubeGrid == this.MotorAz.TopGrid && b.CustomName.Contains(Config.TurretEzNM) && b.TopGrid != null);
            else
                MotorEvs = null;
        }
        public void RunningDefault()
        {
            RemoveEmptyBlocks();
            MotorsRunningDefault();
        }
        public void RunningManual(Vector2? Rotation, Vector2 Mult)
        {
            RemoveEmptyBlocks();
            if (!Enabled || !Rotation.HasValue) { MotorsRunningDefault(); return; }
            var Reverse = new Vector2(ReverseEv ? -1 : 1, ReverseAz ? -1 : 1);
            var finalCtrl = Vector2.Clamp((Rotation ?? Vector2.Zero) * Reverse * Config.mult * Mult, -Config.Max_Speed, Config.Max_Speed);
            MotorAz.TargetVelocityRad = finalCtrl.Y;
            var MD = MotorAz.TopGrid.WorldMatrix.Left;
            if (Utils.Common.IsNullCollection(MotorEvs)) return;
            var MountEv = MotorEvs[0];
            MountEv.TargetVelocityRad = MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(MotorEvs[0].WorldMatrix.Up), -Config.max_ev, Config.max_ev);
            for (int index = 1; index < MotorEvs.Count; index++)
                MotorEvs[index].TargetVelocityRad = MathHelper.Clamp((MathHelper.WrapAngle(Math.Sign(MotorEvs[0].WorldMatrix.Up.Dot(MotorEvs[index].WorldMatrix.Up)) * MountEv.Angle) - MathHelper.WrapAngle(MotorEvs[index].Angle)) * 35, -Config.max_ev, Config.max_ev); ;

        }
        public void RunningDirection(Vector3D? Direction, Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> MotorEvs_WTs)
        {
            RemoveEmptyBlocks();
            if (!Enabled || Direction == null) { MotorsRunningDefault(); return; }
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in MotorEvs_WTs)
            {
                var data = MyWeaponAndTurretApi.Get_ArmOfForce_Point(Gun_Rotor_Group);
                TurretRotor_Torque += MyWeaponAndTurretApi.GetTorque(MotorAz, data, Direction, Config.mult);
                Gun_Rotor_Group.Key.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(MyWeaponAndTurretApi.GetTorque(Gun_Rotor_Group.Key, data, Direction, Config.mult), Gun_Rotor_Group.Key.TargetVelocityRad, Config.max_ev);
            }
            TurretRotor_Torque /= MotorEvs_WTs.Count;
            MotorAz.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(TurretRotor_Torque, MotorAz.TargetVelocityRad, Config.max_az);
        }
        public void SetConfig(MyTurretConfig Config) => this.Config = Config;
        private void RemoveEmptyBlocks() => MotorEvs?.RemoveAll(Utils.Common.NullEntity);
        private void MotorsRunningDefault()
        {
            Utils.MyRotorAPI.RotorSetDefault(MotorAz, Config.max_az);
            Utils.MyRotorAPI.RotorsSetDefault(MotorEvs, Config.max_ev);
        }
        private MyTurretConfig Config = null;
    }
    public class MyToolsManager
    {
        public void InitTools(ICollection<IMyTerminalBlock> InitTools)
        {
            InitShipTools(InitTools);
            InitCameras(InitTools);
            InitWeapons(InitTools);
        }
        #region WeaponConfigs
        private void InitWeapons(ICollection<IMyTerminalBlock> InitTools)
        {
            Weapons.Clear();
            WeaponNMs.Clear();
            CurrentWeapons.Clear();
            CycleWeaponQueue.Clear();
            CurrentWeaponNM = "DefaultWeapon";
            Weapons.UnionWith(InitTools?.Where(MyWeaponAndTurretApi.IsIMyTerminalBlockWeapon) ?? new HashSet<IMyTerminalBlock>());
            HashSet<string> WeaponKinds = new HashSet<string>();
            foreach (var Weapon in Weapons)
                WeaponKinds.Add(Weapon.BlockDefinition.SubtypeId);
            foreach (var WeaponKind in WeaponKinds)
                WeaponNMs.Add(WeaponKind, MyWeaponAndTurretApi.GetWeaponNM(Weapons.First(b => b.BlockDefinition.SubtypeId == WeaponKind)));
            ReferWeapon();
            count = 0;
        }
        public string CurrentWeaponNM { get; private set; } = "DefaultWeapon";
        public void ReferWeapon()
        {
            string WeaponName = "DefaultWeapon";
            if (WeaponNMs.Count == 0) { CurrentWeaponNM = WeaponName; return; }
            if (CycleWeaponQueue.Count < 1)
                foreach (var WeaponNM in WeaponNMs)
                    CycleWeaponQueue.Enqueue(WeaponNM.Key);
            if (CycleWeaponQueue.Count > 0)
            {
                var defaultsetup_key = CycleWeaponQueue.Count > 0 ? CycleWeaponQueue.Peek() : "";
                if (WeaponNMs.ContainsKey(defaultsetup_key))
                {
                    if (CurrentWeaponNM != WeaponNMs[defaultsetup_key])
                    {
                        CurrentWeapons.Clear();
                        CurrentWeaponNM = WeaponNMs[defaultsetup_key];
                        CurrentWeapons.UnionWith(Weapons.Where(b => b.BlockDefinition.SubtypeId == defaultsetup_key) ?? new HashSet<IMyTerminalBlock>());
                    }
                }
            }
        }
        public void CycleWeapon()
        {
            string WeaponName = "DefaultWeapon";
            if (WeaponNMs.Count == 0) { CurrentWeaponNM = WeaponName; return; }
            if (CycleWeaponQueue.Count < 1)
                foreach (var WeaponNM in WeaponNMs)
                    CycleWeaponQueue.Enqueue(WeaponNM.Key);
            if (CycleWeaponQueue.Count > 0)
            {
                var defaultsetup_key = CycleWeaponQueue.Count > 0 ? CycleWeaponQueue.Dequeue() : "";
                if (WeaponNMs.ContainsKey(defaultsetup_key))
                {
                    if (CurrentWeaponNM != WeaponNMs[defaultsetup_key])
                    {
                        CurrentWeapons.Clear();
                        CurrentWeaponNM = WeaponNMs[defaultsetup_key];
                        CurrentWeapons.UnionWith(Weapons.Where(b => b.BlockDefinition.SubtypeId == defaultsetup_key) ?? new HashSet<IMyTerminalBlock>());
                    }
                }
            }
        }
        public void RunningCalculate(IMyTerminalBlock Me) { RemoveEmptyBlocks(); TargetPredict.CalculateDirection(Me, CurrentWeapons); }
        public void SetConfig(MyTurretConfig Config) => this.Config = Config;
        public void SetWeaponAmmoConfigInfo(MyTurretConfig Config, string WeaponName, string AmmoName) { this.Config = Config; SetWeaponAmmos(WeaponName, AmmoName); }
        private void SetWeaponAmmos(string WeaponName, string AmmoName)
        {
            if (Utils.Common.IsNullCollection(CurrentWeapons)) return;
            if (BasicInfoService.WcApi.HasCoreWeapon(CurrentWeapons.First()))
            {
                WeaponName = MyWeaponAndTurretApi.GetWeaponNM(CurrentWeapons.First());
                AmmoName = MyWeaponAndTurretApi.GetCurrentAmmo(CurrentWeapons)?.First().Value ?? "DefaultAmmo";
                foreach (var wp in CurrentWeapons) BasicInfoService.WcApi.SetActiveAmmo(wp, MyWeaponAndTurretApi.GetWeaponID(wp), AmmoName);
                if (TargetPredict.WeaponName != WeaponName || TargetPredict.AmmoName != AmmoName)
                {
                    var value = MyWeaponAndTurretApi.GetWeaponCoreDefinition(CurrentWeapons.First(), WeaponName, AmmoName);
                    if (value == null) TargetPredict.SetWeaponAmmoConfigInfo(Config, WeaponName, AmmoName);
                    else TargetPredict.SetWeaponAmmoConfigInfo(value.Value);
                }
            }
            else
            {
                if (TargetPredict.WeaponName != WeaponName || TargetPredict.AmmoName != AmmoName)
                    TargetPredict.SetWeaponAmmoConfigInfo(Config, WeaponName, AmmoName);
            }
        }
        public Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> GetWeaponRotorBinding(ICollection<IMyMotorStator> Rotors)
        {
            Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Binding = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
            if (Rotors == null) return Binding;
            foreach (var Rotor in Rotors)
            {
                if (Utils.MyRotorAPI.DisabledMotorRotor(Rotor)) continue;
                Binding.Add(Rotor, new HashSet<IMyTerminalBlock>(CurrentWeapons.Where(b => b.CubeGrid == Rotor.TopGrid)?.ToList() ?? new List<IMyTerminalBlock>()));
            }
            return Binding;
        }
        public bool AllWeaponsEnabled { get { return Weapons.Any(b => MyWeaponAndTurretApi.GetIMyTerminalBlock(b) && b.IsFunctional); } set { foreach (var b in Weapons) MyWeaponAndTurretApi.SetIMyTerminalBlock(b, value); } }
        public void RunningAutoFire()
        {
            if (Utils.Common.IsNullCollection(WeaponsToFire)) return;
            if (!CanFire) return;
            if (count > 0) { count--; return; }
            MyWeaponAndTurretApi.FireWeaponOnce(WeaponsToFire.Dequeue());
            count = Config?.firegap ?? 0;
        }
        public void SetFire(bool FireWeapons)
        {
            if (!FireWeapons) { foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, false); return; }
            if (Config == null) { foreach (var Weapon in Weapons) { MyWeaponAndTurretApi.FireWeapon(Weapon, false); } return; }
            if (Config.firegap == 0) { foreach (var weapon in CurrentWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, FireWeapons && CanFire && (Config.firegap == 0)); return; }
            else { if (FireWeapons == true && Utils.Common.IsNullCollection(WeaponsToFire)) { foreach (var Weapon in CurrentWeapons) { if (Utils.Common.NullEntity(Weapon)) continue; MyWeaponAndTurretApi.FireWeapon(Weapon, false); WeaponsToFire.Enqueue(Weapon); } count = 0; } }
        }
        public Vector3D? Direction => TargetPredict.Direction;
        public void SetTarget(IMyTerminalBlock block, MyTargetDetected TargetDetected)
        {
            if (TargetDetected != null && TargetDetected.Entity != null && TargetDetected.Entity?.EntityId != TargetPredict.TargetLocked?.Entity?.EntityId && Vector3D.Distance(TargetDetected.Position.Value, block.GetPosition()) < Config.range)
                TargetPredict.TargetLocked = TargetDetected;
        }
        public void SetTarget(IMyTerminalBlock block, IMyEntity TargetDetected, bool AllTerminalBlocks = false)
        {
            var target = new MyTargetDetected(TargetDetected, block, AllTerminalBlocks);
            if (target != null && target.Entity != null && target.Entity?.EntityId != TargetPredict.TargetLocked?.Entity?.EntityId && Vector3D.Distance(target.Position.Value, block.GetPosition()) < Config.range)
                TargetPredict.TargetLocked = target;
        }
        #region PrivateSignal
        private HashSet<IMyTerminalBlock> CurrentWeapons { get; } = new HashSet<IMyTerminalBlock>();
        private HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
        private Dictionary<string, string> WeaponNMs { get; } = new Dictionary<string, string>();
        private Queue<string> CycleWeaponQueue { get; } = new Queue<string>();
        private Queue<IMyTerminalBlock> WeaponsToFire { get; } = new Queue<IMyTerminalBlock>();
        private int count = 0;
        private MyTurretConfig Config = null;
        private bool CanFire => TargetPredict.CanFireWeapon(CurrentWeapons);
        private MyTargetPredict TargetPredict { get; } = new MyTargetPredict();
        #endregion
        #endregion
        #region Cameras
        public Vector2 Mult { get { if (Utils.Common.IsNull(UserControlCameraBlock)) return Vector2.Zero; return (MathHelper.Clamp(MyAPIGateway.Session.Camera.FovWithZoom, 0.00001f, 0.5f)) * Vector2.One; } }
        public bool CameraBlocksEnabled { get { return CameraBlocks.Any(b => b.Enabled && b.IsFunctional); } set { foreach (var b in CameraBlocks) b.Enabled = value; } }
        public IMyCameraBlock UserControlCameraBlock => CameraBlocks.FirstOrDefault(b => b.IsActive);
        private HashSet<IMyCameraBlock> CameraBlocks { get; } = new HashSet<IMyCameraBlock>();
        private void InitCameras(ICollection<IMyTerminalBlock> InitTools)
        {
            CameraBlocks.Clear();
            CameraBlocks.UnionWith(InitTools?.Where(b => b is IMyCameraBlock)?.ToList()?.ConvertAll(b => b as IMyCameraBlock) ?? new List<IMyCameraBlock>());
        }
        #endregion
        #region ShipTools 
        public bool ShipDrillsEnabled { get { return ShipDrills.Any(b => b.Enabled && b.IsFunctional); } set { foreach (var b in ShipDrills) b.Enabled = value; } }
        public bool ShipGrindersEnabled { get { return ShipGrinders.Any(b => b.Enabled && b.IsFunctional); } set { foreach (var b in ShipGrinders) b.Enabled = value; } }
        public bool ShipWeldersEnabled { get { return ShipWelders.Any(b => b.Enabled && b.IsFunctional); } set { foreach (var b in ShipWelders) b.Enabled = value; } }
        private HashSet<IMyShipDrill> ShipDrills { get; } = new HashSet<IMyShipDrill>();
        private HashSet<IMyShipGrinder> ShipGrinders { get; } = new HashSet<IMyShipGrinder>();
        private HashSet<IMyShipWelder> ShipWelders { get; } = new HashSet<IMyShipWelder>();
        private void InitShipTools(ICollection<IMyTerminalBlock> InitTools)
        {
            ShipDrills.Clear();
            ShipGrinders.Clear();
            ShipWelders.Clear();
            ShipDrills.UnionWith(InitTools?.Where(b => b is IMyShipDrill)?.ToList()?.ConvertAll(b => b as IMyShipDrill) ?? new List<IMyShipDrill>());
            ShipGrinders.UnionWith(InitTools?.Where(b => b is IMyShipGrinder)?.ToList()?.ConvertAll(b => b as IMyShipGrinder) ?? new List<IMyShipGrinder>());
            ShipWelders.UnionWith(InitTools?.Where(b => b is IMyShipWelder)?.ToList()?.ConvertAll(b => b as IMyShipWelder) ?? new List<IMyShipWelder>());
        }
        #endregion
        #region Common
        private void RemoveEmptyBlocks()
        {
            CurrentWeapons.RemoveWhere(Utils.Common.NullEntity);
            Weapons.RemoveWhere(Utils.Common.NullEntity);
            ShipDrills.RemoveWhere(Utils.Common.NullEntity);
            ShipGrinders.RemoveWhere(Utils.Common.NullEntity);
            ShipWelders.RemoveWhere(Utils.Common.NullEntity);
        }
        #endregion
    }
}