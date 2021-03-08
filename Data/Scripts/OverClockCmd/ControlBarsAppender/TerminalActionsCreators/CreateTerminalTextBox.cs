using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using System;
namespace SuperBlocks.Controller
{
    public class CreateTerminalTextBox<TBlockType> : CreateTerminalAction<StringBuilder, TBlockType>
    {
        public CreateTerminalTextBox(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlTextbox, TBlockType>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Enabled = Filter;
            triggle.Visible = Filter;
            triggle.Title = CtrlNM;
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(triggle);
            controls.Add(triggle);
        }
    }

}
