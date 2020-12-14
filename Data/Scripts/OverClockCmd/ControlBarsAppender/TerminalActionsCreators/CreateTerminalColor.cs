using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System;
using VRageMath;
namespace SuperBlocks.Controller
{
    public class CreateTerminalColor<TBlockType> : CreateTerminalAction<Color, TBlockType>
    {
        public CreateTerminalColor(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter) : base(CtrlID, Title, Filter) { }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlColor, TBlockType>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Enabled = Filter;
            triggle.Visible = Filter;
            triggle.Title = CtrlNM;
            triggle.SupportsMultipleBlocks = true;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(triggle);
            controls.Add(triggle);
        }
        public override void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions)
        {
            if (DisabledAddAction(block)) { return; }
            ActionsCreated = true;
            property = new CreateProperty<Color, TBlockType>(ControlID, Filter, GetterFunc, SetterFunc);
        }
    }
}
