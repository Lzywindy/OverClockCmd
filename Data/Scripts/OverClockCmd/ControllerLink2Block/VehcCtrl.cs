using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    public class VehcCtrl : ControlBase
    {
        public bool EnabledThrusters_Getter(IMyTerminalBlock block)
        {
            if (block.EntityId != Entity.EntityId) return false;
            if (Control is ICtrlDevCtrl)
                return (Control as ICtrlDevCtrl).EnabledThrusters;
            return false;
        }
        public void EnabledThrusters_Setter(IMyTerminalBlock block, bool Value)
        {
            if (block.EntityId != Entity.EntityId) return;
            if (Control is ICtrlDevCtrl)
                (Control as ICtrlDevCtrl).EnabledThrusters = Value;
        }
        protected override void CreateController()
        {
            Control = new VehicleControllerBase(Entity as IMyTerminalBlock);
            (Control as VehicleControllerBase).EnabledThrusters = true;
            (Control as VehicleControllerBase).EnabledGyros = true;
        }
    }
}
