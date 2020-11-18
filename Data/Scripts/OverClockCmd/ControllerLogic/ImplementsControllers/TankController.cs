using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;

namespace SuperBlocks
{
    using Sandbox.ModAPI.Interfaces;
    using SpaceEngineers.Game.ModAPI;
    using static Utils;

    /// <summary>
    /// 重力圈中的地面载具（坦克）
    /// 可选择悬浮、轮子、转子的驱动方式
    /// </summary>
    public class TankController : PlanetVehicle
    {
        public bool IsTank { get; set; }
        public TankController(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            MaximumCruiseSpeed = 80f;
            IsTank = true;
        }
        protected override void SensorReading()
        {
            if (MainCtrl == null || Gravity == Vector3.Zero)
            {
                ForwardIndicator = TurnIndicator = 0;
                return;
            }
            ForwardIndicator = MainCtrl.MoveIndicator.Z;
            TurnIndicator = MainCtrl.MoveIndicator.X;
        }
        protected override Action Init4GetAction()
        {
            var WheelsGroup = GridTerminalSystem.GetBlockGroupWithName("Wheels");
            Restrict(WheelsGroup);
            bool CanRunning = false;
            Action Wheels = () => { };
            {
                var action_wheels = 加载悬挂总成(WheelsGroup);
                if (action_wheels != null)
                    Wheels += action_wheels;
                CanRunning = CanRunning || (action_wheels != null);
            }
            {
                var action_wheels = 加载转子轮子(WheelsGroup);
                if (action_wheels != null)
                    Wheels += action_wheels;
                CanRunning = CanRunning || (action_wheels != null);
            }
            CanRunning = CanRunning || 检查是否有浮游设备();
            if (!CanRunning) throw new Exception("No Wheels or Rotor Wheels refered, and not the Hover Vehicle");
            Action UtilsCtrl = () => { };
            var brakelights = GetTs(GridTerminalSystem, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains("Brake Lights"));
            foreach (var item in brakelights) { UtilsCtrl += () => item.Enabled = ForwardIndicator == 0; }
            var backlights = GetTs(GridTerminalSystem, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains("Backward Lights"));
            foreach (var item in backlights) { UtilsCtrl += () => item.Enabled = ForwardIndicator > 0; }
            return (Wheels + UtilsCtrl);
        }
        protected override Action Init4GetAction100()
        {
            Action UtilsCtrl = () => { };
            var Doors = GridTerminalSystem.GetBlockGroupWithName("ACDoors");
            if (Doors != null)
            {
                List<IMyDoor> doors = GetTs<IMyDoor>(Doors);
                foreach (var door in doors) { UtilsCtrl += () => { if (door.OpenRatio == 1) door.CloseDoor(); }; }
            }
            return UtilsCtrl;
        }
        protected override void PoseCtrl()
        {
            var Roll_current_angular_velocity = Calc_Direction_Vector(AngularVelocity, Me.WorldMatrix.Forward, 1f);
            var Pitch_current_angular_velocity = Calc_Direction_Vector(AngularVelocity, Me.WorldMatrix.Right, 1f);
            var Yaw_current_angular_velocity = Calc_Direction_Vector(AngularVelocity, Me.WorldMatrix.Up, 1f);
            GyroControllerSystem?.SetEnabled(TurnIndicator != 0 || (MathHelper.RoundOn2(Yaw_current_angular_velocity) != 0 || MathHelper.RoundOn2(Roll_current_angular_velocity / 10) != 0 || MathHelper.RoundOn2(Pitch_current_angular_velocity / 10) != 0));
            GyroControllerSystem?.GyrosOverride(NoGravity ? null : new Vector3?(new Vector3(0, TurnIndicator * 180000F, 0)));
        }
        protected override void ThrustControl()
        {
            Vector3 Ctrl = (MainCtrl.HandBrake ? Vector3.Zero : Vector3.Backward) * MainCtrl.MoveIndicator;
            ThrustControllerSystem?.SetupMode(false, true, (!EnabledThrusters), MaximumSpeed);
            ThrustControllerSystem?.Running((Ctrl != Vector3.Zero) ? Ctrl : Vector3.Forward, 0, true);
        }
        public float MaxiumRpm { get; set; } = 90f;
        public float DiffRpmPercentage { get; set; } = 1f;
        public float Friction { get; set; } = 80f;
        public float TurnFaction { get; set; } = 20f;
        public float ForwardIndicator { get; set; } = 0;
        public float TurnIndicator { get; set; } = 0;
        #region 私有函数
        private const string MotorOverrideId = @"Propulsion override";
        private float 差速转向信号(int sign)
        {
            Vector2 Indicator = new Vector2(Math.Max(Math.Sign(MaximumSpeed - LinearVelocity.Length()), 0) * ForwardIndicator * sign, TurnIndicator * DiffRpmPercentage);
            if (Indicator != Vector2.Zero)
                Indicator = Vector2.Normalize(Indicator);
            return Vector2.Dot(Vector2.One, Indicator);
        }
        private Action 加载悬挂总成(IMyBlockGroup WheelsGroup)
        {
            var Motors = GetTs<IMyMotorSuspension>(WheelsGroup);
            if (!可以控制(Motors)) return null;
            Action Wheels = () => { };
            foreach (var Motor in Motors)
            {
                Wheels += () =>
                {
                    bool EnTrO = (IsTank || (Me.CubeGrid.Physics.LinearVelocity.LengthSquared() < 120f));
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                    var PropulsionOverride = EnTrO ? 差速转向信号(sign) : ForwardIndicator;
                    Motor.Steering = !IsTank;
                    Motor.SetValue<float>(Motor.GetProperty(MotorOverrideId).Id, Math.Sign(PropulsionOverride));
                    Motor.Power = Math.Abs(PropulsionOverride);
                    Motor.Friction = MathHelper.Clamp((TurnIndicator != 0) ? (IsTank ? (TurnFaction / Vector3.DistanceSquared(Motor.GetPosition(), Me.CubeGrid.GetPosition())) : Friction) : Friction, 0, Friction);
                    Motor.Brake = PropulsionOverride == 0;
                    Motor.InvertSteer = Motor.Steering && EnTrO && (TurnIndicator != 0) && (sign < 0);
                };
            }
            return Wheels;
        }
        private Action 加载转子轮子(IMyBlockGroup WheelsGroup)
        {
            List<IMyMotorStator> Motors = GetTs<IMyMotorStator>(WheelsGroup);
            if (!可以控制(Motors)) return null;
            Action Wheels = () => { };
            foreach (var Motor in Motors)
            {
                Wheels += () =>
                {
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                    Motor.TargetVelocityRPM = -差速转向信号(sign) * MaxiumRpm;
                };
            }
            return Wheels;
        }
        private bool 检查是否有浮游设备()
        {
            List<IMyThrust> Motors_Hover = GetTs(GridTerminalSystem, (IMyThrust thrust) => thrust.BlockDefinition.SubtypeId.Contains("Hover"));
            return 可以控制(Motors_Hover);
        }
        #endregion
    }
    public class PlanetVehicle : VehicleControllerBase
    {
        public PlanetVehicle(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected virtual Vector3? CtrlSignal_Gyros { get; }
        protected virtual Vector3 CtrlSignal_Thrusts { get; }
        protected virtual Action Init4GetAction() { return () => { }; }
        protected virtual Action Init4GetAction10() { return () => { }; }
        protected virtual Action Init4GetAction100() { return () => { }; }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            AppRunning1 += Init4GetAction();
            AppRunning10 += Init4GetAction10();
            AppRunning100 += Init4GetAction100();
        }
        protected override void PoseCtrl()
        {
            GyroControllerSystem?.SetEnabled(EnabledGyros);
            GyroControllerSystem?.GyrosOverride(CtrlSignal_Gyros);
        }
        protected override void ThrustControl()
        {
            ThrustControllerSystem?.SetupMode(false, true, (!EnabledThrusters), MaximumSpeed);
            ThrustControllerSystem?.Running(CtrlSignal_Thrusts, 0, true);
        }
        public float MaximumCruiseSpeed { get { return _MaxiumSpeed * 3.6f; } set { _MaxiumSpeed = Math.Abs(value / 3.6f); } }
        protected override float MaximumSpeed => _MaxiumSpeed;
        private float _MaxiumSpeed;
    }
}
