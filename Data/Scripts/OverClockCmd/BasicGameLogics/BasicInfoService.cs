using System;
using VRage.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public sealed class BasicInfoService : MySessionComponentBase
    {
        public static HashSet<IMyTerminalBlock> Register { get; } = new HashSet<IMyTerminalBlock>();
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!SetupComplete)
            {
                SetupComplete = true;
                Init();
                return;
            }
        }
        public void Init()
        {
            允许运行.GetterFunc = MySuperBlockProgram.Get_Enabled;
            允许运行.SetterFunc = MySuperBlockProgram.Set_Enabled;
            允许运行.TriggerFunc = MySuperBlockProgram.TriggeEnabled;
            重启.TriggerFunc = MySuperBlockProgram.Trigger_Restart;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 允许运行.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 允许运行.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 重启.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 重启.CreateAction;
        }
        protected sealed override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 允许运行.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 允许运行.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 重启.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 重启.CreateAction;
            if (Register.Count < 1) return;
            foreach (var item in Register) { try { MySuperBlockProgram.GetLogic<MySuperBlockProgram>(item)?.SaveData(); } catch (Exception) { } }
        }
        private CreateTerminalFence<IMyTerminalBlock> Fence_0 { get; } = new CreateTerminalFence<IMyTerminalBlock>(MySuperBlockProgram.ActionsEnabled);
        private CreateTerminalSwitch<IMyTerminalBlock> 允许运行 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(MySuperBlockProgram.ActionID, MySuperBlockProgram.ActionNM, MySuperBlockProgram.ActionsEnabled);
        private CreateTerminalButton<IMyTerminalBlock> 重启 { get; } = new CreateTerminalButton<IMyTerminalBlock>("ControlRestartID", "Restart Device", MySuperBlockProgram.ActionsEnabled);
    }
}