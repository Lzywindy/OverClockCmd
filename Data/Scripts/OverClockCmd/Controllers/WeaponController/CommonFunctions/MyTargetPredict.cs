using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRageMath;
using System.Linq;
using VRage.Game;
namespace SuperBlocks.Controller
{
    public sealed class MyTargetPredict
    {
        #region 私有服务函数         
        private static Vector3D GetWeaponDirection(ICollection<IMyTerminalBlock> Weapons)
        {
            if (Utils.Common.IsNullCollection(Weapons)) { return Vector3D.Zero; }
            Vector3 vector = Vector3.Zero;
            foreach (var weapon in Weapons)
                vector += weapon.WorldMatrix.Forward;
            if (vector == Vector3.Zero) return Vector3D.Zero;
            else { vector.Normalize(); return vector; }
        }
        #endregion
        public MyTargetPredict() : base() { }
        public void Init() { ResetResult(); }
        public bool CanFireWeapon(ICollection<IMyTerminalBlock> Weapons)
        {
            if (Direction == null || Utils.Common.IsNullCollection(Weapons)) return false;
            return Weapons.All(B => B.WorldMatrix.Forward.Dot(Direction.Value) > 0.99999);
        }
        public void SetWeaponAmmoConfigInfo(MyTurretConfig Config, string WeaponName, string AmmoName)
        {
            if (Config == null) return;
            Config.GetConfig(WeaponName, AmmoName, out Parameters);
            this.WeaponName = WeaponName;
            this.AmmoName = AmmoName;
        }
        public void SetWeaponAmmoConfigInfo(MyWeaponParametersConfig Parameters)
        {
            this.Parameters = Parameters;
        }
        public string WeaponName { get; private set; }
        public string AmmoName { get; private set; }
        public MyTargetDetected TargetLocked { get; set; }
        public Vector3D? Direction => VpD;
        public void ResetResult() { VpD = null; Tn = null; }
        #region 私有函数
        public void CalculateDirection(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons)
        {
            Vector3D SelfPosition; Vector3D CannonDirection; Vector3D SelfVelocity; Vector3D SelfGravity; Vector3D TargetPosition; Vector3D TargetVelocity; Vector3D TargetLinearAcc;
            if (!GetParameters(Me, Weapons, out SelfPosition, out CannonDirection, out SelfVelocity, out SelfGravity, out TargetPosition, out TargetVelocity, out TargetLinearAcc)) { VpD = null; Tn = null; return; }
            var time_fixed = (Parameters.TimeFixed * TimeGap);
            var dis_v = (TargetPosition - SelfPosition) + (TargetVelocity - SelfVelocity + 0.5 * TargetLinearAcc * time_fixed) * time_fixed;
            var d_length = (float)dis_v.Length();
            var d_vector = ((d_length <= 0) ? Vector3.Zero : Vector3.Normalize(dis_v));
            if (Parameters.IsDirect) { if (d_length <= 10 || d_length > Parameters.Trajectory) { VpD = null; Tn = null; return; } else { VpD = d_vector; Tn = null; return; } }
            var v_r = TargetVelocity - SelfVelocity;
            var min_time = MaxiumTime(0, Parameters.Speed, Parameters.Acc, d_length) * 0.5f;
            var time = min_time;
            var max_time = MaxiumTime(0, Parameters.Speed, Parameters.Acc, Parameters.Trajectory) * 1.5f;
            var V_project_length = AverangeSpeed(0, Parameters.Speed, Parameters.Acc, (min_time + max_time) / 2);
            //SelfGravity = SelfGravity * (float)(Math.Max(d_length / 1000 - 2, 0) * 4e-2f + 1);
            if (min_time >= max_time) { VpD = null; Tn = null; return; }
            var a_r = TargetLinearAcc - SelfGravity;
            var a = a_r.LengthSquared() * 0.25;
            var b = (-v_r.Dot(a_r) * 0.5);
            var c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
            var d = v_r.Dot(dis_v);
            var e = dis_v.LengthSquared();
            int count = 0;
            var Tn_inner = Tn ?? Solve_Subfunction(time, a, b, c, d, e, min_time, max_time, time);
            while (count < Parameters.Calc_t)
            {
                Tn_inner = Solve_Subfunction(Tn_inner, a, b, c, d, e, min_time, max_time, time);
                V_project_length = AverangeSpeed(0, Parameters.Speed, Parameters.Acc, Tn_inner);
                c = (v_r.LengthSquared() - a_r.Dot(dis_v) * 0.5 - V_project_length * V_project_length);
                count++;
            }
            var g = SelfGravity.Length() * Parameters.Gravity_mult;
            Tn = Tn_inner;
            if (Tn_inner > max_time) { VpD = null; Tn = null; return; }
            VpD = GetNormalize(Solve_Direction(1.02f * Tn_inner, v_r, dis_v, SelfGravity));
            Vector3D? PositionResult;
            double? Distance;
            var target_position = TargetPosition + (TargetVelocity + 0.5 * TargetLinearAcc * Tn_inner) * Tn_inner;
            EstimatePosition(SelfPosition, SelfVelocity, VpD ?? CannonDirection, (float)Tn_inner, ref Parameters, out PositionResult, out Distance);
            if (!VpD.HasValue || !PositionResult.HasValue || !Distance.HasValue) return;
            VpD = Vector3D.Normalize(VpD.Value * Distance.Value + (g * 2.5f / Math.Max(Parameters.Speed, 1)) * (target_position - PositionResult.Value));
        }

        private static void EstimatePosition(Vector3D Position_Start, Vector3D InitSpeed, Vector3D Direction_Start, float totaltime, ref MyWeaponParametersConfig Parameters, out Vector3D? PositionResult, out double? Distance)
        {
            PositionResult = null; Distance = null;
            if (Vector3D.IsZero(Direction_Start) || totaltime == 0) return;
            float t_Acc = (Parameters.Acc == 0) ? 0 : Parameters.Speed / Parameters.Acc;
            var count = (int)(totaltime / TimeGap);
            var bis = totaltime - count * TimeGap;
            var step_acc = (int)(t_Acc / TimeGap);
            float Vp_0_l = Parameters.Speed;
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
                Velocity_Next = (acc_enabled ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + Parameters.Acc * TimeGap)) : Velocity) + current_g * TimeGap;
                Position_Next = (acc_enabled ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + 0.5 * Parameters.Acc * TimeGap)) : Velocity) * TimeGap + 0.5f * current_g * TimeGap * TimeGap;
                int_d += Vector3D.Distance(Position_Next, Position);
            }
            if (bis >= 0)
            {
                var current_g = Utils.MyPlanetInfoAPI.GetCurrentGravity(Position);
                Position = Position_Next;
                Position_Next = ((step_acc > step) ? (Vector3D.Normalize(Velocity) * (Velocity.Length() + 0.5 * Parameters.Acc * bis)) : Velocity) * bis + 0.5f * current_g * bis * bis;
                int_d += Vector3D.Distance(Position_Next, Position);
            }
            PositionResult = Position_Next; Distance = int_d;
        }

        private static float ErrorFunction(Vector3D? TargetPosition, Vector3D? TargetVelocity, Vector3D? SelfPosition, Vector3 SelfGravity, Vector3 SelfVelocity, Vector3D VpD, double V_p_length, double t)
        {
            if (!TargetPosition.HasValue || !TargetVelocity.HasValue || SelfPosition == null || VpD == null) return 2;
            Vector3 Vector_GT = TargetVelocity.Value * t + TargetPosition.Value - SelfPosition.Value;
            Vector3 Vector_SF = (SelfVelocity + VpD * V_p_length + (SelfGravity * 0.5f * (float)t)) * t;
            return MyMath.CosineDistance(ref Vector_GT, ref Vector_SF);
        }
        private MyWeaponParametersConfig Parameters;
        private Vector3D? VpD;
        private double? Tn;
        #endregion
        #region 供脚本调用的的函数       
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
        private static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null) { if (t <= 0) return null; var velocity = (dis / t + v_r - (g ?? Vector3D.Zero)); if (velocity.Normalize() == 0) return null; return velocity; }
        private static Vector3D? GetNormalize(Vector3D? Vector) { if (!Vector.HasValue || Vector3D.IsZero(Vector.Value)) return null; return Vector3D.Normalize(Vector.Value); }
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
        private bool GetParameters(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, out Vector3D SelfPosition, out Vector3D SelfDirection, out Vector3D SelfVelocity, out Vector3D SelfGravity, out Vector3D TargetPosition, out Vector3D TargetVelocity, out Vector3D TargetLinearAcc)
        {
            SelfPosition = Vector3D.Zero; SelfDirection = Vector3D.Zero; SelfVelocity = Vector3D.Zero; SelfGravity = Vector3D.Zero; TargetPosition = Vector3D.Zero; TargetVelocity = Vector3D.Zero; TargetLinearAcc = Vector3D.Zero;
            if (Utils.Common.IsNullCollection(Weapons) || Utils.Common.IsNull(Me)) { return false; }
            Vector3 vector = Vector3.Zero; Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons) { vector += weapon.WorldMatrix.Forward; position += GetWeaponOffset(weapon); }
            if (vector == Vector3.Zero) { return false; }
            else { vector.Normalize(); SelfDirection = vector; }
            position /= Weapons.Count(); SelfPosition = position;
            SelfVelocity = Parameters.Ignore_speed_self ? Vector3.Zero : (Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero);
            SelfGravity = (Me.CubeGrid?.Physics?.Gravity ?? Vector3.Zero) * Parameters.Gravity_mult;
            bool Enabled = TargetLocked?.GetTarget_PV(Me, out TargetPosition, out TargetVelocity) ?? false;
            if (!Enabled) { return false; }
            TargetLinearAcc = TargetLocked?.Entity?.Physics?.LinearAcceleration ?? Vector3D.Zero;
            return true;
        }
        #endregion
        public static MyTuple<Vector3D?, double?>? CalculateDirection(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, long TargetID, float GravityMult, float InitialSpeed, float DesiredSpeed, float Acc, float Trajectory, MyTuple<Vector3D?, double?>? CalculateResult)
        {
            Vector3D SelfPosition; Vector3D CannonDirection; Vector3D SelfVelocity; Vector3D SelfGravity; Vector3D TargetPosition; Vector3D TargetVelocity; Vector3D TargetLinearAcc;
            if (!GetParameters(Me, Weapons, TargetID, GravityMult, out SelfPosition, out CannonDirection, out SelfVelocity, out SelfGravity, out TargetPosition, out TargetVelocity, out TargetLinearAcc)) return null;
            var time_fixed = (2 * TimeGap);
            var v_dis = (TargetPosition - SelfPosition) + (TargetVelocity - SelfVelocity + 0.5 * TargetLinearAcc * time_fixed) * time_fixed;
            if (DesiredSpeed == -1) { var d_length = v_dis.Length(); if (d_length <= 10 || d_length > Trajectory) return null; else return new MyTuple<Vector3D?, double?>(v_dis, null); }
            SelfGravity *= (float)Function(v_dis.Length());
            var v_relative = TargetVelocity - SelfVelocity;
            double V_project_length = Math.Max(AverangeSpeed(InitialSpeed, DesiredSpeed, Acc, CalculateResult?.Item2), 0);
            var min_time = MaxiumTime(InitialSpeed, DesiredSpeed, Acc, (float)v_dis.Length()) * 0.7f;
            var time = min_time;
            var max_time = MaxiumTime(InitialSpeed, DesiredSpeed, Acc, Trajectory);
            if (min_time >= max_time) return null;
            var ar = TargetLinearAcc - SelfGravity;
            var a = ar.LengthSquared() * 0.25;
            var b = (-v_relative.Dot(ar) * 0.5);
            var c_start = v_relative.LengthSquared() - (ar).Dot(v_dis) * 0.5;
            var c = (c_start - V_project_length * V_project_length);
            var d = v_relative.Dot(v_dis);
            var e = v_dis.LengthSquared();
            int count = 0;
            var Tn_inner = CalculateResult?.Item2 ?? Solve_Subfunction(time, a, b, c, d, e, min_time, max_time, time);
            var VpD_inner = CalculateResult?.Item1 ?? (Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity) ?? CannonDirection);
            while (count < 10 && ErrorFunction(TargetPosition, TargetVelocity, SelfPosition, SelfGravity, SelfVelocity, VpD_inner, V_project_length, Tn_inner) > 0.00015f)
            {
                Tn_inner = Solve_Subfunction(Tn_inner, a, b, c, d, e, min_time, max_time, time);
                V_project_length = Math.Max(AverangeSpeed(InitialSpeed, DesiredSpeed, Acc, Tn_inner), 0);
                VpD_inner = Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity) ?? CannonDirection;
                c = (c_start - V_project_length * V_project_length);
                count++;
            }
            CalculateResult = new MyTuple<Vector3D?, double?>(Solve_Direction(Tn_inner, v_relative, v_dis, SelfGravity), Tn_inner);
            return CalculateResult;
        }
        private static bool GetParameters(IMyTerminalBlock Me, ICollection<IMyTerminalBlock> Weapons, long TargetID, float GravityMult, out Vector3D SelfPosition, out Vector3D SelfDirection, out Vector3D SelfVelocity, out Vector3D SelfGravity, out Vector3D TargetPosition, out Vector3D TargetVelocity, out Vector3D TargetLinearAcc)
        {
            SelfPosition = Vector3D.Zero; SelfDirection = Vector3D.Zero; SelfVelocity = Vector3D.Zero; SelfGravity = Vector3D.Zero; TargetPosition = Vector3D.Zero; TargetVelocity = Vector3D.Zero; TargetLinearAcc = Vector3D.Zero;
            var target = MyAPIGateway.Entities.GetEntityById(TargetID);
            if (Utils.Common.IsNullCollection(Weapons) || Utils.Common.IsNull(Me) || Utils.Common.IsNull(target)) { return false; }
            Vector3 vector = Vector3.Zero; Vector3 position = Vector3.Zero;
            MyTargetDetected TargetLocked = new MyTargetDetected(target, Me, true);
            foreach (var weapon in Weapons) { vector += weapon.WorldMatrix.Forward; position += GetWeaponOffset(weapon); }
            if (vector == Vector3.Zero) { return false; }
            else { vector.Normalize(); SelfDirection = vector; }
            position /= Weapons.Count(); SelfPosition = position;
            SelfVelocity = (Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero);
            SelfGravity = Utils.MyPlanetInfoAPI.GetCurrentGravity(SelfPosition) * GravityMult;
            bool Enabled = TargetLocked?.GetTarget_PV(Me, out TargetPosition, out TargetVelocity) ?? false;
            if (!Enabled) { return false; }
            TargetLinearAcc = TargetLocked?.Entity?.Physics?.LinearAcceleration ?? Vector3D.Zero;
            return true;
        }
        private static float AverangeSpeed(float InitialSpeed, float DesiredSpeed, float Acc, double? t_n) { if (Acc == 0) return DesiredSpeed; float _tn = (float)(t_n ?? 0); var acc_t = (DesiredSpeed - InitialSpeed) / Acc; if (_tn < acc_t) return InitialSpeed + 0.5f * Acc * _tn; var p = acc_t / _tn * 0.5f; return Math.Max(DesiredSpeed * (1 - p) + InitialSpeed * p, 0); }
        private static float MaxiumTime(float InitialSpeed, float DesiredSpeed, float Acc, float Trajectory) { if (Acc == 0) return Trajectory / DesiredSpeed; var acc_t = (DesiredSpeed - InitialSpeed) / Acc; var dis_acc = (InitialSpeed + 0.5f * Acc * acc_t) * acc_t; if (dis_acc > Trajectory) return ((float)Math.Sqrt(InitialSpeed * InitialSpeed + 2 * Acc * Trajectory) - InitialSpeed) / Acc; if (dis_acc == Trajectory) return acc_t; return Math.Max((Trajectory - dis_acc) / DesiredSpeed + acc_t, 0); }
        private static double Function(double distance) { if (distance <= 2000) return 1; return Math.Max(distance / 1000 - 2, 0) * 0.004f + 1; }
        public static Vector3D GetWeaponOffset(IMyTerminalBlock Weapon) { var size = Weapon.LocalAABB.Size; return Weapon.GetPosition() + Weapon.WorldMatrix.Forward * Math.Max(Math.Max(size.X, size.Y), size.Z); }
    }
}