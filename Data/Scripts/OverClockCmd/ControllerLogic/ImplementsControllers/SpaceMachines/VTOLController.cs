using Sandbox.ModAPI;
using System;
using VRageMath;

namespace SuperBlocks
{
    public class VTOLController : SpaceShipCtrl, IHeilController, IPlaneController, IWingModeController, IPoseParamAdjust
    {
        public VTOLController(IMyTerminalBlock Me) : base(Me) { }
        protected override void SensorReading()
        {
            base.SensorReading();
            if (HoverMode)
            {
                MaxReactions_AngleV = 20f;
            }
            else
            {
                MaxReactions_AngleV = 90f;
            }
        }      
        protected override Vector4 RotationCtrlLines => (HasWings && (!ForwardOrUp) && (!NoGravity)) ? (new Vector4(MainCtrl.MoveIndicator.Z, MainCtrl.MoveIndicator.X, 0, MainCtrl.RotateIndicator.Z)) : base.RotationCtrlLines;
        protected override bool DisabledRotation => !HasWings && base.DisabledRotation;
        #region 控制信号映射
        protected override Vector3 推进器控制参数 => MainCtrl.MoveIndicator * (HandBrake ? Vector3.Zero : EnabledAllDirection ? Vector3.One : ForwardOrUp ? Vector3.Backward : Vector3.Up);
        protected override Vector3? 姿态调整参数 => 姿态处理(EnabledCuriser);
        protected override bool Refer2Velocity => (HasWings && (!NoGravity)) || base.Refer2Velocity;
        protected override bool EnabledAllDirection => HandBrake || NoGravity || (!HasWings);
        protected override bool PoseMode => HasWings ? (!ForwardOrUp) : (EnabledCuriser && (!NoGravity));
        protected override bool 保持高度 => (!ForwardOrUp) && MainCtrl.MoveIndicator.Y == 0;
        protected override bool 忽略高度 => ForwardOrUp || NoGravity;
        public bool HasWings { get; set; }
        public bool EnabledAirBrake { get { bool Enabled = HandBrake; if (MainCtrl != null) { Enabled = Enabled || (MainCtrl.MoveIndicator.Z > 0); } return Enabled; } }
        #endregion
    }
}
