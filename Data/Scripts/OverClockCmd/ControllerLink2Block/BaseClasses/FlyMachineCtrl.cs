using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    public class FlyMachineCtrl : VehcCtrl
    {
        protected override void CreateController()
        {
            Control = new FlyingMachineCtrl_Base(Entity as IMyTerminalBlock);
            (Control as FlyingMachineCtrl_Base).EnabledThrusters = true;
            (Control as FlyingMachineCtrl_Base).EnabledGyros = true;
            (Control as FlyingMachineCtrl_Base).SafetyStage = 1f;
            (Control as FlyingMachineCtrl_Base).MaxReactions_AngleV = 25f;
            (Control as FlyingMachineCtrl_Base).LocationSensetive = 1.5f;
        }
        #region UsefulFunctions
        public float 安全偏转角度_Getter(IMyTerminalBlock block)
        {
            if (block.EntityId != Entity.EntityId) return 0.1f;
            if (Control is IPoseParamAdjust)
                return (Control as IPoseParamAdjust).SafetyStage;
            return 0.1f;
        }
        public void 安全偏转角度_Setter(IMyTerminalBlock block, float Value)
        {
            if (block.EntityId != Entity.EntityId) return;
            if (Control is IPoseParamAdjust)
                (Control as IPoseParamAdjust).SafetyStage = Value;
        }
        public bool EnabledCrusier_Getter(IMyTerminalBlock block)
        {
            if (block.EntityId != Entity.EntityId) return false;
            if ((Control is IPlaneController))
                return (Control as IPlaneController).EnabledCuriser;
            return false;
        }
        public void EnabledCrusier_Setter(IMyTerminalBlock block, bool Value)
        {
            if (block.EntityId != Entity.EntityId) return;
            if ((Control is IPlaneController))
                (Control as IPlaneController).EnabledCuriser = Value;
        }
        public bool EnabledHover_Getter(IMyTerminalBlock block)
        {
            if (block.EntityId != Entity.EntityId) return false;
            if (Control is IHeilController)
                return (Control as IHeilController).HoverMode;
            return false;
        }
        public void EnabledHover_Setter(IMyTerminalBlock block, bool Value)
        {
            if (block.EntityId != Entity.EntityId) return;
            if (Control is IHeilController )
                (Control as IHeilController).HoverMode = Value;
        }
        public bool EnabledWings_Getter(IMyTerminalBlock block)
        {
            if (block.EntityId != Entity.EntityId) return false;
            if (Control is IWingModeController)
                return (Control as IWingModeController).HasWings;
            return false;
        }
        public void EnabledWings_Setter(IMyTerminalBlock block, bool Value)
        {
            if (block.EntityId != Entity.EntityId) return;
            if (Control is IWingModeController)
                (Control as IWingModeController).HasWings = Value;
        }
        #endregion
    }
}
