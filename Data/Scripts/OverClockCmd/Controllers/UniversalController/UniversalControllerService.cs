using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public sealed class UniversalControllerService : MySessionComponentBase
    {
        private static Dictionary<IMyEntity, IMyTerminalBlock> Register { get; } = new Dictionary<IMyEntity, IMyTerminalBlock>();
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!BasicInfoService.SetupComplete) return;
            if (!SetupComplete) { SetupComplete = true; Init(); return; }
        }
        public static bool RegistControllerBlock(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return false;
                if (Register.ContainsKey(TopmostEnt)) return false;
                Register.Add(TopmostEnt, Block);
                return true;
            }
            catch (Exception) { return false; }
        }
        public static bool IsMainController(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return false;
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return true; }
                if (Utils.NullEntity(Register[TopmostEnt])) Register.Remove(TopmostEnt);
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return true; }
                return Register[TopmostEnt] == Block;
            }
            catch (Exception) { return false; }
        }
        public static string GetRegistControllerBlockConfig(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return "";
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return Block.CustomData; }
                if (Utils.NullEntity(Register[TopmostEnt])) Register.Remove(TopmostEnt);
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return Block.CustomData; }
                return Register[TopmostEnt].CustomData;
            }
            catch (Exception) { return ""; }
        }
        public static void UnRegistControllerBlock(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return;
                if (Register.ContainsKey(TopmostEnt) && Register[TopmostEnt] == Block) { Register.Remove(TopmostEnt); return; }
            }
            catch (Exception) { }
        }
        public void Init()
        {
            加载_基础公共控件();
            加载_进阶控件();
            创建_基础公共属性();
            创建_进阶属性();
        }
        protected override void UnloadData()
        {
            卸载_基础公共控件();
            卸载_进阶控件();
            存储信息();
            Register.Clear();
            base.UnloadData();
        }
        #region 基础公共控件
        private void 加载_基础公共控件()
        {
            /*====================TriggerFunc Hook==================================================*/
            LoadConfig.TriggerFunc = UniversalController.ReadConfigs;
            SaveConfig.TriggerFunc = UniversalController.SaveConfigs;
            HasWings.TriggerFunc = UniversalController.TriggleHasWings;
            HoverMode.TriggerFunc = UniversalController.TriggleHoverMode;
            EnabledCuriser.TriggerFunc = UniversalController.TriggleEnabledCuriser;
            EnabledThrusters.TriggerFunc = UniversalController.TriggleEnabledThrusters;
            EnabledGyros.TriggerFunc = UniversalController.TriggleEnabledCuriser;
            ControllerRoleSelect.ComboBoxContent = UniversalController.Controller_Role_List;
            /*====================GetterFunc Hook===================================================*/
            ControllerRoleSelect.GetterFunc = UniversalController.RoleGetter;
            HasWings.GetterFunc = UniversalController.GetHasWings;
            HoverMode.GetterFunc = UniversalController.GetHoverMode;
            EnabledCuriser.GetterFunc = UniversalController.GetEnabledCuriser;
            EnabledThrusters.GetterFunc = UniversalController.GetEnabledThrusters;
            EnabledGyros.GetterFunc = UniversalController.GetEnabledGyros;
            /*====================SetterFunc Hook===================================================*/
            ControllerRoleSelect.SetterFunc = UniversalController.RoleSetter;
            HasWings.SetterFunc = UniversalController.SetHasWings;
            HoverMode.SetterFunc = UniversalController.SetHoverMode;
            EnabledCuriser.SetterFunc = UniversalController.SetEnabledCuriser;
            EnabledThrusters.SetterFunc = UniversalController.SetEnabledThrusters;
            EnabledGyros.SetterFunc = UniversalController.SetEnabledGyros;
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += LoadConfig.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += SaveConfig.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ControllerRoleSelect.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += HasWings.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += HoverMode.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_2.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledCuriser.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledThrusters.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledGyros.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter += LoadConfig.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += SaveConfig.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += HasWings.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += HoverMode.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledCuriser.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledThrusters.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledGyros.CreateAction;
        }
        private void 卸载_基础公共控件()
        {
            /*=======================Terminal UnHook================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= LoadConfig.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= SaveConfig.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ControllerRoleSelect.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= HasWings.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= HoverMode.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_2.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledCuriser.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledThrusters.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledGyros.CreateController;
            /*=========================Action UnHook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter -= LoadConfig.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= SaveConfig.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= HasWings.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= HoverMode.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledCuriser.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledThrusters.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledGyros.CreateAction;
        }
        private void 创建_基础公共属性()
        {
            CreateProperty.CreateProperty_PB_CN<long, IMyTerminalBlock>($"{CtrlNM}Role", UniversalController.EnabledGUI, UniversalController.RoleGetter, UniversalController.RoleSetter);
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}HasWings", UniversalController.EnabledGUI, UniversalController.GetHasWings, UniversalController.SetHasWings);
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}HoverMode", UniversalController.EnabledGUI, UniversalController.GetHoverMode, UniversalController.SetHoverMode);
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledCuriser", UniversalController.EnabledGUI, UniversalController.GetEnabledCuriser, UniversalController.SetEnabledCuriser);
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledThrusters", UniversalController.EnabledGUI, UniversalController.GetEnabledThrusters, UniversalController.SetEnabledThrusters);
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}EnabledGyros", UniversalController.EnabledGUI, UniversalController.GetEnabledGyros, UniversalController.SetEnabledGyros);
        }
        private static void 存储信息()
        {
            MyAPIGateway.Parallel.ForEach(Register, (KeyValuePair<IMyEntity, IMyTerminalBlock> KV) =>
            {
                try
                {
                    if (Utils.NullEntity(KV.Value)) return;
                    UniversalController.SaveDatas(KV.Value);
                }
                catch (Exception) { }
            });
        }
        private CreateTerminalFence<IMyTerminalBlock> Fence_0 { get; } = new CreateTerminalFence<IMyTerminalBlock>(UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> LoadConfig { get; } = new CreateTerminalButton<IMyTerminalBlock>("LoadConfigID", "Load Config", UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> SaveConfig { get; } = new CreateTerminalButton<IMyTerminalBlock>("SaveConfigID", "Save Config", UniversalController.EnabledGUI);
        private CreateTerminalControlCombobox<IMyTerminalBlock> ControllerRoleSelect { get; } = new CreateTerminalControlCombobox<IMyTerminalBlock>("ControllerRoleSelectID", "Controller Role Selecter", UniversalController.EnabledGUI);
        private CreateTerminalFence<IMyTerminalBlock> Fence_1 { get; } = new CreateTerminalFence<IMyTerminalBlock>(UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> HasWings { get; } = new CreateTerminalButton<IMyTerminalBlock>("HasWingsID", "Has Wings", UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> HoverMode { get; } = new CreateTerminalButton<IMyTerminalBlock>("HoverModeID", "Hover Mode", UniversalController.EnabledGUI);
        private CreateTerminalFence<IMyTerminalBlock> Fence_2 { get; } = new CreateTerminalFence<IMyTerminalBlock>(UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> EnabledCuriser { get; } = new CreateTerminalButton<IMyTerminalBlock>("EnabledCuriserID", "Enabled Curiser", UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> EnabledThrusters { get; } = new CreateTerminalButton<IMyTerminalBlock>("EnabledThrustsID", "Enabled Thrusts", UniversalController.EnabledGUI);
        private CreateTerminalButton<IMyTerminalBlock> EnabledGyros { get; } = new CreateTerminalButton<IMyTerminalBlock>("EnabledGyrosID", "Enabled Gyros", UniversalController.EnabledGUI);
        private const string CtrlNM = "UC_";
        #endregion
        #region 进阶控件
        private void 创建_进阶属性()
        {
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_R", UniversalController.EnabledGUI, UniversalController.AngularDampener_R_Getter, UniversalController.AngularDampener_R_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_P", UniversalController.EnabledGUI, UniversalController.AngularDampener_P_Getter, UniversalController.AngularDampener_P_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}AngularDampener_Y", UniversalController.EnabledGUI, UniversalController.AngularDampener_Y_Getter, UniversalController.AngularDampener_Y_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}MaxiumSpeedLimited", UniversalController.EnabledGUI, UniversalController.MaxiumSpeed_Getter, UniversalController.MaxiumSpeed_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}MaxReactions_AngleV", UniversalController.EnabledGUI, UniversalController.MaxReactions_AngleV_Getter, UniversalController.MaxReactions_AngleV_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}SafetyStage", UniversalController.EnabledGUI, UniversalController.SafetyStage_Getter, UniversalController.SafetyStage_Setter);
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}LocationSensetive", UniversalController.EnabledGUI, UniversalController.LocationSensetive_Getter, UniversalController.LocationSensetive_Setter);
            CreateProperty.CreateProperty_PB_CN<Vector3D?, IMyTerminalBlock>($"{CtrlNM}Override_ForwardDirection", UniversalController.EnabledGUI, UniversalController.Override_ForwardDirection_Getter, UniversalController.Override_ForwardDirection_Setter);
            CreateProperty.CreateProperty_PB_CN<Vector3D?, IMyTerminalBlock>($"{CtrlNM}Override_PlaneNormal", UniversalController.EnabledGUI, UniversalController.Override_PlaneNormal_Getter, UniversalController.Override_PlaneNormal_Setter);
        }
        private void 加载_进阶控件()
        {
            /*====================TriggerFunc Hook==================================================*/
            ResistRoll.DecreaseFunc = UniversalController.AngularDampener_R_Dec;
            ResistRoll.IncreaseFunc = UniversalController.AngularDampener_R_Inc;
            ResistPitch.DecreaseFunc = UniversalController.AngularDampener_P_Dec;
            ResistPitch.IncreaseFunc = UniversalController.AngularDampener_P_Inc;
            ResistYaw.DecreaseFunc = UniversalController.AngularDampener_Y_Dec;
            ResistYaw.IncreaseFunc = UniversalController.AngularDampener_Y_Inc;
            MaxiumSpeedLimited.DecreaseFunc = UniversalController.MaxiumSpeed_Dec;
            MaxiumSpeedLimited.IncreaseFunc = UniversalController.MaxiumSpeed_Inc;
            MaxReactions_AngleV.DecreaseFunc = UniversalController.MaxReactions_AngleV_Dec;
            MaxReactions_AngleV.IncreaseFunc = UniversalController.MaxReactions_AngleV_Inc;
            SafetyStage.DecreaseFunc = UniversalController.SafetyStage_Dec;
            SafetyStage.IncreaseFunc = UniversalController.SafetyStage_Inc;
            LocationSensetive.DecreaseFunc = UniversalController.LocationSensetive_Dec;
            LocationSensetive.IncreaseFunc = UniversalController.LocationSensetive_Inc;
            /*====================GetterFunc Hook===================================================*/
            ResistRoll.GetterFunc = UniversalController.AngularDampener_R_Getter;
            ResistPitch.GetterFunc = UniversalController.AngularDampener_P_Getter;
            ResistYaw.GetterFunc = UniversalController.AngularDampener_Y_Getter;
            MaxiumSpeedLimited.GetterFunc = UniversalController.MaxiumSpeed_Getter;
            MaxReactions_AngleV.GetterFunc = UniversalController.MaxReactions_AngleV_Getter;
            SafetyStage.GetterFunc = UniversalController.SafetyStage_Getter;
            LocationSensetive.GetterFunc = UniversalController.LocationSensetive_Getter;
            /*====================SetterFunc Hook===================================================*/
            ResistRoll.SetterFunc = UniversalController.AngularDampener_R_Setter;
            ResistPitch.SetterFunc = UniversalController.AngularDampener_P_Setter;
            ResistYaw.SetterFunc = UniversalController.AngularDampener_Y_Setter;
            MaxiumSpeedLimited.SetterFunc = UniversalController.MaxiumSpeed_Setter;
            MaxReactions_AngleV.SetterFunc = UniversalController.MaxReactions_AngleV_Setter;
            SafetyStage.SetterFunc = UniversalController.SafetyStage_Setter;
            LocationSensetive.SetterFunc = UniversalController.LocationSensetive_Setter;
            /*====================WritterFunc Hook==================================================*/
            ResistRoll.WriterFunc = UniversalController.AngularDampener_R_Writter;
            ResistPitch.WriterFunc = UniversalController.AngularDampener_P_Writter;
            ResistYaw.WriterFunc = UniversalController.AngularDampener_Y_Writter;
            MaxiumSpeedLimited.WriterFunc = UniversalController.MaxiumSpeed_Writter;
            MaxReactions_AngleV.WriterFunc = UniversalController.MaxReactions_AngleV_Writter;
            SafetyStage.WriterFunc = UniversalController.SafetyStage_Writter;
            LocationSensetive.WriterFunc = UniversalController.LocationSensetive_Writter;
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_3.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistRoll.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistPitch.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += ResistYaw.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_4.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += MaxiumSpeedLimited.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += MaxReactions_AngleV.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += SafetyStage.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += LocationSensetive.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistRoll.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistPitch.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += ResistYaw.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += MaxiumSpeedLimited.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += MaxReactions_AngleV.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += SafetyStage.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += LocationSensetive.CreateAction;
        }
        private void 卸载_进阶控件()
        {
            /*=======================Terminal Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_3.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistRoll.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistPitch.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= ResistYaw.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_4.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= MaxiumSpeedLimited.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= MaxReactions_AngleV.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= SafetyStage.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= LocationSensetive.CreateController;
            /*=========================Action Hook==================================================*/
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistRoll.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistPitch.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= ResistYaw.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= MaxiumSpeedLimited.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= MaxReactions_AngleV.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= SafetyStage.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= LocationSensetive.CreateAction;
        }
        private CreateTerminalFence<IMyTerminalBlock> Fence_3 { get; } = new CreateTerminalFence<IMyTerminalBlock>(UniversalController.EnabledGUI);
        private CreateTerminalSliderBar<IMyTerminalBlock> ResistRoll { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistRollID", "Resist Roll", UniversalController.EnabledGUI, 0, 20);
        private CreateTerminalSliderBar<IMyTerminalBlock> ResistPitch { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistPitchID", "Resist Pitch", UniversalController.EnabledGUI, 0, 20);
        private CreateTerminalSliderBar<IMyTerminalBlock> ResistYaw { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("ResistYawID", "Resist Yaw", UniversalController.EnabledGUI, 0, 20);
        private CreateTerminalFence<IMyTerminalBlock> Fence_4 { get; } = new CreateTerminalFence<IMyTerminalBlock>(UniversalController.EnabledGUI);
        private CreateTerminalSliderBar<IMyTerminalBlock> MaxiumSpeedLimited { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("MaxiumSpeedLimitedID", "Maxium Speed Limited:", UniversalController.EnabledGUI, 1, 3e8f);
        private CreateTerminalSliderBar<IMyTerminalBlock> MaxReactions_AngleV { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("MaxReactions_AngleVID", "Maxium RSC Speed:", UniversalController.EnabledGUI, 1, 3e8f);
        private CreateTerminalSliderBar<IMyTerminalBlock> SafetyStage { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("SafetyStageID", "Safety Stage:", UniversalController.EnabledGUI, 1, 3e8f);
        private CreateTerminalSliderBar<IMyTerminalBlock> LocationSensetive { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("LocationSensetiveID", "Location Sensetive:", UniversalController.EnabledGUI, 1, 3e8f);
        #endregion
    }
}
