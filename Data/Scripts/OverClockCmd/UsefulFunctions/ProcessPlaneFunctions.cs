using System;
using Sandbox.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    /// <summary>
    /// 载具参照平面生成器
    /// 用于更好地操控载具
    /// </summary>
    public static partial class ProcessPlaneFunctions
    {
        
        /// <summary>
        /// 计算参照平面
        /// 操控为：垂直起降的飞机的垂降模式、及直升机
        /// 实例：VTOL（F35）、Helicopter（黑鹰直升机）
        /// 重力圈之外失效
        /// </summary>
        /// <param name="Me">飞控方块</param>
        /// <param name="MaxiumHoverSpeed">最大盘旋速度</param>
        /// <param name="LocationSensetive">姿态位置</param>
        /// <param name="MaxReactions_Angle">最大反应角度</param>
        /// <param name="Ctrl_Forward">前倾控制</param>
        /// <param name="Ctrl_Right">右倾控制</param>
        public static Vector3? ProcessPlane_Hover(IMyTerminalBlock Me, float Ctrl_Forward, float Ctrl_Right, float MaxiumHoverSpeed, float LocationSensetive, float MaxReactions_Angle)
        {
            var current_gravity = Me.CubeGrid.Physics.Gravity;
            if (current_gravity == Vector3.Zero) return null;
            Vector3D Control_Signal = Vector3D.ClampToSphere((-Me.WorldMatrix.Forward * Ctrl_Forward + Me.WorldMatrix.Right * Ctrl_Right) * MaxiumHoverSpeed, MaxiumHoverSpeed);
            var current_velocity_linear = (Me.CubeGrid.Physics.LinearVelocity - Control_Signal) * LocationSensetive;
            var gravity_sl = current_gravity.Normalize();
            var velocity_sl = current_velocity_linear.Normalize();
            float percentage = (float)MathHelper.Clamp(velocity_sl / gravity_sl, 0, Math.Tan(MathHelper.ToRadians(MaxReactions_Angle)));
            return (percentage * current_velocity_linear + (1 - percentage) * current_gravity);
        }
        /// <summary>
        /// 计算参照平面（忽略前进方向速度，前进方向会在飞行器无控制时，向水平方向靠近）
        /// 操控为：水平方向飞行的飞机
        /// 实例：C130运输机
        /// 重力圈之外失效
        /// </summary>
        /// <param name="Me">飞控方块</param>
        /// <param name="MaxiumHoverSpeed">最大盘旋速度</param>
        /// <param name="LocationSensetive">姿态位置</param>
        /// <param name="MaxReactions_Angle">最大反应角度</param>
        /// <param name="Ctrl_Forward">前倾控制</param>
        /// <param name="Ctrl_Right">右倾控制</param>
        /// <param name="IngorePitchCtrl">是否忽略前倾控制（飞船不能够通过摇杆前倾）</param>
        public static Vector3? ProcessPlane_Hover_NoPitch(IMyTerminalBlock Me, float Ctrl_Forward, float Ctrl_Right, float MaxiumHoverSpeed, float LocationSensetive, float MaxReactions_Angle, bool IngorePitchCtrl = false)
        {
            var current_gravity = Me.CubeGrid.Physics.Gravity;
            if (current_gravity == Vector3.Zero) return null;
            Vector3D Control_Signal = (IngorePitchCtrl ? Vector3D.Zero : -Me.WorldMatrix.Forward * Ctrl_Forward) + Me.WorldMatrix.Right * Ctrl_Right;
            Control_Signal = Vector3D.ClampToSphere(Control_Signal * MaxiumHoverSpeed, MaxiumHoverSpeed);
            var current_velocity_linear = (ProjectLinnerVelocity_Gravity_CockpitForward(Me) - Control_Signal) * LocationSensetive;
            var gravity_sl = current_gravity.Normalize();
            var velocity_sl = current_velocity_linear.Normalize();
            float percentage = (float)MathHelper.Clamp(velocity_sl / gravity_sl, 0, Math.Cos(MathHelper.ToRadians(MaxReactions_Angle)));
            return (percentage * current_velocity_linear + (1 - percentage) * current_gravity);
        }
        /// <summary>
        /// 计算参照平面(只依赖重力)
        /// 操控为：重力圈飞行的货船居多（也可以是海上单位）
        /// 重力圈之外失效
        /// </summary>
        /// <param name="Me">飞控方块</param>
        public static Vector3? ProcessPlane_GravityRelative(IMyTerminalBlock Me)
        {
            var current_gravity = Me.CubeGrid.Physics.Gravity;
            if (current_gravity == Vector3.Zero) return null;
            return current_gravity;
        }
        /// <summary>
        /// 计算参照平面(只依赖速度)
        /// 操控为：太空中的战机居多
        /// 没有速度就失效
        /// </summary>
        /// <param name="Me">飞控方块</param>
        public static Vector3? ProcessPlane_VelocityRelative(IMyTerminalBlock Me)
        {
            if (Me.CubeGrid.Physics.LinearVelocity == Vector3.Zero) return null;
            var current_velocity_linear = ProjectLinnerVelocity_Gravity_CockpitForward(Me);
            if (current_velocity_linear.LengthSquared() < 900f) return null;
            return Vector3.Normalize(current_velocity_linear);
        }
        /// <summary>
        /// 剔除前进方向的速度（只留下上、下、左、右）
        /// </summary>
        /// <param name="Me">飞控方块</param>
        private static Vector3 ProjectLinnerVelocity_Gravity_CockpitForward(IMyTerminalBlock Me)
        {
            Vector3 velocity_l = Me.CubeGrid.Physics.LinearVelocity;
            Vector3 direction_forward = Me.WorldMatrix.Forward;
            return Vector3.ProjectOnPlane(ref velocity_l, ref direction_forward);
            //return velocity_l - Vector3.ProjectOnVector(ref velocity_l, ref direction_forward);
        }
        /// <summary>
        /// 计算姿态的俯仰和滚转角，用以跟踪参考平面的法线
        /// </summary>
        /// <param name="Me">当前飞控</param>
        /// <param name="current_normal">当前的参考平面的法向量</param>
        /// <param name="LocationSensetive">对角度差的敏感程度</param>
        /// <param name="Angular_Damper_Rate">角速度减速阻抗因素（适量大有助于减少震荡）</param>
        public static Vector3 ProcessPose_Roll_Pitch(IMyTerminalBlock Me, Vector3 current_normal)
        {
            var current_velocity_angular = Me.CubeGrid.Physics.AngularVelocity;
            var up_project = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Up);
            var current_angular_local_add = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Down);
            var Roll_current_angular_local = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Left);
            var Roll_current_angular_velocity = Calc_Direction_Vector(current_velocity_angular, Me.WorldMatrix.Backward);
            if (Math.Abs(Roll_current_angular_local) < 0.005f && current_angular_local_add < 0) { Roll_current_angular_local = current_angular_local_add; Roll_current_angular_velocity = 80f; }
            var Pitch_current_angular_local = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Backward);
            var Pitch_current_angular_velocity = Calc_Direction_Vector(current_velocity_angular, Me.WorldMatrix.Right);
            var roll_indicate = Dampener(Roll_current_angular_local) + Dampener(Roll_current_angular_velocity);
            var pitch_indicate = Dampener(Pitch_current_angular_local) + Dampener(Pitch_current_angular_velocity);
            return new Vector3(pitch_indicate, 0, roll_indicate);
        }
        /// <summary>
        /// 计算姿态的摇摆角，用以稳定飞船朝向
        /// </summary>
        /// <param name="Me">当前飞控</param>
        /// <param name="direction">当前的参考朝向</param>
        /// <param name="LocationSensetive">对角度差的敏感程度</param>
        /// <param name="Angular_Damper_Rate">角速度减速阻抗因素（适量大有助于减少震荡）</param>
        public static Vector3 ProcessPose_Yaw(IMyTerminalBlock Me, Vector3 direction)
        {
            var current_velocity_angular = Me.CubeGrid.Physics.AngularVelocity;
            var forward_project = Calc_Direction_Vector(direction, Me.WorldMatrix.Forward);
            var Yaw_current_angular_local = Calc_Direction_Vector(direction, Me.WorldMatrix.Right) + (forward_project > -0.75f ? 0 : 1);
            var Yaw_current_angular_velocity = Calc_Direction_Vector(current_velocity_angular, Me.WorldMatrix.Up, 1f);
            var yaw_indicate = Dampener(Yaw_current_angular_local) + Dampener(Yaw_current_angular_velocity);
            return new Vector3(0, yaw_indicate, 0);
        }
    }
    public static partial class ProcessPlaneFunctions
    {
        public static Vector3 ProcessPose_RPY(IMyTerminalBlock Me, Vector3 direction, Vector3 current_normal, bool HoverMode = true, float AngularDampener = 10f)
        {
            var roll_indicate = CalculateIndicate_Roll(Me, current_normal, AngularDampener);
            var pitch_indicate = CalculateIndicate_Pitch(Me, current_normal, direction, HoverMode, AngularDampener);
            var yaw_indicate = CalculateIndicate_Yaw(Me, direction, AngularDampener);
            return new Vector3(pitch_indicate, yaw_indicate, roll_indicate);
        }
        public static Vector2 TurretDirection_YP(IMyTerminalBlock Block, Vector3 direction, float AngularDampener = 10f)
        {
            var pitch_indicate = CalculateIndicate_Pitch(Block, Vector3.Zero, direction, false, AngularDampener);
            var yaw_indicate = CalculateIndicate_Yaw(Block, direction, AngularDampener);
            return new Vector2(pitch_indicate, yaw_indicate);
        }
    }
    public static partial class ProcessPlaneFunctions
    {
        static float Dampener(float value) { return value * Math.Abs(value); }
        static float CalculateIndicate_Roll(IMyTerminalBlock Me, Vector3 current_normal, float AngularDampener = 10f)
        {
            var Roll_current_angular_local = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Left);
            var Roll_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Backward);
            var current_angular_local_add = Calc_Direction_Vector(current_normal, Me.WorldMatrix.Down);
            if (Math.Abs(Roll_current_angular_local) < 0.005f && current_angular_local_add < 0) { Roll_current_angular_local = current_angular_local_add; Roll_current_angular_velocity = 80f; }
            return Dampener(Roll_current_angular_local) + Dampener(Roll_current_angular_velocity) * AngularDampener;
        }
        static float CalculateIndicate_Yaw(IMyTerminalBlock Me, Vector3 direction, float AngularDampener = 10f)
        {
            var Yaw_current_angular_add = Calc_Direction_Vector(direction, Me.WorldMatrix.Forward);
            var Yaw_current_angular_local = Calc_Direction_Vector(direction, Me.WorldMatrix.Right);
            var Yaw_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Up);
            if (Math.Abs(Yaw_current_angular_local) < 0.005f && Yaw_current_angular_add < 0) { Yaw_current_angular_local = Yaw_current_angular_add; }
            return Dampener(Yaw_current_angular_local) + Dampener(Yaw_current_angular_velocity) * AngularDampener;
        }
        static float CalculateIndicate_Pitch(IMyTerminalBlock Me, Vector3 current_normal, Vector3 direction, bool HoverMode = true, float AngularDampener = 10f)
        {
            float Pitch_current_angular_local = HoverMode ? Calc_Direction_Vector(current_normal, Me.WorldMatrix.Backward) : Calc_Direction_Vector(direction, Me.WorldMatrix.Down);
            var Pitch_current_angular_velocity = Calc_Direction_Vector(Me.CubeGrid.Physics.AngularVelocity, Me.WorldMatrix.Right);
            return Dampener(Pitch_current_angular_local) + Dampener(Pitch_current_angular_velocity) * AngularDampener;
        }
    }
}
