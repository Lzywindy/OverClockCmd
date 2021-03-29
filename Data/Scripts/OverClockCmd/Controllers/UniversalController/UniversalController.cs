using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;
using VRageMath;
namespace SuperBlocks.Controller
{
    using static Utils;
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "UniversalController", "SmallUniversalController")]
    public partial class UniversalController : MyGridProgram4ISConvert
    {
        protected override void Program()
        {
            try { InitDatas(); } catch (Exception) { StartReady = false; }
            UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency.Update1 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update10 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update100;
        }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource)
        {
            try
            {
                if (Me.CubeGrid.IsStatic) Role = ControllerRole.None;
                //Echo($"[Controller Mode  :{Role}]");
                //Echo($"[Hover Mode       :{HoverMode}]");
                //Echo($"[Enabled Curiser  :{EnabledCuriser}]");
                //Echo($"[Enabled Thrusters:{EnabledThrusters}]");
                //Echo($"[Enabled Gyros    :{EnabledGyros}]");
                //Echo($"[Has Wings        :{HasWings}]");
                if (UniversalControllerManage.IsMainController(Me))
                {
                    switch (updateSource)
                    {
                        case Sandbox.ModAPI.Ingame.UpdateType.IGC:
                        case Sandbox.ModAPI.Ingame.UpdateType.Mod:
                        case Sandbox.ModAPI.Ingame.UpdateType.Script:
                        case Sandbox.ModAPI.Ingame.UpdateType.Terminal:
                        case Sandbox.ModAPI.Ingame.UpdateType.Trigger:
                            if (argument.Contains("HoverMode")) HoverMode = !HoverMode;
                            if (argument.Contains("HasWings")) HasWings = !HasWings;
                            if (argument.Contains("EnabledCuriser")) EnabledCuriser = !EnabledCuriser;
                            if (argument.Contains("EnabledThrusters")) EnabledThrusters = !EnabledThrusters;
                            if (argument.Contains("EnabledGyros")) EnabledGyros = !EnabledGyros;
                            if (argument.Contains("LoadConfig")) ReadDatas();
                            if (argument.Contains("SaveConfig")) WriteDatas();
                            break;
                        case Sandbox.ModAPI.Ingame.UpdateType.Update1:
                            if (Common.NullEntity(Controller))
                                Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
                            PoseCtrl();
                            ThrustControl();
                            WheelControl();
                            UpdateState();
                            RotorThrustRotorCtrl.Running(Me, InThisEntity, HoverMode, RotationIndication.Z, MaximumSpeed, MoveIndication);
                            AutoCloseDoorController.Running(GridTerminalSystem); break;
                        case Sandbox.ModAPI.Ingame.UpdateType.Update10: if (!StartReady) InitDatas(); break;
                        case Sandbox.ModAPI.Ingame.UpdateType.Update100: break;
                        case Sandbox.ModAPI.Ingame.UpdateType.Once: break;
                        default: break;
                    }
                }
                else
                {
                    switch (updateSource)
                    {
                        case Sandbox.ModAPI.Ingame.UpdateType.Update10: Me.CustomData = UniversalControllerManage.GetRegistControllerBlockConfig(Me); InitDatas(); break;
                        default: break;
                    }
                }
            }
            catch (Exception) { StartReady = false; }
        }
        protected override void ClosedBlock() { UniversalControllerManage.UnRegistControllerBlock(Me); }
        protected override void CustomDataChangedProcess() { ReadDatas(); }
        public Vector3? Override_MoveIndication { get; set; } = null;
        public Vector3? Override_RotationIndication { get; set; } = null;
        public bool? Override_HandBrake { get; set; } = null;
        public bool? Override_Dampener { get; set; } = null;
        #region InternalFunctions
        private void InitDatas()
        {
            Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
            ThrustControllerSystem.ForceUpdate(Me, InThisEntity);
            GyroControllerSystem.ForceUpdate(Me, InThisEntity);
            WheelsController.ForceUpdate(Me, InThisEntity);
            AutoCloseDoorController.UpdateBlocks(GridTerminalSystem);
            ReadDatas();
            if (Common.NullEntity(Controller)) { StartReady = false; return; }
            OnModeChange();
            StartReady = true;
        }
        private void OnModeChange() { _Target_Sealevel = sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0; }
        private void WheelControl()
        {
            bool NoWheelCtrl = !(Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle);
            WheelsController.TrackVehicle = Role == ControllerRole.TrackVehicle;
            WheelsController.RetractWheels = Dock;
            WheelsController.MaximumSpeed = MaximumSpeed;
            WheelsController.Running(Me, InThisEntity, Enabled ? NoWheelCtrl ? 0 : ThrustsControlLine.Z : 0, Enabled ? NoWheelCtrl ? 0 : RotationCtrlLines.W : 0, _WheelPowerMult);
        }
        private void UpdateTargetSealevel() { if (IgnoreLevel) diffsealevel = 0; else { sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0; if (!KeepLevel) _Target_Sealevel = sealevel; diffsealevel = (float)(_Target_Sealevel - sealevel) * MultAttitude; } }
        private static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0f, 20f);
        #endregion
        #region Gyros&Thrusts
        private void PoseCtrl()
        {
            if (!Enabled) { GyroControllerSystem.GyrosOverride(Me, InThisEntity, null); return; }
            Vector3? Rotation;
            if (Role == ControllerRole.HoverVehicle || Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle)
            {
                Rotation = PoseProcessFuncs.ProcessRotation_GroundVehicle(Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, MaxReactions_AngleV, DisabledRotation, ForwardDirectionOverride);
                GyroControllerSystem.SetEnabled(EnabledGyros && Rotation.HasValue);
            }
            else if (Role == ControllerRole.SeaShip || Role == ControllerRole.Submarine)
            {
                Rotation = PoseProcessFuncs.ProcessRotation_SeaVehicle(Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, MaxReactions_AngleV, DisabledRotation, ForwardDirectionOverride);
                GyroControllerSystem.SetEnabled(EnabledGyros);
            }
            else
            {
                Rotation = PoseProcessFuncs.ProcessRotation(_EnabledCuriser, Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, ForwardOrUp, PoseMode, MaximumSpeed, MaxReactions_AngleV, Need2CtrlSignal, LocationSensetive, SafetyStage, IgnoreForwardVelocity, Refer2Velocity, DisabledRotation, ForwardDirectionOverride, PlaneNormalOverride);
                GyroControllerSystem.SetEnabled(EnabledGyros);
            }
            GyroControllerSystem.GyrosOverride(Me, InThisEntity, Rotation ?? RotationIndication);
        }
        private void ThrustControl()
        {
            if (!Enabled) { ThrustControllerSystem.RunningDefault(Me, InThisEntity); return; }
            if (Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle || Role == ControllerRole.HoverVehicle)
                ThrustControllerSystem.Running(Me, InThisEntity, ThrustsControlLine, false, true, (!EnabledThrusters), MaximumSpeed, 0, true);
            else
            {
                Vector3 Ctrl = ThrustsControlLine;
                bool CtrlOrCruise = HoverMode || (Ctrl != Vector3.Zero);
                UpdateTargetSealevel();
                target_speed = MathHelper.Clamp(HandBrake ? 0 : (Ctrl != Vector3.Zero) ? ForwardOrUp ? LinearVelocity.Dot(Forward) : 0 : target_speed, 0, MaximumSpeed);
                ThrustControllerSystem.Running(Me, InThisEntity, CtrlOrCruise ? Ctrl : Vector3.Forward, (!ForwardOrUp), EnabledAllDirection, (!EnabledThrusters), CtrlOrCruise ? MaximumSpeed : target_speed, diffsealevel, Dampener);
            }
        }
        #endregion
        #region ConfigRW
        private void ReadDatas()
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
            bool EnabledDefault = false;
            if (!ControllerConfig_Read()) { ControllerConfig_Write(); EnabledDefault = true; }
            if (!ControllerOverclocked_Read()) { ControllerOverclocked_Write(); EnabledDefault = true; }
            if (EnabledDefault) Me.CustomData = MyConfigs.CustomDataConfigSave_INI(Configs);
        }
        private void WriteDatas()
        {
            ControllerConfig_Write();
            ControllerOverclocked_Write();
            Me.CustomData = MyConfigs.CustomDataConfigSave_INI(Configs);
        }
        private void InitParameters() { HasWings = true; if (Me.CubeGrid.IsStatic) Role = ControllerRole.None; else Role = ControllerRole.VTOL; AngularDampeners_Roll = 5; AngularDampeners_Pitch = 5; AngularDampeners_Yaw = 7; SafetyStage = 3; MaxReactions_AngleV = 40; LocationSensetive = 10; EnabledCuriser = false; HoverMode = true; MaxiumFlightSpeed = 1000; MaxiumHoverSpeed = 30; MaximumCruiseSpeed = 80; }
        private bool ControllerConfig_Read()
        {
            if (Common.IsNullCollection(Configs) || !Configs.ContainsKey(VehicleControllerConfigID)) return false;
            var data = Configs[VehicleControllerConfigID];
            foreach (var configitem in data)
            {

                switch (configitem.Key)
                {
                    case "Enabled": Enabled = MyConfigs.ParseBool(configitem.Value); break;
                    case "HasWings": HasWings = MyConfigs.ParseBool(configitem.Value); break;
                    case "EnabledCuriser": EnabledCuriser = MyConfigs.ParseBool(configitem.Value); break;
                    case "Dock": Dock = MyConfigs.ParseBool(configitem.Value); break;
                    case "HoverMode": HoverMode = MyConfigs.ParseBool(configitem.Value); break;
                    case "AngularDampeners_Roll": AngularDampeners_Roll = MyConfigs.ParseFloat(configitem.Value); break;
                    case "AngularDampeners_Pitch": AngularDampeners_Pitch = MyConfigs.ParseFloat(configitem.Value); break;
                    case "AngularDampeners_Yaw": AngularDampeners_Yaw = MyConfigs.ParseFloat(configitem.Value); break;
                    case "SafetyStage": SafetyStage = MyConfigs.ParseFloat(configitem.Value); break;
                    case "MaxReactions_AngleV": MaxReactions_AngleV = MyConfigs.ParseFloat(configitem.Value); break;
                    case "LocationSensetive": LocationSensetive = MyConfigs.ParseFloat(configitem.Value); break;
                    case "MaxiumFlightSpeed": MaxiumFlightSpeed = MyConfigs.ParseFloat(configitem.Value); break;
                    case "MaxiumHoverSpeed": MaxiumHoverSpeed = MyConfigs.ParseFloat(configitem.Value); break;
                    case "MaximumCruiseSpeed": WheelsController.MaximumSpeed = MaximumCruiseSpeed = MyConfigs.ParseFloat(configitem.Value); break;
                    case "Role": Role = (ControllerRole)Enum.Parse(typeof(ControllerRole), configitem.Value); break;
                    case "MultAttitude": MultAttitude = MyConfigs.ParseFloat(configitem.Value); break;
                    case "WheelPowerMult": _WheelPowerMult = MathHelper.Clamp(MyConfigs.ParseFloat(configitem.Value), 0, 1); break;
                    default: break;
                }
            }
            return true;
        }
        private void ControllerConfig_Write()
        {
            if (Common.IsNullCollection(Configs) || !Configs.ContainsKey(VehicleControllerConfigID)) { Configs.Add(VehicleControllerConfigID, new Dictionary<string, string>()); InitParameters(); }
            var data = Configs[VehicleControllerConfigID];
            MyConfigs.ModifyProperty(data, "Enabled", Enabled.ToString());
            MyConfigs.ModifyProperty(data, "Dock", Dock.ToString());
            MyConfigs.ModifyProperty(data, "HasWings", HasWings.ToString());
            MyConfigs.ModifyProperty(data, "EnabledCuriser", EnabledCuriser.ToString());
            MyConfigs.ModifyProperty(data, "HoverMode", HoverMode.ToString());
            MyConfigs.ModifyProperty(data, "AngularDampeners_Roll", AngularDampeners_Roll.ToString());
            MyConfigs.ModifyProperty(data, "AngularDampeners_Pitch", AngularDampeners_Pitch.ToString());
            MyConfigs.ModifyProperty(data, "AngularDampeners_Yaw", AngularDampeners_Yaw.ToString());
            MyConfigs.ModifyProperty(data, "SafetyStage", SafetyStage.ToString());
            MyConfigs.ModifyProperty(data, "MaxReactions_AngleV", MaxReactions_AngleV.ToString());
            MyConfigs.ModifyProperty(data, "LocationSensetive", LocationSensetive.ToString());
            MyConfigs.ModifyProperty(data, "MaxiumFlightSpeed", MaxiumFlightSpeed.ToString());
            MyConfigs.ModifyProperty(data, "MaxiumHoverSpeed", MaxiumHoverSpeed.ToString());
            MyConfigs.ModifyProperty(data, "MaximumCruiseSpeed", MaximumCruiseSpeed.ToString());
            MyConfigs.ModifyProperty(data, "Role", Role.ToString());
            MyConfigs.ModifyProperty(data, "MultAttitude", MultAttitude.ToString());
            MyConfigs.ModifyProperty(data, "WheelPowerMult", _WheelPowerMult.ToString());
        }
        #endregion
        #region OverclockConfigs
        private bool ControllerOverclocked_Read()
        {
            if (Common.IsNullCollection(Configs) || !Configs.ContainsKey(OverclockedID)) return false;
            var data = Configs[OverclockedID];
            foreach (var configitem in data)
            {
                switch (configitem.Key)
                {
                    case "RE": Overclocked_Reactors = MyConfigs.ParseFloat(configitem.Value); break;
                    case "TR": Overclocked_Thrusts = MyConfigs.ParseFloat(configitem.Value); break;
                    case "GY": Overclocked_Gyros = MyConfigs.ParseFloat(configitem.Value); break;
                    case "GG": Overclocked_GasGenerators = MyConfigs.ParseFloat(configitem.Value); break;
                    default: break;
                }
            }
            ThrustControllerSystem.SetOverclocked(Overclocked_Thrusts);
            GyroControllerSystem.SetOverclocked(Overclocked_Gyros);
            var Reactors = Common.GetTs<IMyReactor>(GridTerminalSystem);
            if (!Common.IsNullCollection(Reactors)) { foreach (var Reactor in Reactors) Reactor.PowerOutputMultiplier = Overclocked_Reactors; }
            var gasGenerators = Common.GetTs<IMyGasGenerator>(GridTerminalSystem);
            if (!Common.IsNullCollection(gasGenerators)) { foreach (var gasGenerator in gasGenerators) { gasGenerator.PowerConsumptionMultiplier = Overclocked_GasGenerators; gasGenerator.ProductionCapacityMultiplier = Overclocked_GasGenerators; } }
            return true;
        }
        private void ControllerOverclocked_Write()
        {
            if (Common.IsNullCollection(Configs) || !Configs.ContainsKey(OverclockedID)) { Configs.Add(OverclockedID, new Dictionary<string, string>()); Overclocked_Reactors = 1; Overclocked_Thrusts = 1; Overclocked_Gyros = 1; Overclocked_GasGenerators = 1; }
            var data = Configs[OverclockedID];
            MyConfigs.ModifyProperty(data, "RE", Overclocked_Reactors.ToString());
            MyConfigs.ModifyProperty(data, "TR", Overclocked_Thrusts.ToString());
            MyConfigs.ModifyProperty(data, "GY", Overclocked_Gyros.ToString());
            MyConfigs.ModifyProperty(data, "GG", Overclocked_GasGenerators.ToString());
        }
        private float Overclocked_Reactors = 1;
        private float Overclocked_Thrusts = 1;
        private float Overclocked_Gyros = 1;
        private float Overclocked_GasGenerators = 1;
        #endregion
    }
}