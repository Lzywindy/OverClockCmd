using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "VTOLControl", "SmallVTOLControl")]
    public class VTOLCtrl : FlyMachineCtrl
    {
        protected override void CreateController()
        {
            Control = new VTOLController(Entity as IMyTerminalBlock);
            (Control as VTOLController).EnabledCuriser = false;
            (Control as VTOLController).HasWings = false;
            (Control as VTOLController).HoverMode = true;
            (Control as VTOLController).EnabledThrusters = true;
            (Control as VTOLController).MaxiumFlightSpeed = 2000f;
            (Control as VTOLController).MaxiumHoverSpeed = 50f;
            (Control as VTOLController).MaxReactions_AngleV = 25f;
            (Control as VTOLController).SafetyStage = 1f;
            (Control as VTOLController).LocationSensetive = 2.5f;
        }
    }
}
