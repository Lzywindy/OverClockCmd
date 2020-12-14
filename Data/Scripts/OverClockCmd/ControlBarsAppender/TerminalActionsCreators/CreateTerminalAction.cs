using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;
using System.Collections.Generic;
using System.Text;
using System;
namespace SuperBlocks.Controller
{
    public class CreateTerminalAction<ValueType, TBlockType>
    {
        public CreateTerminalAction(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter = null)
        {
            ControlID = CtrlID;
            this.Filter = Filter ?? ((IMyTerminalBlock block) => { return true; });
            this.Title = Title;
        }
        public virtual void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls) { }
        public virtual void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions) { }
        protected string ControlID { get; set; }
        protected bool ControlsCreated { get; set; } = false;
        protected bool ActionsCreated { get; set; } = false;
        protected MyStringId CtrlNM { get { return MyStringId.GetOrCompute(Title); } }
        protected StringBuilder CtrlNM_S { get { return new StringBuilder(Title); } }
        public Func<IMyTerminalBlock, ValueType> GetterFunc { get; set; } = (IMyTerminalBlock block) => { return default(ValueType); };
        public Action<IMyTerminalBlock, ValueType> SetterFunc { get; set; } = (IMyTerminalBlock block, ValueType value) => { };
        protected bool DisabledAddControl(IMyTerminalBlock block) { return (ControlsCreated || (!Filter(block))); }
        protected bool DisabledAddAction(IMyTerminalBlock block) { return (ActionsCreated || (!Filter(block))); }
        protected string Title { get; private set; }
        protected CreateProperty<ValueType, TBlockType> property { get; set; }
        public Func<IMyTerminalBlock, bool> Filter { get; private set; }
    }
}
