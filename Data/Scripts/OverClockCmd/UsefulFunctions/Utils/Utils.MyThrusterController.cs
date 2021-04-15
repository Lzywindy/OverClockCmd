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
            public void SetOverclocked(float mult = 1)
            {
                if (NullThrust) return;
                foreach (var thrust in thrusts) { if (thrust.BlockDefinition.SubtypeId.Contains("HoverEngine")) continue; if (thrust.BlockDefinition.SubtypeId.Contains("Hover")) continue; if (thrust.BlockDefinition.SubtypeId.Contains("Hover Engine")) continue; thrust.PowerConsumptionMultiplier = mult; thrust.ThrustMultiplier = mult; }
            }
            public void ForceUpdate(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me) || InThisEntity == null) return;
                thrusts = Common.GetTs(Me, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && InThisEntity(thrust));
            }
            public void RunningDefault(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity)
            {
                this.Me = Me;
                if (Common.IsNull(this.Me) || InThisEntity == null) return;
                if (NullThrust) return;
                foreach (var thrust in thrusts)
                {
                    if (Common.NullEntity(thrust)) continue;
                    if (thrust.ThrustOverridePercentage != 0) thrust.ThrustOverridePercentage = 0;
                }
                foreach (var thrust in thrusts)
                {
                    if (!thrust.Enabled) thrust.Enabled = true;
                }
            }
            public void Running(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, Vector3 MoveIndicate, bool UpOrForward, bool EnableAll, bool DisableAll, float MaximumSpeed, float SealevelDiff = 0, bool EnabledDampener = true)
            {
                this.Me = Me;
                if (Common.IsNull(this.Me) || InThisEntity == null) return;
                if (NullThrust) return;
                if (DisableAll)
                {
                    foreach (var thrust in thrusts)
                    {
                        if (DisableAll)
                        {
                            if (thrust.Enabled) thrust.Enabled = false;
                            if (thrust.ThrustOverridePercentage != 0)
                                thrust.ThrustOverridePercentage = 0;
                        }
                    }
                }
                else
                {
                    var velocity = (EnabledDampener ? (LinearVelocity - MaximumSpeed * Vector3.TransformNormal(MoveIndicate, Me.WorldMatrix)) : Vector3.Zero);
                    var ReferValue = (velocity * Math.Max(1, Gravity.Length()) + ((Gravity == Vector3.Zero) ? Vector3.Zero : (Gravity * (1 + SealevelDiff) / GetMultipy()))) * ShipMass;
                    Dictionary<IMyThrust, float> Statistic = new Dictionary<IMyThrust, float>();
                    foreach (var thrust in thrusts)
                    {
                        if (Common.NullEntity(thrust)) continue;
                        bool thrustenabled = EnableAll;
                        if (thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Up) > DirectionGate_Me)
                            thrustenabled = thrustenabled || UpOrForward;
                        else if(thrust.WorldMatrix.Backward.Dot(Me.WorldMatrix.Forward) > DirectionGate_Me)
                            thrustenabled = thrustenabled || !UpOrForward;
                        if (thrust.Enabled != thrustenabled) thrust.Enabled = thrustenabled;
                        if (!thrust.Enabled) { thrust.ThrustOverridePercentage = 0; continue; }
                        float value = ReferValue.Dot(thrust.WorldMatrix.Forward);
                        if (value <= 0 || MathHelper.IsZero(value, 1e-5f)) { thrust.ThrustOverridePercentage = MiniValueC; continue; }
                        Statistic.Add(thrust, value / thrust.MaxEffectiveThrust);
                    }
                    if (Common.IsNullCollection(Statistic)) return;
                    foreach (var item in Statistic)
                        item.Key.ThrustOverridePercentage = item.Value / Statistic.Count;
                }
            }

            #region PrivateParameters
            public MyThrusterController() { }
            private List<IMyThrust> thrusts;
            private IMyTerminalBlock Me;
            private bool NullThrust => Common.IsNullCollection(thrusts);
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private Vector3 Gravity { get { if (Me == null) return Vector3.Zero; return MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()); } }
            private float ShipMass => Me?.CubeGrid?.Physics?.Mass ?? 1;
            private float GetMultipy()
            {
                if (Gravity == Vector3.Zero) return 1;
                var value = Math.Abs(Vector3.Normalize(Gravity).Dot(Me.WorldMatrix.Down));
                if (value == 0) return 1;
                return MathHelper.Clamp(1 / value, 1, 20f);
            }
            private const float MiniValueC = 1e-6f;
            private const double DirectionGate_Me = 0.875f;
            #endregion
        }
    }
}