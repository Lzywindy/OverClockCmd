using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "VTOLControl", "SmallVTOLControl")]
    public class VTOLCtrl : SpaceCtrl
    {
        protected override void InitController()
        {
            Control = new VTOLController(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IWingModeController).HasWings = false;
        }
    }
}
