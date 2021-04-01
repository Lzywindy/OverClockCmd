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
        protected override void Program() { UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency.Update1 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update10 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update100; }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource) { }
        protected override void CustomDataChangedProcess() { LoadData(Me); }
        private void WheelControl()
        {
            bool NoWheelCtrl = !(Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle);
            WheelsController.TrackVehicle = Role == ControllerRole.TrackVehicle;
            WheelsController.RetractWheels = _Dock;
            WheelsController.MaximumSpeed = MaximumSpeed;
            WheelsController.Running(Me, InThisEntity, Enabled.Value ? NoWheelCtrl ? 0 : ThrustsControlLine.Z : 0, Enabled.Value ? NoWheelCtrl ? 0 : RotationCtrlLines.W : 0, _WheelPowerMult, HandBrake);
        }
        private void UpdateTargetSealevel() { if (IgnoreLevel) diffsealevel = 0; else { sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0; if (!KeepLevel) _Target_Sealevel = sealevel; diffsealevel = (float)(_Target_Sealevel - sealevel) * MultAttitude; } }
        private static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0f, 20f);
        private void PoseCtrl()
        {
            if (!Enabled.Value) { GyroControllerSystem.GyrosOverride(Me, InThisEntity, null); return; }
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
            if (!Enabled.Value) { ThrustControllerSystem.RunningDefault(Me, InThisEntity); return; }
            if (Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle || Role == ControllerRole.HoverVehicle)
                ThrustControllerSystem.Running(Me, InThisEntity, ThrustsControlLine, false, true, (!EnabledThrusters), MaximumSpeed, 0, true);
            else
            {
                Vector3 Ctrl = ThrustsControlLine;
                bool CtrlOrCruise = HoverMode || (Ctrl != Vector3.Zero);
                UpdateTargetSealevel();
                target_speed = MathHelper.Clamp(HandBrake ? 0 : (Ctrl != Vector3.Zero) ? ForwardOrUp ? LinearVelocity.Dot(Forward) : 0 : target_speed, 0, MaximumSpeed);
                ThrustControllerSystem.Running(Me, InThisEntity, CtrlOrCruise ? Ctrl : Vector3.Forward, (!ForwardOrUp), EnabledAllDirection, (!EnabledThrusters), CtrlOrCruise ? MaximumSpeed : target_speed, diffsealevel, true);
            }
        }
        protected override void LoadData()
        {
            foreach (var configitem in MyConfigs.Concurrent.AddConfigBlock(Configs, VehicleControllerConfigID))
            {

                switch (configitem.Key)
                {
                    case "Enabled": Enabled.Value = MyConfigs.ParseBool(configitem.Value); break;
                    case "HasWings": HasWings = MyConfigs.ParseBool(configitem.Value); break;
                    case "EnabledCuriser": EnabledCuriser = MyConfigs.ParseBool(configitem.Value); break;
                    case "Dock": _Dock = MyConfigs.ParseBool(configitem.Value); break;
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
            foreach (var configitem in MyConfigs.Concurrent.AddConfigBlock(Configs, OverclockedID))
            {
                switch (configitem.Key)
                {
                    case "RE": Overclocked_Reactors.Value = MyConfigs.ParseFloat(configitem.Value); break;
                    case "TR": Overclocked_Thrusts.Value = MyConfigs.ParseFloat(configitem.Value); break;
                    case "GY": Overclocked_Gyros.Value = MyConfigs.ParseFloat(configitem.Value); break;
                    case "GG": Overclocked_GasGenerators.Value = MyConfigs.ParseFloat(configitem.Value); break;
                    default: break;
                }
            }
        }
        protected override void SaveData()
        {
            {
                var data = MyConfigs.Concurrent.AddConfigBlock(Configs, VehicleControllerConfigID);
                MyConfigs.Concurrent.ModifyProperty(data, "Enabled", Enabled.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "Dock", _Dock.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "HasWings", HasWings.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "EnabledCuriser", EnabledCuriser.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "HoverMode", HoverMode.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "AngularDampeners_Roll", AngularDampeners_Roll.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "AngularDampeners_Pitch", AngularDampeners_Pitch.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "AngularDampeners_Yaw", AngularDampeners_Yaw.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "SafetyStage", SafetyStage.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "MaxReactions_AngleV", MaxReactions_AngleV.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "LocationSensetive", LocationSensetive.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "MaxiumFlightSpeed", MaxiumFlightSpeed.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "MaxiumHoverSpeed", MaxiumHoverSpeed.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "MaximumCruiseSpeed", MaximumCruiseSpeed.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "Role", Role.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "MultAttitude", MultAttitude.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "WheelPowerMult", _WheelPowerMult.ToString());
            }
            {
                var data = MyConfigs.Concurrent.AddConfigBlock(Configs, OverclockedID);
                MyConfigs.Concurrent.ModifyProperty(data, "RE", Overclocked_Reactors.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "TR", Overclocked_Thrusts.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "GY", Overclocked_Gyros.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "GG", Overclocked_GasGenerators.ToString());
            }
        }
    }
    public partial class UniversalController
    {
        protected override void InitBlock()
        {
            _Role.OnValueChanged += _Role_OnValueChanged;
            _Role.OnValueChanged += UpdateState;
            _ForwardOrUp.OnValueChanged += OnModeChange;
            Overclocked_Reactors.OnValueChanged += Overclocked_Reactors_OnValueChanged;
            Overclocked_Thrusts.OnValueChanged += Overclocked_Thrusts_OnValueChanged;
            Overclocked_Gyros.OnValueChanged += Overclocked_Gyros_OnValueChanged;
            Overclocked_GasGenerators.OnValueChanged += Overclocked_GasGenerators_OnValueChanged;
            OnRestart += () =>
            {
                Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
                ThrustControllerSystem.ForceUpdate(Me, InThisEntity);
                GyroControllerSystem.ForceUpdate(Me, InThisEntity);
                WheelsController.ForceUpdate(Me, InThisEntity);
                AutoCloseDoorController.UpdateBlocks(GridTerminalSystem);
                RotorThrustRotorCtrl.UpdateBinding(Me, InThisEntity);
                _Target_Sealevel = sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0;
            };
            OnRestart += _Role_OnValueChanged;
            OnRestart += UpdateState;
            OnRestart += Overclocked_Reactors_OnValueChanged;
            OnRestart += Overclocked_GasGenerators_OnValueChanged;
            OnRestart += Overclocked_Thrusts_OnValueChanged;
            OnRestart += Overclocked_Gyros_OnValueChanged;
            OnRunning1 += () =>
            {
                if (Me.CubeGrid.IsStatic) Role = ControllerRole.None;
                if (!UniversalControllerManage.IsMainController(Me)) return;
                Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
                PoseCtrl();
                ThrustControl();
                WheelControl();
                UpdateState();
                RotorThrustRotorCtrl.Running(Me, HoverMode, RotationIndication.Z, MaximumSpeed, MoveIndication);
                AutoCloseDoorController.Running(GridTerminalSystem);
            };
            OnRunning100 += () =>
            {
                if (UniversalControllerManage.IsMainController(Me)) return;
                Me.CustomData = UniversalControllerManage.GetRegistControllerBlockConfig(Me);
            };
        }
        protected override void ClosedBlock()
        {
            _Role.OnValueChanged -= _Role_OnValueChanged;
            _Role.OnValueChanged -= UpdateState;
            _ForwardOrUp.OnValueChanged -= OnModeChange;
            Overclocked_Reactors.OnValueChanged -= Overclocked_Reactors_OnValueChanged;
            Overclocked_Thrusts.OnValueChanged -= Overclocked_Thrusts_OnValueChanged;
            Overclocked_Gyros.OnValueChanged -= Overclocked_Gyros_OnValueChanged;
            Overclocked_GasGenerators.OnValueChanged -= Overclocked_GasGenerators_OnValueChanged;
            UniversalControllerManage.UnRegistControllerBlock(Me);
        }
        private void Overclocked_Reactors_OnValueChanged()
        {
            try
            {
                var Reactors = Common.GetTs<IMyReactor>(GridTerminalSystem);
                if (!Common.IsNullCollection(Reactors)) { foreach (var Reactor in Reactors) { Reactor.PowerOutputMultiplier = Overclocked_Reactors.Value; } }
            }
            catch (Exception) { }
        }
        private void Overclocked_GasGenerators_OnValueChanged()
        {
            try
            {
                var gasGenerators = Common.GetTs<IMyGasGenerator>(GridTerminalSystem);
                if (!Common.IsNullCollection(gasGenerators))
                {
                    foreach (var gasGenerator in gasGenerators)
                    {
                        gasGenerator.PowerConsumptionMultiplier = Overclocked_GasGenerators.Value;
                        gasGenerator.ProductionCapacityMultiplier = Overclocked_GasGenerators.Value;
                    }
                }
            }
            catch (Exception) { }
        }
        private void Overclocked_Gyros_OnValueChanged()
        {
            try
            {
                GyroControllerSystem.SetOverclocked(Overclocked_Gyros.Value);
            }
            catch (Exception) { }
        }
        private void Overclocked_Thrusts_OnValueChanged()
        {
            try
            {
                ThrustControllerSystem.SetOverclocked(Overclocked_Thrusts.Value);
            }
            catch (Exception) { }
        }
        private void _Role_OnValueChanged()
        {
            WheelsController.TrackVehicle = Role == ControllerRole.TrackVehicle;
        }
        private void OnModeChange() { if (!_ForwardOrUp.Value) _Target_Sealevel = sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0; }
        public static void LoadupInterface_UniversalController()
        {
            LoadupInterface_UniversalController_Basic();
            LoadupInterface_UniversalController_Advance();
        }
        public static void UnloadInterface_UniversalController()
        {
            UnloadInterface_UniversalController_Basic();
            UnloadInterface_UniversalController_Advance();
        }
    }
}