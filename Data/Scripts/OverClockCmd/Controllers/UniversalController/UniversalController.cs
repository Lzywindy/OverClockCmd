using Sandbox.Game.EntityComponents;
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
            //if (!Enabled.Value || !Powered) return;
            bool NoWheelCtrl = !(Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle);
            WheelsController.TrackVehicle = Role == ControllerRole.TrackVehicle;
            WheelsController.RetractWheels = _Dock;
            WheelsController.MaximumSpeed = MaximumSpeed;
            WheelsController.Running(Me, InThisEntity, Enabled.Value ? NoWheelCtrl ? 0 : ThrustsControlLine.Z : 0, Enabled.Value ? NoWheelCtrl ? 0 : RotationCtrlLines.W : 0, _WheelPowerMult, HandBrake);
        }
        private void UpdateTargetSealevel() { if (IgnoreLevel) diffsealevel = 0; else { sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0; if (!KeepLevel) _Target_Sealevel = sealevel; diffsealevel = (float)(_Target_Sealevel - sealevel) * MultAttitude; } }
        private static float SetInRange_AngularDampeners(float data) => MathHelper.Clamp(data, 0.01f, 20f);
        private void PoseCtrl()
        {
            if (!Enabled.Value) return;
            Vector3? Rotation;
            float _MaxReactions_AngleV;
            if (Role == ControllerRole.HoverVehicle || Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle)
            {
                Rotation = PoseProcessFuncs.ProcessRotation_GroundVehicle(Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, MaxReactions_AngleV, DisabledRotation, ForwardDirectionOverride);
                _MaxReactions_AngleV = 25f;
            }
            else if (Role == ControllerRole.SeaShip || Role == ControllerRole.Submarine)
            {
                Rotation = PoseProcessFuncs.ProcessRotation_SeaVehicle(Me, RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, MaxReactions_AngleV, DisabledRotation, ForwardDirectionOverride);
                _MaxReactions_AngleV = 20f;
            }
            else
            {
                Rotation = PoseProcessFuncs.ProcessRotation(_EnabledCuriser, Me, RotationCtrlLines, ref ForwardDirection, Role, InitAngularDampener, AngularDampeners, ForwardOrUp, HasWings, MaximumSpeed, MaxReactions_AngleV, SafetyStage, DisabledRotation, ForwardDirectionOverride, PlaneNormalOverride);
                _MaxReactions_AngleV = MaxReactions_AngleV;
            }
            InnerGyrosController.GyrosOverride(Me, Rotation ?? RotationIndication, _MaxReactions_AngleV, LocationSensetive);
        }
        private void ThrustControl()
        {

            if (!Enabled.Value) { ThrustControllerSystem.RunningDefault(Me, InThisEntity); return; }

            if (Role == ControllerRole.TrackVehicle || Role == ControllerRole.WheelVehicle || Role == ControllerRole.HoverVehicle)
            {
                ThrustControllerSystem.Running(Me, InThisEntity, ThrustsControlLine, false, true, (!EnabledThrusters), MaximumSpeed, 0, true);
                WarpThrusterController.Running(Me, ThrustsControlLine, Get_WarpEnabled(), HandBrake ? 5f : WarpAcc, MaximumSpeed, MaximumSpeed, _Dock, true, 0);
            }
            else
            {
                Vector3 Ctrl = ThrustsControlLine;
                bool CtrlLine = Ctrl != Vector3.Zero;
                bool CtrlOrCruise = HoverMode || CtrlLine;
                UpdateTargetSealevel();
                target_speed = MathHelper.Clamp(Brake() ? 0 : CtrlLine ? ForwardOrUp ? LinearVelocity.Dot(Forward) : 0 : target_speed, 0, MaximumSpeed);
                ThrustControllerSystem.Running(Me, InThisEntity, CtrlOrCruise ? Ctrl : Vector3.Forward, (!ForwardOrUp), EnabledAllDirection || (MyPlanetInfoAPI.GetAtmoEffect(Me.GetPosition()) == null), (!EnabledThrusters), CtrlOrCruise ? MaximumSpeed : target_speed, diffsealevel, true);
                WarpThrusterController.Running(Me, CtrlOrCruise ? Ctrl : Vector3.Forward, Get_WarpEnabled(), HandBrake ? 5f : WarpAcc, CtrlLine ? MaximumSpeed : target_speed, MaximumSpeed, _Dock, !ForwardOrUp, diffsealevel);
            }
        }
        private bool Brake()
        {
            if (Override_HandBrake ?? Controller?.HandBrake ?? false) return true;
            switch (Role)
            {
                case ControllerRole.Aeroplane:
                case ControllerRole.VTOL:
                case ControllerRole.SpaceShip:
                    if (ForwardOrUp) return (target_speed <= 0 || ((MoveIndication * Vector3.Backward).Dot(Vector3.Backward) > 0));
                    return Vector3.IsZero(MoveIndication);
                case ControllerRole.Helicopter:
                    return Vector3.IsZero(MoveIndication);
                default:
                    return false;
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
                    case "EnabledThrusters": EnabledThrusters = MyConfigs.ParseBool(configitem.Value); break;
                    case "EnabledGyros": EnabledGyros = MyConfigs.ParseBool(configitem.Value); break;
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
                    case "WarpEnabled": _WarpEnabled_Signal = MyConfigs.ParseBool(configitem.Value); break;
                    case "WarpAcc": WarpAcc = MathHelper.Clamp(MyConfigs.ParseFloat(configitem.Value), 0, 4e4f); break;
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
                MyConfigs.Concurrent.ModifyProperty(data, "EnabledThrusters", EnabledThrusters.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "EnabledGyros", EnabledGyros.ToString());
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
                MyConfigs.Concurrent.ModifyProperty(data, "WarpEnabled", _WarpEnabled_Signal.ToString());
                MyConfigs.Concurrent.ModifyProperty(data, "WarpAcc", WarpAcc.ToString());
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
            if (Me.ResourceSink == null)
            {
                ResourceSinkInfo = new MyResourceSinkInfo() { RequiredInputFunc = RunningPower_Warp, MaxRequiredInput = float.MaxValue, ResourceTypeId = MyResourceDistributorComponent.ElectricityId };
                if (Me.Components.Get<MyResourceSinkComponent>() != null)
                    Me.Components.Remove<MyResourceSinkComponent>();
                var ResourceSink = new MyResourceSinkComponent();
                ResourceSink.AddType(ref ResourceSinkInfo);
                Me.ResourceSink = ResourceSink;
                Me.Components.Add(Me.ResourceSink);
            }
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
                WheelsController.ForceUpdate(Me, InThisEntity);
                AutoCloseDoorController.UpdateBlocks(GridTerminalSystem);
                RotorThrustRotorCtrl.UpdateBinding(Me, InThisEntity);
                _Target_Sealevel = sealevel = MyPlanetInfoAPI.GetSealevel(Me.GetPosition()) ?? 0;
                UpdateState();
                _Role_OnValueChanged();
                Overclocked_Reactors_OnValueChanged();
                Overclocked_GasGenerators_OnValueChanged();
                Overclocked_Thrusts_OnValueChanged();
                Overclocked_Gyros_OnValueChanged();
            };
            OnRunning1 += () =>
            {
                if (Me.CubeGrid.IsStatic) Role = ControllerRole.None;
                if (!UniversalControllerManage.IsMainController(Me)) return;
                SetPowerConsumption();
                Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
                PoseCtrl();
                ThrustControl();
                WheelControl();
                UpdateState();
                RotorThrustRotorCtrl.Running(Me, HoverMode, RotationIndication.Z, MoveIndication);
                AutoCloseDoorController.Running(GridTerminalSystem);
            };
            OnRunning10 += () =>
            {
                //if (Me.ResourceSink == null) return;

                //Me.ResourceSink.SetInputFromDistributor(MyResourceDistributorComponent.ElectricityId, RunningPower_Warp, true);

            };
            OnRunning100 += () =>
            {
                if (UniversalControllerManage.IsMainController(Me))
                {
                    Controller = Common.GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit || block.IsUnderControl);
                    count = (count + 1) % 10;
                    if (count % 10 != 0) return;
                    ThrustControllerSystem.ForceUpdate(Me, InThisEntity);
                    WheelsController.ForceUpdate(Me, InThisEntity);
                    AutoCloseDoorController.UpdateBlocks(GridTerminalSystem);
                    RotorThrustRotorCtrl.UpdateBinding(Me, InThisEntity);
                    UpdateState();
                    _Role_OnValueChanged();
                }
                else
                    Me.CustomData = UniversalControllerManage.GetRegistControllerBlockConfig(Me);
            };
        }
        private int count = 0;
        private void SetPowerConsumption()
        {
            if (Me.ResourceSink == null) return;
            float powerNeeded = RunningPower_Warp();
            //Powered = Me.ResourceSink.IsPowerAvailable(MyResourceDistributorComponent.ElectricityId, powerNeeded);
            //if (!Powered) return;
            Me.ResourceSink.SetMaxRequiredInputByType(MyResourceDistributorComponent.ElectricityId, powerNeeded);
            Me.ResourceSink.SetRequiredInputByType(MyResourceDistributorComponent.ElectricityId, powerNeeded);

        }
        private bool Powered { get; set; } = true;

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
                //GyroControllerSystem.SetOverclocked(Overclocked_Gyros.Value);
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
            LoadupInterface_UniversalController_Warp();
        }
        public static void UnloadInterface_UniversalController()
        {
            UnloadInterface_UniversalController_Basic();
            UnloadInterface_UniversalController_Advance();
            UnloadInterface_UniversalController_Warp();
        }
    }
}