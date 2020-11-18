using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "AirplaneControl", "SmallAirplaneControl")]
    public class AeroCtrl : FlyMachineCtrl
    {
        protected override void CreateController()
        {
            Control = new AirplaneController(Entity as IMyTerminalBlock);
            (Control as AirplaneController).EnabledCuriser = false;
            (Control as AirplaneController).EnabledThrusters = true;
            (Control as AirplaneController).MaxiumFlightSpeed = 500f;
            (Control as AirplaneController).SafetyStage = 1f;
            (Control as AirplaneController).MaxReactions_AngleV = 25f;
            (Control as AirplaneController).LocationSensetive = 2.5f;
        }
    }
}
