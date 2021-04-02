using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
namespace SuperBlocks.Controller
{
    public partial class MyGridProgram4ISConvert : MyGameLogicComponent
    {
        protected virtual void Program() { }
        protected virtual void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource) { }
        protected virtual void CustomDataChangedProcess() { }
        protected virtual void InitBlock() { }
        protected virtual void ClosedBlock() { }
        protected virtual void UpdateState() { }
        protected virtual void LoadData() { }
        protected virtual void SaveData() { }
        private void UpdateGridGroup()
        {
            try
            {
                if (Utils.Common.NullEntity(Me?.CubeGrid)) return;
                MyAPIGateway.GridGroups.GetGroup(Me.CubeGrid, GridLinkTypeEnum.Mechanical, CurrentGridGroup);
                CurrentGridGroup.Add(Me.CubeGrid);
            }
            catch (Exception) { }
        }
        protected bool InThisEntity(IMyTerminalBlock block)
        {
            if (Utils.Common.NullEntity(block?.CubeGrid)) return false;
            return CurrentGridGroup.Contains(block.CubeGrid);
        }
    }
    public partial class MyGridProgram4ISConvert
    {
        protected IMyGridTerminalSystem GridTerminalSystem => MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid);
        protected IMyTerminalBlock Me => Entity as IMyTerminalBlock;
        protected void Echo(string value) => ShowText.AppendLine(value);
        protected Sandbox.ModAPI.Ingame.UpdateFrequency UpdateFrequency { get; set; } = Sandbox.ModAPI.Ingame.UpdateFrequency.None;
        private StringBuilder ShowText { get; } = new StringBuilder();
        protected bool EnabledRunning { get; private set; } = false;
        private IMyTerminalBlock ThisBlock => Entity as IMyTerminalBlock;
        protected HashSet<IMyCubeGrid> CurrentGridGroup { get; } = new HashSet<IMyCubeGrid>();
        protected Utils.MyEventParameter_Bool Enabled { get; } = new Utils.MyEventParameter_Bool(true);
        protected ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        protected Action OnRestart = () => { };
        protected Action OnRunning1 = () => { };
        protected Action OnRunning10 = () => { };
        protected Action OnRunning100 = () => { };
    }
    public partial class MyGridProgram4ISConvert
    {
        public sealed override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            Restart(Me);
            ThisBlock.AppendingCustomInfo += ThisBlock_AppendingCustomInfo;
            ThisBlock.CustomDataChanged += ThisBlock_CustomDataChanged;
            Enabled.OnValueChanged += UpdateState;
            InitBlock();
            NeedsUpdate |= (MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        public sealed override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            try
            {
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None || (!EnabledRunning)) return;
                try { OnRestart?.Invoke(); } catch (Exception) { }
                if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Once))
                    Main("", Sandbox.ModAPI.Ingame.UpdateType.Once);
            }
            catch (Exception) { EnabledRunning = false; }
        }
        public sealed override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            try
            {
                if (!EnabledRunning) return;
                try { OnRunning1?.Invoke(); } catch (Exception) { }
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None) return;
                if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Update1))
                    Main("", Sandbox.ModAPI.Ingame.UpdateType.Update1);
            }
            catch (Exception) { EnabledRunning = false; }
        }
        public sealed override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            try
            {
                UpdateGridGroup();
                if (!EnabledRunning) return;
                try { OnRunning10?.Invoke(); } catch (Exception) { }
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None) return;
                if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Update10))
                    Main("", Sandbox.ModAPI.Ingame.UpdateType.Update10);
            }
            catch (Exception) { EnabledRunning = false; }
        }
        public sealed override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            try
            {
                if (!EnabledRunning) return;
                try { OnRunning100?.Invoke(); } catch (Exception) { }
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None) return;
                if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Update100))
                    Main("", Sandbox.ModAPI.Ingame.UpdateType.Update100);
            }
            catch (Exception) { EnabledRunning = false; }
            try { OnRestart?.Invoke(); } catch (Exception) { }
        }
        public sealed override void Close()
        {
            base.Close(); try
            {
                ThisBlock.AppendingCustomInfo -= ThisBlock_AppendingCustomInfo;
                ThisBlock.CustomDataChanged -= ThisBlock_CustomDataChanged; ClosedBlock();
                Enabled.OnValueChanged -= UpdateState;
            }
            catch (Exception)
            {
                EnabledRunning = false;
            }
        }
        private void ThisBlock_AppendingCustomInfo(IMyTerminalBlock block, StringBuilder info) { info.Clear(); info.Append(ShowText.ToString()); }
        private void ThisBlock_CustomDataChanged(IMyTerminalBlock obj) { try { CustomDataChangedProcess(); } catch (Exception) { } }
        public void ModRunningProgram(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Mod); } catch (Exception) { EnabledRunning = false; } }
        public void Trigger(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Trigger); } catch (Exception) { EnabledRunning = false; } }
        public void Terminal(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Terminal); } catch (Exception) { EnabledRunning = false; } }
    }
    public partial class MyGridProgram4ISConvert
    {
        public void TriggleEnabled(IMyTerminalBlock Me) { if (!EnabledGUI(Me)) return; Enabled.Value = !Enabled.Value; }
        public void EnabledSetter(IMyTerminalBlock Me, bool value) { if (!EnabledGUI(Me)) return; Enabled.Value = value; }
        public bool EnabledGetter(IMyTerminalBlock Me) { if (!EnabledGUI(Me)) return false; return Enabled.Value; }
        public void Restart(IMyTerminalBlock Me) { if (!EnabledGUI(Me)) return; try { UpdateGridGroup(); OnRestart?.Invoke(); LoadData(Me); SaveData(Me); Program(); EnabledRunning = true; } catch (Exception) { EnabledRunning = false; } }
        public void LoadData(IMyTerminalBlock Me) { if (EnabledGUI(Me)) { Configs.Clear(); MyConfigs.Concurrent.CustomDataConfigRead_INI(Me, Configs); LoadData(); } }
        public void SaveData(IMyTerminalBlock Me) { if (EnabledGUI(Me)) { SaveData(); Me.CustomData = MyConfigs.Concurrent.CustomDataConfigSave_INI(Configs); } }
        public bool EnabledGUI(IMyTerminalBlock Me) { return Me == this.Me; }
        public static void LoadupInterface()
        {
            EnabledCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.TriggleEnabled(Me);
            EnabledCtrl.GetterFunc = (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGetter(Me) ?? false;
            EnabledCtrl.SetterFunc = (Me, value) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledSetter(Me, value);
            RestartCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.Restart(Me);
            LoadCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.LoadData(Me);
            SaveCtrl.TriggerFunc = (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.SaveData(Me);
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += EnabledCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += RestartCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += LoadCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += SaveCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += EnabledCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += RestartCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += LoadCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += SaveCtrl.CreateAction;
            CreateProperty.CreateProperty_PB_CN<bool, IMyTerminalBlock>($"Enabled", Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false, (Me) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGetter(Me) ?? false, (Me, value) => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledSetter(Me, value));
        }
        public static void UnloadInterface()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= EnabledCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= RestartCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= LoadCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= SaveCtrl.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= EnabledCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= RestartCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= LoadCtrl.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= SaveCtrl.CreateAction;
        }
        private static CreateTerminalFence<IMyTerminalBlock> Fence_0 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalSwitch<IMyTerminalBlock> EnabledCtrl { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("EnabledID", "Enabled", Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalButton<IMyTerminalBlock> RestartCtrl { get; } = new CreateTerminalButton<IMyTerminalBlock>("RestartID", "Restart", Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalButton<IMyTerminalBlock> LoadCtrl { get; } = new CreateTerminalButton<IMyTerminalBlock>("LoadConfigID", "Load Config", Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalButton<IMyTerminalBlock> SaveCtrl { get; } = new CreateTerminalButton<IMyTerminalBlock>("SaveConfigID", "Save Config", Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
        private static CreateTerminalFence<IMyTerminalBlock> Fence_1 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Me => Me?.GameLogic?.GetAs<MyGridProgram4ISConvert>()?.EnabledGUI(Me) ?? false);
    }
}
