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
        public IMyTerminalBlock Me { get; set; }
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

        protected virtual Vector3? 姿态处理(ref Vector3 朝向, bool _EnabledCuriser)
        {
            //if (!GyrosIsReady || MainCtrl.NullMainCtrl) return null;
            var 参照面法线 = 参考平面处理(0, 0, MaximumSpeed);
            if (!参照面法线.HasValue) { return null; }
            return 飞船朝向处理(MainCtrl.RotateIndicator.X, MainCtrl.RotateIndicator.Y, 参照面法线.Value, ref 朝向);
        }


        protected Vector3 飞船朝向处理(float 向前信号, float 向右信号, Vector3 法向量, ref Vector3 朝向)
        {
            var CtrlGyro = ProcessPlaneFunctions.ProcessPose_RPY(Me, (向右信号 != 0 || 向前信号 != 0) ? ScaleVectorTimes(Me.WorldMatrix.Forward + 向右信号 * Me.WorldMatrix.Right - 向前信号 * Me.WorldMatrix.Up) : 朝向, 法向量, PoseMode, AngularDampeners) * MaxReactions_AngleV;
            var direction = 朝向;
            if (向右信号 != 0 || 向前信号 != 0) direction = Me.WorldMatrix.Forward;
            if (ProjectGravity && (Gravity != null))
            {
                direction = ProjectOnPlane(Me.WorldMatrix.Forward, Me.CubeGrid.Physics.Gravity);
                if (direction == Vector3.Zero)
                    direction = ProjectOnPlane(Me.WorldMatrix.Down, Me.CubeGrid.Physics.Gravity);
            }
            if (direction != Vector3.Zero) 朝向 = ScaleVectorTimes(Vector3.Normalize(direction));
            return CtrlGyro;
        }




        private Vector3? 参考平面处理(float 向前信号, float 向右信号, float 最大限速)
        {
            Vector3? current_velocity_linear = null;
            Vector3? current_gravity = null;
            if (Refer2Velocity)
                current_velocity_linear = ((IngroForwardVelocity ? ProjectLinnerVelocity_CockpitForward : LinearVelocity) - (Refer2Ctrl ? Vector3.ClampToSphere((-Me.WorldMatrix.Forward * 向前信号 + Me.WorldMatrix.Right * 向右信号) * 最大限速, 最大限速) : Vector3.Zero));
            if (Refer2Gravity)
                current_gravity = Gravity;
            return 参考平面决策(current_velocity_linear, current_gravity);
        }
        private Vector3? 参考平面决策(Vector3? current_velocity_linear, Vector3? current_gravity)
        {
            if (!current_gravity.HasValue)
                return current_velocity_linear;
            else if (!current_velocity_linear.HasValue)
                return current_gravity;
            else
                return Vector3.ClampToSphere(current_velocity_linear.Value * LocationSensetive + Dampener(current_gravity.Value) * SafetyStage, 1f);
        }
        private Vector3 ProjectLinnerVelocity_CockpitForward { get { return ProjectOnPlane(LinearVelocity, Me.WorldMatrix.Forward); } }
        private float CalculateIndicate_Roll(Vector3 current_normal, float AngularDampener = 10f)
        {
            var Roll_current_angular_local = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Left);
            var Roll_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Backward);
            var current_angular_local_add = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Down);
            if (Math.Abs(Roll_current_angular_local) < 0.005f && current_angular_local_add < 0) { Roll_current_angular_local = current_angular_local_add; Roll_current_angular_velocity = 80f; }
            return Dampener(Roll_current_angular_local) + Dampener(Roll_current_angular_velocity) * AngularDampener;
        }
        private float CalculateIndicate_Yaw(Vector3 direction, float AngularDampener = 10f)
        {
            var Yaw_current_angular_add = Calc_Direction_Vector(direction, Me.WorldMatrix.Forward);
            var Yaw_current_angular_local = Calc_Direction_Vector(direction, Me.WorldMatrix.Right);
            var Yaw_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Up);
            if (Math.Abs(Yaw_current_angular_local) < 0.005f && Yaw_current_angular_add < 0) { Yaw_current_angular_local = Yaw_current_angular_add; }
            return Dampener(Yaw_current_angular_local) + Dampener(Yaw_current_angular_velocity) * AngularDampener;
        }
        private float CalculateIndicate_Pitch(Vector3 current_normal, Vector3 direction, bool HoverMode = true, float AngularDampener = 10f)
        {
            float Pitch_current_angular_local = HoverMode ? Calc_Direction_Vector(current_normal, Me.WorldMatrix.Backward) : Calc_Direction_Vector(direction, Me.WorldMatrix.Down);
            var Pitch_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Right);
            return Dampener(Pitch_current_angular_local) + Dampener(Pitch_current_angular_velocity) * AngularDampener;
        }
    }
}
