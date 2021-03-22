using Sandbox.ModAPI;
using System;
namespace SuperBlocks.Controller
{
    public class CreateProperty<ValueType, TBlockType>
    {
        readonly Sandbox.ModAPI.Interfaces.Terminal.IMyTerminalControlProperty<ValueType> Property;
        public CreateProperty(string CtrlID, Func<IMyTerminalBlock, bool> Filter, Func<IMyTerminalBlock, ValueType> GetterFunc, Action<IMyTerminalBlock, ValueType> SetterFunc)
        {
            Property = MyAPIGateway.TerminalControls.CreateProperty<ValueType, TBlockType>($"P_Value_{CtrlID.Replace("Control", "").Replace("control", "").Replace("ID", "").Replace("id", "").Replace("Id", "").Replace("iD", "")}");
            Property.Getter = GetterFunc;
            Property.Setter = SetterFunc;
            Property.Enabled = Filter;
            Property.Visible = Filter;
            try { MyAPIGateway.TerminalControls.RemoveControl<IMyTerminalBlock>(Property); } catch (Exception) { }
            try { MyAPIGateway.TerminalControls.AddControl<IMyTerminalBlock>(Property); } catch (Exception) { }
        }
    }
    public static class CreateProperty
    {
        public static void CreateProperty_PB<ValueType, TBlock>(string CtrlID, Func<IMyTerminalBlock, bool> Filter, Func<IMyTerminalBlock, ValueType> GetterFunc, Action<IMyTerminalBlock, ValueType> SetterFunc)
        {
            var Property = MyAPIGateway.TerminalControls.CreateProperty<ValueType, TBlock>($"LZYAPIs_{CtrlID.Replace("Control", "").Replace("control", "").Replace("ID", "").Replace("id", "").Replace("Id", "").Replace("iD", "")}");
            Property.Getter = GetterFunc;
            Property.Setter = SetterFunc;
            Property.Enabled = Filter;
            Property.Visible = Filter;
            try { MyAPIGateway.TerminalControls.RemoveControl<TBlock>(Property); } catch (Exception) { }
            try { MyAPIGateway.TerminalControls.AddControl<TBlock>(Property); } catch (Exception) { }
        }
        public static void CreateProperty_PB_CN<ValueType, TBlock>(string CtrlID, Func<IMyTerminalBlock, bool> Filter, Func<IMyTerminalBlock, ValueType> GetterFunc, Action<IMyTerminalBlock, ValueType> SetterFunc)
        {
            var Property = MyAPIGateway.TerminalControls.CreateProperty<ValueType, TBlock>(CtrlID);
            Property.Getter = GetterFunc;
            Property.Setter = SetterFunc;
            Property.Enabled = Filter;
            Property.Visible = Filter;
            try { MyAPIGateway.TerminalControls.RemoveControl<TBlock>(Property); } catch (Exception) { }
            try { MyAPIGateway.TerminalControls.AddControl<TBlock>(Property); } catch (Exception) { }
        }
        public static void CreateProperty_PB_IN<ValueType, TBlock>(string CtrlID, Func<IMyTerminalBlock, bool> Filter, Func<IMyTerminalBlock, ValueType> GetterFunc, Action<IMyTerminalBlock, ValueType> SetterFunc)
        {
            var Property = MyAPIGateway.TerminalControls.CreateProperty<ValueType, IMyTerminalBlock>(CtrlID);
            Property.Getter = GetterFunc;
            Property.Setter = SetterFunc;
            Property.Enabled = Filter;
            Property.Visible = Filter;
            try { MyAPIGateway.TerminalControls.RemoveControl<TBlock>(Property); } catch (Exception) { }
            try { MyAPIGateway.TerminalControls.AddControl<TBlock>(Property); } catch (Exception) { }
        }
    }
}
