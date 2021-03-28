using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyThrusterController
        {
            public float MaxSpeedLimit { get; set; } = 1000f;
            public void SetOverclocked(float mult = 1)
            {
                if (NullThrust) return;
                foreach (var thrust in thrusts) { if (thrust.BlockDefinition.SubtypeId.Contains("HoverEngine")) continue; if (thrust.BlockDefinition.SubtypeId.Contains("Hover")) continue; if (thrust.BlockDefinition.SubtypeId.Contains("Hover Engine")) continue; thrust.PowerConsumptionMultiplier = mult; thrust.ThrustMultiplier = mult; }
            }
            public void RunningDefault(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity)
            {
                this.Me = Me;
                if (Common.IsNull(this.Me) || InThisEntity == null) return;
                var count = Common.GetTs(Me, InThisEntity).Count;
                if (NullThrust || BlockCount != count) { thrusts = Common.GetTs(Me, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && InThisEntity(thrust)); BlockCount = count; }
                if (NullThrust) return;
                foreach (var thrust in thrusts)
                {
                    if (Common.NullEntity(thrust)) continue;
                    thrust.ThrustOverridePercentage = 0;
                }
                for (int index = 0; index < 6; index++)
                {
                    Enables[index] = true;
                    Percentages[index] = 0;
                }
                _MiniValue = 0;
                ApplyPercentage();
            }
            public void Running(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, Vector3 MoveIndicate, bool UpOrForward, bool EnableAll, bool DisableAll, float MaximumSpeed, float SealevelDiff = 0, bool EnabledDampener = true)
            {
                this.Me = Me;
                if (Common.IsNull(this.Me) || InThisEntity == null) return;
                var count = Common.GetTs(Me, InThisEntity).Count;
                if (NullThrust || BlockCount != count) { thrusts = Common.GetTs(Me, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && InThisEntity(thrust)); BlockCount = count; }
                if (NullThrust) return;
                if (DisableAll) { for (int index = 0; index < 6; index++) Enables[index] = false; return; }
                Enables[(int)Base6Directions.Direction.Up] = EnableAll || UpOrForward;
                Enables[(int)Base6Directions.Direction.Forward] = EnableAll || (!UpOrForward);
                Enables[(int)Base6Directions.Direction.Down] = EnableAll;
                Enables[(int)Base6Directions.Direction.Left] = EnableAll;
                Enables[(int)Base6Directions.Direction.Right] = EnableAll;
                Enables[(int)Base6Directions.Direction.Backward] = EnableAll;
                MaxSpeedLimit = MaximumSpeed;
                var velocity = (EnabledDampener ? (LinearVelocity - MaxSpeedLimit * Vector3.TransformNormal(MoveIndicate, Me.WorldMatrix)) : Vector3.Zero);
                var ReferValue = (velocity * Math.Max(1, Gravity.Length()) + ((Gravity == Vector3.Zero) ? Vector3.Zero : (Gravity * (1 + SealevelDiff) / GetMultipy()))) * ShipMass;
                _MiniValue = MiniValueC;
                Percentage6Direction(ReferValue, velocity);
                ApplyPercentage();
            }
            #region PrivateParameters
            public MyThrusterController() { }
            private float _MiniValue;
            private List<IMyThrust> thrusts;
            private IMyTerminalBlock Me;
            private int BlockCount = 0;
            private float[] Percentages { get; } = new float[6];
            private bool[] Enables { get; } = new bool[6];
            private bool NullThrust => Common.IsNullCollection(thrusts);
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private Vector3 Gravity { get { if (Me == null) return Vector3.Zero; return MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()); } }
            private float ShipMass => Me?.CubeGrid?.Physics?.Mass ?? 1;
            private float MiniValue { get { return _MiniValue; } set { _MiniValue = MathHelper.Clamp(value, 0, 1); } }
            private float GetMultipy()
            {
                if (Gravity == Vector3.Zero) return 1;
                var value = Math.Abs(Vector3.Normalize(Gravity).Dot(Me.WorldMatrix.Down));
                if (value == 0) return 1;
                return MathHelper.Clamp(1 / value, 1, 20f);
            }
            private void ApplyPercentage()
            {
                if (Common.IsNullCollection(thrusts)) return;
                if (Common.IsNull(Me))
                {
                    foreach (var thrust in thrusts)
                    {
                        if (Common.NullEntity(thrust)) continue;
                        thrust.ThrustOverridePercentage = 0;
                    }
                }
                else
                {
                    foreach (var thrust in thrusts)
                    {
                        if (Common.NullEntity(thrust)) continue;
                        int index = thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Forward) > DirectionGate ? (int)Base6Directions.Direction.Forward :
                            thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Backward) > DirectionGate ? (int)Base6Directions.Direction.Backward :
                            thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Up) > DirectionGate ? (int)Base6Directions.Direction.Up :
                            thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Down) > DirectionGate ? (int)Base6Directions.Direction.Down :
                            thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Left) > DirectionGate ? (int)Base6Directions.Direction.Left :
                            thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Right) > DirectionGate ? (int)Base6Directions.Direction.Right : 6;
                        if (index > 5) continue;
                        thrust.Enabled = Enables[index];
                        thrust.ThrustOverridePercentage = MathHelper.Clamp(Percentages[index], MiniValue, 1);
                    }
                }
            }
            private void Percentage6Direction(Vector3 Force, Vector3 OV)
            {
                if (Common.IsNull(Me) || Common.IsNullCollection(thrusts)) { for (int index = 0; index < 6; index++) { Enables[index] = true; Percentages[index] = 0; } return; }
                float[] TF = new float[6];
                foreach (var thrust in thrusts)
                {
                    if (Common.NullEntity(thrust)) continue;
                    if (!thrust.Enabled) continue;
                    int index = thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Forward) > DirectionGate ? (int)Base6Directions.Direction.Forward :
                        thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Backward) > DirectionGate ? (int)Base6Directions.Direction.Backward :
                        thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Up) > DirectionGate ? (int)Base6Directions.Direction.Up :
                        thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Down) > DirectionGate ? (int)Base6Directions.Direction.Down :
                        thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Left) > DirectionGate ? (int)Base6Directions.Direction.Left :
                        thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Right) > DirectionGate ? (int)Base6Directions.Direction.Right : 6;
                    if (index > 5) continue;
                    TF[index] += thrust.MaxEffectiveThrust;
                }
                for (int index = 0; index < 6; index++)
                {
                    Vector3D Vector = index == (int)Base6Directions.Direction.Forward ? Me.WorldMatrix.Backward :
                                    index == (int)Base6Directions.Direction.Backward ? Me.WorldMatrix.Forward :
                                    index == (int)Base6Directions.Direction.Up ? Me.WorldMatrix.Down :
                                    index == (int)Base6Directions.Direction.Down ? Me.WorldMatrix.Up :
                                    index == (int)Base6Directions.Direction.Left ? Me.WorldMatrix.Right :
                                    index == (int)Base6Directions.Direction.Right ? Me.WorldMatrix.Left : Vector3D.Zero;
                    Percentages[index] = MathHelper.Clamp((TF[index] != 0) ? (OV.Dot(Vector) > VelocityGate) ? 1 : (Force.Dot(Vector) / TF[index]) : 0, 0, 1);
                }
            }
            private const double DirectionGate = 0.95;
            private const float MiniValueC = 1e-6f;
            private const float VelocityGate = 50f;
            #endregion
        }
    }
}