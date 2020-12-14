using Sandbox.ModAPI;
using System;
namespace SuperBlocks.Controller
{
    public class CreateProperty<ValueType,TBlockType>
    {
        readonly Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalControlProperty<ValueType> Property;
        public CreateProperty(string CtrlID, Func<IMyTerminalBlock, bool> Filter, Func<IMyTerminalBlock, ValueType> GetterFunc, Action<IMyTerminalBlock, ValueType> SetterFunc)
        {
            Property = MyAPIGateway.TerminalControls.CreateProperty<ValueType, TBlockType>($"P_Value_{CtrlID.Replace("Control", "").Replace("control", "").Replace("ID", "").Replace("id", "").Replace("Id", "").Replace("iD", "")}");
            Property.Getter = GetterFunc;
            Property.Setter = SetterFunc;
            Property.Enabled = Filter;
            MyAPIGateway.TerminalControls.AddControl<TBlockType>(Property);
        }
    }
}
