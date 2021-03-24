using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System;
using System.Collections.Generic;
namespace SuperBlocks.Controller
{
    public class CreateTerminalButton<TBlockType> : CreateTerminalAction<bool, TBlockType>
    {
        public Action<IMyTerminalBlock> TriggerFunc { get; set; } = (IMyTerminalBlock block) => { };
        public CreateTerminalButton(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter = null) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlButton, TBlockType>(ControlID);
            triggle.Enabled = Filter;
            triggle.Visible = Filter;
            triggle.Title = CtrlNM;
            triggle.Action = TriggerFunc;
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
