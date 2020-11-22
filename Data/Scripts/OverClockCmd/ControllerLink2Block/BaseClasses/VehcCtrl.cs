using Sandbox.ModAPI;
using VRageMath;

namespace SuperBlocks.Controller
{
    public partial class VehcCtrl : ControlBase
    {
        protected override void InitParameters()
        {
            (Control as ICtrlDevCtrl).EnabledGyros = true;
            (Control as ICtrlDevCtrl).EnabledThrusters = true;
        }
        protected override void InitController()
        {
            Control = new VehicleControllerBase(Entity as IMyTerminalBlock);
        }
        #region 推进器和陀螺仪控制接口
        public static bool EnabledThrusters_Getter(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is ICtrlDevCtrl)
                return (target.Control as ICtrlDevCtrl).EnabledThrusters;
            return false;
        }
        public static void EnabledThrusters_Setter(IMyTerminalBlock block, bool Value)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is ICtrlDevCtrl)
                (target.Control as ICtrlDevCtrl).EnabledThrusters = Value;
        }
        public static void EnabledThrusters_Triggler(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is ICtrlDevCtrl)
                (target.Control as ICtrlDevCtrl).EnabledThrusters = !(target.Control as ICtrlDevCtrl).EnabledThrusters;
        }
        public static bool EnabledGyros_Getter(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return false;
            if (target.Control is ICtrlDevCtrl)
                return (target.Control as ICtrlDevCtrl).EnabledGyros;
            return false;
        }
        public static void EnabledGyros_Setter(IMyTerminalBlock block, bool Value)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is ICtrlDevCtrl)
                (target.Control as ICtrlDevCtrl).EnabledGyros = Value;
        }
        public static void EnabledGyros_Triggler(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is ICtrlDevCtrl)
                (target.Control as ICtrlDevCtrl).EnabledGyros = !(target.Control as ICtrlDevCtrl).EnabledGyros;
        }
        #endregion
    }
    public partial class VehcCtrl
    {
        public static float AngularDampener_R_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is VehicleControllerBase)
                return (target.Control as VehicleControllerBase).AngularDampeners_Roll;
            return 0f;
        }
        public static void AngularDampener_R_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Roll = value;
        }
        public static void AngularDampener_R_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Roll += 0.5f;
        }
        public static void AngularDampener_R_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Roll -= 0.5f;
        }
        public static float AngularDampener_P_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is VehicleControllerBase)
                return (target.Control as VehicleControllerBase).AngularDampeners_Pitch;
            return 0f;
        }
        public static void AngularDampener_P_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Pitch = value;
        }
        public static void AngularDampener_P_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Pitch += 0.5f;
        }
        public static void AngularDampener_P_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Pitch -= 0.5f;
        }
        public static float AngularDampener_Y_Getter(IMyTerminalBlock Me)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return 0f;
            if (target.Control is VehicleControllerBase)
                return (target.Control as VehicleControllerBase).AngularDampeners_Yaw;
            return 0f;
        }
        public static void AngularDampener_Y_Setter(IMyTerminalBlock Me, float value)
        {
            var target = Me.GameLogic.GetAs<FlyMachineCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Yaw = value;
        }
        public static void AngularDampener_Y_Inc(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Yaw += 0.5f;
        }
        public static void AngularDampener_Y_Dec(IMyTerminalBlock block)
        {
            var target = block.GameLogic.GetAs<VehcCtrl>();
            if (target == null || target.Control == null) return;
            if (target.Control is VehicleControllerBase)
                (target.Control as VehicleControllerBase).AngularDampeners_Yaw -= 0.5f;
        }
    }
}
