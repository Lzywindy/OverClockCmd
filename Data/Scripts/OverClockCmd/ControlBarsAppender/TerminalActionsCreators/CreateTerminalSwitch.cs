using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using VRage.Utils;
namespace SuperBlocks.Controller
{
    public class CreateTerminalSwitch<TBlockType> : CreateTerminalAction<bool, TBlockType>
    {
        public Action<IMyTerminalBlock> TriggerFunc { get; set; } = (IMyTerminalBlock block) => { };
        public CreateTerminalSwitch(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlOnOffSwitch, TBlockType>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Enabled = Filter;
            triggle.Visible = Filter;
            triggle.Title = CtrlNM;
            triggle.OffText = MyStringId.GetOrCompute($"Off");
            triggle.OnText = MyStringId.GetOrCompute($"On");
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(triggle);
            controls.Add(triggle);
        }
        public override void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (DisabledAddAction(block)) { return; }
            ActionsCreated = true;
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<TBlockType>(ControlID);
                triggle.Action = TriggerFunc;
                triggle.Enabled = Filter;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Start.dds";
                MyAPIGateway.TerminalControls.AddAction<TBlockType>(triggle);
                actions.Add(triggle);
            }
        }
    }
}
