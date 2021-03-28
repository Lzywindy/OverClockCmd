using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
namespace SuperBlocks.Controller
{
    public class MyGridProgram4ISConvert : MyGameLogicComponent
    {
        #region 程序基本运行框架
        public sealed override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            ThisBlock = Me;
            try { Program(); EnabledRunning = true; UpdateGridGroup(); } catch (Exception) { EnabledRunning = false; }
            ThisBlock.AppendingCustomInfo += ThisBlock_AppendingCustomInfo;
            ThisBlock.CustomDataChanged += ThisBlock_CustomDataChanged;
            NeedsUpdate |= (MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        public sealed override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            try
            {
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None || (!EnabledRunning)) return;
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
                if (!(UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None || (!EnabledRunning)))
                {
                    if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Update1))
                        Main("", Sandbox.ModAPI.Ingame.UpdateType.Update1);
                }
                //if (MyAPIGateway.Gui.GetCurrentScreen == VRage.Game.ModAPI.MyTerminalPageEnum.ControlPanel)
                //    ThisBlock.RefreshCustomInfo();
            }
            catch (Exception) { EnabledRunning = false; }
        }
        public sealed override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            try
            {
                UpdateGridGroup();
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None || (!EnabledRunning)) return;
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
                if (!EnabledRunning) { Program(); EnabledRunning = true; return; }
                if (UpdateFrequency == Sandbox.ModAPI.Ingame.UpdateFrequency.None) return;
                if (UpdateFrequency.HasFlag(Sandbox.ModAPI.Ingame.UpdateFrequency.Update100))
                    Main("", Sandbox.ModAPI.Ingame.UpdateType.Update100);
            }
            catch (Exception) { EnabledRunning = false; }
        }
        public sealed override void Close() { base.Close(); try { ClosedBlock(); ThisBlock.AppendingCustomInfo -= ThisBlock_AppendingCustomInfo; } catch (Exception) { EnabledRunning = false; } }
        #endregion
        #region 可以自定义的运行功能
        protected virtual void Program() { }
        protected virtual void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource) { }
        protected virtual void ClosedBlock() { }
        protected virtual void CustomDataChangedProcess() { }
        #endregion
        #region 辅助运行功能
        private void ThisBlock_AppendingCustomInfo(IMyTerminalBlock block, StringBuilder info) { info.Clear(); info.Append(ShowText.ToString()); }
        private void ThisBlock_CustomDataChanged(IMyTerminalBlock obj) { try { CustomDataChangedProcess(); } catch (Exception) { } }
        public void ModRunningProgram(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Mod); } catch (Exception) { EnabledRunning = false; } }
        public void Trigger(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Trigger); } catch (Exception) { EnabledRunning = false; } }
        public void Terminal(string argument) { try { Main(argument, Sandbox.ModAPI.Ingame.UpdateType.Terminal); } catch (Exception) { EnabledRunning = false; } }
        #endregion
        #region 运行中的私有参数
        protected IMyGridTerminalSystem GridTerminalSystem => MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid);
        protected IMyTerminalBlock Me => Entity as IMyTerminalBlock;
        protected void Echo(string value) => ShowText.AppendLine(value);
        protected Sandbox.ModAPI.Ingame.UpdateFrequency UpdateFrequency { get; set; } = Sandbox.ModAPI.Ingame.UpdateFrequency.None;
        private StringBuilder ShowText { get; } = new StringBuilder();
        private bool EnabledRunning { get; set; } = false;
        private IMyTerminalBlock ThisBlock;
        protected HashSet<IMyCubeGrid> CurrentGridGroup { get; } = new HashSet<IMyCubeGrid>();
        #endregion
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
}
