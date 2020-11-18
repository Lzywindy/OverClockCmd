using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TankTrackControl", "SmallTankTrackControl")]
    public class TankCtrl : VehcCtrl
    {
        protected override void CreateController()
        {
            Control = new TankController(Entity as IMyTerminalBlock);
            (Control as TankController).EnabledThrusters = true;
            (Control as TankController).EnabledGyros = true;
        }
    }
}
