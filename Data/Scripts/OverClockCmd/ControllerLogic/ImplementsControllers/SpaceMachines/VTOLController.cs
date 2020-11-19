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
        protected override Vector3? 姿态处理(ref Vector3 朝向, bool _EnabledCuriser)
        {
            if (HasWings)
            {
                bool HasGravity_HoverMode = (!ForwardOrUp) && (!NoGravity);
                float pitch_indicate = 0;
                float roll_indicate = 0;
                float yaw_indicate = 0;
                if (MainCtrl != null)
                {
                    pitch_indicate = HasGravity_HoverMode ? MainCtrl.MoveIndicator.Z : MainCtrl.RotationIndicator.X;
                    roll_indicate = HasGravity_HoverMode ? MainCtrl.MoveIndicator.X : 0;
                    yaw_indicate = HasGravity_HoverMode ? MainCtrl.RollIndicator : MainCtrl.RotationIndicator.Y;
                }
                var GyroSignal = 参考平面处理(HasGravity_HoverMode ? pitch_indicate : 0, roll_indicate, MaximumSpeed);
                if (!GyroSignal.HasValue) { return null; }
                return 飞船朝向处理(HasGravity_HoverMode ? 0 : pitch_indicate, yaw_indicate, _EnabledCuriser, GyroSignal.Value, ref 朝向);
            }
            else
                return base.姿态处理(ref 朝向, _EnabledCuriser);
        }
        #region 控制信号映射
        protected override Vector3 推进器控制参数 => MainCtrl.MoveIndicator * (HandBrake ? Vector3.Zero : EnabledAllDirection ? Vector3.One : ForwardOrUp ? Vector3.Backward : Vector3.Up);
        protected override Vector3? 姿态调整参数 => 姿态处理(ref ForwardDirection, EnabledCuriser);
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
