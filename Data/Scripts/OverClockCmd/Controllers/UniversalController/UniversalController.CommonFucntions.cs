using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRageMath;
using static SuperBlocks.Utils;

namespace SuperBlocks.Controller
{
    public partial class UniversalController
    {
        public bool DockGround(IMyTerminalBlock Me) { try { if (Me == this.Me) return _Dock; } catch (Exception) { } return false; }
        public void DockGround(IMyTerminalBlock Me, bool value) { try { if (Me == this.Me) { _Dock = value; SaveData(Me); } } catch (Exception) { } }
        public void TriggleDockGround(IMyTerminalBlock Me) { try { if (Me == this.Me) { _Dock = !_Dock; SaveData(Me); } } catch (Exception) { } }
        public bool DockGroundReady(IMyTerminalBlock Me) { try { if (Me == this.Me) return WheelsController.DockComplete; } catch (Exception) { } return false; }
        public void TriggleHasWings(IMyTerminalBlock Me) { if (Me == this.Me) { HasWings = !HasWings; SaveData(Me); } }
        public void TriggleHoverMode(IMyTerminalBlock Me) { if (Me == this.Me) { HoverMode = !HoverMode; SaveData(Me); } }
        public void TriggleEnabledCuriser(IMyTerminalBlock Me) { if (Me == this.Me) { EnabledCuriser = !EnabledCuriser; SaveData(Me); } }
        public void TriggleEnabledThrusters(IMyTerminalBlock Me) { if (Me == this.Me) { EnabledThrusters = !EnabledThrusters; SaveData(Me); } }
        public void TriggleEnabledGyros(IMyTerminalBlock Me) { if (Me == this.Me) { EnabledGyros = !EnabledGyros; SaveData(Me); } }
        public void CHasWings(IMyTerminalBlock Me, bool Value) { if (Me == this.Me) { HasWings = Value; SaveData(Me); } }
        public void CHoverMode(IMyTerminalBlock Me, bool Value) { if (Me == this.Me) { HoverMode = Value; SaveData(Me); } }
        public void CEnabledCuriser(IMyTerminalBlock Me, bool Value) { if (Me == this.Me) { EnabledCuriser = Value; SaveData(Me); } }
        public void CEnabledThrusters(IMyTerminalBlock Me, bool Value) { if (Me == this.Me) { EnabledThrusters = Value; SaveData(Me); } }
        public void CEnabledGyros(IMyTerminalBlock Me, bool Value) { if (Me == this.Me) { EnabledGyros = Value; SaveData(Me); } }
        public bool CHasWings(IMyTerminalBlock Me) { if (Me == this.Me) return HasWings; return false; }
        public bool CHoverMode(IMyTerminalBlock Me) { if (Me == this.Me) return HoverMode; return false; }
        public bool CEnabledCuriser(IMyTerminalBlock Me) { if (Me == this.Me) return EnabledCuriser; return false; }
        public bool CEnabledThrusters(IMyTerminalBlock Me) { if (Me == this.Me) return EnabledThrusters; return false; }
        public bool CEnabledGyros(IMyTerminalBlock Me) { if (Me == this.Me) return EnabledGyros; return false; }
        public static void Controller_Role_List(List<VRage.ModAPI.MyTerminalControlComboBoxItem> items) { if (items == null) return; items.Clear(); items.AddRange(角色列表实体_UC); }
        public long CRole(IMyTerminalBlock Me) { if (Me == this.Me) return (long)Role; return 0; }
        public void CRole(IMyTerminalBlock Me, long value) { if (Me == this.Me) { Role = (ControllerRole)value; SaveData(Me); } }
        private static List<VRage.ModAPI.MyTerminalControlComboBoxItem> 角色列表实体_UC { get; } = new List<VRage.ModAPI.MyTerminalControlComboBoxItem>()
        {
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.None,Value=MyStringId.GetOrCompute(ControllerRole.None.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.Aeroplane,Value=MyStringId.GetOrCompute(ControllerRole.Aeroplane.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.Helicopter,Value=MyStringId.GetOrCompute(ControllerRole.Helicopter.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.VTOL,Value=MyStringId.GetOrCompute(ControllerRole.VTOL.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.SpaceShip,Value=MyStringId.GetOrCompute(ControllerRole.SpaceShip.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.SeaShip,Value=MyStringId.GetOrCompute(ControllerRole.SeaShip.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.Submarine,Value=MyStringId.GetOrCompute(ControllerRole.Submarine.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.TrackVehicle,Value=MyStringId.GetOrCompute(ControllerRole.TrackVehicle.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.WheelVehicle,Value=MyStringId.GetOrCompute(ControllerRole.WheelVehicle.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (int)ControllerRole.HoverVehicle,Value=MyStringId.GetOrCompute(ControllerRole.HoverVehicle.ToString())}
        };
        private static void LoadupInterface_UniversalController_Basic()
        {
            /*====================TriggerFunc Hook==================================================*/
            DockGroundCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleDockGround(Me);
            HasWingsCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleHasWings(Me);
            HoverModeCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleHoverMode(Me);
            EnabledCuriserCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleEnabledCuriser(Me);
            EnabledThrustersCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleEnabledThrusters(Me);
            EnabledGyrosCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleEnabledGyros(Me);
            ControllerRoleSelect.ComboBoxContent = Controller_Role_List;
            /*====================GetterFunc Hook===================================================*/
            DockGroundCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.DockGround(Me) ?? false;
            ControllerRoleSelect.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CRole(Me) ?? 0;
            HasWingsCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CHasWings(Me) ?? false;
            HoverModeCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CHoverMode(Me) ?? false;
            EnabledCuriserCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledCuriser(Me) ?? false;
            EnabledThrustersCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledThrusters(Me) ?? false;
            EnabledGyrosCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledGyros(Me) ?? false;
            /*====================SetterFunc Hook===================================================*/
            DockGroundCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.DockGround(Me, value);
            ControllerRoleSelect.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CRole(Me, value);
            HasWingsCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CHasWings(Me, value);
            HoverModeCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CHoverMode(Me, value);
            EnabledCuriserCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledCuriser(Me, value);
            EnabledThrustersCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledThrusters(Me, value);
            EnabledGyrosCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledGyros(Me, value);
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter += DockGroundCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ControllerRoleSelect.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += HasWingsCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += HoverModeCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledCuriserCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledThrustersCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledGyrosCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter += DockGroundCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += HasWingsCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += HoverModeCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledCuriserCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledThrustersCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledGyrosCtrl.CreateAction;
            /*=========================Property Hook==================================================*/
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}DockGround", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.DockGround(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.DockGround(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}DockGroundReady", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.DockGroundReady(Me) ?? false, (Me, value) => { });
            CreateProperty.CreateProperty_PB_CN<long, IMyTerminalBlock>($"{CtrlNM}Role", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CRole(Me) ?? 0, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CRole(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}HasWings", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CHasWings(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CHasWings(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}HoverMode", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CHoverMode(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CHoverMode(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledCuriser", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledCuriser(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledCuriser(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledThrusters", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledThrusters(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledThrusters(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledGyros", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledGyros(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CEnabledGyros(Me, value));
        }
        private static void UnloadInterface_UniversalController_Basic()
        {
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter -= DockGroundCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ControllerRoleSelect.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= HasWingsCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= HoverModeCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledCuriserCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledThrustersCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledGyrosCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter -= DockGroundCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= HasWingsCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= HoverModeCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledCuriserCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledThrustersCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledGyrosCtrl.CreateAction;
        }
        private bool Airbrake(IMyTerminalBlock Me)
        {
            if (Me != this.Me) return false;
            var atmo = MyPlanetInfoAPI.GetAtmoEffect(Me.GetPosition()).HasValue;
            bool brake = Override_HandBrake ?? Controller?.HandBrake ?? false;
            if (brake) return atmo;
            switch (Role)
            {
                case ControllerRole.Aeroplane:
                case ControllerRole.VTOL:
                case ControllerRole.SpaceShip:
                    if (ForwardOrUp) return (target_speed <= 0 || ((MoveIndication * Vector3.Backward).Dot(Vector3.Backward) > 0)) && atmo;
                    return Vector3.IsZero(MoveIndication) && atmo;
                case ControllerRole.Helicopter:
                    return Vector3.IsZero(MoveIndication) && atmo;
                default:
                    return false;
            }
        }
        private static CreateTerminalSwitch<IMyTerminalBlock> DockGroundCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("DockGroundID", "DockGround", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> HasWingsCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("HasWingsID", "Has Wings", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> HoverModeCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("HoverModeID", "Hover Mode", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalControlCombobox<IMyTerminalBlock> ControllerRoleSelect { get; } = new CreateTerminalControlCombobox<IMyTerminalBlock>("ControllerRoleSelectID", "Controller Role Selecter", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> EnabledCuriserCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("EnabledCuriserID", "Enabled Curiser", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> EnabledThrustersCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("EnabledThrustsID", "Enabled Thrusts", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> EnabledGyrosCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("EnabledGyrosID", "Enabled Gyros", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalFence<IMyTerminalBlock> Fence { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private const string CtrlNM = "UC_";
    }
    public partial class UniversalController
    {
        public void CAngularDampener_R_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"x{MathHelper.RoundOn2(AngularDampeners_Roll)}"); }
        public float CAngularDampener_R(IMyTerminalBlock Me) { if (Me == this.Me) return AngularDampeners_Roll; return 1; }
        public void CAngularDampener_R(IMyTerminalBlock Me, float value) { if (Me == this.Me) { AngularDampeners_Roll = value; SaveData(Me); } }
        public void CAngularDampener_R_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Roll++; SaveData(Me); } }
        public void CAngularDampener_R_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Roll--; SaveData(Me); } }
        public void CAngularDampener_P_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"x{MathHelper.RoundOn2(AngularDampeners_Pitch)}"); }
        public float CAngularDampener_P(IMyTerminalBlock Me) { if (Me == this.Me) return AngularDampeners_Pitch; return 1; }
        public void CAngularDampener_P(IMyTerminalBlock Me, float value) { if (Me == this.Me) { AngularDampeners_Pitch = value; SaveData(Me); } }
        public void CAngularDampener_P_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Pitch++; SaveData(Me); } }
        public void CAngularDampener_P_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Pitch--; SaveData(Me); } }
        public void CAngularDampener_Y_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"x{MathHelper.RoundOn2(AngularDampeners_Yaw)}"); }
        public float CAngularDampener_Y(IMyTerminalBlock Me) { if (Me == this.Me) return AngularDampeners_Yaw; return 1; }
        public void CAngularDampener_Y(IMyTerminalBlock Me, float value) { if (Me == this.Me) { AngularDampeners_Yaw = value; SaveData(Me); } }
        public void CAngularDampener_Y_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Yaw++; SaveData(Me); } }
        public void CAngularDampener_Y_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { AngularDampeners_Yaw--; SaveData(Me); } }
        public void CMaxiumSpeed_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"{CMaxiumSpeedShow()}"); }
        public float CMaxiumSpeed(IMyTerminalBlock Me) { if (Me == this.Me) return MaximumSpeed; return 1; }
        public void CMaxiumSpeed(IMyTerminalBlock Me, float value) { if (Me == this.Me) { MaximumSpeed = value; SaveData(Me); } }
        public void CMaxiumSpeed_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { MaximumSpeed++; SaveData(Me); } }
        public void CMaxiumSpeed_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { MaximumSpeed--; SaveData(Me); } }
        private string CMaxiumSpeedShow()
        {
            switch (Role)
            {
                case ControllerRole.Aeroplane:
                case ControllerRole.Helicopter:
                case ControllerRole.VTOL://m/s
                    return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
                case ControllerRole.SpaceShip:// v<3e6:m/s/km/s,v>3e6:c
                    if (MaximumSpeed < 3e3f)
                        return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
                    else if (MaximumSpeed < 3e6f)
                        return $"{MathHelper.RoundOn2(MaximumSpeed / 1000)} km/s";
                    else
                        return $"{MathHelper.RoundOn2(MaximumSpeed / 3e8f)} c";
                case ControllerRole.SeaShip:
                case ControllerRole.Submarine://knot
                    return $"{MathHelper.RoundOn2(MaximumSpeed * 3.6f / 1.852f)} kn";
                case ControllerRole.TrackVehicle:
                case ControllerRole.WheelVehicle:
                case ControllerRole.HoverVehicle://km/h
                    return $"{MathHelper.RoundOn2(MaximumSpeed * 3.6f)} km/h";
                default:
                    return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
            }
        }
        public void MaxReactions_AngleV_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"{MaxReactions_AngleV} deg/s"); }
        public float MaxReactions_AngleV_Getter(IMyTerminalBlock Me) { if (Me == this.Me) return MaxReactions_AngleV; return 1; }
        public void MaxReactions_AngleV_Setter(IMyTerminalBlock Me, float value) { if (Me == this.Me) { MaxReactions_AngleV = value; SaveData(Me); } }
        public void MaxReactions_AngleV_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { MaxReactions_AngleV++; SaveData(Me); } }
        public void MaxReactions_AngleV_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { MaxReactions_AngleV--; SaveData(Me); } }
        public void SafetyStage_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"{SafetyStage * 100}%"); }
        public float SafetyStage_Getter(IMyTerminalBlock Me) { if (Me == this.Me) return SafetyStage; return 1; }
        public void SafetyStage_Setter(IMyTerminalBlock Me, float value) { if (Me == this.Me) { SafetyStage = value; SaveData(Me); } }
        public void SafetyStage_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { SafetyStage++; SaveData(Me); } }
        public void SafetyStage_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { SafetyStage--; SaveData(Me); } }
        public void LocationSensetive_Writter(IMyTerminalBlock Me, StringBuilder value) { if (Me != this.Me) return; value.Clear(); value.Append($"{LocationSensetive * 100}%"); }
        public float LocationSensetive_Getter(IMyTerminalBlock Me) { if (Me == this.Me) return LocationSensetive; return 1; }
        public void LocationSensetive_Setter(IMyTerminalBlock Me, float value) { if (Me == this.Me) { LocationSensetive = value; SaveData(Me); } }
        public void LocationSensetive_Inc(IMyTerminalBlock Me) { if (Me == this.Me) { LocationSensetive++; SaveData(Me); } }
        public void LocationSensetive_Dec(IMyTerminalBlock Me) { if (Me == this.Me) { LocationSensetive--; SaveData(Me); } }
        public void Override_ForwardDirection_Setter(IMyTerminalBlock Me, Vector3D? Vector) { if (Me == this.Me) ForwardDirectionOverride = Vector; }
        public Vector3D? Override_ForwardDirection_Getter(IMyTerminalBlock Me) { if (Me == this.Me) return ForwardDirectionOverride; return null; }
        public void Override_PlaneNormal_Setter(IMyTerminalBlock Me, Vector3D? Vector) { if (Me == this.Me) PlaneNormalOverride = Vector; }
        public Vector3D? Override_PlaneNormal_Getter(IMyTerminalBlock Me) { if (Me == this.Me) return PlaneNormalOverride; return null; }
        private static void LoadupInterface_UniversalController_Advance()
        {
            /*====================TriggerFunc Hook==================================================*/
            ResistRollCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R_Dec(Me);
            ResistRollCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R_Inc(Me);
            ResistPitchCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P_Dec(Me);
            ResistPitchCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P_Inc(Me);
            ResistYawCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y_Dec(Me);
            ResistYawCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y_Inc(Me);
            MaxiumSpeedLimitedCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed_Dec(Me);
            MaxiumSpeedLimitedCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed_Inc(Me);
            MaxReactions_AngleVCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Dec(Me);
            MaxReactions_AngleVCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Inc(Me);
            SafetyStageCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Dec(Me);
            SafetyStageCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Inc(Me);
            LocationSensetiveCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Dec(Me);
            LocationSensetiveCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Inc(Me);
            /*====================GetterFunc Hook===================================================*/
            ResistRollCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R(Me) ?? 1;
            ResistPitchCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P(Me) ?? 1;
            ResistYawCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y(Me) ?? 1;
            MaxiumSpeedLimitedCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed(Me) ?? 0;
            MaxReactions_AngleVCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Getter(Me) ?? 1;
            SafetyStageCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Getter(Me) ?? 1;
            LocationSensetiveCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Getter(Me) ?? 1;
            /*====================SetterFunc Hook===================================================*/
            ResistRollCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R(Me, value);
            ResistPitchCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P(Me, value);
            ResistYawCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y(Me, value);
            MaxiumSpeedLimitedCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed(Me, value);
            MaxReactions_AngleVCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Setter(Me, value);
            SafetyStageCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Setter(Me, value);
            LocationSensetiveCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Setter(Me, value);
            /*====================WritterFunc Hook==================================================*/
            ResistRollCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R_Writter(Me, value);
            ResistPitchCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P_Writter(Me, value);
            ResistYawCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y_Writter(Me, value);
            MaxiumSpeedLimitedCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed_Writter(Me, value);
            MaxReactions_AngleVCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Writter(Me, value);
            SafetyStageCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Writter(Me, value);
            LocationSensetiveCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Writter(Me, value);
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_3.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistRollCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistPitchCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistYawCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_4.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += MaxiumSpeedLimitedCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += MaxReactions_AngleVCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += SafetyStageCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += LocationSensetiveCtrl.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistRollCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistPitchCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistYawCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += MaxiumSpeedLimitedCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += MaxReactions_AngleVCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += SafetyStageCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += LocationSensetiveCtrl.CreateAction;
            /*=========================Property Hook==================================================*/
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_R", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_R(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_P", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_P(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_Y", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CAngularDampener_Y(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}MaxiumSpeedLimited", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed(Me) ?? 0, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CMaxiumSpeed(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}MaxReactions_AngleV", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Getter(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.MaxReactions_AngleV_Setter(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}SafetyStage", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Getter(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.SafetyStage_Setter(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}LocationSensetive", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Getter(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.LocationSensetive_Setter(Me, value));
            CreateProperty.CreateProperty_PB_CN<Vector3D?, IMyTerminalBlock>($"{CtrlNM}Override_ForwardDirection", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.Override_ForwardDirection_Getter(Me), (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.Override_ForwardDirection_Setter(Me, value));
            CreateProperty.CreateProperty_PB_CN<Vector3D?, IMyTerminalBlock>($"{CtrlNM}Override_PlaneNormal", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.Override_PlaneNormal_Getter(Me), (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.Override_PlaneNormal_Setter(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}CanAirbrake", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.Airbrake(Me) ?? false, (Me, value) => { });
        }
        private static void UnloadInterface_UniversalController_Advance()
        {
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_3.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistRollCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistPitchCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistYawCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_4.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= MaxiumSpeedLimitedCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= MaxReactions_AngleVCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= SafetyStageCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= LocationSensetiveCtrl.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistRollCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistPitchCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistYawCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= MaxiumSpeedLimitedCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= MaxReactions_AngleVCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= SafetyStageCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= LocationSensetiveCtrl.CreateAction;
        }
        private static CreateTerminalFence<IMyTerminalBlock> Fence_3 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSliderBar<IMyTerminalBlock> ResistRollCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistRollID", "Resist Roll", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 0, 20);
        private static CreateTerminalSliderBar<IMyTerminalBlock> ResistPitchCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistPitchID", "Resist Pitch", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 0, 20);
        private static CreateTerminalSliderBar<IMyTerminalBlock> ResistYawCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistYawID", "Resist Yaw", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 0, 20);
        private static CreateTerminalFence<IMyTerminalBlock> Fence_4 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSliderBar<IMyTerminalBlock> MaxiumSpeedLimitedCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("MaxiumSpeedLimitedID", "Maxium Speed Limited:", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 1, 3e8f);
        private static CreateTerminalSliderBar<IMyTerminalBlock> MaxReactions_AngleVCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("MaxReactions_AngleVID", "Maxium RSC Speed:", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 1, 90f);
        private static CreateTerminalSliderBar<IMyTerminalBlock> SafetyStageCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("SafetyStageID", "Safety Percentage:", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 0, 1);
        private static CreateTerminalSliderBar<IMyTerminalBlock> LocationSensetiveCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("LocationSensetiveID", "Location Sensetive Percentage:", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, 0, 1);
    }
    public partial class UniversalController
    {
        private bool WarpEnabled(IMyTerminalBlock Me) => EnabledGUI(Me) && GetEnabled_Warp();

        public bool WarpMode(IMyTerminalBlock Me) { try { if (WarpEnabled(Me)) return _WarpEnabled_Signal; } catch (Exception) { } return false; }
        public void WarpMode(IMyTerminalBlock Me, bool value) { try { if (WarpEnabled(Me)) { _WarpEnabled_Signal = value; SaveData(Me); } } catch (Exception) { } }
        public void TriggleWarpMode(IMyTerminalBlock Me) { try { if (WarpEnabled(Me)) { _WarpEnabled_Signal = !_WarpEnabled_Signal; SaveData(Me); } } catch (Exception) { } }
        public void CWarpMode_Acc_Writter(IMyTerminalBlock Me, StringBuilder value) { if (!WarpEnabled(Me)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(WarpAcc)}m/(s^2)"); }
        public float CWarpMode_Acc(IMyTerminalBlock Me) { if (WarpEnabled(Me)) return WarpAcc; return 1; }
        public void CWarpMode_Acc(IMyTerminalBlock Me, float value) { if (WarpEnabled(Me)) { WarpAcc = value; SaveData(Me); } }
        public void CWarpMode_Acc_Inc(IMyTerminalBlock Me) { if (WarpEnabled(Me)) { WarpAcc++; SaveData(Me); } }
        public void CWarpMode_Acc_Dec(IMyTerminalBlock Me) { if (WarpEnabled(Me)) { WarpAcc--; SaveData(Me); } }


        private static void LoadupInterface_UniversalController_Warp()
        {
            /*====================TriggerFunc Hook==================================================*/
            WarpModeCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.TriggleWarpMode(Me);
            WarpModeAccCtrl.IncreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc_Inc(Me);
            WarpModeAccCtrl.DecreaseFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc_Dec(Me);
            /*====================GetterFunc Hook===================================================*/
            WarpModeCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.WarpMode(Me) ?? false;
            WarpModeAccCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc(Me) ?? 1;
            /*====================SetterFunc Hook===================================================*/
            WarpModeCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.WarpMode(Me, value);
            WarpModeAccCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc(Me, value);
            /*====================WritterFunc Hook==================================================*/
            WarpModeAccCtrl.WriterFunc = (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc_Writter(Me, value);
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_5.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += WarpModeCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += WarpModeAccCtrl.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter += WarpModeCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += WarpModeAccCtrl.CreateAction;
            /*=========================Property Hook==================================================*/
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}WarpMode", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.WarpMode(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.WarpMode(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}WarpModeAcc", Me => Me?.GameLogic?.GetAs<UniversalController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc(Me) ?? 1, (Me, value) => Me?.GameLogic?.GetAs<UniversalController>()?.CWarpMode_Acc(Me, value));

        }
        private static void UnloadInterface_UniversalController_Warp()
        {
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_5.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= WarpModeCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= WarpModeAccCtrl.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter -= WarpModeCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= WarpModeAccCtrl.CreateAction;
        }
        private static CreateTerminalFence<IMyTerminalBlock> Fence_5 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<UniversalController>()?.WarpEnabled(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> WarpModeCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("WarpModeID", "Warp Enabled", Me => Me?.GameLogic?.GetAs<UniversalController>()?.WarpEnabled(Me) ?? false);
        private static CreateTerminalSliderBar<IMyTerminalBlock> WarpModeAccCtrl { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("WarpModeAccID", "Warp Mode Acc", Me => Me?.GameLogic?.GetAs<UniversalController>()?.WarpEnabled(Me) ?? false, 1, 4e4f);
    }
}
