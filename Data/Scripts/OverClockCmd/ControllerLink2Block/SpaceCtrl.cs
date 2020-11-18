using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "SpaceShipControl", "SmallSpaceShipControl")]
    public class SpaceCtrl: FlyMachineCtrl
    {
        protected override void CreateController()
        {
            Control = new SpaceShipCtrl(Entity as IMyTerminalBlock);
            (Control as SpaceShipCtrl).EnabledCuriser = false;
            (Control as SpaceShipCtrl).HoverMode = true;
            (Control as SpaceShipCtrl).EnabledThrusters = true;
            (Control as SpaceShipCtrl).MaxiumFlightSpeed = 1800f;
            (Control as SpaceShipCtrl).MaxiumHoverSpeed = 50f;
            (Control as SpaceShipCtrl).SafetyStage = 1f;
            (Control as SpaceShipCtrl).MaxReactions_AngleV = 25f;
            (Control as SpaceShipCtrl).LocationSensetive = 2.5f;
        }
    }
}
