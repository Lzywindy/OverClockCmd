using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRageMath;
using System.Linq;
using System.Collections.Concurrent;
using System;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
namespace SuperBlocks.Controller
{
    public class MyTurretBinding_Base
    {
        public virtual void Init(IMyMotorStator motorAz, ref MyTurretConfig Config)
        {
            this.Config = Config;
            motorEvs_WTs.Clear();
            if (motorAz.CustomName.Contains(Config.TurretAzNM) && ActiveRotorBasicFilter(motorAz))
                this.motorAz = motorAz;
            else
                this.motorAz = null;
            var EvName = Config.TurretEzNM;
            if (Utils.NullEntity(this.motorAz)) return;
            var evs = Utils.GetTs<IMyMotorStator>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(this.motorAz.TopGrid), b => b.CubeGrid == this.motorAz.TopGrid && b.CustomName.Contains(EvName) && b.TopGrid != null);
            if (Utils.IsNullCollection(evs)) return;
            foreach (var ev in evs) { motorEvs_WTs.AddOrUpdate(ev, new HashSet<IMyTerminalBlock>(), (key, value) => { return value; }); }
        }
        public void RunningDefault()
        {
            RemoveEmptyMotorEvs();
            MotorsRunningDefault();
        }
        public void RunningDirection(Vector3D? Direction)
        {
            RemoveEmptyMotorEvs();
            if (Utils.IsNull(motorAz) || Utils.IsNullCollection(motorEvs_WTs) || Direction == null) { MotorsRunningDefault(); return; }
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in motorEvs_WTs)
            {
                var data = Get_ArmOfForce_Point(Gun_Rotor_Group);
                TurretRotor_Torque += GetTorque(motorAz, data, Direction, Config.mult);
                Gun_Rotor_Group.Key.TargetVelocityRad = RotorDampener(GetTorque(Gun_Rotor_Group.Key, data, Direction, Config.mult), Gun_Rotor_Group.Key.TargetVelocityRad, Config.max_ev);
            }
            TurretRotor_Torque /= motorEvs_WTs.Count;
            motorAz.TargetVelocityRad = RotorDampener(TurretRotor_Torque, motorAz.TargetVelocityRad, Config.max_az);
        }
        public void RunningManual(Vector2? Rotation)
        {
            RemoveEmptyMotorEvs();
            if (Utils.IsNull(motorAz) || Utils.IsNullCollection(motorEvs_WTs) || Rotation == null) { MotorsRunningDefault(); return; }
            var Reverse = new Vector2(ReverseEv ? -1 : 1, ReverseAz ? -1 : 1);
            var finalCtrl = Vector2.Clamp(Rotation.Value * Reverse * Mult, -Max_Speed, Max_Speed);
            motorAz.TargetVelocityRPM = finalCtrl.Y;
            var MD = motorAz.WorldMatrix.Left;
            var TurretEvs = motorEvs_WTs?.Keys?.ToList();
            if (Utils.IsNullCollection(TurretEvs)) return;
            var MountEv = TurretEvs[0];
            MountEv.TargetVelocityRad = finalCtrl.X;
            var MountDirection = TurretEvs[0].WorldMatrix.Up;
            var t_sign = MD.Dot(MountDirection);
            for (int index = 1; index < TurretEvs.Count; index++)
            {
                var sign = (float)Math.Sign(t_sign * MountDirection.Dot(TurretEvs[index].WorldMatrix.Up));
                TurretEvs[index].TargetVelocityRPM = MathHelper.WrapAngle(TurretEvs[index].Angle) - sign * MathHelper.WrapAngle(MountEv.Angle);
            }
        }
        #region 转子控制函数
        private void MotorsRunningDefault()
        {
            Utils.RotorSetDefault(motorAz, Config.max_az);
            Utils.RotorsSetDefault(motorEvs_WTs?.Keys?.ToList(), Config.max_ev);
        }
        private static float RotorDampener(float ControlValue, float CurretValue, float MaxValue)
        {
            return MathHelper.Clamp(ControlValue - MathHelper.Clamp(Math.Max(ControlValue, CurretValue), -1, 1) * 0.5f, -MaxValue, MaxValue);
        }
        private static float GetTorque(IMyMotorStator Motor, MyTuple<Vector3D?, Vector3D?> ArmOfForce_Point, Vector3D? Direction, double ForceLength)
        {
            if (Motor == null || Motor.TopGrid == null) return 0;
            if (ArmOfForce_Point.Item1 == null || ArmOfForce_Point.Item2 == null || Direction == null || Direction.Value == Vector3D.Zero || ForceLength == 0)
                return -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -30, 30);
            var arm = ArmOfForce_Point.Item1.Value;
            var force = Direction.Value * ForceLength;
            return (float)Vector3D.Dot(Vector3D.Cross(force, arm), Motor.Top.WorldMatrix.Up);
        }
        private static MyTuple<Vector3D?, Vector3D?> Get_ArmOfForce_Point(KeyValuePair<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Group)
        {
            if (Gun_Rotor_Group.Key == null || Gun_Rotor_Group.Value == null || Gun_Rotor_Group.Value.Count < 1) return new MyTuple<Vector3D?, Vector3D?>(null, null);
            Vector3D center = Vector3D.Zero;
            Vector3D direction = Vector3D.Zero;
            foreach (var gun in Gun_Rotor_Group.Value)
            {
                center += gun.GetPosition();
                if (direction == Vector3D.Zero)
                    direction = gun.WorldMatrix.Forward;
            }
            center /= Gun_Rotor_Group.Value.Count;
            return new MyTuple<Vector3D?, Vector3D?>(direction * Vector3D.Distance(center, Gun_Rotor_Group.Key.GetPosition()), center);//Get arm of force and it force point
        }
        protected static bool ActiveRotorBasicFilter(IMyMotorStator block)
        {
            if (block?.TopGrid == null || !block.IsFunctional) return false;
            return true;
        }
        #endregion
        #region 其他方法与函数
        public MyTurretConfig Config;
        protected IMyMotorStator motorAz { get; private set; }
        protected ConcurrentDictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> motorEvs_WTs { get; } = new ConcurrentDictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
        public bool ReverseAz { get; set; } = false;
        public bool ReverseEv { get; set; } = false;
        private Vector2 Max_Speed => new Vector2(Config.max_ev, Config.max_az);
        protected virtual Vector2 Mult { get; }
        protected virtual void RemoveEmptyMotorEvs()
        {
            var motors = motorEvs_WTs.Keys.Where(Utils.NullEntity);
            if (Utils.IsNullCollection(motors)) return;
            foreach (var motor in motors)
                motorEvs_WTs.Remove(motor);
        }
        #endregion
    }
    public class MyTurretBinding : MyTurretBinding_Base
    {
        public override void Init(IMyMotorStator motorAz, ref MyTurretConfig Config)
        {
            base.Init(motorAz, ref Config);
            Cameras.Clear();
            Weapons.Clear();
            var evs = motorEvs_WTs.Keys.ToList();
            foreach (var ev_motor in evs)
            {
                if (Utils.NullEntity(ev_motor) || (!ActiveRotorBasicFilter(motorAz))) continue;
                var weapons = Utils.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid && !(b is IMyLargeTurretBase) && b.CustomName.Contains("Gun"));
                var cameras = Utils.GetTs<IMyCameraBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(ev_motor.TopGrid), b => b.CubeGrid == ev_motor.TopGrid);
                if (!Utils.IsNullCollection(weapons)) Weapons.UnionWith(weapons);
                if (!Utils.IsNullCollection(cameras)) Cameras.UnionWith(cameras);
                if (!Utils.IsNullCollection(weapons))
                    motorEvs_WTs.AddOrUpdate(ev_motor, new HashSet<IMyTerminalBlock>(weapons), (key, value) => { return new HashSet<IMyTerminalBlock>(weapons); });
                else if (!Utils.IsNullCollection(cameras))
                    motorEvs_WTs.AddOrUpdate(ev_motor, new HashSet<IMyTerminalBlock>(weapons), (key, value) => { return new HashSet<IMyTerminalBlock>(weapons); });
            }
        }
        protected override Vector2 Mult { get { if (!UnderControl) return Vector2.Zero; return MathHelper.Clamp(MyAPIGateway.Session.Camera.FovWithZoom, 0.00001f, 0.5f) * Vector2.One; } }
        public bool Enabled => (!(Utils.NullEntity(motorAz) || Utils.IsNullCollection(motorEvs_WTs)));
        public HashSet<IMyCameraBlock> Cameras { get; } = new HashSet<IMyCameraBlock>();
        public HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
        public bool ManuelOnly => Utils.IsNullCollection(Weapons) && CanManuel;
        public bool CanManuel => !Utils.IsNullCollection(Cameras);
        public bool UnderControl => CanManuel && !Utils.NullEntity(ControlledCamera);
        private IMyCameraBlock ControlledCamera { get { if (Utils.IsNullCollection(Cameras)) return null; IMyCameraBlock cameraBlock = Cameras.FirstOrDefault(b => b.IsActive); return cameraBlock; } }
        protected override void RemoveEmptyMotorEvs()
        {
            base.RemoveEmptyMotorEvs();
            Cameras.RemoveWhere(Utils.NullEntity);
            Weapons.RemoveWhere(Utils.NullEntity);
        }
    }
    public class MyWeaponBinding
    {
        private const string GunPointNM = "muzzle_projectile";
        Dictionary<IMyTerminalBlock, Dictionary<IMyEntity, List<IMyModelDummy>>> WeaponMuzzles { get; } = new Dictionary<IMyTerminalBlock, Dictionary<IMyEntity, List<IMyModelDummy>>>();
        public void Init(List<IMyTerminalBlock> Weapons)
        {
            foreach (var weapon in Weapons)
                WeaponMuzzles.Add(weapon, GetWeaponMuzzles(weapon));
        }
        private Dictionary<IMyEntity, List<IMyModelDummy>> GetWeaponMuzzles(IMyTerminalBlock weapon)
        {
            var w = weapon as MyEntity;
            Dictionary<IMyEntity, List<IMyModelDummy>> muzzle_projectiles_l = new Dictionary<IMyEntity, List<IMyModelDummy>>();
            if (!Utils.IsNullCollection(w.Subparts))
            {
                foreach (var Subpart in w.Subparts)
                    muzzle_projectiles_l.Add(Subpart.Value, Utils.GetDummies(Subpart.Value, GunPointNM));
            }
            else
                muzzle_projectiles_l.Add(weapon, Utils.GetDummies(weapon, GunPointNM));
            return muzzle_projectiles_l;
        }
        private Vector3D? GetDummiesWM(Dictionary<IMyEntity, List<IMyModelDummy>> EntityDummies)
        {
            if (Utils.IsNullCollection(EntityDummies)) return null;
            Vector3D Position=Vector3D.Zero;
            int count = 0;
            foreach (var EntityDummy in EntityDummies)
            {
                foreach (var dummy in EntityDummy.Value)
                {
                    Position += Vector3D.Transform(dummy.Matrix.Translation, EntityDummy.Key.WorldMatrix);
                    count++;
                }
            }
            if (count <= 0) return null;
            return (Position / count);
        }
    }
}