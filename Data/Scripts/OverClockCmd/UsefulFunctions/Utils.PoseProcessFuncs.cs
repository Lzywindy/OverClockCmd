using Sandbox.ModAPI;
using System;
using VRageMath;

namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class PoseProcessFuncs
        {
            public static double? GetSealevel(IMyShipController Controller) { double value; if (IsNull(Controller) || (!Controller.TryGetPlanetElevation(Sandbox.ModAPI.Ingame.MyPlanetElevation.Sealevel, out value))) return null; return value; }
            public static Vector3? ProcessRotation(bool _EnabledCuriser, IMyShipController ShipController, Vector4 RotationCtrlLines, ref Vector3 ForwardDirection, Vector3? InitAngularDampener = null, Vector3? AngularDampeners = null, bool ForwardOrUp = false, bool PoseMode = false, float MaximumSpeedLimited = 100f, float MaxReactions_AngleV = 1f, bool Need2CtrlSignal = true, float LocationSensetive = 1f, float SafetyStage = 1f, bool IgnoreForwardVelocity = true, bool Refer2Velocity = true, bool DisabledRotation = true, Vector3? ForwardDirectionOverride = null, Vector3? PlaneNormalOverride = null)
            {
                if (IsNull(ShipController) || DisabledRotation) return null;
                Vector3? current_gravity = ShipController?.GetNaturalGravity();
                Vector3? ReferNormal;
                if (PlaneNormalOverride.HasValue && PlaneNormalOverride.Value != Vector3.Zero)
                {
                    ReferNormal = PlaneNormalOverride;
                }
                else
                {
                    Vector3? current_velocity_linear = Refer2Velocity ? ((Vector3?)(ProjectLinnerVelocity_CockpitForward(ShipController, Refer2Velocity, IgnoreForwardVelocity)
                      - ((Need2CtrlSignal ? (Vector3.ClampToSphere((-ShipController.WorldMatrix.Forward * RotationCtrlLines.X + ShipController.WorldMatrix.Right * RotationCtrlLines.Y), 1) * MaximumSpeedLimited) : Vector3.Zero)))) : null;
                    if (!current_gravity.HasValue)
                        ReferNormal = current_velocity_linear;
                    else if (!current_velocity_linear.HasValue)
                        ReferNormal = current_gravity;
                    else
                        ReferNormal = Vector3.ClampToSphere(current_velocity_linear.Value * LocationSensetive + Dampener(current_gravity.Value) * SafetyStage, 1f);
                }
                if (IsNull(ReferNormal)) { return null; }
                Vector3 Direciton;
                if (!IsNull(ForwardDirectionOverride))
                {
                    Direciton = ForwardDirectionOverride.Value + RotationCtrlLines.W * ShipController.WorldMatrix.Right - RotationCtrlLines.Z * ShipController.WorldMatrix.Up;
                }
                else
                {
                    if (RotationCtrlLines.W != 0 || RotationCtrlLines.Z != 0)
                        ForwardDirection = ShipController.WorldMatrix.Forward;
                    if (_EnabledCuriser && ForwardOrUp && (current_gravity != null))
                    {
                        ForwardDirection = ProjectOnPlane(ForwardDirection, current_gravity.Value);
                        if (ForwardDirection == Vector3.Zero)
                            ForwardDirection = ProjectOnPlane(ShipController.WorldMatrix.Down, current_gravity.Value);
                    }
                    if (ForwardDirection != Vector3.Zero)
                        ForwardDirection = ScaleVectorTimes(Vector3.Normalize(ForwardDirection));
                    Direciton = ForwardDirection + RotationCtrlLines.W * ShipController.WorldMatrix.Right - RotationCtrlLines.Z * ShipController.WorldMatrix.Up;
                }
                return (ProcessDampeners(ShipController, InitAngularDampener, AngularDampeners) + (new Vector3(
                    Dampener(PoseMode && (ReferNormal.Value != Vector3.Zero) ? Calc_Direction_Vector(ReferNormal.Value, ShipController.WorldMatrix.Backward) : Calc_Direction_Vector(Direciton, ShipController.WorldMatrix.Down)),
                    Dampener(SetupAngle(Calc_Direction_Vector(Direciton, ShipController.WorldMatrix.Right), Calc_Direction_Vector(Direciton, ShipController.WorldMatrix.Forward))),
                    (ReferNormal.Value != Vector3.Zero) ? Dampener(SetupAngle(Calc_Direction_Vector(ReferNormal.Value, ShipController.WorldMatrix.Left), Calc_Direction_Vector(ReferNormal.Value, ShipController.WorldMatrix.Down))) : 0
                    ) * MaxReactions_AngleV));
            }
            public static float SetupAngle(float current_angular_local, float current_angular_add) { if (Math.Abs(current_angular_local) < 0.005f && current_angular_add < 0f) return current_angular_add; return current_angular_local; }
            public static float Calc_Direction_Vector(Vector3 vector, Vector3 direction) => Vector3.Normalize(direction).Dot(vector);
            public static Vector3 ScaleVectorTimes(Vector3 vector, float Times = 10f) => vector * Times;
            public static Vector3 ProjectLinnerVelocity_CockpitForward(IMyShipController ShipController, bool EnableToGet = true, bool IgnoreForwardVelocity = false) { if (ShipController == null) return Vector3.Zero; var LinearVelocity = EnableToGet ? ShipController.GetShipVelocities().LinearVelocity : Vector3D.Zero; if (IgnoreForwardVelocity) return ProjectOnPlane(LinearVelocity, ShipController.WorldMatrix.Forward); else return LinearVelocity; }
            public static Vector3 ProcessDampeners(IMyShipController ShipController, Vector3? InitAngularDampener = null, Vector3? AngularDampeners = null)
            {
                if (ShipController == null) return Vector3.Zero;
                var temp = Vector3.TransformNormal(ShipController.GetShipVelocities().AngularVelocity, Matrix.Transpose(ShipController.WorldMatrix));
                var a_temp = Vector3.Abs(temp);
                var _InitAngularDampener = InitAngularDampener ?? (new Vector3(70, 50, 10));
                return Vector3.Clamp(a_temp * temp * _InitAngularDampener / 4, -_InitAngularDampener, _InitAngularDampener) * (AngularDampeners ?? Vector3.One);
            }
            public static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal) => Vector3.ProjectOnPlane(ref direction, ref planeNormal);
            public static float Dampener(float value) => value * Math.Abs(value);
            public static Vector3 Dampener(Vector3 value) => value * Math.Abs(value.Length());
        }
    }
}
