using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
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
        private bool Dampener => Override_Dampener ?? Controller?.DampenersOverride ?? true;
        private bool HandBrake => (Override_HandBrake ?? Controller?.HandBrake ?? true);
        private Vector3 MoveIndication => Override_MoveIndication ?? Controller?.MoveIndicator ?? Vector3.Zero;
        private Vector3 RotationIndication => Override_RotationIndication ?? new Vector3(Controller?.RotationIndicator ?? Vector2.Zero, Controller?.RollIndicator ?? 0);
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
        public float LocationSensetive { get { return _LocationSensetive; } set { _LocationSensetive = MathHelper.Clamp(value, 0, 1); } }
        public float MaxReactions_AngleV { get { return _MaxReactions_AngleV; } set { _MaxReactions_AngleV = MathHelper.Clamp(value, 1f, 90f); } }
        public float SafetyStage { get { return _SafetyStage; } set { _SafetyStage = MathHelper.Clamp(value, 0, 1); } }
        public bool EnabledThrusters { get; set; } = true;
        public bool EnabledGyros { get; set; } = true;
        public float AngularDampeners_Roll { get { AngularDampeners.Z = SetInRange_AngularDampeners(AngularDampeners.Z); return AngularDampeners.Z; } set { AngularDampeners.Z = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Yaw { get { AngularDampeners.Y = SetInRange_AngularDampeners(AngularDampeners.Y); return AngularDampeners.Y; } set { AngularDampeners.Y = SetInRange_AngularDampeners(value); } }
        public float AngularDampeners_Pitch { get { AngularDampeners.X = SetInRange_AngularDampeners(AngularDampeners.X); return AngularDampeners.X; } set { AngularDampeners.X = SetInRange_AngularDampeners(value); } }
        public bool HasWings { get { return _hasWings; } set { _hasWings = value; } }
    }
    public partial class UniversalController
    {
        private const float RunningPower_NoWarp = 1e-3f;
        private bool _WarpEnabled_Signal = false;
        MyResourceSinkInfo ResourceSinkInfo;
        private bool Get_WarpEnabled() => _WarpEnabled_Signal && GetEnabled_Warp();
        private float RunningPower_Warp() => RunningPower_NoWarp + InnerGyrosController.CurrentPowerRequire + (Get_WarpEnabled() ? ((Me.CubeGrid?.Physics?.Mass ?? 0) * (Math.Max(MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()).Length(), 1) + WarpAcc) * (float)(Me.CubeGrid?.WorldAABB.Volume ?? 0)) : 0);

        private bool GetEnabled_Warp() { return !Common.NullEntity(Common.GetT<IMyJumpDrive>(Me)); }
        private bool Enabled_CubeGridProtected => Common.GetTs<IMyJumpDrive>(Me, b => b.BlockDefinition.SubtypeId == "FastChargeJumpDrive" || b.BlockDefinition.SubtypeId == "LongRangeJumpDrive").Count > 10;

        private float WarpAcc = 1;
        MyWarpThrusterController WarpThrusterController { get; } = new MyWarpThrusterController();
        MyInnerGyrosController InnerGyrosController { get; } = new MyInnerGyrosController();
    }
    public class MyWarpThrusterController
    {
        public void Running(IMyTerminalBlock Me, Vector3 MoveIndicate, bool EngineEnabled, float Acc, float MaxTargetSpeed, float MaxSpeedLimit, bool DockModel = false, bool HoverModel = false, float SealevelDiff = 0)
        {
            if (Common.IsNull(Me)) return;
            var Grid = Me.CubeGrid;
            if (Common.NullEntity(Grid) || Grid.Physics == null || !EngineEnabled) return;
            var ShipMass = Grid?.Physics?.Mass ?? 0;
            var LinearVelocity = Grid?.Physics?.LinearVelocity ?? Vector3.Zero;
            var Gravity = Grid?.Physics?.Gravity ?? Vector3.Zero;
            var speeddiff = MaxTargetSpeed - LinearVelocity.Length();
            var AccModify = DockModel ? 0.5 : HoverModel ? 5 : MathHelper.IsZero(speeddiff * 0.05f, 1e-2f) ? 5 : Acc;
            //Vector3D Velocity = Vector3D.Zero;
            Vector3D Velocity = Vector3D.ClampToSphere((MaxTargetSpeed * Vector3.TransformNormal(MoveIndicate, Me.WorldMatrix) - LinearVelocity) * 1.5f, 2f) * AccModify;
            var ReferValue = (Gravity == Vector3.Zero) ? Vector3.Zero : (Gravity * (1 + SealevelDiff) / GetMultipy(Me, Gravity));
            var TargetForce = (Velocity - ReferValue) * ShipMass;
            Grid?.Physics?.AddForce(MyPhysicsForceType.APPLY_WORLD_FORCE, TargetForce, Grid?.Physics?.CenterOfMassWorld, null, MaxSpeedLimit);
        }
        private static float GetMultipy(IMyTerminalBlock Me, Vector3 Gravity)
        {
            if (Gravity == Vector3.Zero) return 1;
            var value = Math.Abs(Vector3.Normalize(Gravity).Dot(Me.WorldMatrix.Down));
            if (value == 0) return 1;
            return MathHelper.Clamp(1 / value, 1, 20f);
        }
    }
    public class MyInnerGyrosController
    {
        public float CurrentPowerRequire { get; private set; }
        public void GyrosOverride(IMyTerminalBlock Me, Vector3? RotationIndicate, float MaxReactions_AngleV, float LocationSensetive = 1f)
        {
            if (Common.IsNull(Me)) return;
            var Grid = Me.CubeGrid;
            if (Common.NullEntity(Grid) || Grid.Physics == null) return;
            var _AngularVelocity = Vector3.TransformNormal(Grid?.Physics?.AngularVelocity ?? Vector3.Zero, MatrixD.Transpose(Me.WorldMatrix));
            var _RotationIndicate = RotationIndicate ?? Vector3.Zero;
            var ShipMass = Grid?.Physics?.Mass ?? 0;
            var value = MathHelper.ToRadians(MathHelper.Clamp(MaxReactions_AngleV, 1, 90));
            Grid.Physics.AngularVelocity = (Vector3.IsZero(_RotationIndicate) && Vector3.IsZero(Grid.Physics.AngularVelocity, 1e-2f)) ? Vector3.Zero : Vector3.ClampToSphere(Grid.Physics.AngularVelocity, MathHelper.PiOver2);
            var LocalV = Vector3D.TransformNormal(Vector3D.ClampToSphere(-(_RotationIndicate * value + _AngularVelocity * 10f), 25) * MaxReactions_AngleV * GridTorqueMultipy(Me) * ShipMass * LocationSensetive, Me.LocalMatrix);
            CurrentPowerRequire = (float)LocalV.Length() * 1e-9f;
            Grid?.Physics?.AddForce(MyPhysicsForceType.ADD_BODY_FORCE_AND_BODY_TORQUE, null, Grid?.Physics?.CenterOfMassLocal, LocalV, null);
            //Grid.Physics.AngularVelocity = Vector3D.TransformNormal(Vector3.Clamp(-_RotationIndicate * MaxReactions_AngleV, Min, Max), Me.WorldMatrix);
        }
    }
}
