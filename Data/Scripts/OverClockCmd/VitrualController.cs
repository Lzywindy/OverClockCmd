using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Game.Models;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;
using VRageRender.Messages;

namespace SuperBlocks
{
    public partial class VitrualController
    {
        public VitrualController()
        {

        }

    }
    public partial class VitrualController
    {
        private IMyTerminalBlock Me;
        public IMyShipController LinkedShipController { get; set; }
        public IMyEntity Target { get; set; }
        public float Gun_Projector_Speed { get; set; }
        public bool EnabledOverrideByOtherblock { get; set; }
    }
    public class PredictionPosition
    {

        public PredictionPosition(IMyTerminalBlock Me)
        {
            this.Me = Me;
        }
        public IMyEntity Target { get; set; }
        public IMyTerminalBlock ReferedGunRoot { get; set; }
        public float 弹头速度 { get { return _弹头速度; } set { _弹头速度 = MathHelper.Clamp(value, 100f, float.MaxValue); } }
        public float 抛物线起点偏移 { get { return _抛物线起点偏移; } set { _抛物线起点偏移 = MathHelper.Clamp(value, -50, 50f); } }
        public int 牛顿迭代次数 { get { return _牛顿迭代次数; } set { _牛顿迭代次数 = MathHelper.Clamp(value, 1, 10); } }
        public float 精度缩放 { get { return _精度缩放; } set { _精度缩放 = MathHelper.Clamp(value, 0.01f, 100f); } }
        public float 瞄准微调 { get { return _瞄准微调; } set { _瞄准微调 = MathHelper.Clamp(value, 0.9f, 1.1f); } }
        public bool 忽略自己的速度 { get; set; } = false;
        public bool 忽略重力影响 { get; set; } = false;
        public bool 是否是直瞄武器 { get; set; } = true;

        public Vector3? 直瞄武器炮口方向
        {
            get
            {
                var FirePoint = GunFirePoint;
                if (Me == null || Me.CubeGrid.Physics == null || !FirePoint.HasValue)
                    return null;
                var v_dis = Target.GetPosition() - GunFirePoint.Value;
                if (v_dis.Normalize() > 0)
                    return v_dis;
                return null;
            }
        }

        public Vector3? 炮口方向带长度
        {
            get
            {
                var FirePoint = GunFirePoint;
                if (Me == null || Me.CubeGrid.Physics == null || !FirePoint.HasValue) return null;
                var v_dis = Target.GetPosition() - GunFirePoint.Value;
                if (是否是直瞄武器) return v_dis;
                var targetvelocity = TargetVelocity;
                if (!targetvelocity.HasValue) return null;
                var v_relative = targetvelocity.Value - (忽略自己的速度 ? Vector3.Zero : Me.CubeGrid.Physics.LinearVelocity);
                if (忽略重力影响)
                {
                    var a = v_relative.LengthSquared() - 弹头速度 * 弹头速度;
                    var b = v_relative.Dot(v_dis);
                    var c = v_dis.LengthSquared();
                    var k = b * b - 4 * a * c;
                    if (k < 0) return null;
                    var sqrt_k = (float)Math.Sqrt(k);
                    var t = Math.Min(Math.Max(-b - sqrt_k / (2 * a), 0), Math.Max(-b + sqrt_k / (2 * a), 0));
                    return Solve_Velocity(t, v_relative, v_dis, Vector3.Zero, false);
                }
                else
                {
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
                    while (t_n.HasValue && (count < 牛顿迭代次数))
                    {
                        t_n = Solve_Subfunction(t_n.Value, b, c, d, e);
                        if (!t_n.HasValue) return null;
                        var velocity = Solve_Velocity(t_n.Value, v_relative, v_dis, g, true);
                        if (!velocity.HasValue) return null;
                        if (MathHelper.RoundOn2((velocity.Value.LengthSquared() - 弹头速度 * 弹头速度) * 精度缩放) == 0)
                            return velocity;
                    }
                    t_n = Solve_Subfunction(t_n.Value, b, c, d, e);
                    if (!t_n.HasValue) return null;
                    return Solve_Velocity(t_n.Value, v_relative, v_dis, g, false);
                }
            }
        }
        public Vector3? 碰撞位置预测 { get { return null; } }
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
        private Vector3? GunFirePoint { get { if (ReferedGunRoot == null) return null; return ReferedGunRoot.GetPosition() + 抛物线起点偏移 * ReferedGunRoot.WorldMatrix.Forward; } }
        private Vector3? TargetVelocity { get { if (Target == null) return null; if (!(Target is IMyTerminalBlock)) { if (Target.Physics == null) return null; else return Target.Physics.LinearVelocity; } else { var block = Target as IMyTerminalBlock; if (block.CubeGrid.Physics == null) return null; else return block.CubeGrid.Physics.LinearVelocity; } } }
        #region 私有成员
        private int _牛顿迭代次数 = 5;
        private float _精度缩放 = 1f;
        private float _瞄准微调 = 1f;
        private float _抛物线起点偏移 = 0f;
        private float _弹头速度 = 200f;
        private readonly IMyTerminalBlock Me;
        #endregion
    }
}
