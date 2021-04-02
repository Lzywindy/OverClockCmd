using ParallelTasks;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public sealed partial class MyTurretBinding
    {
        public MyTurretBinding(IMyMotorStator MotorAz)
        {
            if (Utils.Common.NullEntity(MotorAz)) return;
            this.MotorAz = MotorAz;
            BasicInit(this.MotorAz);
            ReadConfig_Turret_Rotors();
        }
        public void SetConfig(ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs)
        {
            try
            {
                if (SlaveWeapons.UnabledFire || Configs.ContainsKey(_TurretWeaponConfigID))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, _TurretWeaponConfigID);
                else if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons.First()))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultWeaponCoreWeapon");
                else if (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Plasam") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("plasam"))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "EnergyWeapon");
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeGatlingTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallGatlingGun))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case null:
                        case "":
                        case "SmallGatlingTurret":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_LargeWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeMissileTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncher) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncherReload))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case null:
                        case "":
                        case "LargeMissileLauncher":
                        case "SmallMissileTurret":
                        case "SmallRocketLauncherReload":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensRocketWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeInteriorTurret))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case "LargeInteriorTurret":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_SmallWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
            }
            catch (Exception) { Clear(); }
        }
        public void Running()
        {
            try
            {
                RemoveEmptyBlocks();
                MotorAz.Enabled = RotorsEnabled;
                foreach (var motorEv in motorEvs) { motorEv.Enabled = RotorsEnabled; }
                var fire = AutoFire && Enabled && RotorsEnabled && CanFire(DefaultConfig.Delta_precious);
                SetFire(fire);
                if (!Enabled || !CanRunning) { RunningDefault(); return; }
                if (UnderControl)
                {
                    RunningManual(Utils.Common.GetT<IMyShipController>(MotorAz, block => block.IsUnderControl)?.RotationIndicator);
                }
                else if (ManuelOnly) { RunningDefault(); }
                else
                {
                    RunningAutoAimAt(MotorAz);
                    RunningAutoFire(fire);
                }
            }
            catch (Exception) { }

        }

        public void Restart()
        {
            BasicInit(MotorAz);
            ReadConfig_Turret_Rotors();
        }
        public MyTargetDetected AimTarget { get { return TargetPredict.TargetLocked; } set { TargetPredict.TargetLocked = value; } }
        public volatile bool AutoFire = false;
        public volatile bool Enabled = false;
        public volatile bool RotorsEnabled = true;
        public Vector3D? PredictDirection => TargetPredict.Direction;
    }
    public sealed partial class MyTurretBinding
    {
        private void RunningAutoFire(bool FireWeapons)
        {
            if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons?.FirstOrDefault()))
                SlaveWeapons.RunningAutoFire(FireWeapons);
            else
            {
                var block = Utils.Common.GetT<IMyTimerBlock>(MotorAz, b => b.CustomName.Contains("weapon") && b.CubeGrid == MotorAz.TopGrid);
                if (block == null) return;
                block.Enabled = FireWeapons;
                if (FireWeapons) block.Trigger();
            }
        }
        private void SetFire(bool FireWeapons)
        {
            if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons?.FirstOrDefault()))
                SlaveWeapons.SetFire(FireWeapons);
            else
            {
                var block = Utils.Common.GetT<IMyTimerBlock>(MotorAz, b => b.CustomName.Contains("weapon") && b.CubeGrid == MotorAz.TopGrid);
                if (block == null) return;
                block.Enabled = FireWeapons; if (FireWeapons) block.Trigger();
            }
        }
        private void RunningDefault() => MotorsRunningDefault();
        private void RunningDirection(Vector3D? Direction)
        {
            if (!CanRunning || Direction == null) { MotorsRunningDefault(); return; }
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in motorEvs_WTs)
            {
                var data = MyWeaponAndTurretApi.Get_ArmOfForce_Point(Gun_Rotor_Group);
                TurretRotor_Torque += MyWeaponAndTurretApi.GetTorque(MotorAz, data, Direction, _Multipy);
                Gun_Rotor_Group.Key.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(Gun_Rotor_Group.Key, MyWeaponAndTurretApi.RotorDampener(MyWeaponAndTurretApi.GetTorque(Gun_Rotor_Group.Key, data, Direction, _Multipy * 1.4f), Gun_Rotor_Group.Key.TargetVelocityRad, _Max_AV_ev));
            }
            TurretRotor_Torque /= motorEvs_WTs.Count;
            MotorAz.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(MotorAz, MyWeaponAndTurretApi.RotorDampener(TurretRotor_Torque, MotorAz.TargetVelocityRad, _Max_AV_az));
        }
        private void RunningManual(Vector2? Rotation)
        {
            if (!CanRunning || Rotation == null) { MotorsRunningDefault(); return; }
            var finalCtrl = Vector2.Clamp(Rotation.Value * Mult, -_MaxSpeed, _MaxSpeed);
            MotorAz.TargetVelocityRad = finalCtrl.Y;
            var MD = MotorAz.TopGrid.WorldMatrix.Left;
            var TurretEvs = motorEvs_WTs.Keys.ToList();
            if (Utils.Common.IsNullCollection(TurretEvs)) return;
            var MountEv = TurretEvs[0];
            MountEv.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(MountEv, MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -_Max_AV_ev, _Max_AV_ev));// MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -Config.max_ev, Config.max_ev);
            for (int index = 1; index < TurretEvs.Count; index++)
                TurretEvs[index].TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(TurretEvs[index], MathHelper.Clamp((MathHelper.WrapAngle(Math.Sign(TurretEvs[0].WorldMatrix.Up.Dot(TurretEvs[index].WorldMatrix.Up)) * MountEv.Angle) - MathHelper.WrapAngle(TurretEvs[index].Angle)) * 35, _Max_AV_ev, _Max_AV_ev));

        }
        private void RunningAutoAimAt(IMyTerminalBlock Me)
        {
            ReferWeapon();

            //if (TargetPredictTask.Item == null) TargetPredictTask = MyAPIGateway.Parallel.StartBackground(() => { TargetPredict.CalculateDirection(Me, SlaveWeapons.CurrentWeapons, ref ModifiedConfig); });
            //if (TargetPredictTask.IsComplete) TargetPredictTask.Execute();

            TargetPredict.CalculateDirection(Me, SlaveWeapons.CurrentWeapons, ref ModifiedConfig);
            if (InRangeDirection(TargetPredict.Direction))
                RunningDirection(TargetPredict.Direction);
            else
            {
                TargetPredict.Clear();
                RunningDefault();
            }
        }
        private void MotorsRunningDefault()
        {
            Utils.MyRotorAPI.RotorSetDefault(MotorAz, _Max_AV_az);
            Utils.MyRotorAPI.RotorsSetDefault(motorEvs_WTs?.Keys?.ToList(), _Max_AV_ev);
        }
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
        public void CycleWeapons() { }

        private Task TargetPredictTask;
    }
    public sealed partial class MyTurretBinding
    {
        private void BasicInit(IMyMotorStator motorAz)
        {
            try
            {
                Cameras.Clear(); Weapons.Clear(); motorEvs.Clear(); motorEvs_WTs.Clear();
                if (MyWeaponAndTurretApi.ActiveRotorBasicFilter(motorAz))
                    MotorAz = motorAz;
                else
                    MotorAz = null;
                if (Utils.Common.NullEntity(MotorAz)) return;
                motorEvs.UnionWith(Utils.Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985));
                if (Utils.Common.IsNullCollection(motorEvs)) return;
                foreach (var ev in motorEvs) { motorEvs_WTs.Add(ev, new HashSet<IMyTerminalBlock>()); }
                foreach (var ev_motor in motorEvs)
                {
                    if (Utils.Common.NullEntity(ev_motor) || (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(ev_motor))) continue;
                    var weapons = Utils.Common.GetTs<IMyTerminalBlock>(ev_motor, b => b.CubeGrid == ev_motor.TopGrid && Utils.Common.IsStaticWeapon(b));
                    var cameras = Utils.Common.GetTs<IMyCameraBlock>(ev_motor, b => b.CubeGrid == ev_motor.TopGrid);
                    if (!Utils.Common.IsNullCollection(weapons)) Weapons.UnionWith(weapons);
                    if (!Utils.Common.IsNullCollection(cameras)) Cameras.UnionWith(cameras);
                    if (!Utils.Common.IsNullCollection(weapons))
                    {
                        if (!motorEvs_WTs.ContainsKey(ev_motor))
                            motorEvs_WTs.Add(ev_motor, new HashSet<IMyTerminalBlock>(weapons));
                        motorEvs_WTs[ev_motor].UnionWith(weapons);
                    }
                    else if (!Utils.Common.IsNullCollection(cameras))
                    {
                        if (!motorEvs_WTs.ContainsKey(ev_motor))
                            motorEvs_WTs.Add(ev_motor, new HashSet<IMyTerminalBlock>(weapons));
                        motorEvs_WTs[ev_motor].UnionWith(weapons);
                    }
                }
                ReferWeapon();
            }
            catch (Exception) { Clear(); }
        }
        private void ReferWeapon()
        {
            string WeaponName = Definitions.ConfigName.WeaponDef.DefaultWeapon;
            string AmmoName = Definitions.ConfigName.ProjectileDef.DefaultAmmo;
            SlaveWeapons.ResetFireRPM(DefaultConfig.RPM);
            if (SlaveWeapons.UnabledFire) SlaveWeapons.LoadCurrentWeapons(Weapons, DefaultConfig.RPM);
            if (!SlaveWeapons.UnabledFire)
            {
                foreach (var CurrentWeapon in SlaveWeapons.CurrentWeapons)
                {
                    if (!BasicInfoService.WcApi.HasCoreWeapon(CurrentWeapon)) continue;
                    if (Utils.Common.NullEntity(TargetPredict.TargetLocked?.Entity)) continue;
                    BasicInfoService.WcApi.SetWeaponTarget(CurrentWeapon, TargetPredict.TargetLocked.Entity, MyWeaponAndTurretApi.GetWeaponID(CurrentWeapon));
                }
            }
            SetWeaponAmmos(WeaponName, AmmoName);
        }
        //private void WeaponsInit(MyTurretConfig Config)
        //{
        //    var evs = motorEvs_WTs.Keys;
        //    Cameras.Clear();
        //    Weapons.Clear();
        //    foreach (var ev_motor in evs)
        //    {
        //        if (Utils.Common.NullEntity(ev_motor) || (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(ev_motor))) continue;
        //        var weapons = Utils.Common.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid && Utils.Common.IsStaticWeapon(b));
        //        var cameras = Utils.Common.GetTs<IMyCameraBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid);
        //        if (!Utils.Common.IsNullCollection(weapons)) Weapons.AddRange(weapons);
        //        if (!Utils.Common.IsNullCollection(cameras)) Cameras.AddRange(cameras);
        //        if (!Utils.Common.IsNullCollection(weapons))
        //        {
        //            if (!motorEvs_WTs.ContainsKey(ev_motor))
        //                motorEvs_WTs.Add(ev_motor, new HashSet<IMyTerminalBlock>(weapons));
        //            motorEvs_WTs[ev_motor].UnionWith(weapons);
        //        }
        //        else if (!Utils.Common.IsNullCollection(cameras))
        //        {
        //            if (!motorEvs_WTs.ContainsKey(ev_motor))
        //                motorEvs_WTs.Add(ev_motor, new HashSet<IMyTerminalBlock>(weapons));
        //            motorEvs_WTs[ev_motor].UnionWith(weapons);
        //        }
        //    }
        //    ReferWeapon();
        //}
        private void SetWeaponAmmos(string WeaponName, string AmmoName)
        {
            if (Utils.Common.IsNullCollection(SlaveWeapons.CurrentWeapons)) return;
            var weapon = SlaveWeapons.CurrentWeapons.FirstOrDefault();
            if (Utils.Common.NullEntity(weapon) || !BasicInfoService.WcApi.HasCoreWeapon(weapon)) return;
            WeaponName = weapon.BlockDefinition.SubtypeId;
            AmmoName = BasicInfoService.WcApi.GetActiveAmmo(weapon, MyWeaponAndTurretApi.GetWeaponID(weapon));
            if (Utils.Common.IsNull(AmmoName)) return;
            foreach (var wp in SlaveWeapons.CurrentWeapons) BasicInfoService.WcApi.SetActiveAmmo(wp, MyWeaponAndTurretApi.GetWeaponID(wp), AmmoName);
            var value = MyWeaponAndTurretApi.GetWeaponCoreDefinition(weapon, AmmoName);
            if (value == null) return;
            ModifiedConfig = value.Value;
            ModifiedConfig.Trajectory.GravityMultiplier = DefaultConfig.Trajectory.GravityMultiplier * value.Value.Trajectory.GravityMultiplier;
        }
    }
    public sealed partial class MyTurretBinding
    {
        private MyWeaponParametersConfig DefaultConfig = default(MyWeaponParametersConfig);
        private MyWeaponParametersConfig ModifiedConfig = default(MyWeaponParametersConfig);
        public bool CanRunning => (!(Utils.Common.NullEntity(MotorAz) || Utils.Common.IsNullCollection(motorEvs_WTs)));
        private bool ManuelOnly => Utils.Common.IsNullCollection(Weapons) && CanManuel;
        private bool UnderControl => CanManuel && !Utils.Common.NullEntity(ControlledCamera);
        private MyTargetPredict TargetPredict { get; } = new MyTargetPredict();
        private Utils.FireWeaponManage SlaveWeapons { get; } = new Utils.FireWeaponManage();
        private IMyCameraBlock ControlledCamera { get { if (Utils.Common.IsNullCollection(Cameras)) return null; IMyCameraBlock cameraBlock = Cameras.FirstOrDefault(b => b.IsActive); return cameraBlock; } }

        private bool CanManuel => !Utils.Common.IsNullCollection(Cameras);
        private bool CanFire(float Precious = 0.00001f) => TargetPredict.CanFireWeapon(SlaveWeapons.CurrentWeapons, Precious);
        private float Mult { get { if (!UnderControl) return 0; return MathHelper.Clamp(MyAPIGateway.Session.Camera.FovWithZoom, 0.00001f, 0.5f); } }
    }
    public sealed partial class MyTurretBinding
    {
        private float _Max_AV_az = 3;
        private float _Max_AV_ev = 3;
        private float _Multipy = 8;
        private volatile bool _AzLimit = false;
        private volatile bool _EvLimit = true;
        private volatile float _AzUpperLimit = 170;
        private volatile float _AzLowerLimit = -170;
        private volatile float _EvUpperLimit = 80;
        private volatile float _EvLowerLimit = -5;
        private const float _AzUpperLimitMax = 170;
        private const float _AzLowerLimitMax = -170;
        private const float _EvUpperLimitMax = 89;
        private const float _EvLowerLimitMax = -89;
        private string _TurretWeaponConfigID = "DefaultSetup";
        private Vector2 _MaxSpeed => new Vector2(_Max_AV_ev, _Max_AV_az);
        public void ReadConfig_Turret_Rotors()
        {
            if (Utils.Common.IsNull(MotorAz)) return;
            Dictionary<string, Dictionary<string, string>> Config = MyConfigs.CustomDataConfigRead_INI(MotorAz.CustomData);
            if (Utils.Common.IsNullCollection(Config) || !Config.ContainsKey("TurretLimited")) { WriteTemplateConfig(); SetLimitedMotors(); return; }
            var value = Config["TurretLimited"];
            foreach (var item in value)
            {
                switch (item.Key)
                {
                    case "Max_AV_az": _Max_AV_az = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "Max_AV_ev": _Max_AV_ev = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "Multipy": _Multipy = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "AzLimit": _AzLimit = MyConfigs.ParseBool(item.Value); break;
                    case "EvLimit": _EvLimit = MyConfigs.ParseBool(item.Value); break;
                    case "AzUpperLimit": _AzUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
                    case "AzLowerLimit": _AzLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
                    case "EvUpperLimit": _EvUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
                    case "EvLowerLimit": _EvLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
                    case "TurretWeaponConfigID": _TurretWeaponConfigID = item.Value; break;
                    default: break;
                }
            }
            SetLimitedMotors();
        }
        private void WriteTemplateConfig()
        {
            if (Utils.Common.IsNull(MotorAz)) return;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("[TurretLimited]");
            sb.AppendLine($"Max_AV_az={_Max_AV_az}");
            sb.AppendLine($"Max_AV_ev={_Max_AV_ev}");
            sb.AppendLine($"Multipy={_Multipy}");
            sb.AppendLine($"AzLimit={_AzLimit}");
            sb.AppendLine($"EvLimit={_EvLimit}");
            sb.AppendLine($"AzUpperLimit={_AzUpperLimit}");
            sb.AppendLine($"AzLowerLimit={_AzLowerLimit}");
            sb.AppendLine($"EvUpperLimit={_EvUpperLimit}");
            sb.AppendLine($"EvLowerLimit={_EvLowerLimit}");
            sb.AppendLine($"TurretWeaponConfigID={_TurretWeaponConfigID}");
            MotorAz.CustomData = sb.ToString();
        }
        private void SetLimitedMotors()
        {
            try
            {
                if (!Utils.Common.IsNull(MotorAz))
                {
                    if (_AzLimit)
                    {
                        MotorAz.LowerLimitRad = MathHelper.ToRadians(_AzLowerLimit);
                        MotorAz.UpperLimitRad = MathHelper.ToRadians(_AzUpperLimit);
                    }
                    else
                    {
                        MotorAz.LowerLimitRad = float.MinValue;
                        MotorAz.UpperLimitRad = float.MaxValue;
                    }
                }
                if (!Utils.Common.IsNullCollection(motorEvs_WTs))
                {
                    if (_EvLimit && !Utils.Common.IsNull(MotorAz?.TopGrid))
                    {
                        foreach (var motorEvs_WT in motorEvs_WTs)
                        {
                            var sign = Math.Sign(MotorAz.TopGrid.WorldMatrix.Left.Dot(motorEvs_WT.Key.WorldMatrix.Up));
                            if (sign > 0)
                            {
                                motorEvs_WT.Key.LowerLimitRad = MathHelper.ToRadians(_EvLowerLimit);
                                motorEvs_WT.Key.UpperLimitRad = MathHelper.ToRadians(_EvUpperLimit);
                            }
                            else if (sign < 0)
                            {
                                motorEvs_WT.Key.LowerLimitRad = MathHelper.ToRadians(_EvUpperLimit) * sign;
                                motorEvs_WT.Key.UpperLimitRad = MathHelper.ToRadians(_EvLowerLimit) * sign;
                            }
                            else
                            {
                                motorEvs_WT.Key.LowerLimitRad = 0;
                                motorEvs_WT.Key.UpperLimitRad = 0;
                            }

                        }
                    }
                    else
                    {
                        foreach (var motorEvs_WT in motorEvs_WTs)
                        {
                            motorEvs_WT.Key.LowerLimitRad = float.MinValue;
                            motorEvs_WT.Key.UpperLimitRad = float.MaxValue;
                        }
                    }
                }
            }
            catch (Exception) { Clear(); }
        }
        public bool TargetInRange_Angle(MyTargetDetected _TargetDetected)
        {
            if (Utils.Common.NullEntity(_TargetDetected?.Entity)) return false;
            return InRangeDirection(_TargetDetected.GetEntityPosition(MotorAz));
        }
        private bool InRangeDirection(Vector3D? Direction)
        {
            if (Direction == null || Vector3D.IsZero(Direction.Value)) return false;
            var local_vector = Vector3D.TransformNormal(Direction.Value, MatrixD.Transpose(MotorAz.WorldMatrix));
            double az, ev;
            Vector3D.GetAzimuthAndElevation(local_vector, out az, out ev);
            if (_AzLimit && (az > MathHelper.ToRadians(_AzUpperLimit) || az < MathHelper.ToRadians(_AzLowerLimit))) return false;
            if (_EvLimit && (ev > MathHelper.ToRadians(_EvUpperLimit) || ev < MathHelper.ToRadians(_EvLowerLimit))) return false;
            return true;
        }
    }

    public sealed partial class MyTurretBinding
    {

        private IMyGridTerminalSystem GridTerminalSystem { get { if (Utils.Common.NullEntity(MotorAz?.CubeGrid)) return null; return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(MotorAz.CubeGrid); } }
        private HashSet<IMyCameraBlock> Cameras { get; } = new HashSet<IMyCameraBlock>();
        private HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
        public IMyMotorStator MotorAz { get; private set; }
        private HashSet<IMyMotorStator> motorEvs { get; set; } = new HashSet<IMyMotorStator>();
        private Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> motorEvs_WTs { get; } = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
        private Dictionary<string, HashSet<IMyTerminalBlock>> WeaponKinds { get; } = new Dictionary<string, HashSet<IMyTerminalBlock>>();
        private List<string> WeaponKindsNM => WeaponKinds.Keys.ToList();
        private string CurrentWeaponNM;

        //private void GetTurretParts()
        //{
        //    if (Utils.Common.NullEntity(motorAz?.CubeGrid) || Utils.Common.NullEntity(motorAz?.TopGrid)) return;
        //    Cameras.Clear();
        //    Weapons.Clear();
        //    motorEvs.Clear();
        //    motorEvs_WTs.Clear();
        //    WeaponKinds.Clear();
        //    GridTerminalSystem?.GetBlocksOfType(motorEvs, UsefulRotorEvs);
        //    if (Utils.Common.IsNullCollection(motorEvs)) return;
        //    HashSet<IMyTerminalBlock> AllTipBlocks = new HashSet<IMyTerminalBlock>();
        //    foreach (var motorEv in motorEvs)
        //    {
        //        if (!motorEvs_WTs.ContainsKey(motorEv))
        //            motorEvs_WTs.Add(motorEv, new HashSet<IMyTerminalBlock>());
        //        var weapons = Utils.Common.GetTs<IMyTerminalBlock>(GridTerminalSystem, b => b.CubeGrid == motorEv.TopGrid && Utils.Common.IsStaticWeapon(b) || (b is IMyCameraBlock));
        //        if (Utils.Common.IsNullCollection(weapons)) continue;
        //        motorEvs_WTs[motorEv].UnionWith(weapons);
        //        AllTipBlocks.UnionWith(weapons);
        //    }
        //    var weaponlist = AllTipBlocks.Where(Utils.Common.IsStaticWeapon);
        //    if (!Utils.Common.IsNullCollection(weaponlist))
        //        Weapons.AddRange(weaponlist);
        //    var cameralist = AllTipBlocks.Where(b => b is IMyCameraBlock)?.ToList()?.ConvertAll(b => b as IMyCameraBlock);
        //    if (!Utils.Common.IsNullCollection(cameralist))
        //        Cameras.AddRange(cameralist);
        //    ReferWeapon();
        //    foreach (var Weapon in Weapons)
        //    {
        //        if (!WeaponKinds.ContainsKey(Weapon.BlockDefinition.SubtypeId))
        //            WeaponKinds.Add(Weapon.BlockDefinition.SubtypeId, new HashSet<IMyTerminalBlock>() { Weapon });
        //        WeaponKinds[Weapon.BlockDefinition.SubtypeId].Add(Weapon);
        //    }
        //    CurrentWeaponNM = WeaponKindsNM.FirstOrDefault();
        //    if (Utils.Common.IsNull(CurrentWeaponNM)) return;
        //}



        private bool UsefulRotorEvs(IMyMotorStator MotorStatorEv)
        {
            if (Utils.Common.NullEntity(MotorStatorEv?.TopGrid) || Utils.Common.NullEntity(MotorAz?.TopGrid) || MotorAz.TopGrid != MotorStatorEv.CubeGrid) return false;
            return Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(MotorStatorEv.WorldMatrix.Up)) > 0.985;
        }
        private void Clear()
        {
            Cameras.Clear();
            Weapons.Clear();
            motorEvs.Clear();
            motorEvs_WTs.Clear();
            WeaponKinds.Clear();
            SlaveWeapons.CurrentWeapons?.Clear();
        }
    }
}