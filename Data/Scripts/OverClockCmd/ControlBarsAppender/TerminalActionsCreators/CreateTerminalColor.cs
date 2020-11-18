using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using VRageMath;

namespace SuperBlocks.Controller
{
    public class CreateTerminalColor : CreateTerminalAction<Color>
    {
        public CreateTerminalColor(string CtrlID, string Title, Func<ControllerManageBase, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }

            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, IMyTerminalBlock>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Enabled = IsinEnabledList;
            triggle.Visible = IsinEnabledList;
            triggle.Title = CtrlNM;
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(triggle);
            controls.Add(triggle);
        }
        public override void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {

            if (DisabledAddAction(block)) { return; }
            ActionsCreated = true;
            {
                var Property = MyAPIGateway.TerminalControls.CreateProperty<Color, IMyTerminalBlock>($"Value_{ControlID}");
                Property.Getter = GetterFunc;
                Property.Setter = SetterFunc;
                Property.Enabled = IsinEnabledList;
            }
        }
    }
}
