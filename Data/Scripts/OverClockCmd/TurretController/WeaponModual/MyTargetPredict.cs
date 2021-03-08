using System;
using VRageMath;
namespace SuperBlocks.Controller
{
    public static class MyTargetPredict
    {
        public static void GunDirection(Vector3D? TargetPosition, Vector3D? SelfPosition, Vector3D? TargetVelocity, Vector3D? SelfVelocity, Vector3D? Gravity, double V_project_length, ref Vector3D? V_project_direction, ref double? t_n, Vector3D CannonDirection, int CalcCount = 4, float Delta_Time = 1, float Delta_Precious = 1)
        {
            if (!TargetPosition.HasValue || !SelfPosition.HasValue) return;
            var v_dis = TargetPosition.Value - SelfPosition.Value;
            if (v_dis == Vector3.Zero) return;
            if (!TargetVelocity.HasValue) { v_dis.Normalize(); V_project_direction = v_dis; return; }
            var v_relative = TargetVelocity.Value - (SelfVelocity ?? Vector3D.Zero);
            Delta_Precious = MathHelper.Clamp(Delta_Precious, 0.01f, 100f); V_project_length = Math.Max(V_project_length, 100f); Delta_Time = MathHelper.Clamp(Delta_Time, 0.9f, 1.1f);
            var min = v_dis.Length() / Math.Max(V_project_length + Vector3D.Dot(Vector3D.Normalize(v_dis), TargetVelocity.Value), 1f);
            if (Gravity.HasValue && Gravity != Vector3.Zero)
            {
                var g_l = Gravity.Value.LengthSquared(); var v_l = v_relative.LengthSquared();
                var b = -2 * v_relative.Dot(Gravity.Value) / g_l; var c = (4 * v_l - 2 * Gravity.Value.Dot(v_dis) - 4 * V_project_length * V_project_length) / g_l;
                var d = 4 * v_relative.Dot(v_dis) / g_l; var e = 4 * v_l / g_l;
                int count = 0;
                if (!V_project_direction.HasValue && CannonDirection != Vector3D.Zero)
                    V_project_direction = Vector3D.Normalize(CannonDirection);
                if (!t_n.HasValue || t_n.Value < 0)
                    t_n = Solve_Subfunction(min, b, c, d, e);
                while (count < CalcCount && ErrorFunction(TargetVelocity.Value, TargetPosition.Value, SelfVelocity.Value, V_project_direction.Value * V_project_length, SelfPosition.Value, t_n.Value, Gravity) > Delta_Precious)
                {
                    t_n = Math.Max(Solve_Subfunction(t_n.Value, b, c, d, e), min);
                    V_project_direction = Solve_Direction(t_n.Value, v_relative, v_dis, Gravity, Delta_Time, true);
                    if (!V_project_direction.HasValue && CannonDirection != Vector3D.Zero)
                        V_project_direction = Vector3D.Normalize(CannonDirection);
                    count++;
                }
            }
            else
            {
                var a = v_relative.LengthSquared() - V_project_length * V_project_length;
                var b = v_relative.Dot(v_dis);
                var c = v_dis.LengthSquared();
                var k = b * b - 4 * a * c;
                if (k < 0) { return; }
                var sqrt_k = (float)Math.Sqrt(k);
                t_n = Math.Max(Math.Max((-b - sqrt_k) / (2 * a), min), Math.Max((-b + sqrt_k) / (2 * a), min));
            }
            if (t_n == null) return;
            V_project_direction = Solve_Direction(t_n.Value, v_relative, v_dis, Gravity, Delta_Time, false);
        }
        public static double ErrorFunction(Vector3D V_target, Vector3D P_target, Vector3D V_me, Vector3D V_projector, Vector3D P_me, double t, Vector3D? Gravity = null)
        {
            var P_target_1 = V_target * t + P_target;
            var P_target_2 = (V_me + V_projector + (Gravity.HasValue ? (Gravity.Value * 0.5f * t) : Vector3D.Zero)) * t + P_me;
            return (P_target_1 - P_target_2).Length();
        }
        public static double Solve_Subfunction(double t, double b, double c, double d, double e)
        {
            var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
            var t_b = 4 * t_3 + 3 * t_2 * b + 2 * c * t + d;
            if (t_b == 0) return 0;
            var t_v = t_4 + b * t_3 + c * t_2 + d * t + e;
            return t - t_v / t_b;
        }
        public static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null, double Delta_Time = 1, bool InSimulate = false)
        {
            if (t <= 0) return null; if (!InSimulate) t *= Delta_Time;
            var velocity = (dis / t + v_r - (g.HasValue ? (g.Value * t / 2) : Vector3D.Zero));
            if (velocity == Vector3D.Zero) return null; velocity = Vector3D.Normalize(velocity);
            return velocity;
        }
    }
}