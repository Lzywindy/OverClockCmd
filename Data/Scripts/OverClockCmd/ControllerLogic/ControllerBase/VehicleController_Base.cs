using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VRage.Game.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    public class VehicleControllerBase : ControllerManageBase, ICtrlDevCtrl, IPoseParamAdjust
    {
        public VehicleControllerBase(IMyTerminalBlock Me) : base(Me) { }
        protected virtual void SensorReading() { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            ForwardDirection = ScaleVectorTimes(Me.WorldMatrix.Forward);
            InitBasicDevice();
            SensorReading();
            AppRunning1 += SensorReading;
            AppRunning1 += PoseCtrl;
            AppRunning1 += ThrustControl;
            AppRunning1 += SetDefault;
        }
        protected override void SetDefault()
        {
            if (MainCtrl != null) return;
            ThrustControllerSystem?.SetAll(true);
            GyroControllerSystem?.SetOverride(false);
            GyroControllerSystem?.GyrosOverride(null);
        }
        protected virtual void PoseCtrl() { }
        protected virtual void ThrustControl() { }       
        #region 基础参数设置
        #endregion
        #region 可定制电路
        protected virtual bool ForwardOrUp { get; }
        protected virtual bool Refer2Gravity { get; }
        protected virtual bool Refer2Velocity { get; }
        protected virtual bool Need2CtrlSignal { get; }
        protected virtual bool IngroForwardVelocity { get; }
        protected virtual bool EnabledAllDirection { get; }
        protected virtual float MaximumSpeed { get; }
        protected virtual bool PoseMode { get; }
        #endregion
        #region 一些私有函数
        protected Vector3? 参考平面处理(float 向前信号, float 向右信号, float 最大限速)
        {
            Vector3? current_velocity_linear = null;
            Vector3? current_gravity = null;
            if (Refer2Velocity)
                current_velocity_linear = ((IngroForwardVelocity ? ProjectLinnerVelocity_CockpitForward : LinearVelocity) - (Need2CtrlSignal ? Vector3.ClampToSphere((-Me.WorldMatrix.Forward * 向前信号 + Me.WorldMatrix.Right * 向右信号) * 最大限速, 最大限速) : Vector3.Zero) * LocationSensetive);
            if (Refer2Gravity)
                current_gravity = Gravity;
            return 参考平面决策(current_velocity_linear, current_gravity);
            //return ((!current_gravity.HasValue) ? current_velocity_linear : (!current_velocity_linear.HasValue) ? current_gravity : Vector3.ClampToSphere((Vector3.ClampToSphere(current_velocity_linear.Value, 1f) * SafetyStage) + (1 - SafetyStage) * Vector3.ClampToSphere(current_gravity.Value, 1f), 1f));
        }
        protected Vector3 飞船朝向处理(float 向前信号, float 向右信号, bool 是否巡航, Vector3 法向量, ref Vector3 朝向)
        {
            var CtrlGyro = ProcessPlaneFunctions.ProcessPose_RPY(Me, (向右信号 != 0 || 向前信号 != 0) ? ScaleVectorTimes(Me.WorldMatrix.Forward + 向右信号 * Me.WorldMatrix.Right - 向前信号 * Me.WorldMatrix.Up) : 朝向, 法向量, PoseMode, MaxReactions_AngleV) * MaxReactions_AngleV;
            var direction = 朝向;
            if (向右信号 != 0 || 向前信号 != 0) direction = Me.WorldMatrix.Forward;
            if (是否巡航 && ForwardOrUp && (!NoGravity))
            {
                direction = ProjectOnPlane(Me.WorldMatrix.Forward, Me.CubeGrid.Physics.Gravity);
                if (direction == Vector3.Zero)
                    direction = ProjectOnPlane(Me.WorldMatrix.Down, Me.CubeGrid.Physics.Gravity);
            }
            if (direction != Vector3.Zero) 朝向 = ScaleVectorTimes(Vector3.Normalize(direction));
            return CtrlGyro;
        }
        protected Vector3? 参考平面决策(Vector3? current_velocity_linear, Vector3? current_gravity)
        {
            if (!current_gravity.HasValue)
                return current_velocity_linear;
            else if (!current_velocity_linear.HasValue)
                return current_gravity;
            else
            {
                var current_velocity_linear_p_1 = Vector3.ClampToSphere(ProjectOnPlane(current_velocity_linear.Value, current_gravity.Value), 1f);
                var current_gravity_1 = Vector3.ClampToSphere(current_gravity.Value, 1f);
                var ls_cv = current_velocity_linear_p_1.Normalize();
                Vector2 vector = Vector2.Normalize(new Vector2(ls_cv * SafetyStage, current_gravity_1.Normalize()));
                if (ls_cv == 0)
                    return current_gravity;
                else
                    return vector.X * current_velocity_linear_p_1 + vector.Y * current_gravity_1;
            }
        }
        protected static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        }
        protected bool 可以控制<T>(List<T> ElemList)
        {
            return !(ElemList == null || ElemList.Count < 1);
        }
        private void InitBasicDevice()
        {
            var gyros = GetTs(GridTerminalSystem, (IMyGyro gyro) => gyro.CubeGrid == Me.CubeGrid);
            if (!可以控制(gyros)) { Information += ("Warning:Missing Gyros!"); }
            else GyroControllerSystem = new 姿态控制器(gyros, Me);
            var thrusts = GetTs(GridTerminalSystem, (IMyThrust thrust) => (!thrust.BlockDefinition.SubtypeId.Contains("Hover") && thrust.CubeGrid == Me.CubeGrid));
            if (!可以控制(thrusts)) { Information += ("Warning:Missing Thrusts!"); }
            else ThrustControllerSystem = new 推进控制器(thrusts, Me);
        }
        public bool Dampener { get; set; } = true;
        public float LocationSensetive { get; set; } = 1.5f;
        public float MaxReactions_AngleV { get; set; } = 45f;
        public bool NoGravity { get { return Me.CubeGrid.Physics.Gravity == Vector3.Zero; } }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float SafetyStage { get { return SafetyStageCurrent; } set { SafetyStageCurrent = MathHelper.Clamp(value, SafetyStageMin, SafetyStageMax); } }
        protected 推进控制器 ThrustControllerSystem { get; private set; }
        protected 姿态控制器 GyroControllerSystem { get; private set; }
        protected bool GyrosIsReady { get { return GyroControllerSystem != null; } }
        protected bool ThrustsIsReady { get { return ThrustControllerSystem != null; } }
        protected bool HandBrake { get { if (MainCtrl == null) return true; return MainCtrl.HandBrake; } }
        protected Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;
        protected Vector3 ForwardDirection;
        private float SafetyStageCurrent;
        #endregion
    }
}
