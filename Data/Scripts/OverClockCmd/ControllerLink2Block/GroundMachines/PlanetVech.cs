using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    public class PlanetVech : VehcCtrl
    {

        public static float MaximumCruiseSpeed_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<PlanetVech>();
            if (target == null || target.Control == null) return 0;
            if (target.Control is IPlanetVehicle)
                return (target.Control as IPlanetVehicle).MaximumCruiseSpeed;
            return 0;
        }
        public static void MaximumCruiseSpeed_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<PlanetVech>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlanetVehicle)
                (target.Control as IPlanetVehicle).MaximumCruiseSpeed = value;
        }
        public static void MaximumCruiseSpeed_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<PlanetVech>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlanetVehicle)
                (target.Control as IPlanetVehicle).MaximumCruiseSpeed += 10;
        }
        public static void MaximumCruiseSpeed_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<PlanetVech>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlanetVehicle)
                (target.Control as IPlanetVehicle).MaximumCruiseSpeed -= 10;
        }

        protected override void InitController()
        {
            Control = new PlanetVehicle(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IPlanetVehicle).MaximumCruiseSpeed = 0;
        }
    }
}
