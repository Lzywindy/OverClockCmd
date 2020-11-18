using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "HeilcopterControl", "SmallHeilcopterControl")]
    public class HeliCtrl : FlyMachineCtrl
    {
        protected override void CreateController()
        {
            Control = new HelicopterController(Entity as IMyTerminalBlock);
            (Control as HelicopterController).EnabledThrusters = true;
            (Control as HelicopterController).MaxiumHoverSpeed = 50f;
            (Control as HelicopterController).SafetyStage = 1f;
            (Control as HelicopterController).MaxReactions_AngleV = 25f;
            (Control as HelicopterController).LocationSensetive = 2.5f;
        }
    }
}
