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
        protected Vector3 飞船朝向处理(float 向前信号, float 向右信号, bool 是否巡航, Vector3 法向量)
        {
            Direction(向右信号 != 0 || 向前信号 != 0, 是否巡航);
            return (ProcessDampeners() +
                ProcessPose_RPY(
                    (向右信号 != 0 || 向前信号 != 0) ? ScaleVectorTimes(Me.WorldMatrix.Forward + 向右信号 * Me.WorldMatrix.Right - 向前信号 * Me.WorldMatrix.Up) : ForwardDirection,
                    法向量,
                    PoseMode) * MaxReactions_AngleV);
        }

        protected static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        }
        protected bool 可以控制<T>(List<T> ElemList)
        {
            return !(ElemList == null || ElemList.Count < 1);
        }
        private void 初始化推进器和陀螺仪()
        {
            var gyros = GetTs(GridTerminalSystem, (IMyGyro gyro) => gyro.CubeGrid == Me.CubeGrid);
            if (!可以控制(gyros)) { Information += ("Warning:Missing Gyros!"); }
            else GyroControllerSystem = new 姿态控制器(gyros, Me);
            var thrusts = GetTs(GridTerminalSystem, (IMyThrust thrust) => (!thrust.BlockDefinition.SubtypeId.Contains("Hover") && thrust.CubeGrid == Me.CubeGrid));
            if (!可以控制(thrusts)) { Information += ("Warning:Missing Thrusts!"); }
            else ThrustControllerSystem = new 推进控制器(thrusts, Me);
        }
        public bool Dampener { get; set; } = true;
        public float LocationSensetive { get { return _LocationSensetive; } set { _LocationSensetive = MathHelper.Clamp(value, 0.5f, 4f); } }
        public float MaxReactions_AngleV { get { return _MaxReactions_AngleV; } set { _MaxReactions_AngleV = MathHelper.Clamp(value, 1f, 90f); } }
        public bool NoGravity { get { return Me.CubeGrid.Physics.Gravity == Vector3.Zero; } }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float SafetyStage { get { return SafetyStageCurrent; } set { SafetyStageCurrent = MathHelper.Clamp(value, SafetyStageMin, SafetyStageMax); } }
       
        protected Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        protected uint CtrlMode { get; } = 0;

        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;

        private float SafetyStageCurrent;
        private float _LocationSensetive;
        private float _MaxReactions_AngleV;


        #endregion
        #region 信号变换
        protected void SignalTransform_Move(Vector3 O_Move, out Vector3 T_Move)
        {
            switch (CtrlMode)
            {
                //大气中的直升机模式/大气中的垂降战机垂降模式
                case 1U:
                case 3U:
                    T_Move = O_Move * Vector3.Up;
                    break;
                //大气中的飞机模式/太空飞船高速飞行模式
                case 2U:
                case 4U:
                    T_Move = O_Move * Vector3.Backward;
                    break;
                default:
                    T_Move = O_Move;
                    break;
            }
        }
        protected void SignalTransform_Rotate(Vector3 O_Move, Vector3 O_Rotate, out float signal_0, out float signal_1, out float signal_2, out float signal_3)
        {
            var HasGravity_HoverMode = (!ForwardOrUp) && (!NoGravity);
            switch (CtrlMode)
            {
                //大气中的直升机模式/大气中的垂降战机垂降模式
                case 1U:
                case 3U:
                    signal_0 = O_Move.Z;
                    signal_1 = O_Move.X;
                    signal_2 = 0;
                    signal_3 = O_Rotate.Z;
                    break;
                //大气中的飞机模式/太空飞船高速飞行模式/太空飞船/空天战机悬浮模式
                case 2U:
                case 4U:
                    signal_0 = 0;
                    signal_1 = 0;
                    signal_2 = O_Rotate.X;
                    signal_3 = O_Rotate.Y;
                    break;               
                default:
                    signal_0 = 0;
                    signal_1 = 0;
                    signal_2 = 0;
                    signal_3 = 0;
                    break;
            }
        }
        #endregion
        #region 角速度阻尼
        public float AngularDampeners_Roll { get { AngularDampeners.Z = SetInRange_AngularDampeners(AngularDampeners.Z); return AngularDampeners.Z; } set { AngularDampeners.Z = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Yaw { get { AngularDampeners.Y = SetInRange_AngularDampeners(AngularDampeners.Y); return AngularDampeners.Y; } set { AngularDampeners.Y = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Pitch { get { AngularDampeners.X = SetInRange_AngularDampeners(AngularDampeners.X); return AngularDampeners.X; } set { AngularDampeners.X = SetInRange_AngularDampeners(value); } }
        protected Vector3? 参考平面决策(Vector3? current_velocity_linear, Vector3? current_gravity)
        {
            if (!current_gravity.HasValue)
                return current_velocity_linear;
            else if (!current_velocity_linear.HasValue)
                return current_gravity;
            else
                return Vector3.ClampToSphere(current_velocity_linear.Value + Utils.Dampener(current_gravity.Value) * SafetyStage, 1f);
        }
        protected Vector3 ProcessPose_RPY(Vector3 朝向, Vector3 法向量, bool HoverMode = true)
        {
            return new Vector3(
                Dampener(HoverMode && (法向量 != Vector3.Zero) ? Calc_Direction_Vector(法向量, Me.WorldMatrix.Backward) : Calc_Direction_Vector(朝向, Me.WorldMatrix.Down)),
                Dampener(SetupAngle(Calc_Direction_Vector(朝向, Me.WorldMatrix.Right), Calc_Direction_Vector(朝向, Me.WorldMatrix.Forward))),
                (法向量 != Vector3.Zero) ? Dampener(SetupAngle(Calc_Direction_Vector(法向量, Me.WorldMatrix.Left), Calc_Direction_Vector(法向量, Me.WorldMatrix.Down))) : 0
                );
        }
        protected Vector3 ProcessDampeners()
        {
            return Vector3.TransformNormal(AngularVelocity, Matrix.Transpose(Me.WorldMatrix)) * AngularDampeners * InitAngularDampener;
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
        protected virtual Vector3 InitAngularDampener { get; } = new Vector3(50, 50, 10);
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
        protected 推进控制器 ThrustControllerSystem { get; private set; }
        protected 姿态控制器 GyroControllerSystem { get; private set; }
        protected bool GyrosIsReady { get { return GyroControllerSystem != null; } }
        protected bool ThrustsIsReady { get { return ThrustControllerSystem != null; } }
        protected bool HandBrake { get { if (MainCtrl == null) return true; return MainCtrl.HandBrake; } }
        #endregion
    }
}
