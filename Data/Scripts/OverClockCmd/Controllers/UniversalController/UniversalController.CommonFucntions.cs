using Sandbox.ModAPI;
namespace SuperBlocks.Controller
{
    using static Utils;
    public partial class UniversalController
    {
        #region CommonFunction
        public static bool EnabledGUI(IMyTerminalBlock Me) { if (Me == null || Me.GameLogic == null) return false; return Me.GameLogic.GetAs<UniversalController>() != null; }
        public static void SaveDatas(IMyTerminalBlock Me) { Me?.GameLogic?.GetAs<UniversalController>()?.WriteDatas(); }
        #endregion
        #region TriggerFunctions
        public bool EnabledBlock(IMyTerminalBlock Me) { try { if (Me == this.Me) return Enabled; } catch (System.Exception) { } return false; }
        public void EnabledBlock(IMyTerminalBlock Me, bool value) { try { if (Me == this.Me) Enabled = value; } catch (System.Exception) { } }
        public void TriggleEnabledBlock(IMyTerminalBlock Me) { try { if (Me == this.Me) Enabled = !Enabled; } catch (System.Exception) { } }
        public bool DockGround(IMyTerminalBlock Me) { try { if (Me == this.Me) return Dock; } catch (System.Exception) { } return false; }
        public void DockGround(IMyTerminalBlock Me, bool value) { try { if (Me == this.Me) Dock = value; } catch (System.Exception) { } }
        public void TriggleDockGround(IMyTerminalBlock Me) { try { if (Me == this.Me) Dock = !Dock; } catch (System.Exception) { } }
        public bool DockGroundReady(IMyTerminalBlock Me) { try { if (Me == this.Me) return WheelsController.DockComplete; } catch (System.Exception) { } return false; }
        public static void ReadConfigs(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("LoadConfig");
        }
        public static void SaveConfigs(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("SaveConfig");
        }
        public static void TriggleHasWings(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("HasWings");
        }
        public static void TriggleHoverMode(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("HoverMode");
        }
        public static void TriggleEnabledCuriser(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("EnabledCuriser");
        }
        public static void TriggleEnabledThrusters(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("EnabledThrusters");
        }
        public static void TriggleEnabledGyros(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.Trigger("EnabledGyros");
        }
        #endregion
        #region 用于设置的函数（姿态切换）
        public static void SetHasWings(IMyTerminalBlock Me, bool Value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.HasWings = Value;
        }
        public static void SetHoverMode(IMyTerminalBlock Me, bool Value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.HoverMode = Value;
        }
        public static void SetEnabledCuriser(IMyTerminalBlock Me, bool Value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.EnabledCuriser = Value;
        }
        public static void SetEnabledThrusters(IMyTerminalBlock Me, bool Value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.EnabledThrusters = Value;
        }
        public static void SetEnabledGyros(IMyTerminalBlock Me, bool Value)
        {
            if (Me == null || Me.GameLogic == null) return;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return;
            script.EnabledGyros = Value;
        }
        #endregion
        #region 用于返回的函数（姿态切换）
        public static bool GetHasWings(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return false;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return false;
            return script.HasWings;
        }
        public static bool GetHoverMode(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return false;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return false;
            return script.HoverMode;
        }
        public static bool GetEnabledCuriser(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return false;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return false;
            return script.EnabledCuriser;
        }
        public static bool GetEnabledThrusters(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return false;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return false;
            return script.EnabledThrusters;
        }
        public static bool GetEnabledGyros(IMyTerminalBlock Me)
        {
            if (Me == null || Me.GameLogic == null) return false;
            var script = Me.GameLogic.GetAs<UniversalController>();
            if (Common.IsNull(script)) return false;
            return script.EnabledGyros;
        }
        #endregion
    }
}
