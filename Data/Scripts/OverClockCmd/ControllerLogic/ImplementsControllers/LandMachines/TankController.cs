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
    public class TankController : PlanetVehicle, ICtrlDevCtrl, IPlanetVehicle, ILandVehicle
    {
        public bool IsTank { get; set; }
        public TankController(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            MaximumCruiseSpeed = 80f;
            IsTank = true;
            AngularDampeners = Vector3.One * 10f;
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
            Action Wheels = () => { };
            if (WheelsGroup == null) return Wheels;
            bool CanRunning = false;            
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
        public float MaxiumRpm { get; set; } = 90f;
        public float DiffRpmPercentage { get; set; } = 1f;
        public float Friction { get; set; } = 80f;
        public float TurnFaction { get; set; } = 20f;
        public float ForwardIndicator { get; set; } = 0;
        public float TurnIndicator { get; set; } = 0;
        protected override Vector3? 姿态调整参数 { get { if (NoGravity) return null; return (Vector3.Up * 180000F * TurnIndicator + ProcessDampeners()); } }
        protected override Vector3 推进器控制参数 { get { if (HandBrake) return Vector3.Zero; Vector3 Ctrl = Vector3.Backward * ForwardIndicator; return (Ctrl != Vector3.Zero) ? Ctrl : Vector3.Forward; } }
        protected override bool ExtraEnabledGyros => (TurnIndicator != 0 || Vector3.Round(Vector3.TransformNormal(AngularVelocity, Matrix.Transpose(GetWorldMatrix(Me))) * (new Vector3(0.1f, 1, 0.1f)), 2) != Vector3.Zero);
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
        protected override bool Refer2Gravity => true;
        protected override bool Refer2Velocity => false;
        protected override bool Need2CtrlSignal => false;
        protected override bool IgnoreForwardVelocity => true;
        protected override bool ForwardOrUp => false;
        protected override bool EnabledAllDirection => true;
        protected override bool PoseMode => false;
        protected override Vector3 InitAngularDampener => new Vector3(60, 70, 60);
    }
}
