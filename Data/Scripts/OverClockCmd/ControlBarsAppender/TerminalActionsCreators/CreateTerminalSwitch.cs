using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using VRage.Utils;

namespace SuperBlocks.Controller
{
    public class CreateTerminalSwitch : CreateTerminalAction<bool>
    {
        public Action<IMyTerminalBlock> TriggerFunc { get; set; } = (IMyTerminalBlock block) => { };
        public CreateTerminalSwitch(string CtrlID, string Title, Func<ControllerManageBase, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, IMyTerminalBlock>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Enabled = IsinEnabledList;
            triggle.Visible = IsinEnabledList;
            triggle.Title = CtrlNM;
            triggle.OffText = MyStringId.GetOrCompute($"Off");
            triggle.OnText = MyStringId.GetOrCompute($"On");
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(triggle);
            controls.Add(triggle);
        }
        public override void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (DisabledAddAction(block)) { return; }
            ActionsCreated = true;
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<IMyTerminalBlock>(ControlID);
                triggle.Action = TriggerFunc;
                triggle.Enabled = IsinEnabledList;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Start.dds";
                MyAPIGateway.TerminalControls.AddAction<IMyTerminalBlock>(triggle);
                actions.Add(triggle);
            }
            {
                var Property = MyAPIGateway.TerminalControls.CreateProperty<bool, IMyTerminalBlock>($"Value_{ControlID}");
                Property.Getter = GetterFunc;
                Property.Setter = SetterFunc;
                Property.Enabled = IsinEnabledList;
            }
        }
    }
}
