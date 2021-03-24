using VRage.Game;
using VRage.Game.Components;
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TurretController", "SmallTurretController")]
    public class TurretController : MyGridProgram4ISConvert
    {
        protected override void Program()
        {
        }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource)
        {
        }
        protected override void ClosedBlock()
        {
        }
        protected override void CustomDataChangedProcess()
        {
        }
    }
}
