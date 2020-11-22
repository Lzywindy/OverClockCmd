using Sandbox.Game.World;
using Sandbox.ModAPI;
using System;
using VRageMath;

namespace SuperBlocks
{
    public class HelicopterController : FlyingMachineCtrl_Base, IHeilController
    {
        protected override bool Refer2Gravity => true;
        protected override bool Refer2Velocity => true;
        protected override bool Need2CtrlSignal => true;
        protected override bool IngroForwardVelocity => false;
        protected override bool ForwardOrUp => !HoverMode;
        protected override bool EnabledAllDirection => (MainCtrl == null) || HandBrake || NoGravity;
        protected override float MaximumSpeed => MaxiumHoverSpeed;
        public HelicopterController(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void SensorReading()
        {
            base.SensorReading();
        }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            MaxReactions_AngleV = 20f;
        }

        protected override Vector3? 姿态调整参数 => 姿态处理(true);
        protected override Vector3? 姿态处理(bool _EnabledCuriser)
        {
            if (MainCtrl == null || NoGravity) return null;
            var GyroSignal = 参考平面处理(MainCtrl.MoveIndicator.Z, MainCtrl.MoveIndicator.X, MaximumSpeed);
            if (!GyroSignal.HasValue) { return null; }
            return 飞船朝向处理(0, MainCtrl.RotateIndicator.Z, _EnabledCuriser, GyroSignal.Value);
        }
        protected override Vector3 推进器控制参数 => MainCtrl.MoveIndicator * Vector3.Up;
        public bool HoverMode { get { return true; } set { } }
        protected override bool 保持高度 => MainCtrl.MoveIndicator.Y == 0;
        protected override bool 忽略高度 => ForwardOrUp || NoGravity;
        protected override bool PoseMode => true;
        public float MaxiumHoverSpeed { get { return _MaxiumHoverSpeed; } set { _MaxiumHoverSpeed = MathHelper.Clamp(value, 5, 100); } }
        private float _MaxiumHoverSpeed;
    }
}
