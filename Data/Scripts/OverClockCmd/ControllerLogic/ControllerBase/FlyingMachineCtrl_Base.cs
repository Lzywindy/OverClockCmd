using Sandbox.ModAPI;
using System;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    public class FlyingMachineCtrl_Base : VehicleControllerBase
    {

        protected virtual Vector3? 姿态调整参数 { get; }
        protected virtual Vector3 推进器控制参数 { get; }
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
            ForwardDirection = Me.WorldMatrix.Forward;
            sealevel = MainCtrl.SeaLevel;
            _Target_Sealevel = sealevel;
            diffsealevel = 0;
            //UpdateTargetSealevel();
        }
        protected override void PoseCtrl()
        {
            if (!GyrosIsReady) return;
            var value = 姿态调整参数;
            GyroControllerSystem?.SetEnabled(EnabledGyros);
            GyroControllerSystem?.GyrosOverride(value);
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
        protected virtual Vector3? 姿态处理(ref Vector3 朝向, bool _EnabledCuriser)
        {
            if (MainCtrl == null) return null;
            var 参照面法线 = 参考平面处理(0, 0, MaximumSpeed);
            if (!参照面法线.HasValue) { return null; }
            return 飞船朝向处理(MainCtrl.RotateIndicator.X, MainCtrl.RotateIndicator.Y, _EnabledCuriser, 参照面法线.Value, ref 朝向);
        }

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
