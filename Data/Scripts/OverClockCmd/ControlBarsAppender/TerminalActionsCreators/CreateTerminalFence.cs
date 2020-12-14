using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
namespace SuperBlocks.Controller
{
    public class CreateTerminalFence<TBlockType>
    {
        protected static HashSet<string> RegList { get; } = new HashSet<string>();
        protected const string FenceID = @"FenceID";
        public static int count = 0;
        public CreateTerminalFence(Func<IMyTerminalBlock, bool> Filter = null)
        {
            ControlID = $"{FenceID}{count++}";
            while (!RegList.Add(ControlID))
                count++;
            if (Filter != null) this.Filter = Filter;
        }
        public void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var separator = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSeparator, TBlockType>(ControlID);
            separator.Enabled = Filter;
            separator.Visible = Filter;
            separator.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(separator);
            controls.Add(separator);
        }
        protected string ControlID { get; set; }
        protected bool ControlsCreated { get; set; } = false;
        protected bool DisabledAddControl(IMyTerminalBlock block) { return (ControlsCreated || (!Filter(block))); }
        public Func<IMyTerminalBlock, bool> Filter { get; private set; } = (IMyTerminalBlock block) => { return true; };
    }
}
