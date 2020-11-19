using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "AirplaneControl", "SmallAirplaneControl")]
    public class AeroCtrl : FlyMachineCtrl
    {
        protected override void InitController()
        {
            Control = new AirplaneController(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IPlaneController).MaxiumFlightSpeed = 500f;
            (Control as IPlaneController).EnabledCuriser = false;
            (Control as IWingModeController).HasWings = false;
        }      
    }
}
