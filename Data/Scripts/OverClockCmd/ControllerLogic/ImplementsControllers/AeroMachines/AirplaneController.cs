using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    public class AirplaneController : FlyingMachineCtrl_Base, IPlaneController, IWingModeController, IPoseParamAdjust
    {
        public AirplaneController(IMyTerminalBlock Me) : base(Me) { }
        public bool EnabledAirBrake { get { bool Enabled = false; if (MainCtrl != null) { Enabled = MainCtrl.HandBrake || (MainCtrl.MoveIndicator.Z > 0); } return Enabled; } }
        protected override bool Refer2Gravity => true;
        protected override bool Refer2Velocity => true;
        protected override bool Need2CtrlSignal => false;
        protected override bool IngroForwardVelocity => true;
        protected override bool ForwardOrUp => true;
        protected override bool EnabledAllDirection => (MainCtrl == null) || HandBrake || NoGravity || (!HasWings);
        protected override float MaximumSpeed => MaxiumFlightSpeed;

        protected override void SensorReading()
        {
            base.SensorReading();
        }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            MaxReactions_AngleV = 45f;
            target_speed = 0;
            EnabledCuriser = false;
            HasWings = false;
        }
        protected override Vector3? 姿态调整参数 => 姿态处理(ref ForwardDirection, EnabledCuriser);
        protected override Vector3? 姿态处理(ref Vector3 朝向, bool _EnabledCuriser)
        {
            if (MainCtrl == null || NoGravity) return null;
            var GyroSignal = 参考平面处理(0, 0, MaximumSpeed);
            if (!GyroSignal.HasValue) { return null; }
            return 飞船朝向处理(MainCtrl.RotationIndicator.X, MainCtrl.RotationIndicator.Y, _EnabledCuriser, GyroSignal.Value, ref 朝向);
        }
        protected override Vector3 推进器控制参数 => MainCtrl.MoveIndicator * Vector3.Backward;
        public bool EnabledCuriser { get; set; }
        protected override bool PoseMode => false;
        protected override bool 保持高度 => false;
        protected override bool 忽略高度 => true;
        public bool HasWings { get; set; }
        public float MaxiumFlightSpeed { get { return _MaxiumFlightSpeed; } set { _MaxiumFlightSpeed = MathHelper.Clamp(value, 30, float.MaxValue); } }
        private float _MaxiumFlightSpeed;
    }
}
