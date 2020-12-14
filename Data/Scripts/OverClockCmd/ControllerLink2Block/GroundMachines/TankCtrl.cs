using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TankTrackControl", "SmallTankTrackControl")]
    public class TankCtrl : PlanetVech
    {
        public static bool IsTank_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<TankCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control == null) return false;
            if (target.Control is ILandVehicle)
                return (target.Control as ILandVehicle).IsTank;
            return false;
        }
        public static void IsTank_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<TankCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control == null) return;
            if (target.Control is ILandVehicle)
                (target.Control as ILandVehicle).IsTank = value;
        }
        public static void IsTank_Trigger(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<TankCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control == null) return;
            if (target.Control is ILandVehicle)
                (target.Control as ILandVehicle).IsTank = !(target.Control as ILandVehicle).IsTank;
        }
        protected override void InitController()
        {
            Control = new TankController(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as ILandVehicle).IsTank = false;
            (Control as IPlanetVehicle).MaximumCruiseSpeed = 80f;
        }
    }
}
