using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ModAPI;
namespace SuperBlocks.Controller
{
    public static class TurretRegister
    {
        private static Dictionary<IMyEntity, IMyTerminalBlock> Register { get; } = new Dictionary<IMyEntity, IMyTerminalBlock>();
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
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"TurretRegister"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); return false; }
        }
        public static bool IsMainController(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return false;
                if (Register.ContainsKey(TopmostEnt)) return Register[TopmostEnt] == Block;
                Register.Add(TopmostEnt, Block); return true;
            }
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"TurretRegister"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); return false; }
        }
        public static string GetRegistControllerBlockConfig(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return "";
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return Block.CustomData; }
                if (Utils.Common.NullEntity(Register[TopmostEnt])) Register.Remove(TopmostEnt);
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
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"TurretRegister"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); }
        }
        public static void SaveDatas()
        {
            foreach (var KV in Register)
            {
                try
                {
                    if (Utils.Common.NullEntity(KV.Value)) return;
                    KV.Value?.GameLogic?.GetAs<TurretController>()?.SaveData(KV.Value);
                }
                catch (Exception) { }
            }
        }
        public static void SaveDatasExit()
        {
            foreach (var KV in Register)
            {
                try
                {
                    if (Utils.Common.NullEntity(KV.Value)) return;
                    KV.Value?.GameLogic?.GetAs<TurretController>()?.SaveData(KV.Value);
                }
                catch (Exception) { }
            }
        }
    }


    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.Simulation)]
    public sealed class TurretControllerService : MySessionComponentBase
    {

        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!Initialized) return;
                if (!MyGridProgram4ISConvertService.SetupComplete) return;
                if (!SetupComplete) { SetupComplete = true; Init(); return; }
            }
            catch (Exception) { }
        }
        public void Init()
        {
            加载_基础公共控件();
            创建_基础公共属性();
        }
        protected override void UnloadData()
        {
            卸载_基础公共控件();
            TurretRegister.SaveDatasExit();
            base.UnloadData();
        }
        #region 基础公共控件
        private void 加载_基础公共控件()
        {
            /*====================TriggerFunc Hook==================================================*/

            TurretEnabled.TriggerFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.TriggerTurretEnabled(Me);
            AutoFire.TriggerFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.TriggerAutoFire(Me);
            UsingWeaponCoreTracker.TriggerFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.TriggerUsingWeaponCoreTracker(Me);
            RangeMultiply.IncreaseFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Inc(Me);
            RangeMultiply.DecreaseFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Dec(Me);
            RangeMultiply.WriterFunc = (Me, sb) => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Writter(Me, sb);
            /*====================GetterFunc Hook===================================================*/

            TurretEnabled.GetterFunc = Me => (Me?.GameLogic?.GetAs<TurretController>()?.GetterTurretEnabled(Me) ?? false);
            AutoFire.GetterFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.GetterAutoFire(Me) ?? false;
            UsingWeaponCoreTracker.GetterFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.GetterUsingWeaponCoreTracker(Me) ?? false;
            RangeMultiply.GetterFunc = Me => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Getter(Me) ?? 0;
            /*====================SetterFunc Hook===================================================*/

            TurretEnabled.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterTurretEnabled(Me, value);
            AutoFire.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterAutoFire(Me, value);
            UsingWeaponCoreTracker.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterUsingWeaponCoreTracker(Me, value);
            RangeMultiply.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Setter(Me, value);
            /*=======================Terminal Hook==================================================*/

            MyAPIGateway.TerminalControls.CustomControlGetter += TurretEnabled.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += AutoFire.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += UsingWeaponCoreTracker.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += RangeMultiply.CreateController;
            /*=========================Action Hook==================================================*/

            MyAPIGateway.TerminalControls.CustomActionGetter += TurretEnabled.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += AutoFire.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += UsingWeaponCoreTracker.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += RangeMultiply.CreateAction;
        }
        private void 卸载_基础公共控件()
        {
            /*=======================Terminal Hook==================================================*/

            MyAPIGateway.TerminalControls.CustomControlGetter -= TurretEnabled.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= AutoFire.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= UsingWeaponCoreTracker.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= RangeMultiply.CreateController;
            /*=========================Action Hook==================================================*/

            MyAPIGateway.TerminalControls.CustomActionGetter -= TurretEnabled.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= AutoFire.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= UsingWeaponCoreTracker.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= RangeMultiply.CreateAction;
        }
        private void 创建_基础公共属性()
        {
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}TurretEnabled", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false, Me => (Me?.GameLogic?.GetAs<TurretController>()?.GetterTurretEnabled(Me) ?? false), (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterTurretEnabled(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}AutoFire", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<TurretController>()?.GetterAutoFire(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterAutoFire(Me, value));
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"{CtrlNM}UsingWeaponCoreTracker", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<TurretController>()?.GetterUsingWeaponCoreTracker(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.SetterUsingWeaponCoreTracker(Me, value));
            CreateProperty.CreateProperty_PB_CN<float, IMyTerminalBlock>($"{CtrlNM}RangeMultiply", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false, Me => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Getter(Me) ?? 0, (Me, value) => Me?.GameLogic?.GetAs<TurretController>()?.RangeMultiply_Setter(Me, value));
        }
        private CreateTerminalSwitch<IMyTerminalBlock> TurretEnabled { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("TTurretEnabled", "Turret Enabled", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false);
        private CreateTerminalSwitch<IMyTerminalBlock> AutoFire { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("TAutoFire", "Auto Fire", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false);
        private CreateTerminalSwitch<IMyTerminalBlock> UsingWeaponCoreTracker { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("TUsingWeaponCoreTracker", "Focus WC Target", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false);
        private CreateTerminalSliderBar<IMyTerminalBlock> RangeMultiply { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>("TRangeMultiply", "Range Multiply", (Me) => Me?.GameLogic?.GetAs<TurretController>()?.EnabledGUI(Me) ?? false, 0.001f, 1f);
        private const string CtrlNM = "TC_";
        #endregion
    }
}
