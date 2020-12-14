using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    /// <summary>
    /// 这个类专注推进器推力控制的
    /// 通过速度反馈来对飞行器的6方向的推力进行调节
    /// 可以设置最高速度限制（相对运动的时候是相对最高速度）
    /// 可以设置参考的参照物（一旦设定成功，就需要将最大限速设定到相对最大速度，否则飞船会猛加速或则减速）
    /// </summary>
    public class 推进控制器
    {
        public float MaxSpeedLimit { get; set; } = 1000f;
        public IMyEntity RelativeEntity { get; set; }
        public float MiniValue { get { return _MiniValue; } set { _MiniValue = MathHelper.Clamp(value, 0, 1); } }
        public 推进控制器(List<IMyThrust> thrusts, IMyTerminalBlock Me)
        {
            UpdateBlocks(thrusts, Me);
        }
        public void UpdateBlocks(List<IMyThrust> thrusts, IMyTerminalBlock Me)
        {
            this.thrusts = thrusts;
            this.Me = Me;
            _MiniValue = MiniValueC;
            StatisticU = (ref float force) => { };
            StatisticD = (ref float force) => { };
            StatisticL = (ref float force) => { };
            StatisticR = (ref float force) => { };
            StatisticF = (ref float force) => { };
            StatisticB = (ref float force) => { };
            ApplyPercentageU = (float percentage) => { };
            ApplyPercentageD = (float percentage) => { };
            ApplyPercentageL = (float percentage) => { };
            ApplyPercentageR = (float percentage) => { };
            ApplyPercentageF = (float percentage) => { };
            ApplyPercentageB = (float percentage) => { };
            EnabledU = (bool enabled) => { };
            EnabledD = (bool enabled) => { };
            EnabledL = (bool enabled) => { };
            EnabledR = (bool enabled) => { };
            EnabledF = (bool enabled) => { };
            EnabledB = (bool enabled) => { };
            if (NullThrust || this.Me == null) return;
            foreach (var thrust in thrusts)
            {
                if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Forward) > DirectionGate)
                {
                    StatisticF += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageF += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledF += (bool enabled) => { thrust.Enabled = enabled; };
                }
                else if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Backward) > DirectionGate)
                {
                    StatisticB += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageB += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledB += (bool enabled) => { thrust.Enabled = enabled; };
                }
                else if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Up) > DirectionGate)
                {
                    StatisticU += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageU += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledU += (bool enabled) => { thrust.Enabled = enabled; };
                }
                else if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Down) > DirectionGate)
                {
                    StatisticD += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageD += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledD += (bool enabled) => { thrust.Enabled = enabled; };
                }
                else if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Left) > DirectionGate)
                {
                    StatisticL += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageL += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledL += (bool enabled) => { thrust.Enabled = enabled; };
                }
                else if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Right) > DirectionGate)
                {
                    StatisticR += (ref float force) => { if (thrust == null) return; force += thrust.MaxEffectiveThrust; };
                    ApplyPercentageR += (float percentage) => { if (thrust == null) return; thrust.ThrustOverridePercentage = MathHelper.Clamp(percentage, MiniValue, 1); };
                    EnabledR += (bool enabled) => { thrust.Enabled = enabled; };
                }
            }
        }
        public void SetupMode(bool UpOrForward, bool EnableAll, bool DisableAll, float MaximumSpeed)
        {
            if (DisableAll) { EnabledU(false); EnabledF(false); EnabledD(false); EnabledL(false); EnabledR(false); EnabledB(false); return; }
            EnabledU(EnableAll || UpOrForward); EnabledF(EnableAll || (!UpOrForward)); EnabledD(EnableAll); EnabledL(EnableAll); EnabledR(EnableAll); EnabledB(EnableAll); MaxSpeedLimit = MaximumSpeed;
            MiniValue = DisableAll ? 0 : MiniValueC;
        }
        public void Running(Vector3 MoveIndicate, float SealevelDiff = 0, bool EnabledDampener = true)
        {
            if (NullThrust) return;
            if (Me == null) { foreach (var thrust in thrusts) thrust.ThrustOverridePercentage = 0; return; }
            var velocity = (EnabledDampener ? (Me.CubeGrid.Physics.LinearVelocity - RelativeVelocity) : Vector3.Zero) - MaxSpeedLimit * Vector3.TransformNormal(MoveIndicate, Me.WorldMatrix);
            var ReferValue = (velocity * Math.Max(1, Me.CubeGrid.Physics.Gravity.Length()) + ((Me.CubeGrid.Physics.Gravity == Vector3.Zero) ? Vector3.Zero : (Me.CubeGrid.Physics.Gravity * (1 + SealevelDiff) / GetMultipy()))) * Me.CubeGrid.Physics.Mass;
            Percentage6Direction(ReferValue, velocity);
        }
        private float GetMultipy()
        {
            if (Me.CubeGrid.Physics.Gravity == Vector3.Zero) return 1;
            var value = Math.Abs(Vector3.Normalize(Me.CubeGrid.Physics.Gravity).Dot(Me.WorldMatrix.Down));
            if (value == 0) return 1;
            return MathHelper.Clamp(1 / value, 1, 20f);
        }
        public void SetAll(bool Enabled)
        {
            EnabledU(Enabled);
            EnabledD(Enabled);
            EnabledL(Enabled);
            EnabledR(Enabled);
            EnabledF(Enabled);
            EnabledB(Enabled);
            ApplyPercentageU(0);
            ApplyPercentageD(0);
            ApplyPercentageL(0);
            ApplyPercentageR(0);
            ApplyPercentageF(0);
            ApplyPercentageB(0);
        }
        private float[] StatisticThrustForce6Direction()
        {
            float[] Force6Direction = new float[6];
            StatisticU(ref Force6Direction[(int)Base6Directions.Direction.Up]);
            StatisticD(ref Force6Direction[(int)Base6Directions.Direction.Down]);
            StatisticL(ref Force6Direction[(int)Base6Directions.Direction.Left]);
            StatisticR(ref Force6Direction[(int)Base6Directions.Direction.Right]);
            StatisticF(ref Force6Direction[(int)Base6Directions.Direction.Forward]);
            StatisticB(ref Force6Direction[(int)Base6Directions.Direction.Backward]);
            return Force6Direction;
        }
        private float[] RequiredForce6Direction(Vector3 Force)
        {
            float[] Force6Direction = new float[6];
            Force6Direction[(int)Base6Directions.Direction.Forward] = Force.Dot(Me.WorldMatrix.Backward);
            Force6Direction[(int)Base6Directions.Direction.Backward] = Force.Dot(Me.WorldMatrix.Forward);
            Force6Direction[(int)Base6Directions.Direction.Up] = Force.Dot(Me.WorldMatrix.Down);
            Force6Direction[(int)Base6Directions.Direction.Down] = Force.Dot(Me.WorldMatrix.Up);
            Force6Direction[(int)Base6Directions.Direction.Left] = Force.Dot(Me.WorldMatrix.Right);
            Force6Direction[(int)Base6Directions.Direction.Right] = Force.Dot(Me.WorldMatrix.Left);
            return Force6Direction;
        }
        private bool[] VelocityOverGate(Vector3 velocity)
        {
            bool[] Force6Direction = new bool[6];
            Force6Direction[(int)Base6Directions.Direction.Forward] = velocity.Dot(Me.WorldMatrix.Backward) > VelocityGate;
            Force6Direction[(int)Base6Directions.Direction.Backward] = velocity.Dot(Me.WorldMatrix.Forward) > VelocityGate;
            Force6Direction[(int)Base6Directions.Direction.Up] = velocity.Dot(Me.WorldMatrix.Down) > VelocityGate;
            Force6Direction[(int)Base6Directions.Direction.Down] = velocity.Dot(Me.WorldMatrix.Up) > VelocityGate;
            Force6Direction[(int)Base6Directions.Direction.Left] = velocity.Dot(Me.WorldMatrix.Right) > VelocityGate;
            Force6Direction[(int)Base6Directions.Direction.Right] = velocity.Dot(Me.WorldMatrix.Left) > VelocityGate;
            return Force6Direction;
        }
        private void Percentage6Direction(Vector3 Force, Vector3 OV)
        {
            float[] TF = StatisticThrustForce6Direction();
            float[] RF = RequiredForce6Direction(Force);
            bool[] OF = VelocityOverGate(OV);
            float[] Percentage = new float[6];
            for (int index = 0; index < 6; index++)
                Percentage[index] = MathHelper.Clamp((TF[index] != 0) ? OF[index] ? 1 : (RF[index] / TF[index]) : 0, 0, 1);
            ApplyPercentageU(Percentage[(int)Base6Directions.Direction.Up]);
            ApplyPercentageD(Percentage[(int)Base6Directions.Direction.Down]);
            ApplyPercentageL(Percentage[(int)Base6Directions.Direction.Left]);
            ApplyPercentageR(Percentage[(int)Base6Directions.Direction.Right]);
            ApplyPercentageF(Percentage[(int)Base6Directions.Direction.Forward]);
            ApplyPercentageB(Percentage[(int)Base6Directions.Direction.Backward]);
        }
        private Vector3 RelativeVelocity { get { if (RelativeEntity == null || RelativeEntity.Physics == null) { RelativeEntity = null; return Vector3.Zero; } return RelativeEntity.Physics.LinearVelocity; } }
        private bool NullThrust { get { return (thrusts == null) || (thrusts.Count == 0); } }
        private const double DirectionGate = 0.95;
        private const float MiniValueC = 1e-6f;
        private const float VelocityGate = 50f;
        private float _MiniValue;
        private List<IMyThrust> thrusts;
        private IMyTerminalBlock Me;
        #region 执行的动作（用于取消循环中的判别）
        private ActionRef<float> StatisticU;
        private ActionRef<float> StatisticD;
        private ActionRef<float> StatisticL;
        private ActionRef<float> StatisticR;
        private ActionRef<float> StatisticF;
        private ActionRef<float> StatisticB;
        private Action<float> ApplyPercentageU;
        private Action<float> ApplyPercentageD;
        private Action<float> ApplyPercentageL;
        private Action<float> ApplyPercentageR;
        private Action<float> ApplyPercentageF;
        private Action<float> ApplyPercentageB;
        private Action<bool> EnabledU;
        private Action<bool> EnabledD;
        private Action<bool> EnabledL;
        private Action<bool> EnabledR;
        private Action<bool> EnabledF;
        private Action<bool> EnabledB;
        #endregion
    }
}
