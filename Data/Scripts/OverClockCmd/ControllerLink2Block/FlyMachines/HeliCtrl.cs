using VRage.Game.Components;
using VRage.Game;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "HeilcopterControl", "SmallHeilcopterControl")]
    public class HeliCtrl : FlyMachineCtrl
    {
        protected override void InitController()
        {
            Control = new HelicopterController(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IHeilController).HoverMode = true;
            (Control as IHeilController).MaxiumHoverSpeed = 50f;
        }
        public static bool HoverMode_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<HeliCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is IHeilController)
                return (target.Control as IHeilController).HoverMode;
            return false;
        }
        public static void HoverMode_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<HeliCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).HoverMode = value;
        }
        public static float MaxiumHoverSpeed_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<HeliCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is IHeilController)
                return (target.Control as IHeilController).MaxiumHoverSpeed;
            return 0f;
        }
        public static void MaxiumHoverSpeed_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<HeliCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).MaxiumHoverSpeed = value;
        }
    }
}
