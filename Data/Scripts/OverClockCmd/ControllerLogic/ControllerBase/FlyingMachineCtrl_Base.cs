using Sandbox.ModAPI;
using System;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    public class FlyingMachineCtrl_Base : VehicleControllerBase
    {
        protected virtual bool 保持高度 { get; }
        protected virtual bool 忽略高度 { get; }
        protected override void SensorReading()
        {
            base.SensorReading();
            if (NoGravity) return;
            sealevel = MainCtrl.SeaLevel;
            UpdateTargetSealevel();
        }

        public FlyingMachineCtrl_Base(IMyTerminalBlock Me) : base(Me) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            sealevel = MainCtrl.SeaLevel;
            _Target_Sealevel = sealevel;
            diffsealevel = 0;
            //UpdateTargetSealevel();
        }
        protected override void PoseCtrl()
        {
            if (!GyrosIsReady) return;
            GyroControllerSystem?.SetEnabled(EnabledGyros);
            GyroControllerSystem?.GyrosOverride(姿态调整参数);
        }
        protected override void ThrustControl()
        {
            if (!ThrustsIsReady) return;
            Vector3 Ctrl = 推进器控制参数;
            bool CtrlOrCruise = (!ForwardOrUp || (Ctrl != Vector3.Zero));
            UpdateTargetSealevel();
            target_speed = MathHelper.Clamp(HandBrake ? 0 : (Ctrl != Vector3.Zero) ? ForwardOrUp ? Me.CubeGrid.Physics.LinearVelocity.Dot(Me.WorldMatrix.Forward) : 0 : target_speed, 0, MaximumSpeed);
            ThrustControllerSystem?.SetupMode((!ForwardOrUp), EnabledAllDirection, (!EnabledThrusters), CtrlOrCruise ? MaximumSpeed : target_speed);
            ThrustControllerSystem?.Running(CtrlOrCruise ? Ctrl : Vector3.Forward, diffsealevel, Dampener);
        }

        #region 一些私有函数       
        protected override bool DisabledRotation => (!GyrosIsReady || MainCtrl.NullMainCtrl);
        protected override Vector4 RotationCtrlLines => new Vector4(0, 0, MainCtrl.RotateIndicator.X, MainCtrl.RotateIndicator.Y);
        protected double sealevel;
        protected double _Target_Sealevel;
        protected float target_speed = 0;
        protected float diffsealevel;
        protected void UpdateTargetSealevel()
        {
            if (忽略高度)
                diffsealevel = 0;
            else
            {
                if (!保持高度) _Target_Sealevel = sealevel;
                diffsealevel = (float)(_Target_Sealevel - sealevel) * 25f;
            }
        }
        #endregion
    }
}
