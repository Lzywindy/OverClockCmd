using Sandbox.ModAPI;
using System;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public partial class PredictionPosition
    {
        public IMyTerminalBlock Me { get; set; }
        public Vector3? TargetPosition { get; set; }
        public Vector3? TargetVelocity { get; set; }
        public Vector3? GunFirePoint { get; set; }
        public float 弹头速度 { get { return _弹头速度; } set { _弹头速度 = MathHelper.Clamp(value, 100f, float.MaxValue); } }
        public int 牛顿迭代次数 { get { return _牛顿迭代次数; } set { _牛顿迭代次数 = MathHelper.Clamp(value, 1, 10); } }
        public float 精度缩放 { get { return _精度缩放; } set { _精度缩放 = MathHelper.Clamp(value, 0.01f, 100f); } }
        public float 瞄准微调 { get { return _瞄准微调; } set { _瞄准微调 = MathHelper.Clamp(value, 0.9f, 1.1f); } }
        public bool 忽略自己的速度 { get; set; } = false;
        public bool 忽略重力影响 { get; set; } = false;
        public bool 是否是直瞄武器 { get; set; } = true;
        public Vector3? 炮口方向
        {
            get
            {
                if (DisabledCalculate) return null;
                if (是否是直瞄武器) return 直瞄武器炮口方向();
                if (忽略重力影响 || Me.Physics.Gravity == Vector3.Zero) 忽略重力炮口方向();
                return 考虑诸多因素的炮口方向();
            }
        }
    }
    public partial class PredictionPosition
    {
        private Vector3? 直瞄武器炮口方向()
        {
            var _v_dis = V_dis;
            if (!_v_dis.HasValue) return null;
            var v_dis = _v_dis.Value;
            if (v_dis.Normalize() > 0)
                return v_dis;
            return null;
        }
        private Vector3? 忽略重力炮口方向()
        {
            var _v_dis = V_dis; if (!_v_dis.HasValue) return null; var v_dis = _v_dis.Value;
            var _v_relative = V_relative; if (!_v_relative.HasValue) return null; var v_relative = _v_relative.Value;
            var a = v_relative.LengthSquared() - 弹头速度 * 弹头速度;
            var b = v_relative.Dot(v_dis);
            var c = v_dis.LengthSquared();
            var k = b * b - 4 * a * c;
            if (k < 0) return null;
            var sqrt_k = (float)Math.Sqrt(k);
            var t1 = Math.Max((-b - sqrt_k) / (2 * a), 0);
            var t2 = Math.Max((-b + sqrt_k) / (2 * a), 0);
            if (t1 == 0 && t2 == 0) return null;
            else if (t1 == 0 && t2 != 0) return Solve_Velocity(t2, v_relative, v_dis, Vector3.Zero, false);
            else if (t1 != 0 && t2 == 0) return Solve_Velocity(t1, v_relative, v_dis, Vector3.Zero, false);
            else return Solve_Velocity(Math.Min(t1, t2), v_relative, v_dis, Vector3.Zero, false);
        }
        private Vector3? 考虑诸多因素的炮口方向()
        {
            var _v_dis = V_dis; if (!_v_dis.HasValue) return null;
            var v_dis = _v_dis.Value;
            var _v_relative = V_relative; if (!_v_relative.HasValue) return null;
            var v_relative = _v_relative.Value;
            var g = Me.CubeGrid.Physics.Gravity;
            var g_l = g.LengthSquared();
            var v_l = v_relative.LengthSquared();
            var b = -2 * v_relative.Dot(g) / g_l;
            var c = (4 * v_l - 2 * g.Dot(v_dis) - 4 * 弹头速度 * 弹头速度) / g_l;
            var d = 4 * v_relative.Dot(v_dis) / g_l;
            var e = 4 * v_l / g_l;
            var t_0 = 2.0f * (float)v_dis.Length() / 弹头速度;
            int count = 0;
            float? t_n = Solve_Subfunction(t_0, b, c, d, e);
            Vector3? velocity;
            while (t_n.HasValue && (count < 牛顿迭代次数))
            {
                t_n = Solve_Subfunction(t_n.Value, b, c, d, e);
                if (!t_n.HasValue) return null;
                velocity = Solve_Velocity(t_n.Value, v_relative, v_dis, g, true);
                if (!velocity.HasValue) return null;
                if (MathHelper.RoundOn2((velocity.Value.LengthSquared() - 弹头速度 * 弹头速度) * 精度缩放) == 0)
                    return velocity;
            }
            if (!t_n.HasValue) return null;
            velocity = Solve_Velocity(t_n.Value, v_relative, v_dis, g, false);
            if (!velocity.HasValue || velocity.Value.Normalize() == 0) return null;
            return velocity;
        }
    }
    public partial class PredictionPosition
    {
        private bool DisabledCalculate { get { return (Me == null || Me.CubeGrid.Physics == null || !GunFirePoint.HasValue); } }
        private Vector3? V_dis
        {
            get
            {
                if (!TargetPosition.HasValue)
                    return null;
                return TargetPosition.Value - GunFirePoint.Value;
            }
        }
        private Vector3? V_relative
        {
            get
            {
                if (!TargetVelocity.HasValue)
                    return null;
                return TargetVelocity.Value - (忽略自己的速度 ? Vector3.Zero : Me.CubeGrid.Physics.LinearVelocity);
            }
        }
        private float? Solve_Subfunction(float t, float b, float c, float d, float e)
        {
            var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
            var t_b = 4 * t_3 + 3 * t_2 * b + 2 * c + d;
            if (t_b == 0) return null;
            var t_v = 3 * t_4 + 2 * b * t_3 + c * t_2 - e;
            var t_next = t_v / t_b;
            if (t_next <= 0) return null;
            return t_next;
        }
        private Vector3? Solve_Velocity(float t, Vector3 v_r, Vector3 dis, Vector3 g, bool InSimulate = true)
        {
            if (t <= 0) return null;
            if (!InSimulate) t *= 瞄准微调;
            return (dis / t + v_r - g * t / 2);
        }
        private int _牛顿迭代次数 = 5;
        private float _精度缩放 = 1f;
        private float _瞄准微调 = 1f;
        private float _弹头速度 = 200f;
    }
}
