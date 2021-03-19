using Sandbox.ModAPI;
using System;
using VRageMath;
namespace SuperBlocks.Controller
{
    using static Utils;
    public partial class UniversalController
    {
        private bool ForwardOrUp
        {
            get { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: return _ForwardOrUp; case ControllerRole.Aeroplane: return true; default: return false; } }
            set { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: _ForwardOrUp = value; break; default: _ForwardOrUp = false; break; } }
        }
        private Vector3 ThrustsControlLine
        {
            get
            {
                if (HandBrake) return Vector3.Zero;
                switch (Role)
                {
                    case ControllerRole.Aeroplane: return MoveIndication * Vector3.Backward;
                    case ControllerRole.Helicopter: return MoveIndication * Vector3.Up;
                    case ControllerRole.VTOL: return MoveIndication * (HandBrake ? Vector3.Zero : EnabledAllDirection ? Vector3.One : ForwardOrUp ? Vector3.Backward : Vector3.Up);
                    case ControllerRole.SpaceShip: return MoveIndication;
                    case ControllerRole.SeaShip: return MoveIndication * Vector3.Backward;
                    case ControllerRole.Submarine: return new Vector3(0, MoveIndication.Y, MoveIndication.Z);
                    case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return Vector3.Backward * MoveIndication;
                    default: return Vector3.Zero;
                }
            }
        }
        private Vector4 RotationCtrlLines
        {
            get
            {
                if (HandBrake) return Vector4.Zero;
                switch (Role)
                {
                    case ControllerRole.Aeroplane: case ControllerRole.SpaceShip: return new Vector4(0, 0, RotationIndication.X,RotationIndication.Y);
                    case ControllerRole.Helicopter: return new Vector4(MoveIndication.Z, MoveIndication.X, 0, RotationIndication.Z);
                    case ControllerRole.VTOL: return (HasWings && (!ForwardOrUp) && (Gravity != Vector3.Zero)) ? (new Vector4(MoveIndication.Z, MoveIndication.X, 0, RotationIndication.Z)) : new Vector4(0, 0, RotationIndication.X, RotationIndication.Y);
                    case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return new Vector4(0, 0, 0, MoveIndication.X);
                    default: return Vector4.Zero;
                }
            }
        }
        private bool KeepLevel
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.Helicopter: return MoveIndication.Y == 0;
                    case ControllerRole.VTOL: case ControllerRole.SpaceShip: return (!ForwardOrUp) && MoveIndication.Y == 0;
                    default: return false;
                }
            }
        }
        private bool IgnoreLevel
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.Aeroplane: return true;
                    case ControllerRole.Helicopter: return Gravity == Vector3.Zero;
                    case ControllerRole.VTOL: case ControllerRole.SpaceShip: return ForwardOrUp || Gravity == Vector3.Zero;
                    default: return false;
                }
            }
        }
        private bool DisabledRotation
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.VTOL:
                    case ControllerRole.SpaceShip:
                        return false;
                    case ControllerRole.Aeroplane:
                    case ControllerRole.Helicopter:
                    case ControllerRole.SeaShip:
                    case ControllerRole.Submarine:
                    case ControllerRole.TrackVehicle:
                    case ControllerRole.WheelVehicle:
                    case ControllerRole.HoverVehicle:
                        return Gravity == Vector3.Zero;
                    default: return true;
                }
            }
        }
        private bool EnabledAllDirection
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.Helicopter:
                        return HandBrake || Gravity == Vector3.Zero;
                    case ControllerRole.Aeroplane:
                    case ControllerRole.VTOL:
                        return HandBrake || Gravity == Vector3.Zero || (!HasWings);
                    default: return true;
                }
            }
        }
        private Vector3 InitAngularDampener
        {
            get
            {
                if (Role == ControllerRole.HoverVehicle || Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle)
                    return new Vector3(60, 70, 60);
                return new Vector3(70, 30, 10);
            }
        }
        private float MaximumSpeed
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.Aeroplane: return _MaxiumFlightSpeed;
                    case ControllerRole.Helicopter: return _MaxiumHoverSpeed;
                    case ControllerRole.VTOL: case ControllerRole.SpaceShip: return ForwardOrUp ? _MaxiumFlightSpeed : _MaxiumHoverSpeed;
                    case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return _MaxiumSpeed;
                    default: return 100;
                }
            }
            set
            {
                var thisvalue = Math.Max(1, value);
                switch (Role)
                {
                    case ControllerRole.Aeroplane: _MaxiumFlightSpeed = thisvalue; return;
                    case ControllerRole.Helicopter: _MaxiumHoverSpeed = thisvalue; return;
                    case ControllerRole.VTOL: case ControllerRole.SpaceShip: if (ForwardOrUp) _MaxiumFlightSpeed = thisvalue; else _MaxiumHoverSpeed = thisvalue; return;
                    case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: _MaxiumSpeed = thisvalue; return;
                    default: return;
                }
            }
        }
        private IMyShipController Controller { get { return _Controller; } set { _Controller = value; } }
        private Vector3 Gravity => MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition());
        private Vector3 LinearVelocity => Me.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
        private Vector3D Forward => Me.WorldMatrix.Forward;
        private Vector3 ProjectLinnerVelocity_CockpitForward { get { return PoseProcessFuncs.ProjectOnPlane(LinearVelocity, Forward); } }
        private bool Dampener => Override_Dampener ?? Controller?.DampenersOverride ?? true;
        private bool HandBrake => Override_HandBrake ?? Controller?.HandBrake ?? true;
        private Vector3 MoveIndication => Override_MoveIndication ?? Controller?.MoveIndicator ?? Vector3.Zero;
        private Vector3 RotationIndication => Override_RotationIndication ?? new Vector3(Controller?.RotationIndicator ?? Vector2.Zero, Controller?.RollIndicator ?? 0);
        private bool PoseMode { get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return HasWings ? (!ForwardOrUp) : (_EnabledCuriser && (Gravity != Vector3.Zero)); case ControllerRole.SpaceShip: return _EnabledCuriser && (Gravity != Vector3.Zero); default: return false; } } }
        private bool IgnoreForwardVelocity { get { switch (Role) { case ControllerRole.Helicopter: return false; case ControllerRole.VTOL: return ForwardOrUp || Gravity == Vector3.Zero; default: return true; } } }
        private bool Need2CtrlSignal { get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return !(ForwardOrUp || Gravity == Vector3.Zero); default: return false; } } }
        private bool Refer2Velocity { get { switch (Role) { case ControllerRole.Aeroplane: case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return (HasWings && (Gravity != Vector3.Zero)) || Refer2Velocity_SpaceShip; case ControllerRole.SpaceShip: return Refer2Velocity_SpaceShip; default: return false; } } }
        private bool Refer2Velocity_SpaceShip => (ProjectLinnerVelocity_CockpitForward.LengthSquared() >= _MaxiumHoverSpeed * _MaxiumHoverSpeed) && (Gravity == Vector3.Zero || ForwardOrUp);
    }
}
