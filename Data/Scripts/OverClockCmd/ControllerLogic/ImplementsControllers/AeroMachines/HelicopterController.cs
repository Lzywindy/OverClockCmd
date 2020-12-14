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
        protected override bool IgnoreForwardVelocity => false;
        protected override bool ForwardOrUp => false;
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
        protected override Vector3? 姿态调整参数 => 姿态处理(false);
        protected override Vector4 RotationCtrlLines => new Vector4(MainCtrl.MoveIndicator.Z, MainCtrl.MoveIndicator.X, 0, MainCtrl.RotateIndicator.Z);
        protected override bool DisabledRotation => (MainCtrl.NullMainCtrl || NoGravity);
        protected override Vector3 推进器控制参数 => MainCtrl.MoveIndicator * Vector3.Up;
        public bool HoverMode { get { return true; } set { } }
        protected override bool 保持高度 => MainCtrl.MoveIndicator.Y == 0;
        protected override bool 忽略高度 => NoGravity;
        protected override bool PoseMode => true;
        public float MaxiumHoverSpeed { get { return _MaxiumHoverSpeed; } set { _MaxiumHoverSpeed = MathHelper.Clamp(value, 5, 100); } }
        private float _MaxiumHoverSpeed;
    }
}
