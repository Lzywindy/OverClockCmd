using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Utils;
using VRageMath;

namespace SuperBlocks.Controller
{
    using static Utils;
    public partial class UniversalController
    {
        #region 用于控制角色的选择
        public static void Controller_Role_List(List<VRage.ModAPI.MyTerminalControlComboBoxItem> items)
        {
            if (items == null) return;
            items.Clear();
            items.AddRange(角色列表实体_UC);
        }
        public static long RoleGetter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return (long)script.Role;
        }
        public static void RoleSetter(IMyTerminalBlock Me, long value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.Role = (ControllerRole)Enum.ToObject(typeof(ControllerRole), value);
        }
        private static List<VRage.ModAPI.MyTerminalControlComboBoxItem> 角色列表实体_UC { get; } = new List<VRage.ModAPI.MyTerminalControlComboBoxItem>()
        {
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.None,Value=MyStringId.GetOrCompute(ControllerRole.None.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.Aeroplane,Value=MyStringId.GetOrCompute(ControllerRole.Aeroplane.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.Helicopter,Value=MyStringId.GetOrCompute(ControllerRole.Helicopter.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.VTOL,Value=MyStringId.GetOrCompute(ControllerRole.VTOL.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.SpaceShip,Value=MyStringId.GetOrCompute(ControllerRole.SpaceShip.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.SeaShip,Value=MyStringId.GetOrCompute(ControllerRole.SeaShip.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.Submarine,Value=MyStringId.GetOrCompute(ControllerRole.Submarine.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.TrackVehicle,Value=MyStringId.GetOrCompute(ControllerRole.TrackVehicle.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.WheelVehicle,Value=MyStringId.GetOrCompute(ControllerRole.WheelVehicle.ToString())},
            new VRage.ModAPI.MyTerminalControlComboBoxItem() {Key= (long)ControllerRole.HoverVehicle,Value=MyStringId.GetOrCompute(ControllerRole.HoverVehicle.ToString())}
        };
        #endregion
        #region 角速度阻尼设置
        public static void AngularDampener_R_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"x{MathHelper.RoundOn2(script.AngularDampeners_Roll)}");
        }
        public static float AngularDampener_R_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.AngularDampeners_Roll;
        }
        public static void AngularDampener_R_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Roll = value;
        }
        public static void AngularDampener_R_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Roll++;
        }
        public static void AngularDampener_R_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Roll--;
        }
        public static void AngularDampener_P_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"x{MathHelper.RoundOn2(script.AngularDampeners_Pitch)}");
        }
        public static float AngularDampener_P_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.AngularDampeners_Pitch;
        }
        public static void AngularDampener_P_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Pitch = value;
        }
        public static void AngularDampener_P_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Pitch++;
        }
        public static void AngularDampener_P_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Pitch--;
        }
        public static void AngularDampener_Y_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"x{MathHelper.RoundOn2(script.AngularDampeners_Yaw)}");
        }
        public static float AngularDampener_Y_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.AngularDampeners_Yaw;
        }
        public static void AngularDampener_Y_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Yaw = value;
        }
        public static void AngularDampener_Y_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Yaw++;
        }
        public static void AngularDampener_Y_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.AngularDampeners_Yaw--;
        }
        #endregion
        #region 最大速度调节
        public static void MaxiumSpeed_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"{script.MaxiumSpeedShow()}");
        }
        public static float MaxiumSpeed_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.MaximumSpeed;
        }
        public static void MaxiumSpeed_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaximumSpeed = value;
        }
        public static void MaxiumSpeed_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaximumSpeed++;
        }
        public static void MaxiumSpeed_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaximumSpeed--;
        }
        private string MaxiumSpeedShow()
        {
            switch (Role)
            {
                case ControllerRole.Aeroplane:
                case ControllerRole.Helicopter:
                case ControllerRole.VTOL://m/s
                    return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
                case ControllerRole.SpaceShip:// v<3e6:m/s/km/s,v>3e6:c
                    if (MaximumSpeed < 3e3f)
                        return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
                    else if (MaximumSpeed < 3e6f)
                        return $"{MathHelper.RoundOn2(MaximumSpeed / 1000)} km/s";
                    else
                        return $"{MathHelper.RoundOn2(MaximumSpeed / 3e8f)} c";
                case ControllerRole.SeaShip:
                case ControllerRole.Submarine://knot
                    return $"{MathHelper.RoundOn2(MaximumSpeed / 1.852f)} kn";
                case ControllerRole.TrackVehicle:
                case ControllerRole.WheelVehicle:
                case ControllerRole.HoverVehicle://km/h
                    return $"{MathHelper.RoundOn2(MaximumSpeed)} km/h";
                default:
                    return $"{MathHelper.RoundOn2(MaximumSpeed)} m/s";
            }
        }
        #endregion
        #region 反应灵敏度调节
        public static void MaxReactions_AngleV_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"{script.MaxReactions_AngleV} deg/s");
        }
        public static float MaxReactions_AngleV_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.MaxReactions_AngleV;
        }
        public static void MaxReactions_AngleV_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaxReactions_AngleV = value;
        }
        public static void MaxReactions_AngleV_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaxReactions_AngleV++;
        }
        public static void MaxReactions_AngleV_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.MaxReactions_AngleV--;
        }
        public static void SafetyStage_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"{script.SafetyStage}");
        }
        public static float SafetyStage_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.SafetyStage;
        }
        public static void SafetyStage_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.SafetyStage = value;
        }
        public static void SafetyStage_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.SafetyStage++;
        }
        public static void SafetyStage_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.SafetyStage--;
        }
        public static void LocationSensetive_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            value.Clear();
            value.Append($"{script.LocationSensetive}");
        }
        public static float LocationSensetive_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return 0;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return 0;
            return script.LocationSensetive;
        }
        public static void LocationSensetive_Setter(IMyTerminalBlock Me, float value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.LocationSensetive = value;
        }
        public static void LocationSensetive_Inc(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.LocationSensetive++;
        }
        public static void LocationSensetive_Dec(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.LocationSensetive--;
        }
        #endregion
        #region 设置超载控制
        public static void Override_ForwardDirection_Setter(IMyTerminalBlock Me, Vector3D? Vector)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.ForwardDirectionOverride = Vector;
        }
        public static Vector3D? Override_ForwardDirection_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return null;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return null;
            return script.ForwardDirectionOverride;
        }
        public static void Override_PlaneNormal_Setter(IMyTerminalBlock Me, Vector3D? Vector)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return;
            script.PlaneNormalOverride = Vector;
        }
        public static Vector3D? Override_PlaneNormal_Getter(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return null;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (IsNull(script)) return null;
            return script.PlaneNormalOverride;
        }
        #endregion
    }
}
