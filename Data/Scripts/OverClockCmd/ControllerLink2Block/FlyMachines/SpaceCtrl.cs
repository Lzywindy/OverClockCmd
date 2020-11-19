using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "SpaceShipControl", "SmallSpaceShipControl")]
    public class SpaceCtrl: FlyMachineCtrl
    {
        protected override void InitController()
        {
            Control = new SpaceShipCtrl(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IPlaneController).MaxiumFlightSpeed = 1800f;
            (Control as IPlaneController).EnabledCuriser = false;
            (Control as IHeilController).HoverMode = true;
            (Control as IHeilController).MaxiumHoverSpeed = 50f;
        }      
    }
}
