using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRageMath;
namespace SuperBlocks.Controller
{
    public sealed partial class MyTargetPredict
    {
        public MyTargetPredict() : base() { }
        public void Init() { ResetResult(); }
        public bool CanFireWeapon(ICollection<IMyTerminalBlock> Weapons)
        {
            if (Direction == null || Utils.Common.IsNullCollection(Weapons)) return false;
            return Weapons.All(CanFireWeapon);
        }
        public bool CanFireWeapon(IMyTerminalBlock Weapons)
        {
            if (Direction == null || Utils.Common.NullEntity(Weapons)) return false;
            return Weapons.WorldMatrix.Forward.Dot(Direction.Value) > 0.99999;
        }
        public MyTargetDetected TargetLocked { get; set; }
        public Vector3D? Direction => VpD;
        public void ResetResult() { VpD = null; Tn = null; }
        public void Clear() { VpD = null; Tn = null; TargetLocked = null; }
        #region 私有函数
        public void CalculateDirection(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, ref Definitions.Structures.MyWeaponParametersConfig Parameters)
        {
            Vector3D SelfPosition; Vector3D CannonDirection; Vector3 SelfVelocity; Vector3 SelfGravity; Vector3D TargetPosition; Vector3 TargetVelocity; Vector3 TargetLinearAcc;
            if (!GetParameters(Me, Weapons, ref Parameters, out SelfPosition, out CannonDirection, out SelfVelocity, out SelfGravity, out TargetPosition, out TargetVelocity, out TargetLinearAcc)) { VpD = null; Tn = null; return; }
            var time_fixed = (Parameters.TimeFixed * Definitions.TimeGap);
            var dis_v = (TargetPosition - SelfPosition) + (TargetVelocity - SelfVelocity + 0.5f * TargetLinearAcc * time_fixed) * time_fixed;
            var d_length = (float)dis_v.Length();
            var d_vector = ((d_length <= 0) ? Vector3.Zero : Vector3.Normalize(dis_v));
            if (Parameters.Trajectory.IsDirect) { if (d_length <= 1 || d_length > Parameters.Trajectory.MaxTrajectory) { VpD = null; Tn = null; return; } else { VpD = d_vector; Tn = null; return; } }
            var v_r = TargetVelocity - SelfVelocity;
            var max_time = Parameters.Trajectory.MaxTrajectoryTime * Definitions.TimeGap;
            var V_project_length = Math.Max(AverangeSpeed(Parameters.Trajectory.InitialSpeed, Parameters.Trajectory.DesiredSpeed, Parameters.Trajectory.AccelPerSec, Definitions.TimeGap), 10);
            var min_time = d_length / V_project_length * 0.65f;
            var time = min_time;
            //if (min_time >= max_time) { VpD = null; Tn = null; return; }
            var a_r = TargetLinearAcc - SelfGravity;
            var a = a_r.LengthSquared() * 0.25;
            var b = (-v_r.Dot(a_r) * 0.5);
            var c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
            var d = v_r.Dot(dis_v);
            var e = dis_v.LengthSquared();
            int count = 0;
            var Tn_inner = Tn ?? Solve_Subfunction(time, a, b, c, d, e, min_time, max_time, time);
            var VpD_inner = VpD ?? GetNormalize(Solve_Direction(Tn_inner, v_r, dis_v, SelfGravity)) ?? CannonDirection;
            while (count < Parameters.Calc_t && ErrorFunction(TargetPosition, TargetVelocity, SelfPosition, SelfGravity, SelfVelocity, VpD_inner, V_project_length, Tn_inner) > Parameters.Delta_precious)
            {
                Tn_inner = Math.Max(Solve_Subfunction(Tn_inner, a, b, c, d, e, min_time, max_time, time), float.Epsilon);
                V_project_length = AverangeSpeed(Parameters.Trajectory.InitialSpeed, Parameters.Trajectory.DesiredSpeed, Parameters.Trajectory.AccelPerSec, Math.Max(Tn_inner, Definitions.TimeGap));
                VpD_inner = GetNormalize(Solve_Direction(Tn_inner, v_r, dis_v, SelfGravity)) ?? CannonDirection;
                c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
                count++;
            }
            Tn = Tn_inner;
            VpD = GetNormalize(Solve_Direction_Result(Tn_inner, v_r, dis_v, SelfGravity));
        }





        private Vector3D? VpD;
        private double? Tn;
        #endregion
        #region 供脚本调用的的函数       

        #endregion

        #region 得到参数
        /// <summary>
        /// 得的自己和炮弹的参数
        /// </summary>
        /// <param name="Me">我自己</param>
        /// <param name="Weapons">武器组</param>
        /// <param name="SelfPosition">自己的位置</param>
        /// <param name="SelfDirection">自己的朝向</param>
        /// <param name="SelfVelocity">自己的速度</param>
        /// <param name="SelfGravity">自己所受到的重力</param>
        /// <param name="TargetPosition">目标位置</param>
        /// <param name="TargetVelocity">目标速度</param>
        /// <param name="TargetLinearAcc">目标加速度</param>
        /// <returns>返回是否可以进行计算</returns>
        private bool GetParameters(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, ref Definitions.Structures.MyWeaponParametersConfig Parameters, out Vector3D SelfPosition, out Vector3D SelfDirection, out Vector3 SelfVelocity, out Vector3 SelfGravity, out Vector3D TargetPosition, out Vector3 TargetVelocity, out Vector3 TargetLinearAcc)
        {
            SelfPosition = Vector3D.Zero; SelfDirection = Vector3D.Zero; SelfVelocity = Vector3.Zero; SelfGravity = Vector3.Zero; TargetPosition = Vector3D.Zero; TargetVelocity = Vector3.Zero; TargetLinearAcc = Vector3.Zero;
            if (Utils.Common.IsNullCollection(Weapons) || Utils.Common.IsNull(Me)) { return false; }
            Vector3 vector = Vector3.Zero; Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons) { vector += weapon.WorldMatrix.Forward; position += GetWeaponOffset(weapon); }
            if (vector == Vector3.Zero) { return false; }
            else { vector.Normalize(); SelfDirection = vector; }
            position /= Weapons.Count(); SelfPosition = position;
            SelfVelocity = (Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero);
            SelfGravity = Utils.MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()) * Parameters.Trajectory.GravityMultiplier;
            bool Enabled = TargetLocked?.GetTarget_PV(Me, out TargetPosition, out TargetVelocity, out TargetLinearAcc) ?? false;
            if (!Enabled) return false;
            return true;
        }
        #endregion



    }
    public sealed partial class MyTargetPredict
    {
        private static double Function(double distance, double rate) { return Math.Max(distance / 1000 - 2.5, 0) * Math.Max(1 - rate, 0) + 1; }
        private static Vector3D GetWeaponOffset(IMyTerminalBlock Weapon) { var size = Weapon.LocalAABB.Size; return Weapon.GetPosition() + Weapon.WorldMatrix.Forward * Math.Max(Math.Max(size.X, size.Y), size.Z); }
        private static Vector3D? Solve_Direction_Result(double t, Vector3D v_r, Vector3D dis, Vector3D g) { if (t <= 0) return null; var velocity = (dis / t + v_r - g * t); if (velocity.Normalize() == 0) return null; return velocity; }
        private static double Solve_Subfunction(double t, double a, double b, double c, double d, double e, double min, double max, double mid)
        {
            var t_b = dF_t(t, a, b, c, d);
            if (t_b == 0) return min;
            var t_v = F_t(t, a, b, c, d, e);
            var t_next = t - t_v * MathHelper.Clamp(Math.Abs(t_v), 1, 2) / t_b;
            if (Math.Abs(F_t(t_next, a, b, c, d, e)) >= F_t(t, a, b, c, d, e))
                t_next = mid;
            return MathHelper.Clamp(t_next, min, max);
        }
        private static double F_t(double t, double a, double b, double c, double d, double e) { var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3; return (a * t_4 + b * t_3 + c * t_2 + d * t + e); }
        private static double dF_t(double t, double a, double b, double c, double d) { var t_2 = t * t; var t_3 = t * t_2; return 4 * a * t_3 + 3 * t_2 * b + 2 * c * t + d; }
        private static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null) { if (t <= 0) return null; var velocity = (dis / t + v_r - (g ?? Vector3D.Zero) * t); if (velocity.Normalize() == 0) return null; return velocity; }
        private static Vector3D? GetNormalize(Vector3D? Vector) { if (!Vector.HasValue || Vector3D.IsZero(Vector.Value)) return null; return Vector3D.Normalize(Vector.Value); }
        private static float AverangeSpeed(float InitialSpeed, float DesiredSpeed, float Acc, double? t_n)
        {
            if (Acc <= 0) return DesiredSpeed;
            float _tn = Math.Max((float)(t_n ?? 0), Definitions.TimeGap);
            Acc = Math.Max(Acc, float.Epsilon);
            var acc_t = (DesiredSpeed - InitialSpeed) / Math.Max(Acc, float.Epsilon);
            if (_tn < acc_t)
                return InitialSpeed + 0.5f * Acc * _tn;
            var p = acc_t / _tn * 0.5f;
            return Math.Max(DesiredSpeed * (1 - p) + InitialSpeed * p, 0);
        }
        private static float MaxiumTime(float InitialSpeed, float DesiredSpeed, float Acc, float Trajectory)
        {
            DesiredSpeed = Math.Max(DesiredSpeed, float.Epsilon);
            if (Acc <= 0) return Trajectory / DesiredSpeed;
            Acc = Math.Max(Acc, float.Epsilon);
            var acc_t = (DesiredSpeed - InitialSpeed) / Acc;
            var dis_acc = (InitialSpeed + 0.5f * Acc * acc_t) * acc_t;
            if (dis_acc > Trajectory) return ((float)Math.Sqrt(InitialSpeed * InitialSpeed + 2 * Acc * Trajectory) - InitialSpeed) / Acc;
            if (dis_acc == Trajectory) return acc_t;
            return Math.Max((Trajectory - dis_acc) / DesiredSpeed + acc_t, 0);
        }
        private static float ErrorFunction(Vector3D? TargetPosition, Vector3D? TargetVelocity, Vector3D? SelfPosition, Vector3 SelfGravity, Vector3 SelfVelocity, Vector3D VpD, double V_p_length, double t)
        {
            if (!TargetPosition.HasValue || !TargetVelocity.HasValue || SelfPosition == null || VpD == null) return 2;
            Vector3 Vector_GT = TargetVelocity.Value * t + TargetPosition.Value - SelfPosition.Value;
            Vector3 Vector_SF = (SelfVelocity + VpD * V_p_length + (SelfGravity * 0.5f * (float)t)) * t;
            return MyMath.CosineDistance(ref Vector_GT, ref Vector_SF);
        }
        private static Vector3D? HeightAddOn(Vector3D? VpD, Vector3D g, float d, float v, float rate)
        {
            if (!VpD.HasValue) return null;
            return VpD.Value * v - 2 * g * d * d * rate / v;
        }
        private static void EstimatePosition(Vector3D Position_Start, Vector3D InitSpeed, Vector3D Direction_Start, float totaltime, ref Definitions.Structures.MyWeaponParametersConfig Parameters, out Vector3D? PositionResult, out double? Distance)
        {
            PositionResult = null; Distance = null;
            if (Vector3D.IsZero(Direction_Start) || totaltime == 0) return;
            float t_Acc = (Parameters.Trajectory.AccelPerSec == 0) ? 0 : Parameters.Trajectory.DesiredSpeed / Parameters.Trajectory.AccelPerSec;
            var count = (int)(totaltime / Definitions.TimeGap);
            var bis = totaltime - count * Definitions.TimeGap;
            var step_acc = (int)(t_Acc / Definitions.TimeGap);
            float Vp_0_l = Parameters.Trajectory.DesiredSpeed;
            Vector3D Position, Position_Next, Velocity, Velocity_Next;
            Velocity = Velocity_Next = ((t_Acc == 0) ? (Direction_Start * Vp_0_l) : Direction_Start) + InitSpeed;
            Position = Position_Next = Position_Start;
            double int_d = 0; int step;
            for (step = 0; step < count; step++)
            {
                var current_g = Utils.MyPlanetInfoAPI.GetCurrentGravity(Position);
                Velocity = Velocity_Next;
                Position = Position_Next;
                bool acc_enabled = (step_acc > step);
                Velocity_Next = (acc_enabled ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + Parameters.Trajectory.AccelPerSec * Definitions.TimeGap)) : Velocity) + current_g * Definitions.TimeGap;
                Position_Next = (acc_enabled ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + 0.5 * Parameters.Trajectory.AccelPerSec * Definitions.TimeGap)) : Velocity) * Definitions.TimeGap + 0.5f * current_g * Definitions.TimeGap * Definitions.TimeGap;
                int_d += Vector3D.Distance(Position_Next, Position);
            }
            if (bis >= 0)
            {
                var current_g = Utils.MyPlanetInfoAPI.GetCurrentGravity(Position);
                Position = Position_Next;
                Position_Next = ((step_acc > step) ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + 0.5 * Parameters.Trajectory.AccelPerSec * bis)) : Velocity) * bis + 0.5f * current_g * bis * bis;
                int_d += Vector3D.Distance(Position_Next, Position);
            }
            PositionResult = Position_Next; Distance = int_d;
        }
    }
    public sealed partial class MyTargetPredict
    {
        public static Vector3D? CalculateDirection_TargetTest(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, MyTargetDetected TargetLocked, ref Definitions.Structures.MyWeaponParametersConfig Parameters)
        {
            Vector3D SelfPosition; Vector3D CannonDirection; Vector3 SelfVelocity; Vector3 SelfGravity; Vector3D TargetPosition; Vector3 TargetVelocity; Vector3 TargetLinearAcc;
            if (!GetParameters(Me, Weapons, TargetLocked, ref Parameters, out SelfPosition, out CannonDirection, out SelfVelocity, out SelfGravity, out TargetPosition, out TargetVelocity, out TargetLinearAcc)) return null;
            var time_fixed = (Parameters.TimeFixed * Definitions.TimeGap);
            var dis_v = (TargetPosition - SelfPosition) + (TargetVelocity - SelfVelocity + 0.5f * TargetLinearAcc * time_fixed) * time_fixed;
            var d_length = (float)dis_v.Length();
            var d_vector = ((d_length <= 0) ? Vector3.Zero : Vector3.Normalize(dis_v));
            if (Parameters.Trajectory.IsDirect) { if (d_length <= 0 || d_length > Parameters.Trajectory.MaxTrajectory) return null; else return d_vector; }
            var v_r = TargetVelocity - SelfVelocity;
            var V_project_length = AverangeSpeed(Parameters.Trajectory.InitialSpeed, Parameters.Trajectory.DesiredSpeed, Parameters.Trajectory.AccelPerSec, null);
            var min_time = d_length / V_project_length * 0.7f;
            var time = min_time;
            var max_time = Parameters.Trajectory.MaxTrajectoryTime * Definitions.TimeGap;
            if (min_time >= max_time) return null;
            var a_r = TargetLinearAcc - SelfGravity;
            var a = a_r.LengthSquared() * 0.25;
            var b = (-v_r.Dot(a_r) * 0.5);
            var c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
            var d = v_r.Dot(dis_v);
            var e = dis_v.LengthSquared();
            int count = 0;
            var Tn_inner = Solve_Subfunction(time, a, b, c, d, e, min_time, max_time, time);
            var VpD_inner = GetNormalize(Solve_Direction(Tn_inner, v_r, dis_v, SelfGravity)) ?? CannonDirection;
            while (count < MathHelper.Clamp(Parameters.Calc_t, 2, 5) && ErrorFunction(TargetPosition, TargetVelocity, SelfPosition, SelfGravity, SelfVelocity, VpD_inner, V_project_length, Tn_inner) > Parameters.Delta_precious)
            {
                Tn_inner = Solve_Subfunction(Tn_inner, a, b, c, d, e, min_time, max_time, time);
                V_project_length = AverangeSpeed(Parameters.Trajectory.InitialSpeed, Parameters.Trajectory.DesiredSpeed, Parameters.Trajectory.AccelPerSec, Tn_inner);
                VpD_inner = GetNormalize(Solve_Direction(Tn_inner, v_r, dis_v, SelfGravity)) ?? CannonDirection;
                c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
                count++;
            }
            return GetNormalize(Solve_Direction_Result(Tn_inner, v_r, dis_v, SelfGravity));
        }
        private static bool GetParameters(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, MyTargetDetected TargetLocked, ref Definitions.Structures.MyWeaponParametersConfig Parameters, out Vector3D SelfPosition, out Vector3D SelfDirection, out Vector3 SelfVelocity, out Vector3 SelfGravity, out Vector3D TargetPosition, out Vector3 TargetVelocity, out Vector3 TargetLinearAcc)
        {
            SelfPosition = Vector3D.Zero; SelfDirection = Vector3D.Zero; SelfVelocity = Vector3.Zero; SelfGravity = Vector3.Zero; TargetPosition = Vector3D.Zero; TargetVelocity = Vector3.Zero; TargetLinearAcc = Vector3.Zero;
            if (Utils.Common.IsNullCollection(Weapons) || Utils.Common.IsNull(Me)) { return false; }
            Vector3 vector = Vector3.Zero; Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons) { vector += weapon.WorldMatrix.Forward; position += GetWeaponOffset(weapon); }
            if (vector == Vector3.Zero) { return false; }
            else { vector.Normalize(); SelfDirection = vector; }
            position /= Weapons.Count(); SelfPosition = position;
            SelfVelocity = (Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero);
            SelfGravity = Utils.MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()) * Parameters.Trajectory.GravityMultiplier;
            bool Enabled = TargetLocked?.GetTarget_PV(Me, out TargetPosition, out TargetVelocity, out TargetLinearAcc) ?? false;
            if (!Enabled) return false;
            return true;
        }
    }
}