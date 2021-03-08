using System;
using VRageMath;

namespace SuperBlocks.Controller
{
    public partial class UniversalController
    {
        public bool HasWings { get; set; } = false;
        public bool HoverMode
        {
            get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: case ControllerRole.SpaceShip: return !ForwardOrUp; default: return false; } }
            set { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: ForwardOrUp = !value; if (!ForwardOrUp) { OnModeChange(); diffsealevel = (float)(_Target_Sealevel - sealevel) * MultAttitude; } else target_speed = LinearVelocity.Length(); break; default: ForwardOrUp = true; break; } }
        }
        public float MaxiumFlightSpeed { get { return _MaxiumFlightSpeed; } set { _MaxiumFlightSpeed = MathHelper.Clamp(value, 30, float.MaxValue); } }
        public float MaxiumHoverSpeed { get { return _MaxiumHoverSpeed; } set { _MaxiumHoverSpeed = MathHelper.Clamp(value, 5, 100); } }
        public float MaximumCruiseSpeed { get { return _MaxiumSpeed * 3.6f; } set { _MaxiumSpeed = MathHelper.Clamp(Math.Abs(value / 3.6f), -360f, 360f); } }
        public bool EnabledCuriser
        {
            get { switch (Role) { case ControllerRole.Aeroplane: case ControllerRole.VTOL: case ControllerRole.SpaceShip: return _EnabledCuriser; case ControllerRole.SeaShip: case ControllerRole.Submarine: return true; default: return false; } }
            set { switch (Role) { case ControllerRole.Aeroplane: case ControllerRole.VTOL: case ControllerRole.SpaceShip: _EnabledCuriser = value; break; default: break; } }
        }
        public Vector3? ForwardDirectionOverride { get; set; } = null;
        public Vector3? PlaneNormalOverride { get; set; } = null;
        public float LocationSensetive
        {
            get { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return 1; default: return _LocationSensetive; } }
            set { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: _LocationSensetive = 1; break; default: _LocationSensetive = MathHelper.Clamp(value, 1f, 10f); break; } }
        }
        public float MaxReactions_AngleV
        {
            get { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return 1; default: return _MaxReactions_AngleV; } }
            set { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: _MaxReactions_AngleV = 1; break; default: _MaxReactions_AngleV = MathHelper.Clamp(value, 1f, 90f); break; } }
        }
        public float SafetyStage
        {
            get { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return 1; default: return _SafetyStage; } }
            set { switch (Role) { case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: _SafetyStage = 1; break; default: _SafetyStage = MathHelper.Clamp(value, SafetyStageMin, SafetyStageMax); break; } }
        }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float AngularDampeners_Roll { get { AngularDampeners.Z = SetInRange_AngularDampeners(AngularDampeners.Z); return AngularDampeners.Z; } set { AngularDampeners.Z = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Yaw { get { AngularDampeners.Y = SetInRange_AngularDampeners(AngularDampeners.Y); return AngularDampeners.Y; } set { AngularDampeners.Y = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Pitch { get { AngularDampeners.X = SetInRange_AngularDampeners(AngularDampeners.X); return AngularDampeners.X; } set { AngularDampeners.X = SetInRange_AngularDampeners(value); } }
    }
}
