using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRageMath;
using System.Linq;
using VRage.Game;
namespace SuperBlocks.Controller
{
    public sealed class MyTargetPredictEnt
    {
        #region 私有服务函数         
        private static Vector3D GetWeaponPostionDirection(IEnumerable<IMyTerminalBlock> Weapons)
        {
            if (Utils.IsNullCollection(Weapons)) { return Vector3D.Zero; }
            Vector3 vector = Vector3.Zero;
            foreach (var weapon in Weapons)
                vector += weapon.WorldMatrix.Forward;
            if (vector == Vector3.Zero) return Vector3D.Zero;
            else { vector.Normalize(); return vector; }
        }
        #endregion
        public MyTargetPredictEnt() : base() { }
        public void Init() { ResetResult(); }
        public bool CanFireWeapon(IEnumerable<IMyTerminalBlock> Weapons)
        {
            if (Direction == null || Utils.IsNullCollection(Weapons)) return false;
            if (Direction.Value.Dot(GetWeaponPostionDirection(Weapons)) < 0.99999) return false;
            return true;
        }
        public void SetWeaponAmmoConfigInfo(Dictionary<string, Dictionary<string, string>> Config, string WeaponName, string AmmoName)
        {
            WeaponAmmoConfig.GetDataFromConfig(Config, WeaponName, AmmoName);
        }
        public Target TargetLocked { get; set; }
        public float 时间补偿 { get; set; }
        public Vector3D? Direction => VpD;
        public void ResetResult() { VpD = null; Tn = null; }
        #region 私有函数
        public double Function(double distance)
        {
            if (distance < 3000) return 1;
            var a = Math.Max(WeaponAmmoConfig.weaponConfig.Delta_t - 1, 0);
            var x = MathHelper.RoundToInt(distance / 1000) - 3;
            return a * x + 1;
        }
        public void CalculateDirection(IMyTerminalBlock Me, IEnumerable<IMyTerminalBlock> Weapons, double TurretScanRange = int.MaxValue)
        {
            Vector3D SelfPosition; Vector3D CannonDirection; Vector3D SelfVelocity; Vector3D SelfGravity; Vector3D TargetPosition; Vector3D TargetVelocity; Vector3D TargetLinearAcc;
            if (!GetParameters(Me, Weapons, out SelfPosition, out CannonDirection, out SelfVelocity, out SelfGravity, out TargetPosition, out TargetVelocity, out TargetLinearAcc)) { VpD = null; Tn = null; return; }
            var time_fixed = (时间补偿 * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS);
            var v_dis = (TargetPosition - SelfPosition) + (TargetVelocity - SelfVelocity + 0.5 * TargetLinearAcc * time_fixed) * time_fixed;
            SelfGravity *= (float)Function(v_dis.Length());
            if (WeaponAmmoConfig.weaponConfig.IsDirect) { var d_length = v_dis.Length(); VpD = v_dis; if (d_length <= 0 || d_length > Math.Min(WeaponAmmoConfig.ammoConfig.Trajectory, TurretScanRange)) { VpD = null; Tn = null; return; } }
            var v_relative = TargetVelocity - SelfVelocity;
            double V_project_length = Math.Max(AverangeSpeed(ref WeaponAmmoConfig.ammoConfig, Tn), 0);
            var time = v_dis.Length() / V_project_length;
            var min_time = time * 0.5f;
            var max_time = MaxiumTime(ref WeaponAmmoConfig.ammoConfig);
            if (min_time >= max_time) { VpD = null; Tn = null; return; }
            var ar = TargetLinearAcc - SelfGravity;
            if (SelfGravity != Vector3.Zero)
            {
                var a = ar.LengthSquared() * 0.25;
                var da = 1 / a;
                var b = (-v_relative.Dot(ar) * 0.5) * da;
                var c_start = v_relative.LengthSquared() - (ar).Dot(v_dis) * 0.5;
                var c = (c_start - V_project_length * V_project_length) * da;
                var d = v_relative.Dot(v_dis) * da;
                var e = v_dis.LengthSquared() * da;
                int count = 0;
                var Tn_inner = Tn ?? Solve_Subfunction(time, b, c, d, e, min_time, max_time, time);
                var VpD_inner = VpD ?? (Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity, WeaponAmmoConfig.weaponConfig.Delta_t, true) ?? CannonDirection);
                while (count < WeaponAmmoConfig.weaponConfig.Calc_t && ErrorFunction(TargetPosition, TargetVelocity, SelfPosition, SelfGravity, SelfVelocity, VpD_inner, V_project_length, Tn_inner) > WeaponAmmoConfig.weaponConfig.Delta_precious)
                {
                    Tn_inner = Solve_Subfunction(Tn_inner, b, c, d, e, min_time, max_time, time);
                    V_project_length = Math.Max(AverangeSpeed(ref WeaponAmmoConfig.ammoConfig, Tn_inner), 25f);
                    VpD_inner = Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity, WeaponAmmoConfig.weaponConfig.Delta_t, true) ?? CannonDirection;
                    c = (c_start - V_project_length * V_project_length) * da;
                    count++;
                }
                Tn = Tn_inner;
                VpD = Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity, WeaponAmmoConfig.weaponConfig.Delta_t, false);
            }
            else
            {
                var a = v_relative.LengthSquared() - V_project_length * V_project_length;
                var b = v_relative.Dot(v_dis);
                var c = v_dis.LengthSquared();
                var k = b * b - 4 * a * c;
                if (k < 0) { VpD = null; Tn = null; return; }
                var sqrt_k = (float)Math.Sqrt(k);
                Tn = Math.Max(Math.Max((-b - sqrt_k) / (2 * a), min_time), Math.Max((-b + sqrt_k) / (2 * a), min_time));
                VpD = Solve_Direction(Tn.Value, v_relative, v_dis, SelfGravity, WeaponAmmoConfig.weaponConfig.Delta_t, false);
            }
        }
        private static float ErrorFunction(Vector3D? TargetPosition, Vector3D? TargetVelocity, Vector3D? SelfPosition, Vector3 SelfGravity, Vector3 SelfVelocity, Vector3D VpD, double V_p_length, double t)
        {
            if (!TargetPosition.HasValue || !TargetVelocity.HasValue || SelfPosition == null || VpD == null) return 2;
            Vector3 Vector_GT = TargetVelocity.Value * t + TargetPosition.Value - SelfPosition.Value;
            Vector3 Vector_SF = (SelfVelocity + VpD * V_p_length + (SelfGravity * 0.5f * (float)t)) * t;
            return MyMath.CosineDistance(ref Vector_GT, ref Vector_SF);
        }
        private static double AverangeSpeed(ref MyAmmoConfig AmmoConfig, double? t_n)
        {
            if (t_n == null || AmmoConfig.Acc == 0) return AmmoConfig.Speed;
            var t_total = Math.Max(t_n.Value, 0);
            var acc_t = AmmoConfig.Speed / AmmoConfig.Acc;
            if (acc_t >= t_total) return AmmoConfig.Acc * t_total * 0.5;
            return AmmoConfig.Acc * 0.5 + (t_total - acc_t) * AmmoConfig.Speed;
        }
        private static double MaxiumTime(ref MyAmmoConfig AmmoConfig)
        {
            if (AmmoConfig.Speed == 0) return int.MaxValue;
            if (AmmoConfig.Acc == 0) return AmmoConfig.Trajectory / AmmoConfig.Speed;
            var acc_t = AmmoConfig.Speed / AmmoConfig.Acc;
            var acc_dis_max = AmmoConfig.Acc * 0.5 * acc_t * acc_t;
            if (acc_dis_max >= AmmoConfig.Trajectory)
                return Math.Sqrt(AmmoConfig.Trajectory * 2 / AmmoConfig.Acc);
            return (AmmoConfig.Acc - acc_dis_max) / AmmoConfig.Trajectory + acc_t;
        }
        private MyWeaponAmmoConfig WeaponAmmoConfig = new MyWeaponAmmoConfig();
        private Vector3D? VpD;
        private double? Tn;
        #endregion
        #region 最新自用公共函数
        private static MyTuple<Vector3D?, Vector3D> GetWeaponPostionDirection(IEnumerable<IMyTerminalBlock> Weapons, MyTuple<bool, bool, float, float, int, float> WeaponConfig)
        {
            MyTuple<Vector3D?, Vector3D> defaultvalue = default(MyTuple<Vector3D?, Vector3D>);
            Vector3 vector = Vector3.Zero;
            Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons)
            {
                vector += weapon.WorldMatrix.Forward;
                position += weapon.GetPosition() + WeaponConfig.Item6 * weapon.WorldMatrix.Forward;
            }
            if (vector == Vector3.Zero) defaultvalue.Item2 = Vector3D.Zero;
            else { vector.Normalize(); defaultvalue.Item2 = vector; }
            position /= Weapons.Count();
            defaultvalue.Item1 = position;
            return defaultvalue;
        }
        #endregion
        #region 供脚本调用的的函数
        private static double Solve_Subfunction(double t, double b, double c, double d, double e, double min, double max, double mid)
        {
            var t_b = dF_t(t, b, c, d);
            if (t_b == 0) return min;
            var t_v = F_t(t, b, c, d, e);
            var t_next = t - t_v * MathHelper.Clamp(Math.Abs(t_v), 1, 2) / t_b;
            if (Math.Abs(F_t(t_next, b, c, d, e)) >= F_t(t, b, c, d, e))
                t_next = mid;
            return MathHelper.Clamp(t_next, min, max);
        }
        private static double F_t(double t, double b, double c, double d, double e)
        {
            var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
            return (t_4 + b * t_3 + c * t_2 + d * t + e);
        }
        private static double dF_t(double t, double b, double c, double d)
        {
            var t_2 = t * t; var t_3 = t * t_2;
            return 4 * t_3 + 3 * t_2 * b + 2 * c * t + d;
        }
        private static Vector3D ApplyGravityMultipy(Vector3D? Gravity, float Mult)
        {
            if (Gravity == null || Mult <= 0) return Vector3D.Zero;
            return Gravity.Value * Mult;
        }
        private static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null, double Delta_Time = 1, bool InSimulate = false)
        {
            if (t <= 0) return null; if (!InSimulate) t *= Delta_Time;
            var velocity = (dis / t + v_r - (g.HasValue ? (g.Value * t / 2) : Vector3D.Zero));
            if (velocity.Normalize() == 0) return null;
            return velocity;
        }
        #endregion
        public const float TimeGap = MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS;
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
        private bool GetParameters(IMyTerminalBlock Me, IEnumerable<IMyTerminalBlock> Weapons, out Vector3D SelfPosition, out Vector3D SelfDirection, out Vector3D SelfVelocity, out Vector3D SelfGravity, out Vector3D TargetPosition, out Vector3D TargetVelocity, out Vector3D TargetLinearAcc)
        {
            SelfPosition = Vector3D.Zero; SelfDirection = Vector3D.Zero; SelfVelocity = Vector3D.Zero; SelfGravity = Vector3D.Zero; TargetPosition = Vector3D.Zero; TargetVelocity = Vector3D.Zero; TargetLinearAcc = Vector3D.Zero;
            if (Utils.IsNullCollection(Weapons) || Utils.IsNull(Me)) { return false; }
            Vector3 vector = Vector3.Zero; Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons) { vector += weapon.WorldMatrix.Forward; position += weapon.GetPosition() + WeaponAmmoConfig.weaponConfig.Offset * weapon.WorldMatrix.Forward; }
            if (vector == Vector3.Zero) { return false; }
            else { vector.Normalize(); SelfDirection = vector; }
            position /= Weapons.Count(); SelfPosition = position;
            SelfVelocity = WeaponAmmoConfig.weaponConfig.Ignore_speed_self ? Vector3.Zero : (Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero);
            float GI; SelfGravity = MyAPIGateway.Physics.CalculateNaturalGravityAt(SelfPosition, out GI) * WeaponAmmoConfig.ammoConfig.Gravity_mult;
            TargetLocked?.GetTarget_PV(Me, out TargetPosition, out TargetVelocity);
            if (TargetPosition == null) { return false; }
            TargetLinearAcc = TargetLocked?.Entity?.Physics?.LinearAcceleration ?? Vector3D.Zero;
            return true;
        }
        #endregion
        #region 辅助函数
        #endregion
    }
}