using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "ShipControl", "SmallShipControl")]
    public class ShipCtrl : PlanetVech
    {
        public static bool IsSubmarine_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<ShipCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is ISeaVehicle)
                return (target.Control as ISeaVehicle).IsSubmarine;
            return false;
        }
        public static void IsSubmarine_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<ShipCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is ISeaVehicle)
                (target.Control as ISeaVehicle).IsSubmarine = value;
        }
        public static void IsSubmarine_Trigger(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<ShipCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control == null) return;
            if (target.Control is ISeaVehicle)
                (target.Control as ISeaVehicle).IsSubmarine = !(target.Control as ISeaVehicle).IsSubmarine;
        }
        protected override void InitController()
        {
            Control = new ShipController(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as ISeaVehicle).IsSubmarine = false;
        }
    }
}
