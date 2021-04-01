using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks.Controller
{
    using static Utils;
    public partial class UniversalController
    {
        private volatile bool _Dock = false;
        private volatile bool _hasWings = false;
        private volatile bool _EnabledCuriser = false;
        private volatile float _SafetyStage = 1f;
        private volatile float _LocationSensetive = 1f;
        private volatile float _MaxReactions_AngleV = 1f;
        private volatile float _WheelPowerMult = 1f;
        private volatile float MultAttitude = 25f;
        private volatile float _MaxiumHoverSpeed = 30;
        private volatile float _MaxiumFlightSpeed = 1000;
        private volatile float _MaxiumSpeed = 27;
        private MyEventParameter_Bool _ForwardOrUp { get; } = new MyEventParameter_Bool(false);
        private MyEventParameter_ControlRole _Role { get; } = new MyEventParameter_ControlRole(ControllerRole.None);
        private MyEventParameter_Float Overclocked_Reactors { get; } = new MyEventParameter_Float(1);
        private MyEventParameter_Float Overclocked_Thrusts { get; } = new MyEventParameter_Float(1);
        private MyEventParameter_Float Overclocked_Gyros { get; } = new MyEventParameter_Float(1);
        private MyEventParameter_Float Overclocked_GasGenerators { get; } = new MyEventParameter_Float(1);
        private MyThrusterController ThrustControllerSystem { get; } = new MyThrusterController();
        private MyGyrosController GyroControllerSystem { get; } = new MyGyrosController();
        private MyWheelsController WheelsController { get; } = new MyWheelsController();
        private MyRotorThrustRotorCtrl RotorThrustRotorCtrl { get; } = new MyRotorThrustRotorCtrl();
        private MyAutoCloseDoorController AutoCloseDoorController { get; } = new MyAutoCloseDoorController();
        private Vector3 AngularDampeners = new Vector3(5, 7, 5);
        private Vector3 ForwardDirection;
        private IMyShipController _Controller;
        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;
        private double sealevel, _Target_Sealevel;
        private float target_speed = 0, diffsealevel = 0;
    }
    public partial class UniversalController
    {
        private bool ForwardOrUp
        {
            get { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: return _ForwardOrUp.Value; case ControllerRole.Aeroplane: return true; default: return false; } }
            set { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: _ForwardOrUp.Value = value; break; default: _ForwardOrUp.Value = false; break; } }
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
                    case ControllerRole.Aeroplane: case ControllerRole.SpaceShip: return new Vector4(0, 0, RotationIndication.X, RotationIndication.Y);
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
                if (_Dock) return 4;
                switch (Role)
                {
                    case ControllerRole.Aeroplane: return _MaxiumFlightSpeed;
                    case ControllerRole.Helicopter: return _MaxiumHoverSpeed;
                    case ControllerRole.VTOL: case ControllerRole.SpaceShip: return ForwardOrUp ? _MaxiumFlightSpeed : _MaxiumHoverSpeed;
                    case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: return MaximumCruiseSpeed;
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
                    case ControllerRole.SeaShip: case ControllerRole.Submarine: case ControllerRole.TrackVehicle: case ControllerRole.WheelVehicle: case ControllerRole.HoverVehicle: MaximumCruiseSpeed = thisvalue; return;
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
        private bool HandBrake
        {
            get
            {
                switch (Role)
                {
                    case ControllerRole.Aeroplane:
                    case ControllerRole.VTOL:
                    case ControllerRole.SpaceShip:
                        if (ForwardOrUp) return (Override_HandBrake ?? Controller?.HandBrake ?? true) || ((MoveIndication * Vector3.Backward).Dot(Vector3.Backward) > 0);
                        else return Override_HandBrake ?? Controller?.HandBrake ?? true;
                    default:
                        return Override_HandBrake ?? Controller?.HandBrake ?? true;
                }
            }
        }
        private Vector3 MoveIndication => Override_MoveIndication ?? Controller?.MoveIndicator ?? Vector3.Zero;
        private Vector3 RotationIndication => Override_RotationIndication ?? new Vector3(Controller?.RotationIndicator ?? Vector2.Zero, Controller?.RollIndicator ?? 0);
        private bool PoseMode { get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return HasWings ? (!ForwardOrUp) : (_EnabledCuriser && (Gravity != Vector3.Zero)); case ControllerRole.SpaceShip: return _EnabledCuriser && (Gravity != Vector3.Zero); default: return false; } } }
        private bool IgnoreForwardVelocity { get { switch (Role) { case ControllerRole.Helicopter: return false; case ControllerRole.VTOL: return ForwardOrUp || Gravity == Vector3.Zero; default: return true; } } }
        private bool Need2CtrlSignal { get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return !(ForwardOrUp || Gravity == Vector3.Zero); default: return false; } } }
        private bool Refer2Velocity { get { switch (Role) { case ControllerRole.Aeroplane: case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: return (HasWings && (Gravity != Vector3.Zero)) || Refer2Velocity_SpaceShip; case ControllerRole.SpaceShip: return Refer2Velocity_SpaceShip; default: return false; } } }
        private bool Refer2Velocity_SpaceShip => (ProjectLinnerVelocity_CockpitForward.LengthSquared() >= _MaxiumHoverSpeed * _MaxiumHoverSpeed) && (Gravity == Vector3.Zero || ForwardOrUp);
        protected override void UpdateState()
        {
            Color CurrentColor = Enabled.Value ? (Role != ControllerRole.None ? Color.Cyan : Color.Green) : Color.Black;
            float e = Enabled.Value ? 16 : 0;
            try { Me.SetEmissiveParts("Emissive", CurrentColor, e); } catch (Exception) { }
        }
    }
    public partial class UniversalController
    {
        public Vector3? Override_MoveIndication { get; set; } = null;
        public Vector3? Override_RotationIndication { get; set; } = null;
        public bool? Override_HandBrake { get; set; } = null;
        public bool? Override_Dampener { get; set; } = null;
        public bool HoverMode
        {
            get { switch (Role) { case ControllerRole.Helicopter: return true; case ControllerRole.VTOL: case ControllerRole.SpaceShip: return !ForwardOrUp; default: return false; } }
            set { switch (Role) { case ControllerRole.VTOL: case ControllerRole.SpaceShip: ForwardOrUp = !value; if (!ForwardOrUp) { diffsealevel = (float)(_Target_Sealevel - sealevel) * MultAttitude; } else target_speed = LinearVelocity.Length(); break; default: ForwardOrUp = true; break; } }
        }
        public float MaxiumFlightSpeed { get { return _MaxiumFlightSpeed; } set { _MaxiumFlightSpeed = MathHelper.Clamp(value, 30, float.MaxValue); } }
        public float MaxiumHoverSpeed { get { return _MaxiumHoverSpeed; } set { _MaxiumHoverSpeed = MathHelper.Clamp(value, 5, 100); } }
        public float MaximumCruiseSpeed { get { return _MaxiumSpeed; } set { _MaxiumSpeed = MathHelper.Clamp(Math.Abs(value), 0, 360f); } }
        public ControllerRole Role { get { return _Role.Value; } set { _Role.Value = value; } }
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
        public bool HasWings { get { return _hasWings; } set { _hasWings = value; } }
    }
}
