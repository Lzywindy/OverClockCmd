using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyWheelsController
        {
            public void Running(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, float ForwardIndicator, float TurnIndicator)
            {
                this.Me = Me;
                if (Common.IsNull(Me) || InThisEntity == null) return;
                LoadIndicateLights(InThisEntity);
                LoadLandingGears(InThisEntity);
                LoadConnect(InThisEntity);
                LoadPistons(InThisEntity);
                if (Wheels == null && !HoverDevices)
                {
                    Wheels = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid).GetBlockGroupWithName(WheelsGroupNM);
                    Motors_Hover = Common.GetTs(Me, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM));
                }
                if (Wheels == null) return;
                var count = Common.GetTs(Wheels, InThisEntity).Count;
                if (BlockCount != count || NullWheels)
                {
                    SWheels = Common.GetTs<IMyMotorSuspension>(Wheels, InThisEntity);
                    MWheels = Common.GetTs<IMyMotorStator>(Wheels, InThisEntity);
                    BlockCount = count;
                }
                this.ForwardIndicator = ForwardIndicator;
                this.TurnIndicator = TurnIndicator;
                LoadSuspends(InThisEntity);
                LoadMotorWheels(InThisEntity);
            }
            #region PublicControllerLines
            public bool DockComplete => (LandingGears?.Any(b => b.IsLocked) ?? false) || (ShipConnectors?.Any(b => b.Status == Sandbox.ModAPI.Ingame.MyShipConnectorStatus.Connected) ?? false);
            public bool RetractWheels { get; set; } = false;
            public bool TrackVehicle { get; set; } = true;
            public float MaxiumRpm { get; set; } = 90f;
            public float DiffRpmPercentage { get; set; } = 1f;
            public float Friction { get; set; } = 100f;
            public float TurnFaction { get; set; } = 25f;
            public float MaximumSpeed { get; set; } = 20f;
            private float ForwardIndicator { get; set; } = 0;
            private float TurnIndicator { get; set; } = 0;
            #endregion
            #region PrivateFields&Functions
            public MyWheelsController() { }
            private List<IMyMotorSuspension> SWheels;
            private List<IMyMotorStator> MWheels;
            private List<IMyTerminalBlock> Motors_Hover;
            private IMyBlockGroup Wheels;
            private IMyTerminalBlock Me;
            private List<IMyInteriorLight> brakelights;
            private List<IMyInteriorLight> backlights;
            private List<IMyLandingGear> LandingGears;
            private List<IMyShipConnector> ShipConnectors;
            private List<IMyPistonBase> Pistons;
            private int BlockCount = 0;
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private bool NullWheels => Common.IsNull(Me) || (NullSWheel && NullMWheel);
            private bool NullSWheel => Common.IsNullCollection(SWheels);
            private bool NullMWheel => Common.IsNullCollection(MWheels);
            private bool HoverDevices => !Common.IsNullCollection(Motors_Hover);
            private void LoadIndicateLights(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me)) return;
                if (Common.IsNullCollection(brakelights)) brakelights = Common.GetTs(Me, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BrakeNM) && InThisEntity(lightblock));
                foreach (var item in brakelights) { item.Enabled = ForwardIndicator == 0; }
                if (Common.IsNullCollection(backlights)) backlights = Common.GetTs(Me, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BackwardNM) && InThisEntity(lightblock));
                foreach (var item in backlights) { item.Enabled = ForwardIndicator > 0; }
            }
            private void LoadSuspends(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me) || HoverDevices) return;
                if (NullSWheel) SWheels = Common.GetTs<IMyMotorSuspension>(Wheels, InThisEntity);
                if (NullSWheel) return;
                foreach (var Wheel in SWheels)
                {
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Wheel.WorldMatrix.Up));
                    bool EnTrO = (TrackVehicle || (LinearVelocity.LengthSquared() < 4f));
                    float PropulsionOverride = EnTrO ? DiffTurns(sign) : 0;
                    Wheel.Brake = PropulsionOverride == 0;
                    Wheel.InvertSteer = false;
                    Wheel.SetValue<float>(Wheel.GetProperty(MotorOverrideId).Id, Math.Sign(PropulsionOverride));
                    Wheel.Power = Math.Abs(PropulsionOverride);
                    Wheel.Steering = !TrackVehicle;
                    Wheel.Friction = MathHelper.Clamp((TurnIndicator != 0) ? (TrackVehicle ? (TurnFaction / Vector3.DistanceSquared(Wheel.GetPosition(), Me.CubeGrid.GetPosition())) : Friction) : Friction, 0, Friction);
                    if (Wheel.Steering && EnTrO && TurnIndicator != 0)
                        Wheel.SetValue<float>(Wheel.GetProperty(SteerOverrideId).Id, Math.Sign(Me.WorldMatrix.Left.Dot(Wheel.WorldMatrix.Up)) * (Wheel.CustomName.Contains("Rear") ? -1 : 1));
                    else
                        Wheel.SetValue<float>(Wheel.GetProperty(SteerOverrideId).Id, 0);
                    Wheel.Height = RetractWheels ? 100 : -100;
                    Wheel.Enabled = !RetractWheels;
                    Wheel.Brake = RetractWheels;
                }
            }
            private void LoadMotorWheels(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me) || HoverDevices) return;
                if (NullMWheel) MWheels = Common.GetTs<IMyMotorStator>(Wheels, InThisEntity);
                if (NullMWheel) return;
                foreach (var Motor in MWheels)
                {
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                    Motor.TargetVelocityRPM = RetractWheels ? 0 : (-DiffTurns(sign) * MaxiumRpm);
                    Motor.RotorLock = RetractWheels;
                }
            }
            private void LoadLandingGears(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me)) return;
                if (Common.IsNullCollection(LandingGears)) LandingGears = Common.GetTs<IMyLandingGear>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                if (Common.IsNullCollection(LandingGears)) return;
                foreach (var LandingGear in LandingGears)
                {
                    if (RetractWheels)
                    {
                        if (!LandingGear.IsLocked) LandingGear.Lock();
                        LandingGear.AutoLock = true;
                    }
                    else
                    {
                        LandingGear.Unlock();
                        LandingGear.AutoLock = false;
                    }

                }
            }
            private void LoadConnect(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me)) return;
                if (Common.IsNullCollection(ShipConnectors)) ShipConnectors = Common.GetTs<IMyShipConnector>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                if (Common.IsNullCollection(ShipConnectors)) return;
                foreach (var ShipConnector in ShipConnectors)
                {
                    if (RetractWheels)
                        ShipConnector.Connect();
                    else
                        ShipConnector.Disconnect();
                    ShipConnector.PullStrength = 1;
                }
            }
            private void LoadPistons(Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me)) return;
                if (Common.IsNullCollection(Pistons)) Pistons = Common.GetTs<IMyPistonBase>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                if (Common.IsNullCollection(Pistons)) return;
                foreach (var Piston in Pistons)
                {
                    if (RetractWheels)
                        Piston.Velocity = 1;
                    else
                        Piston.Velocity = -1;
                }
            }
            private float DiffTurns(int sign) { Vector2 Indicator = new Vector2(Math.Max(Math.Sign(MaximumSpeed - LinearVelocity.Length()), 0) * ForwardIndicator * sign, TurnIndicator * DiffRpmPercentage); if (Indicator != Vector2.Zero) Indicator = Vector2.Normalize(Indicator); return Vector2.Dot(Vector2.One, Indicator); }
            #endregion
        }
    }
}