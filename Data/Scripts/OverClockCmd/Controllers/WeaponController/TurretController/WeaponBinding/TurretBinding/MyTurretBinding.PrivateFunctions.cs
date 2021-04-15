using ParallelTasks;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRage;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public sealed partial class MyTurretBinding
    {
        private void RunningAutoFire(bool FireWeapons)
        {
            if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons?.FirstOrDefault()))
            {
                SlaveWeapons.SetFire(FireWeapons);
                SlaveWeapons.RunningAutoFire(FireWeapons);
            }
            else
            {
                var block = Utils.Common.GetT<IMyTimerBlock>(MotorAz, b => b.CustomName.Contains("weapon") && b.CubeGrid == MotorAz.TopGrid);
                if (block == null) return;
                block.Enabled = FireWeapons;
                if (FireWeapons) block.Trigger();
            }
        }
        private void RunningDefault() => MotorsRunningDefault();
        private void RunningDirection(Vector3D? Direction)
        {
            if (!CanRunning || Direction == null) { MotorsRunningDefault(); return; }
            float TurretRotor_Torque = 0;
            int count = 0;
            foreach (var MotorEvWp in MotorEvs_WTs)
            {
                if (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(MotorEvWp.Key)) continue;
                MotorEvWp.Value.RemoveWhere(Utils.Common.NullEntity);
                if (Utils.Common.IsNullCollection(MotorEvWp.Value)) { Utils.MyRotorAPI.RotorSetDefault(MotorEvWp.Key); continue; }
                var data = MyWeaponAndTurretApi.Get_ArmOfForce_Point(MotorEvWp);
                TurretRotor_Torque += MyWeaponAndTurretApi.GetTorque(MotorAz, data, Direction, _Multipy);
                MotorEvWp.Key.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(MotorEvWp.Key, MyWeaponAndTurretApi.RotorDampener(MyWeaponAndTurretApi.GetTorque(MotorEvWp.Key, data, Direction, _Multipy * 1.4f), MotorEvWp.Key.TargetVelocityRad, _Max_AV_ev));
                count++;
            }
            if (count > 1) TurretRotor_Torque /= count;
            MotorAz.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(MotorAz, MyWeaponAndTurretApi.RotorDampener(TurretRotor_Torque, MotorAz.TargetVelocityRad, _Max_AV_az));
        }
        private void RunningManual(Vector2? Rotation)
        {
            if (!CanRunning || Rotation == null) { MotorsRunningDefault(); return; }
            var finalCtrl = Vector2.Clamp(Rotation.Value * Mult, -_MaxSpeed, _MaxSpeed);
            MotorAz.TargetVelocityRad = finalCtrl.Y;
            var MD = MotorAz.TopGrid.WorldMatrix.Left;
            var TurretEvs = MotorEvs.ToList();
            if (Utils.Common.IsNullCollection(TurretEvs)) return;
            var MountEv = TurretEvs[0];
            MountEv.TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(MountEv, MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -_Max_AV_ev, _Max_AV_ev));// MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -Config.max_ev, Config.max_ev);
            for (int index = 1; index < TurretEvs.Count; index++)
                TurretEvs[index].TargetVelocityRad = Utils.MyRotorAPI.RotorRunning(TurretEvs[index], MathHelper.Clamp((MathHelper.WrapAngle(Math.Sign(TurretEvs[0].WorldMatrix.Up.Dot(TurretEvs[index].WorldMatrix.Up)) * MountEv.Angle) - MathHelper.WrapAngle(TurretEvs[index].Angle)) * 35, _Max_AV_ev, _Max_AV_ev));

        }
        private void RunningAutoAimAt(IMyTerminalBlock Me)
        {
            ReferWeapon();
            var Direction = MyTargetPredict.CalculateDirection_TargetTest(Me, Weapons, AimTarget, ref ModifiedConfig);
            if (InRangeDirection(Direction))
                RunningDirection(Direction);
            else
            {
                RunningDefault();
            }
        }
        private void MotorsRunningDefault()
        {
            Utils.MyRotorAPI.RotorSetDefault(MotorAz, _Max_AV_az);
            Utils.MyRotorAPI.RotorsSetDefault(MotorEvs, _Max_AV_ev);
        }
        private void RemoveEmptyMotorEvs()
        {
            //var motors = motorEvs_WTs.Keys.Where(Utils.Common.NullEntity);
            //if (Utils.Common.IsNullCollection(motors)) return;
            //foreach (var motor in motors) motorEvs_WTs.Remove(motor);

        }
        private void RemoveEmptyBlocks()
        {
            RemoveEmptyMotorEvs();
            Cameras.RemoveWhere(Utils.Common.NullEntity);
            Weapons.RemoveWhere(Utils.Common.NullEntity);
        }

        internal static float GetTorque(IMyMotorStator Motor, MyTuple<Vector3D?, Vector3D?> ArmOfForce_Point, Vector3D? Direction, double ForceLength)
        {
            if (Motor == null || Motor.TopGrid == null) return 0;
            if (ArmOfForce_Point.Item1 == null || ArmOfForce_Point.Item2 == null || Direction == null || Direction.Value == Vector3D.Zero || ForceLength == 0)
                return -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -30, 30);
            var arm = ArmOfForce_Point.Item1.Value;
            var force = Direction.Value * ForceLength;
            return (float)Vector3D.Dot(Vector3D.Cross(force, arm), Motor.Top.WorldMatrix.Up);
        }
        internal static MyTuple<Vector3D?, Vector3D?> Get_ArmOfForce_Point(IMyMotorStator Motor, IEnumerable<IMyTerminalBlock> Guns)
        {
            if (Utils.Common.NullEntity(Motor) || Utils.Common.IsNullCollection(Guns)) return new MyTuple<Vector3D?, Vector3D?>(null, null);
            Vector3D center = Vector3D.Zero;
            Vector3D direction = Vector3D.Zero;
            foreach (var gun in Guns)
            {
                center += gun.GetPosition();
                if (direction == Vector3D.Zero)
                    direction = gun.WorldMatrix.Forward;
            }
            center /= Guns.Count();
            return new MyTuple<Vector3D?, Vector3D?>(direction * Vector3D.Distance(center, Motor.GetPosition()), center);//Get arm of force and it force point
        }
    }
    public sealed partial class MyTurretBinding
    {
        public IMyMotorStator MotorAz { get; private set; }
        private HashSet<IMyMotorStator> MotorEvs { get; set; } = new HashSet<IMyMotorStator>();
        private HashSet<IMyCameraBlock> Cameras { get; } = new HashSet<IMyCameraBlock>();
        private HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
        private Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> MotorEvs_WTs { get; } = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
        public void UpdateBinding()
        {
            if (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(MotorAz)) return;
            MotorEvs.Clear();
            MotorEvs.UnionWith(Utils.Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985));
            if (Utils.Common.IsNullCollection(MotorEvs)) return;
            MotorEvs_WTs.Clear();
            Weapons.Clear();
            Cameras.Clear();
            foreach (var MotorEv in MotorEvs)
            {
                if (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(MotorEv)) continue;
                var weapons = Utils.Common.GetTs<IMyTerminalBlock>(MotorEv, b => b.CubeGrid == MotorEv.TopGrid && Utils.Common.IsStaticWeapon(b));
                var cameras = Utils.Common.GetTs<IMyCameraBlock>(MotorEv, b => b.CubeGrid == MotorEv.TopGrid);
                if (!Utils.Common.IsNullCollection(weapons)) Weapons.UnionWith(weapons);
                if (!Utils.Common.IsNullCollection(cameras)) Cameras.UnionWith(cameras);
                if (Utils.Common.IsNullCollection(weapons)) { Utils.MyRotorAPI.RotorSetDefault(MotorEv); continue; }
                MotorEvs_WTs.Add(MotorEv, new HashSet<IMyTerminalBlock>(weapons));
            }
            ReferWeapon();
        }
    }
}