using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using System.Collections.Generic;
using System.Text;
using System;

namespace SuperBlocks.Controller
{
    public class CreateTerminalSliderBar : CreateTerminalAction<float>
    {       
        public CreateTerminalSliderBar(string CtrlID, string Title, Func<ControllerManageBase, bool> Filter,float Min,float Max) : base(CtrlID, Title, Filter) { this.Min = Min;this.Max = Max; }
        public override void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls)
        {
           if (DisabledAddControl(block)) { return; }
           
            ControlsCreated = true;
            var triggle = MyAPIGateway.TerminalControls.CreateControl<IMyTerminalControlSlider, IMyTerminalBlock>(ControlID);
            triggle.Getter = GetterFunc;
            triggle.Setter = SetterFunc;
            triggle.Writer = WriterFunc;
            triggle.SetLimits(Min, Max);
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
                var Property = MyAPIGateway.TerminalControls.CreateProperty<float, IMyTerminalBlock>($"Value_{ControlID}");
                Property.Getter = GetterFunc;
                Property.Setter = SetterFunc;
                Property.Enabled = IsinEnabledList;
            }
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<IMyTerminalBlock>($"{ControlID} Increase");
                triggle.Action = IncreaseFunc;
                triggle.Enabled = IsinEnabledList;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Increase.dds";
                MyAPIGateway.TerminalControls.AddAction<IMyTerminalBlock>(triggle);
                actions.Add(triggle);
            }
            {
                var triggle = MyAPIGateway.TerminalControls.CreateAction<IMyTerminalBlock>($"{ControlID} Decrease");
                triggle.Action = DecreaseFunc;
                triggle.Enabled = IsinEnabledList;
                triggle.Name = CtrlNM_S;
                triggle.Icon = @"Textures\GUI\Icons\Actions\Decrease.dds";
                MyAPIGateway.TerminalControls.AddAction<IMyTerminalBlock>(triggle);
                actions.Add(triggle);
            }
        }
        public Action<IMyTerminalBlock> IncreaseFunc { get; set; } = (IMyTerminalBlock block) => { };
        public Action<IMyTerminalBlock> DecreaseFunc { get; set; } = (IMyTerminalBlock block) => { };
        public Action<IMyTerminalBlock, StringBuilder> WriterFunc { get; set; } = (IMyTerminalBlock block,StringBuilder strbuild) => { };
        private float Min, Max;
    }
}
