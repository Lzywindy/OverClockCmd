using Sandbox.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    public class SpaceShipCtrl : FlyingMachineCtrl_Base, IHeilController, IPlaneController, IPoseParamAdjust
    {
        public SpaceShipCtrl(IMyTerminalBlock Me) : base(Me) { }
        protected override void SensorReading()
        {
            base.SensorReading();
        }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            EnabledCuriser = false;
            target_speed = 0;
            MaxReactions_AngleV = 20f;
        }
        #region 控制信号映射
        protected override bool Refer2Gravity => !NoGravity;
        protected override bool Refer2Velocity => (ProjectLinnerVelocity_CockpitForward.LengthSquared() >= MaxiumHoverSpeed * MaxiumHoverSpeed) && (NoGravity || ForwardOrUp);
        protected override bool Need2CtrlSignal => !(ForwardOrUp || NoGravity);
        protected override bool IngroForwardVelocity => ForwardOrUp || NoGravity;
        protected override bool ForwardOrUp => !HoverMode;
        protected override bool EnabledAllDirection => true;
        protected override float MaximumSpeed => ForwardOrUp ? MaxiumFlightSpeed : MaxiumHoverSpeed;
        protected override Vector3? 姿态调整参数 => 姿态处理(ref ForwardDirection, EnabledCuriser);
        protected override Vector3 推进器控制参数 => HandBrake ? Vector3.Zero : MainCtrl.MoveIndicator;
        protected override bool 保持高度 => (!ForwardOrUp) && MainCtrl.MoveIndicator.Y == 0;
        protected override bool 忽略高度 => ForwardOrUp || NoGravity;
        protected override bool PoseMode => EnabledCuriser && (!NoGravity);
        #endregion
        public float MaxiumFlightSpeed { get; set; } = 1000f;
        public float MaxiumHoverSpeed { get; set; } = 30f;
        public bool EnabledCuriser { get; set; } = false;
        public bool HoverMode { get { return _HoverMode; } set { _HoverMode = value; if (_HoverMode) diffsealevel = (float)(_Target_Sealevel - sealevel) * 25f; else target_speed = LinearVelocity.Length(); } }
        private bool _HoverMode;
    }
}
