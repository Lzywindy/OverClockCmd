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
        protected virtual Vector3 推进器控制参数 { get; }
        protected virtual Vector3? 姿态调整参数 { get; }
        protected virtual void SensorReading() { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            ThrustControllerSystem?.SetAll(true);
            GyroControllerSystem?.GyrosOverride(null);
            InitDirection();
            初始化推进器和陀螺仪();
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
        protected Vector3? 姿态处理(bool _EnabledCuriser)
        {
            if (DisabledRotation) return null;
            var 参照面法线 = 参考平面处理(RotationCtrlLines.X, RotationCtrlLines.Y, MaximumSpeed);
            if (!参照面法线.HasValue) { return null; }
            return 飞船朝向处理(RotationCtrlLines.Z, RotationCtrlLines.W, _EnabledCuriser, 参照面法线.Value);
        }
        private Vector3? 参考平面处理(float 向前信号, float 向右信号, float 最大限速)
        {
            Vector3? current_velocity_linear = null;
            Vector3 current_gravity = Refer2Gravity ? Gravity : Vector3.Zero;
            if (Refer2Velocity)
                current_velocity_linear = (IngroForwardVelocity ? ProjectLinnerVelocity_CockpitForward : LinearVelocity) - ((Need2CtrlSignal ? (Vector3.ClampToSphere((-Me.WorldMatrix.Forward * 向前信号 + Me.WorldMatrix.Right * 向右信号), 1) * 最大限速) : Vector3.Zero));
            return 参考平面决策(current_velocity_linear, current_gravity);
            //return ((!current_gravity.HasValue) ? current_velocity_linear : (!current_velocity_linear.HasValue) ? current_gravity : Vector3.ClampToSphere((Vector3.ClampToSphere(current_velocity_linear.Value, 1f) * SafetyStage) + (1 - SafetyStage) * Vector3.ClampToSphere(current_gravity.Value, 1f), 1f));
        }
        private Vector3 飞船朝向处理(float 向前信号, float 向右信号, bool 是否巡航, Vector3 法向量)
        {
            Direction(向右信号 != 0 || 向前信号 != 0, 是否巡航);
            return ProcessDampeners() +
                ProcessPose_RPY(
                    (向右信号 != 0 || 向前信号 != 0) ? ScaleVectorTimes(Me.WorldMatrix.Forward + 向右信号 * Me.WorldMatrix.Right - 向前信号 * Me.WorldMatrix.Up) : ForwardDirection,
                    法向量,
                    PoseMode);
        }
        private static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        }
        protected bool 可以控制<T>(List<T> ElemList)
        {
            return !(ElemList == null || ElemList.Count < 1);
        }
        private void 初始化推进器和陀螺仪()
        {
            var gyros = GetTs(GridTerminalSystem, (IMyGyro gyro) => 排除的关键关键字(gyro) && gyro.CubeGrid == Me.CubeGrid);
            if (!可以控制(gyros)) { Information += "Warning:Missing Gyros!"; }
            else GyroControllerSystem = new 姿态控制器(gyros, Me);
            var thrusts = GetTs(GridTerminalSystem, (IMyThrust thrust) => 排除的关键关键字(thrust) && thrust.CubeGrid == Me.CubeGrid);
            if (!可以控制(thrusts)) { Information += "Warning:Missing Thrusts!"; }
            else ThrustControllerSystem = new 推进控制器(thrusts, Me);
        }
        public bool Dampener { get; set; } = true;
        public float LocationSensetive { get { return _LocationSensetive; } set { _LocationSensetive = MathHelper.Clamp(value, 0.5f, 4f); } }
        public float MaxReactions_AngleV { get { return _MaxReactions_AngleV; } set { _MaxReactions_AngleV = MathHelper.Clamp(value, 1f, 90f); } }
        public bool NoGravity { get { return Me.CubeGrid.Physics.Gravity == Vector3.Zero; } }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float SafetyStage { get { return SafetyStageCurrent; } set { SafetyStageCurrent = MathHelper.Clamp(value, SafetyStageMin, SafetyStageMax); } }
        protected uint CtrlMode { get; } = 0;
        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;
        protected virtual float SafetyStageCurrent { get; set; }
        protected virtual float _LocationSensetive { get; set; }
        protected virtual float _MaxReactions_AngleV { get; set; }
        #endregion       
        #region 角速度阻尼
        public float AngularDampeners_Roll { get { AngularDampeners.Z = SetInRange_AngularDampeners(AngularDampeners.Z); return AngularDampeners.Z; } set { AngularDampeners.Z = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Yaw { get { AngularDampeners.Y = SetInRange_AngularDampeners(AngularDampeners.Y); return AngularDampeners.Y; } set { AngularDampeners.Y = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Pitch { get { AngularDampeners.X = SetInRange_AngularDampeners(AngularDampeners.X); return AngularDampeners.X; } set { AngularDampeners.X = SetInRange_AngularDampeners(value); } }
        protected Vector3? 参考平面决策(Vector3? current_velocity_linear, Vector3 current_gravity)
        {
            if (current_gravity == Vector3.Zero)
                return current_velocity_linear;
            else if (!current_velocity_linear.HasValue)
                return current_gravity;
            else
                return Vector3.ClampToSphere(current_velocity_linear.Value * LocationSensetive + Utils.Dampener(current_gravity) * SafetyStage, 1f);
        }
        protected Vector3 ProcessPose_RPY(Vector3 朝向, Vector3 法向量, bool HoverMode = true)
        {
            return new Vector3(
                Dampener(HoverMode && (法向量 != Vector3.Zero) ? Calc_Direction_Vector(法向量, Me.WorldMatrix.Backward) : Calc_Direction_Vector(朝向, Me.WorldMatrix.Down)),
                Dampener(SetupAngle(Calc_Direction_Vector(朝向, Me.WorldMatrix.Right), Calc_Direction_Vector(朝向, Me.WorldMatrix.Forward))),
                (法向量 != Vector3.Zero) ? Dampener(SetupAngle(Calc_Direction_Vector(法向量, Me.WorldMatrix.Left), Calc_Direction_Vector(法向量, Me.WorldMatrix.Down))) : 0
                ) * MaxReactions_AngleV;
        }
        protected Vector3 ProcessDampeners()
        {
            var temp = Vector3.TransformNormal(AngularVelocity, Matrix.Transpose(Me.WorldMatrix));
            var a_temp = Vector3.Abs(temp);
            return Vector3.Clamp(a_temp * temp * InitAngularDampener / 4, -InitAngularDampener, InitAngularDampener) * AngularDampeners;
        }
        protected void Direction(bool EnabledUpdate, bool Project2Gravity)
        {
            Vector3 direction = EnabledUpdate ? (Vector3)Me.WorldMatrix.Forward : ForwardDirection;
            if (Project2Gravity && ForwardOrUp && (!NoGravity))
            {
                direction = ProjectOnPlane(Me.WorldMatrix.Forward, Gravity);
                if (direction == Vector3.Zero)
                    direction = ProjectOnPlane(Me.WorldMatrix.Down, Gravity);
            }
            if (direction != Vector3.Zero) ForwardDirection = ScaleVectorTimes(Vector3.Normalize(direction));
        }
        protected virtual Vector3 InitAngularDampener { get; } = new Vector3(70, 30, 10);
        private static float SetInRange_AngularDampeners(float data)
        {
            return MathHelper.Clamp(data, 0.1f, 10f);
        }
        private static float SetupAngle(float current_angular_local, float current_angular_add)
        {
            if (Math.Abs(current_angular_local) < 0.005f && current_angular_add < 0f)
                return current_angular_add;
            return current_angular_local;
        }
        private void InitDirection()
        {
            ForwardDirection = Me.WorldMatrix.Forward;
        }
        protected Vector3 AngularDampeners = Vector3.One;
        private Vector3 ForwardDirection;
        #endregion
        #region 基础设备
        protected virtual Vector4 RotationCtrlLines => Vector4.Zero;
        protected virtual bool DisabledRotation => true;
        protected 推进控制器 ThrustControllerSystem { get; private set; }
        protected 姿态控制器 GyroControllerSystem { get; private set; }
        protected bool GyrosIsReady { get { return GyroControllerSystem != null; } }
        protected bool ThrustsIsReady { get { return ThrustControllerSystem != null; } }
        protected bool HandBrake { get { if (MainCtrl.NullMainCtrl) return true; return MainCtrl.HandBrake; } }
        protected Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        #endregion

        protected bool 排除的关键关键字(IMyTerminalBlock block)
        {
            foreach (var item in BlackList)
            {
                if (block.BlockDefinition.SubtypeId.Contains(item))
                    return false;
            }
            return true;
        }
        private string[] BlackList { get; } = new string[] { "Hover", "Torpedo","Torp","Payload", "Missile",
            "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", };
    }
}
