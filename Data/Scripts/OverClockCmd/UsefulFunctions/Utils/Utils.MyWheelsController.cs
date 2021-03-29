using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using SuperBlocks.Controller;
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
            public void ForceUpdate(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me) || InThisEntity == null) return;
                Wheels = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid).GetBlockGroupWithName(WheelsGroupNM);
                Motors_Hover = Common.GetTs(Me, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM));
                SWheels = Common.GetTs<IMyMotorSuspension>(Wheels, InThisEntity);
                MWheels = Common.GetTs<IMyMotorStator>(Wheels, InThisEntity);
                Pistons = Common.GetTs<IMyPistonBase>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                ShipConnectors = Common.GetTs<IMyShipConnector>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                LandingGears = Common.GetTs<IMyLandingGear>(Me, p => InThisEntity(p) && p.CustomName.Contains("UCR"));
                brakelights = Common.GetTs(Me, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BrakeNM) && InThisEntity(lightblock));
                backlights = Common.GetTs(Me, (IMyInteriorLight lightblock) => lightblock.CustomName.Contains(BackwardNM) && InThisEntity(lightblock));
            }
            public void Running(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, float ForwardIndicator, float TurnIndicator, float PowerMult, bool Brake)
            {
                this.Me = Me;
                if (Common.IsNull(Me) || InThisEntity == null) return;
                this.ForwardIndicator = ForwardIndicator;
                this.TurnIndicator = TurnIndicator;
                this.PowerMult = PowerMult;
                LoadIndicateLights();
                LoadLandingGears();
                LoadConnect();
                LoadPistons();
                if (Wheels == null && !HoverDevices)
                {
                    Wheels = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid).GetBlockGroupWithName(WheelsGroupNM);
                    Motors_Hover = Common.GetTs(Me, (IMyTerminalBlock thrust) => thrust.BlockDefinition.SubtypeId.Contains(HoverEngineNM));
                }
                if (Wheels == null) return;
                if (NullWheels)
                {
                    SWheels = Common.GetTs<IMyMotorSuspension>(Wheels, InThisEntity);
                    MWheels = Common.GetTs<IMyMotorStator>(Wheels, InThisEntity);
                }
                LoadSuspends(Brake);
                LoadMotorWheels(Brake);
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
            private float PowerMult { get; set; } = 1;
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
            private Vector3 LinearVelocity => Me?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
            private bool NullWheels => Common.IsNull(Me) || (NullSWheel && NullMWheel);
            private bool NullSWheel => Common.IsNullCollection(SWheels);
            private bool NullMWheel => Common.IsNullCollection(MWheels);
            private bool HoverDevices => !Common.IsNullCollection(Motors_Hover);
            private void LoadIndicateLights()
            {
                if (Common.IsNull(Me)) return;
                if (!Common.IsNullCollection(brakelights))
                    foreach (var item in brakelights) { item.Enabled = ForwardIndicator == 0; }
                if (!Common.IsNullCollection(backlights))
                    foreach (var item in backlights) { item.Enabled = ForwardIndicator > 0; }
            }
            private void LoadSuspends(bool Brake)
            {
                if (Common.IsNull(Me) || HoverDevices) return;
                if (NullSWheel) return;
                foreach (var Wheel in SWheels)
                {
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Wheel.WorldMatrix.Up));
                    bool EnTrO = (TrackVehicle || (LinearVelocity.LengthSquared() < 4f));
                    float PropulsionOverride = Brake ? 0 : EnTrO ? DiffTurns(sign) : Direct(sign);
                    Wheel.Brake = PropulsionOverride == 0 || Brake;
                    Wheel.InvertSteer = false;
                    Wheel.SetValue(Wheel.GetProperty(MotorOverrideId).Id, MathHelper.Clamp(PropulsionOverride, -PowerMult, PowerMult));
                    Wheel.Power = Math.Min(Math.Abs(PropulsionOverride), PowerMult);
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
            private void LoadMotorWheels(bool Brake)
            {
                if (Common.IsNull(Me) || HoverDevices) return;
                if (NullMWheel) return;
                foreach (var Motor in MWheels)
                {
                    var sign = Math.Sign(Me.WorldMatrix.Right.Dot(Motor.WorldMatrix.Up));
                    Motor.TargetVelocityRPM = (RetractWheels || Brake) ? 0 : (-DiffTurns(sign) * MaxiumRpm * PowerMult);
                    Motor.RotorLock = RetractWheels;
                }
            }
            private void LoadLandingGears()
            {
                if (Common.IsNull(Me)) return;
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
            private void LoadConnect()
            {
                if (Common.IsNull(Me)) return;
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
            private void LoadPistons()
            {
                if (Common.IsNull(Me)) return;
                if (Common.IsNullCollection(Pistons)) return;
                foreach (var Piston in Pistons)
                {
                    if (RetractWheels)
                        Piston.Velocity = 1;
                    else
                        Piston.Velocity = -1;
                }
            }
            private float DiffTurns(int sign)
            {
                var indicate = (float)(LinearVelocity - MaximumSpeed * Me.WorldMatrix.Forward * (-ForwardIndicator)).Dot(Me.WorldMatrix.Forward) * sign / 100;
                return Vector2.Dot(Vector2.One, new Vector2(indicate, TurnIndicator * DiffRpmPercentage));
            }
            private float Direct(int sign)
            {
                return (float)(LinearVelocity - MaximumSpeed * Me.WorldMatrix.Forward * (-ForwardIndicator)).Dot(Me.WorldMatrix.Forward) * sign / 100;
            }
            #endregion

            private const float SmallGridHightMax = 2.5f;
            private const float LargeGridHightMax = 12.5f;
            public Vector3D? GetCurrentPlaneNormal()
            {
                if (!HoverDevices || NullWheels) return null;
                var gravity = MyPlanetInfoAPI.GetCurrentGravity(Me.CubeGrid.WorldAABB.Center);
                var centerheight = MyPlanetInfoAPI.GetSurfaceHight(Me.CubeGrid.WorldAABB.Center);
                if (Vector3D.IsZero(gravity) || !centerheight.HasValue) return null;
                var corners = Me.CubeGrid.WorldAABB.GetCorners();
                if (Common.IsNullCollection(corners)) return null;
                var corners_height = corners.ToList().ConvertAll(vertex => MyPlanetInfoAPI.GetSurfaceHight(vertex) ?? 0);
                var min_corners_height = corners_height.Min();
                if ((Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Small && min_corners_height > SmallGridHightMax) || (Me.CubeGrid.GridSizeEnum == VRage.Game.MyCubeSize.Large && min_corners_height > LargeGridHightMax)) return null;
                var vectors = corners.ToList().ConvertAll(vertex => (vertex - Me.CubeGrid.WorldAABB.Center) / MathHelper.Clamp(MyPlanetInfoAPI.GetSurfaceHight(vertex) ?? 0, 0.005f, 20f));
                Vector3D vector = new Vector3D(vectors.Average(t => t.X), vectors.Average(t => t.Y), vectors.Average(t => t.Z)) + gravity * 3;
                if (Vector3D.IsZero(vector)) return null;
                return Vector3D.Normalize(vector);
            }
        }
    }
}