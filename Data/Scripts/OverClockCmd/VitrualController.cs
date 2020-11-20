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
        private IMyTerminalBlock Me;
        public IMyEntity Target { get; set; }
        public IMyTerminalBlock ReferedGunRoot { get; set; }
        public float 弹头速度 { get; set; }
        public float 抛物线起点偏移 { get; set; } = 0f;
        public bool 忽略自己的速度 { get; set; } = false;
        public bool 忽略重力影响 { get; set; } = false;
        public bool 是否是直瞄武器 { get; set; } = false;
        public int 牛顿迭代次数 { get; set; } = 5;
        public float 精度缩放 { get; set; } = 1f;
        public Vector3? Direction()
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
                return Solve_Velocity(t, v_relative, v_dis, Vector3.Zero);
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
                    var velocity = Solve_Velocity(t_n.Value, v_relative, v_dis, g);
                    if (!velocity.HasValue) return null;
                    if (MathHelper.RoundOn2((velocity.Value.LengthSquared() - 弹头速度 * 弹头速度) * 精度缩放) == 0)
                        return velocity;
                }
                t_n = Solve_Subfunction(t_n.Value, b, c, d, e);
                if (!t_n.HasValue) return null;
                return Solve_Velocity(t_n.Value, v_relative, v_dis, g);
            }

        }
        private float? Solve_Subfunction(float t, float b, float c, float d, float e)
        {
            var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
            var t_b = 4 * t_3 + 3 * t_2 * b + 2 * c + d;
            if (t_b == 0) return null;
            var t_v = 3 * t_4 + 2 * b * t_3 + c * t_2 - e;
            var t_next = t_v / t_b;
            if (t_next < 0) return null;
            return t_next;
        }
        private Vector3? Solve_Velocity(float t, Vector3 v_r, Vector3 dis, Vector3 g)
        {
            if (t <= 0) return null;
            return (dis / t + v_r - g * t / 2);
        }
        public Vector3? GunFirePoint { get { if (ReferedGunRoot == null) return null; return ReferedGunRoot.GetPosition() + GunFirePointOffset * ReferedGunRoot.WorldMatrix.Forward; } }
        public Vector3? TargetVelocity { get { if (Target == null) return null; if (!(Target is IMyTerminalBlock)) { if (Target.Physics == null) return null; else return Target.Physics.LinearVelocity; } else { var block = Target as IMyTerminalBlock; if (block.CubeGrid.Physics == null) return null; else return block.CubeGrid.Physics.LinearVelocity; } } }
    }
}
