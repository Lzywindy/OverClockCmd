using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    using static Utils;
    public class StablizeProcessor
    {
        public SignalController MainCtrl { get; set; }
        public IMyTerminalBlock Me { get { return _Me; } set { _Me = value; if (_Me != null) ForwardDirection = _Me.WorldMatrix.Forward; else ForwardDirection = Vector3.Zero; } }
        public bool Refer2Velocity { get; set; }
        public bool Refer2Gravity { get; set; }
        public bool Refer2Ctrl { get; set; }
        public bool ProjectGravity { get; set; }
        public bool IngroForwardVelocity { get; set; }
        public bool PoseMode { get; set; }
        public float MaximumSpeed { get; set; }
        public float SafetyStage { get; set; }
        public float LocationSensetive { get; set; }
        public Vector3 AngularDampeners { get; set; }
        public Vector3 MaxReactions_AngleV { get; set; }
        public Vector3 LinearVelocity { get { return Me.CubeGrid.Physics.LinearVelocity; } }
        public Vector3 AngularVelocity { get { return Me.CubeGrid.Physics.AngularVelocity; } }
        public Vector3 Gravity { get { return Me.CubeGrid.Physics.Gravity; } }
        protected Vector3? 参考平面处理(float 向前信号, float 向右信号, float 最大限速)
        {
            Vector3? current_velocity_linear = null;
            Vector3? current_gravity = null;
            if (Refer2Velocity)
                current_velocity_linear = ((IngroForwardVelocity ? ProjectLinnerVelocity_CockpitForward : LinearVelocity) - (Refer2Ctrl ? Vector3.ClampToSphere((-Me.WorldMatrix.Forward * 向前信号 + Me.WorldMatrix.Right * 向右信号) * 最大限速, 最大限速) : Vector3.Zero) * LocationSensetive);
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
        protected Vector3? 参考平面决策(Vector3? current_velocity_linear, Vector3? current_gravity)
        {
            if (!current_gravity.HasValue)
                return current_velocity_linear;
            else if (!current_velocity_linear.HasValue)
                return current_gravity;
            else
                return Vector3.ClampToSphere(current_velocity_linear.Value + Dampener(current_gravity.Value) * SafetyStage, 1f);
        }
        private Vector3 ProcessPose_RPY(Vector3 朝向, Vector3 法向量, bool HoverMode = true)
        {
            return new Vector3(
                Dampener(HoverMode && (法向量 != Vector3.Zero) ? Calc_Direction_Vector(法向量, Me.WorldMatrix.Backward) : Calc_Direction_Vector(朝向, Me.WorldMatrix.Down)),
                Dampener(SetupAngle(Calc_Direction_Vector(朝向, Me.WorldMatrix.Right), Calc_Direction_Vector(朝向, Me.WorldMatrix.Forward))),
                (法向量 != Vector3.Zero) ? Dampener(SetupAngle(Calc_Direction_Vector(法向量, Me.WorldMatrix.Left), Calc_Direction_Vector(法向量, Me.WorldMatrix.Down))) : 0
                );
        }
        private Vector3 ProcessDampeners()
        {
            return Vector3.TransformNormal(AngularVelocity, Matrix.Transpose(Me.WorldMatrix)) * AngularDampeners * InitAngularDampener;
        }
        private void Direction(bool EnabledUpdate, bool Project2Gravity)
        {
            Vector3 direction = EnabledUpdate ? (Vector3)Me.WorldMatrix.Forward : ForwardDirection;
            if (Project2Gravity)
            {
                direction = ProjectOnPlane(Me.WorldMatrix.Forward, Gravity);
                if (direction == Vector3.Zero)
                    direction = ProjectOnPlane(Me.WorldMatrix.Down, Gravity);
            }
            if (direction != Vector3.Zero) ForwardDirection = ScaleVectorTimes(Vector3.Normalize(direction));
        }
        private Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        public Vector3 InitAngularDampener { get; set; }
        private static float SetupAngle(float current_angular_local, float current_angular_add)
        {
            if (Math.Abs(current_angular_local) < 0.005f && current_angular_add < 0f)
                return current_angular_add;
            return current_angular_local;
        }
        private Vector3 ForwardDirection;
        private IMyTerminalBlock _Me;
    }
}
