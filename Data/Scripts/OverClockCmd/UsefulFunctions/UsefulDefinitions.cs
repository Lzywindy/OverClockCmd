﻿using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage;
using VRageMath;
namespace SuperBlocks
{
    public struct EventVariable<T> : IEquatable<T>
    {
        public T Value { get { return data; } set { if (data.Equals(value)) return; data = value; DoVariableChanged?.Invoke(); } }
        private T data;
        public Action DoVariableChanged;
        public bool Equals(T other) { return Value.Equals(other); }
    }
    public static partial class Utils
    {
        public class MyWheelsController
        {
            public MyWheelsController() { }
            public void UpdateBlocks(IMyGridTerminalSystem GridTerminalSystem, IMyTerminalBlock Me)
            {
                this.Me = null;
                if (Common.IsNull(Me) || Common.IsNull(GridTerminalSystem)) return;
                Motors_Hover = Common.GetTs(GridTerminalSystem, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM));
                var Group = GridTerminalSystem.GetBlockGroupWithName(WheelsGroupNM);
                SWheels = Common.GetTs<IMyMotorSuspension>(Group);
                MWheels = Common.GetTs<IMyMotorStator>(Group);
                this.Me = Me;
                if (Common.IsNullCollection(Motors_Hover)) HoverDevices = false;
                else { HoverDevices = true; return; }
                if (NullWheels) return;
                Wheels = Init4GetAction(GridTerminalSystem);
            }
            internal ControllerRole ControlMode => NullWheels ? ControllerRole.None : HoverDevices ? ControllerRole.HoverVehicle : TrackVehicle ? ControllerRole.TrackVehicle : ControllerRole.WheelVehicle;
            public void Running()
            {
                if (NullWheels) return;
                Wheels?.Invoke();
            }
            private List<IMyMotorSuspension> SWheels;
            private List<IMyMotorStator> MWheels;
            private List<IMyTerminalBlock> Motors_Hover;
            private IMyTerminalBlock Me;
            public bool TrackVehicle { get; set; } = true;
            public float MaxiumRpm { get; set; } = 90f;
            public float DiffRpmPercentage { get; set; } = 1f;
            public float Friction { get; set; } = 100f;
            public float TurnFaction { get; set; } = 25f;
            public float MaximumSpeed { get; set; } = 20f;
            public float ForwardIndicator { get; set; }
            public float TurnIndicator { get; set; }
            public bool NullWheels => Common.IsNull(Me) || (NullSWheel && NullMWheel);
            public bool NullSWheel => Common.IsNullCollection(SWheels);
            public bool NullMWheel => Common.IsNullCollection(MWheels);
            public bool HoverDevices { get; private set; } = false;
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private Action Init4GetAction(IMyGridTerminalSystem GridTerminalSystem)
            {
                Action Wheels = () => { };
                if (HoverDevices) { HoverDevices = true; return Wheels + LoadIndicateLights(GridTerminalSystem); }
                bool CanRunning = false;
                {
                    var action_wheels = LoadSuspends();
                    if (action_wheels != null)
                        Wheels += action_wheels;
                    CanRunning = CanRunning || (action_wheels != null);
                }
                {
                    var action_wheels = LoadMotorWheels();
                    if (action_wheels != null)
                        Wheels += action_wheels;
                    CanRunning = CanRunning || (action_wheels != null);
                }
                if (!CanRunning) return Wheels;
                return (Wheels + LoadIndicateLights(GridTerminalSystem));
            }
            private Action LoadIndicateLights(IMyGridTerminalSystem GridTerminalSystem)
            {
                Action UtilsCtrl = () => { };
                var brakelights = Common.GetTs(GridTerminalSystem, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BrakeNM));
                foreach (var item in brakelights) { UtilsCtrl += () => item.Enabled = ForwardIndicator == 0; }
                var backlights = Common.GetTs(GridTerminalSystem, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BackwardNM));
                foreach (var item in backlights) { UtilsCtrl += () => item.Enabled = ForwardIndicator > 0; }
                return UtilsCtrl;
            }
            private Action LoadSuspends()
            {
                if (Common.IsNull(Me) || Common.IsNullCollection(SWheels)) return null;
                Action Wheels = () => { };
                foreach (var Motor in SWheels)
                {
                    Wheels += () =>
                    {
                        var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                        bool EnTrO = (TrackVehicle || (LinearVelocity.LengthSquared() < 4f));
                        float PropulsionOverride = (EnTrO ? DiffTurns(sign) : 0) + (ForwardIndicator * sign);
                        Motor.Brake = PropulsionOverride == 0;
                        Motor.InvertSteer = false;
                        Motor.SetValue<float>(Motor.GetProperty(MotorOverrideId).Id, Math.Sign(PropulsionOverride));
                        Motor.Power = Math.Abs(PropulsionOverride);
                        Motor.Steering = !TrackVehicle;
                        Motor.Friction = MathHelper.Clamp((TurnIndicator != 0) ? (TrackVehicle ? (TurnFaction / Vector3.DistanceSquared(Motor.GetPosition(), Me.CubeGrid.GetPosition())) : Friction) : Friction, 0, Friction);
                        if (Motor.Steering && EnTrO && TurnIndicator != 0)
                            Motor.SetValue<float>(Motor.GetProperty(SteerOverrideId).Id, Math.Sign(Me.WorldMatrix.Left.Dot(Motor.WorldMatrix.Up)) * (Motor.CustomName.Contains("Rear") ? -1 : 1));
                        else
                            Motor.SetValue<float>(Motor.GetProperty(SteerOverrideId).Id, 0);
                    };
                }
                return Wheels;
            }
            private Action LoadMotorWheels()
            {
                Action Wheels = () => { };
                if (Common.IsNull(Me) || Common.IsNullCollection(MWheels)) return Wheels;
                foreach (var Motor in MWheels)
                {
                    Wheels += () =>
                    {
                        var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                        Motor.TargetVelocityRPM = -DiffTurns(sign) * MaxiumRpm;
                    };
                }
                return Wheels;
            }
            private float DiffTurns(int sign)
            {
                Vector2 Indicator = new Vector2(Math.Max(Math.Sign(MaximumSpeed - LinearVelocity.Length()), 0) * ForwardIndicator * sign, TurnIndicator * DiffRpmPercentage);
                if (Indicator != Vector2.Zero)
                    Indicator = Vector2.Normalize(Indicator);
                return Vector2.Dot(Vector2.One, Indicator);
            }
            private Action Wheels = () => { };
        }
        public class MyThrusterController
        {
            public float MaxSpeedLimit { get; set; } = 1000f;
            public float MiniValue { get { return _MiniValue; } set { _MiniValue = MathHelper.Clamp(value, 0, 1); } }
            public MyThrusterController() { }
            public void UpdateBlocks(IMyGridTerminalSystem GridTerminalSystem, IMyTerminalBlock Me)
            {
                if (Common.IsNull(Me) || Common.IsNull(GridTerminalSystem)) return;
                this.Me = Me;
                thrusts = Common.GetTs(GridTerminalSystem, (IMyThrust thrust) => Common.ExceptKeywords(thrust) && thrust.CubeGrid == this.Me.CubeGrid);
                if (Common.IsNullCollection(thrusts)) return;
                _MiniValue = MiniValueC;
                StatisticU = (ref float force) => { }; StatisticD = (ref float force) => { }; StatisticL = (ref float force) => { };
                StatisticR = (ref float force) => { }; StatisticF = (ref float force) => { }; StatisticB = (ref float force) => { };
                ApplyPercentageU = (float percentage) => { }; ApplyPercentageD = (float percentage) => { }; ApplyPercentageL = (float percentage) => { };
                ApplyPercentageR = (float percentage) => { }; ApplyPercentageF = (float percentage) => { }; ApplyPercentageB = (float percentage) => { };
                EnabledU = (bool enabled) => { }; EnabledD = (bool enabled) => { }; EnabledL = (bool enabled) => { };
                EnabledR = (bool enabled) => { }; EnabledF = (bool enabled) => { }; EnabledB = (bool enabled) => { };
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
                if (NullThrust) return;
                if (DisableAll) { EnabledU(false); EnabledF(false); EnabledD(false); EnabledL(false); EnabledR(false); EnabledB(false); return; }
                EnabledU(EnableAll || UpOrForward); EnabledF(EnableAll || (!UpOrForward)); EnabledD(EnableAll); EnabledL(EnableAll); EnabledR(EnableAll); EnabledB(EnableAll); MaxSpeedLimit = MaximumSpeed;
                MiniValue = DisableAll ? 0 : MiniValueC;
            }
            public void Running(Vector3 MoveIndicate, float SealevelDiff = 0, bool EnabledDampener = true)
            {
                if (NullThrust) return;
                if (Me == null) { foreach (var thrust in thrusts) thrust.ThrustOverridePercentage = 0; return; }
                var velocity = (EnabledDampener ? (LinearVelocity - MaxSpeedLimit * Vector3.TransformNormal(MoveIndicate, Me.WorldMatrix)) : Vector3.Zero);
                var ReferValue = (velocity * Math.Max(1, Gravity.Length()) + ((Gravity == Vector3.Zero) ? Vector3.Zero : (Gravity * (1 + SealevelDiff) / GetMultipy()))) * ShipMass;
                Percentage6Direction(ReferValue, velocity);
            }
            private float GetMultipy()
            {
                if (Gravity == Vector3.Zero) return 1;
                var value = Math.Abs(Vector3.Normalize(Gravity).Dot(Me.WorldMatrix.Down));
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
            public void SetOverclocked(float mult = 1)
            {
                if (NullThrust) return;
                foreach (var thrust in thrusts)
                {
                    if (thrust.BlockDefinition.SubtypeId.Contains("HoverEngine")) continue;
                    if (thrust.BlockDefinition.SubtypeId.Contains("Hover")) continue;
                    if (thrust.BlockDefinition.SubtypeId.Contains("Hover Engine")) continue;
                    thrust.PowerConsumptionMultiplier = mult;
                    thrust.ThrustMultiplier = mult;
                }
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
            private bool NullThrust { get { return Common.IsNullCollection(thrusts); } }
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private Vector3 Gravity { get { if (Me == null) return Vector3.Zero; return MyPlanetInfoAPI.GetCurrentGravity(Me.GetPosition()); } }
            private float ShipMass => Me?.CubeGrid?.Physics?.Mass ?? 1;
            private const double DirectionGate = 0.95;
            private const float MiniValueC = 1e-6f;
            private const float VelocityGate = 50f;
            private float _MiniValue;
            private List<IMyThrust> thrusts;
            private IMyTerminalBlock Me;
            #region DoingActions
            private MyActionRef<float> StatisticU;
            private MyActionRef<float> StatisticD;
            private MyActionRef<float> StatisticL;
            private MyActionRef<float> StatisticR;
            private MyActionRef<float> StatisticF;
            private MyActionRef<float> StatisticB;
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
        public class MyGyrosController
        {
            public Vector3 PowerScale3Axis { get; set; } = Vector3.One;
            public MyGyrosController() { }
            public void UpdateBlocks(IMyGridTerminalSystem GridTerminalSystem, IMyTerminalBlock Me)
            {
                if (Common.IsNull(Me) || Common.IsNull(GridTerminalSystem)) return;
                this.Me = Me;
                gyros = Common.GetTs(GridTerminalSystem, (IMyGyro gyro) => Common.ExceptKeywords(gyro) && gyro.CubeGrid == Me.CubeGrid);
                if (Common.IsNullCollection(gyros)) return;
            }
            public void GyrosOverride(Vector3? RotationIndicate)
            {
                if (Common.IsNullCollection(gyros)) return;
                foreach (var gyro in gyros)
                {
                    gyro.GyroOverride = RotationIndicate.HasValue && (Me != null);
                    if (Me == null) gyro.Roll = gyro.Yaw = gyro.Pitch = 0;
                }
                if (!RotationIndicate.HasValue) return;
                Matrix matrix_Main = Common.GetWorldMatrix(Me);
                foreach (var gyro in gyros)
                {
                    var result = Vector3.TransformNormal(RotationIndicate.Value * PowerScale3Axis, matrix_Main * Matrix.Transpose(Common.GetWorldMatrix(gyro)));
                    gyro.Roll = result.Z; gyro.Yaw = result.Y; gyro.Pitch = result.X;
                }
            }
            public void SetPowerPercentage(float power)
            {
                if (gyros == null || gyros.Count < 1) return;
                foreach (var gyro in gyros)
                    gyro.GyroPower = power;
            }
            public void SetEnabled(bool Enabled)
            {
                if (gyros == null || gyros.Count < 1) return;
                foreach (var gyro in gyros)
                    gyro.Enabled = Enabled;
            }
            public void SetOverride(bool Enabled)
            {
                if (gyros == null || gyros.Count < 1) return;
                foreach (var gyro in gyros)
                    gyro.GyroOverride = Enabled;
            }
            public void SetOverclocked(float mult = 1)
            {
                if (Common.IsNullCollection(gyros)) return;
                foreach (var gyro in gyros)
                {
                    gyro.PowerConsumptionMultiplier = mult;
                    gyro.GyroStrengthMultiplier = mult;
                }
            }
            private List<IMyGyro> gyros;
            private IMyTerminalBlock Me;
        }
        public class MyAutoCloseDoorController
        {
            private List<MyAutoCloseDoorTimer> Timers { get; } = new List<MyAutoCloseDoorTimer>();
            public void UpdateBlocks(IMyGridTerminalSystem GridTerminalSystem) { var doors_group = GridTerminalSystem.GetBlockGroupWithName(ACDoorsGroupNM); if (doors_group == null) return; var doors = Common.GetTs<IMyDoor>(doors_group); foreach (var door in doors) { Timers.Add(new MyAutoCloseDoorTimer(door)); } }
            public void Running(IMyGridTerminalSystem GridTerminalSystem) { try { if (Timers.Count == 0) UpdateBlocks(GridTerminalSystem); else { foreach (var Timer in Timers) { Timer.Running(); } } } catch (Exception) { Timers.Clear(); } }
        }
        public class MyAutoCloseDoorTimer
        {
            public MyAutoCloseDoorTimer(IMyDoor Door) { this.Door = Door; }
            public void Running()
            {
                if (Door == null) return;
                switch (Door.Status)
                {
                    case Sandbox.ModAPI.Ingame.DoorStatus.Opening: Count = Gap; return;
                    case Sandbox.ModAPI.Ingame.DoorStatus.Open: if (Count > 0) Count--; else Door.CloseDoor(); return;
                    default: break;
                }
            }
            private readonly IMyDoor Door;
            private const int Gap = 25;
            private int Count;
        }
        #region ConstValues
        public const string HoverEngineNM = "Hover";
        public const string WheelsGroupNM = @"Wheels";
        public const string ACDoorsGroupNM = @"ACDoors";
        public const string BrakeNM = @"Brake";
        public const string BackwardNM = @"Backward";
        public const string MotorOverrideId = @"Propulsion override";
        public const string SteerOverrideId = @"Steer override";
        public const string VehicleControllerConfigID = @"VehicleController";
        public const string OverclockedID = @"Overclocked";
        #endregion
    }
    public delegate void MyActionRef<T>(ref T value);
    public enum ControllerRole : long { None, Aeroplane, Helicopter, VTOL, SpaceShip, SeaShip, Submarine, TrackVehicle, WheelVehicle, HoverVehicle }
    public struct Direction6Values
    {
        public float Forward;
        public float Backward;
        public float Left;
        public float Right;
        public float Up;
        public float Down;
        public Direction6Values(float Forward, float Backward, float Left, float Right, float Up, float Down)
        {
            this.Forward = Forward; this.Backward = Backward; this.Left = Left;
            this.Right = Right; this.Up = Up; this.Down = Down;
        }
        public Direction6Values(MyTuple<float, float, float, float, float, float> Values)
        {
            Forward = Values.Item1; Backward = Values.Item2; Left = Values.Item3;
            Right = Values.Item4; Up = Values.Item5; Down = Values.Item6;
        }
        public Direction6Values(float[] Values)
        {
            Forward = Backward = Left = Right = Up = Down = 0;
            if (Utils.Common.IsNullCollection(Values)) return;
            for (int index = 0; index < Values.Length; index++)
            {
                switch (index)
                {
                    case 0: Forward = Values[index]; break;
                    case 1: Backward = Values[index]; break;
                    case 2: Left = Values[index]; break;
                    case 3: Right = Values[index]; break;
                    case 4: Up = Values[index]; break;
                    case 5: Down = Values[index]; break;
                    default: break;
                }
            }
        }
        public static Direction6Values operator +(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward + b.Forward,
                Backward = a.Backward + b.Backward,
                Left = a.Left + b.Left,
                Right = a.Right + b.Right,
                Up = a.Up + b.Up,
                Down = a.Down + b.Down
            };
        }
        public static Direction6Values operator -(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward - b.Forward,
                Backward = a.Backward - b.Backward,
                Left = a.Left - b.Left,
                Right = a.Right - b.Right,
                Up = a.Up - b.Up,
                Down = a.Down - b.Down
            };
        }
        public static Direction6Values operator *(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward * b.Forward,
                Backward = a.Backward * b.Backward,
                Left = a.Left * b.Left,
                Right = a.Right * b.Right,
                Up = a.Up * b.Up,
                Down = a.Down * b.Down
            };
        }
        public static Direction6Values operator /(Direction6Values a, Direction6Values b)
        {
            return new Direction6Values()
            {
                Forward = a.Forward / b.Forward,
                Backward = a.Backward / b.Backward,
                Left = a.Left / b.Left,
                Right = a.Right / b.Right,
                Up = a.Up / b.Up,
                Down = a.Down / b.Down
            };
        }
    }
}