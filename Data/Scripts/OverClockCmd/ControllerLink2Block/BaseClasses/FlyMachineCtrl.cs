using Sandbox.ModAPI;
using VRageMath;

namespace SuperBlocks.Controller
{
    public partial class FlyMachineCtrl : VehcCtrl
    {

        protected override void InitController()
        {
            Control = new FlyingMachineCtrl_Base(Entity as IMyTerminalBlock);
        }
        protected override void InitParameters()
        {
            base.InitParameters();
            (Control as IPoseParamAdjust).SafetyStage = 1f;
            (Control as IPoseParamAdjust).MaxReactions_AngleV = 25f;
            (Control as IPoseParamAdjust).LocationSensetive = 1.5f;
        }
        #region UsefulFunctions
        public static float SafetyStage_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0;
            if (target.Control is IPoseParamAdjust)
                return (target.Control as IPoseParamAdjust).SafetyStage;
            return 0;
        }
        public static void SafetyStage_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).SafetyStage = value;
        }
        public static void SafetyStage_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).SafetyStage += 1;
        }
        public static void SafetyStage_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).SafetyStage -= 1;
        }
        public static float LocationSensetive_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0;
            if (target.Control is IPoseParamAdjust)
                return (target.Control as IPoseParamAdjust).LocationSensetive;
            return 0;
        }
        public static void LocationSensetive_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).LocationSensetive = value;
        }
        public static void LocationSensetive_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).SafetyStage += 0.1f;
        }
        public static void LocationSensetive_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).SafetyStage -= 0.1f;
        }
        public static float MaxReactions_AngleV_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0;
            if (target.Control is IPoseParamAdjust)
                return (target.Control as IPoseParamAdjust).MaxReactions_AngleV;
            return 0;
        }
        public static void MaxReactions_AngleV_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).MaxReactions_AngleV = value;
        }
        public static void MaxReactions_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).MaxReactions_AngleV += 1; ;
        }
        public static void MaxReactions_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPoseParamAdjust)
                (target.Control as IPoseParamAdjust).MaxReactions_AngleV -= 1; ;
        }
        #endregion
    }
    public partial class FlyMachineCtrl
    {
        public static bool HoverMode_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is IHeilController)
                return (target.Control as IHeilController).HoverMode;
            return false;
        }
        public static void HoverMode_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).HoverMode = value;
        }
        public static void HoverMode_Trigger(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).HoverMode = !(target.Control as IHeilController).HoverMode;
        }
        public static float MaxiumHoverSpeed_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is IHeilController)
                return (target.Control as IHeilController).MaxiumHoverSpeed;
            return 0f;
        }
        public static void MaxiumHoverSpeed_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).MaxiumHoverSpeed = value;
        }
        public static void MaxiumHoverSpeed_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).MaxiumHoverSpeed += 10;
        }
        public static void MaxiumHoverSpeed_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IHeilController)
                (target.Control as IHeilController).MaxiumHoverSpeed -= 10;
        }
        public static bool EnabledCuriser_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is IPlaneController)
                return (target.Control as IPlaneController).EnabledCuriser;
            return false;
        }
        public static void EnabledCuriser_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlaneController)
                (target.Control as IPlaneController).EnabledCuriser = value;
        }
        public static void EnabledCuriser_Trigger(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlaneController)
                (target.Control as IPlaneController).EnabledCuriser = !(target.Control as IPlaneController).EnabledCuriser;
        }
        public static float MaxiumFlightSpeed_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is IPlaneController)
                return (target.Control as IPlaneController).MaxiumFlightSpeed;
            return 0f;
        }
        public static void MaxiumFlightSpeed_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlaneController)
                (target.Control as IPlaneController).MaxiumFlightSpeed = value;
        }
        public static void MaxiumFlightSpeed_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlaneController)
                (target.Control as IPlaneController).MaxiumFlightSpeed += 10;
        }
        public static void MaxiumFlightSpeed_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IPlaneController)
                (target.Control as IPlaneController).MaxiumFlightSpeed -= 10;
        }
        public static bool HasWings_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is IWingModeController)
                return (target.Control as IWingModeController).HasWings;
            return false;
        }
        public static void HasWings_Setter(IMyTerminalBlock Me, bool value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IWingModeController)
                (target.Control as IWingModeController).HasWings = value;
        }
        public static void HasWings_Trigger(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is IWingModeController)
                (target.Control as IWingModeController).HasWings = !(target.Control as IWingModeController).HasWings;
        }
    }
}
