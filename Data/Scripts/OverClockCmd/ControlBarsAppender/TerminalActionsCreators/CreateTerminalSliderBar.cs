using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using System;
namespace SuperBlocks.Controller
{
    public class CreateTerminalSliderBar<TBlockType> : CreateTerminalAction<float, TBlockType>
    {
        public CreateTerminalSliderBar(string CtrlID, string Title, Func<IMyTerminalBlock, bool> Filter, float Min, float Max) : base(CtrlID, Title, Filter) { this.Min = Min; this.Max = Max; }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
            if (DisabledAddControl(block)) { return; }
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, TBlockType>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Writer = WriterFunc;
            triggle.SetLimits(Min, Max);
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
            property = new CreateProperty<float, TBlockType>(ControlID, Filter, GetterFunc, SetterFunc);
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<TBlockType>($"{ControlID} Increase");
                triggle.Action = IncreaseFunc;
                triggle.Enabled = Filter;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
                MyAPIGateway.TerminalControls.AddAction<TBlockType>(triggle);
                actions.Add(triggle);
            }
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<TBlockType>($"{ControlID} Decrease");
                triggle.Action = DecreaseFunc;
                triggle.Enabled = Filter;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
                MyAPIGateway.TerminalControls.AddAction<TBlockType>(triggle);
                actions.Add(triggle);
            }
        }
        public Action<IMyTerminalBlock> IncreaseFunc { get; set; } = (IMyTerminalBlock block) => { };
        public Action<IMyTerminalBlock> DecreaseFunc { get; set; } = (IMyTerminalBlock block) => { };
        public Action<IMyTerminalBlock, StringBuilder> WriterFunc { get; set; } = (IMyTerminalBlock block, StringBuilder strbuild) => { };
        private float Min, Max;
    }
}
