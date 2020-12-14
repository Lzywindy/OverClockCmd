using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using VRage.Game.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    using static Utils;
    public partial class VehicleControllerBase : ControllerManageBase, ICtrlDevCtrl, IPoseParamAdjust
    {
        public VehicleControllerBase(IMyTerminalBlock Me) : base(Me) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            ThrustControllerSystem?.SetAll(true);
            GyroControllerSystem?.GyrosOverride(null);
            InitDirection();
            初始化推进器和陀螺仪();
            SensorReading();
            MainCtrl.MainCtrl = GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit);
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
        protected Vector3? 姿态处理(bool _EnabledCuriser)
        {
            return ProcessRotation(_EnabledCuriser, Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners,
                ForwardOrUp, PoseMode, MaximumSpeed, MaxReactions_AngleV, Need2CtrlSignal, LocationSensetive
                , SafetyStage, IgnoreForwardVelocity, Refer2Gravity, Refer2Gravity, DisabledRotation, 附加朝向, null);
            //if (DisabledRotation) return null;
            ////参考平面法线
            ////飞船以该方块的down方向与实际的down方向对齐
            //Vector3? current_velocity_linear = Refer2Velocity ? ((Vector3?)((IgnoreForwardVelocity ? ProjectLinnerVelocity_CockpitForward : LinearVelocity)
            //        - ((Need2CtrlSignal ? (Vector3.ClampToSphere((-Me.WorldMatrix.Forward * RotationCtrlLines.X + Me.WorldMatrix.Right * RotationCtrlLines.Y), 1) * MaximumSpeed) : Vector3.Zero)))) : null;
            //Vector3 current_gravity = Refer2Gravity ? Gravity : Vector3.Zero;
            //Vector3? 参照面法线;
            //if (current_gravity == Vector3.Zero)
            //    参照面法线 = current_velocity_linear;
            //else if (!current_velocity_linear.HasValue)
            //    参照面法线 = current_gravity;
            //else
            //    参照面法线 = Vector3.ClampToSphere(current_velocity_linear.Value * LocationSensetive + Utils.Dampener(current_gravity) * SafetyStage, 1f);
            ////如果参考面法线为空，则让飞船恢复飞控控制之前的控制方式
            ////ToDo:这个地方实际可以加入一个变量，使得该飞行器可以直接对齐所需的参考平面
            ////比如对齐航母的甲板法线或者是自动停泊的连接器
            //if (!参照面法线.HasValue) { return null; }
            ////朝向控制
            ////用来纠正偏航
            //if (RotationCtrlLines.W != 0 || RotationCtrlLines.Z != 0)
            //    ForwardDirection = Me.WorldMatrix.Forward;
            //if (_EnabledCuriser && ForwardOrUp && (!NoGravity))
            //{
            //    ForwardDirection = ProjectOnPlane(ForwardDirection, Gravity);
            //    if (ForwardDirection == Vector3.Zero)
            //        ForwardDirection = ProjectOnPlane(Me.WorldMatrix.Down, Gravity);
            //}
            //if (ForwardDirection != Vector3.Zero)
            //    ForwardDirection = ScaleVectorTimes(Vector3.Normalize(ForwardDirection));
            //Vector3 朝向;
            //if (附加朝向.HasValue && 附加朝向.Value != Vector3.Zero)
            //    朝向 = 附加朝向.Value + RotationCtrlLines.W * Me.WorldMatrix.Right - RotationCtrlLines.Z * Me.WorldMatrix.Up;
            //else
            //    朝向 = ForwardDirection + RotationCtrlLines.W * Me.WorldMatrix.Right - RotationCtrlLines.Z * Me.WorldMatrix.Up;
            ////完成法线和朝向的对齐之后，就可以开始控制陀螺仪工作了
            ////加入速度阻尼以免转向过快导致无法控制
            //return (ProcessDampeners() + (new Vector3(
            //    Dampener(PoseMode && (参照面法线.Value != Vector3.Zero) ? Calc_Direction_Vector(参照面法线.Value, Me.WorldMatrix.Backward) : Calc_Direction_Vector(朝向, Me.WorldMatrix.Down)),
            //    Dampener(SetupAngle(Calc_Direction_Vector(朝向, Me.WorldMatrix.Right), Calc_Direction_Vector(朝向, Me.WorldMatrix.Forward))),
            //    (参照面法线.Value != Vector3.Zero) ? Dampener(SetupAngle(Calc_Direction_Vector(参照面法线.Value, Me.WorldMatrix.Left), Calc_Direction_Vector(参照面法线.Value, Me.WorldMatrix.Down))) : 0
            //    ) * MaxReactions_AngleV));
        }
        private static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        }
        protected static bool 可以控制<T>(List<T> ElemList)
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
    }
    public partial class VehicleControllerBase
    {
        public bool Dampener { get; set; } = true;
        public float LocationSensetive { get { return _LocationSensetive; } set { _LocationSensetive = MathHelper.Clamp(value, 0.5f, 4f); } }
        public float MaxReactions_AngleV { get { return _MaxReactions_AngleV; } set { _MaxReactions_AngleV = MathHelper.Clamp(value, 1f, 90f); } }
        public float SafetyStage { get { return SafetyStageCurrent; } set { SafetyStageCurrent = MathHelper.Clamp(value, SafetyStageMin, SafetyStageMax); } }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float AngularDampeners_Roll { get { AngularDampeners.Z = SetInRange_AngularDampeners(AngularDampeners.Z); return AngularDampeners.Z; } set { AngularDampeners.Z = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Yaw { get { AngularDampeners.Y = SetInRange_AngularDampeners(AngularDampeners.Y); return AngularDampeners.Y; } set { AngularDampeners.Y = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Pitch { get { AngularDampeners.X = SetInRange_AngularDampeners(AngularDampeners.X); return AngularDampeners.X; } set { AngularDampeners.X = SetInRange_AngularDampeners(value); } }
    }
    public partial class VehicleControllerBase
    {
        protected Vector3 ProcessDampeners()
        {
            var temp = Vector3.TransformNormal(AngularVelocity, Matrix.Transpose(Me.WorldMatrix));
            var a_temp = Vector3.Abs(temp);
            return Vector3.Clamp(a_temp * temp * InitAngularDampener / 4, -InitAngularDampener, InitAngularDampener) * AngularDampeners;
        }
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
    }
    public partial class VehicleControllerBase
    {
        protected virtual void PoseCtrl() { }
        protected virtual void ThrustControl() { }
        protected virtual void SensorReading() { }
        protected bool 排除的关键关键字(IMyTerminalBlock block)
        {
            foreach (var item in BlackList)
            {
                if (block.BlockDefinition.SubtypeId.Contains(item))
                    return false;
            }
            return true;
        }
        private string[] BlackList { get; } = new string[] { "Hover", "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", };
        protected 推进控制器 ThrustControllerSystem { get; private set; }
        protected 姿态控制器 GyroControllerSystem { get; private set; }
        protected bool GyrosIsReady { get { return GyroControllerSystem != null; } }
        protected bool ThrustsIsReady { get { return ThrustControllerSystem != null; } }
        protected bool HandBrake { get { if (MainCtrl.NullMainCtrl) return true; return MainCtrl.HandBrake; } }
        protected Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        public bool NoGravity { get { return Me.CubeGrid.Physics.Gravity == Vector3.Zero; } }
        private void InitDirection() { ForwardDirection = Me.WorldMatrix.Forward; }
        protected Vector3 AngularDampeners = Vector3.One;
        private Vector3 ForwardDirection;
        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;
        protected SignalController MainCtrl { get; } = new SignalController();
    }
    public partial class VehicleControllerBase
    {
        protected virtual Vector3 推进器控制参数 { get; } = Vector3.Zero;
        protected virtual Vector3? 姿态调整参数 { get; } = null;
        protected virtual bool ForwardOrUp { get; set; } = false;
        protected virtual bool Refer2Gravity { get; } = true;
        protected virtual bool Refer2Velocity { get; } = false;
        protected virtual bool Need2CtrlSignal { get; } = false;
        protected virtual bool IgnoreForwardVelocity { get; } = true;
        protected virtual bool EnabledAllDirection { get; } = true;
        protected virtual float MaximumSpeed { get; } = 100f;
        protected virtual bool PoseMode { get; } = false;
        protected virtual float SafetyStageCurrent { get; set; } = 1f;
        protected virtual float _LocationSensetive { get; set; } = 1f;
        protected virtual float _MaxReactions_AngleV { get; set; } = 1f;
        protected virtual Vector4 RotationCtrlLines => Vector4.Zero;
        protected virtual bool DisabledRotation => true;
        protected virtual Vector3 InitAngularDampener { get; } = new Vector3(70, 30, 10);
    }
}
