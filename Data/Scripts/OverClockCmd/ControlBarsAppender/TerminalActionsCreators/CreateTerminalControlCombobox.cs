using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
namespace SuperBlocks.Controller
{
    public class CreateTerminalControlCombobox<TBlockType> : CreateTerminalAction<long, TBlockType>
    {
        public CreateTerminalControlCombobox(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlCombobox, TBlockType>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.ComboBoxContent = ComboBoxContent;
            triggle.Enabled = Filter;
            triggle.Visible = Filter;
            triggle.Title = CtrlNM;
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(triggle);
            controls.Add(triggle);
        }
        public Action<List<VRage.ModAPI.MyTerminalControlComboBoxItem>> ComboBoxContent { get; set; }
    }
}
