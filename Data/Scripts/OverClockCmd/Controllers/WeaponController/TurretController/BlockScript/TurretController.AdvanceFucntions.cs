using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRageMath;
using static SuperBlocks.Definitions.Structures;
using static SuperBlocks.Utils;

namespace SuperBlocks.Controller
{
    public partial class TurretController
    {
        protected override void LoadData()
        {
            if (!Configs.ContainsKey("TurretState"))
            {
                Configs.AddOrUpdate("TurretState", new ConcurrentDictionary<string, string>(), (k, v) => v);
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "TurretEnabled", TurretEnabled.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "AutoFire", AutoFire.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "UsingWeaponCoreTracker", UsingWeaponCoreTracker.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "RangeMultiply", RangeMultiply.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "BlockEnabled", Enabled.ToString());
            }
            else
            {
                foreach (var item in Configs["TurretState"])
                {
                    switch (item.Key)
                    {
                        case "TurretEnabled": TurretEnabled.Value = MyConfigs.ParseBool(item.Value); break;
                        case "AutoFire": AutoFire.Value = MyConfigs.ParseBool(item.Value); break;
                        case "UsingWeaponCoreTracker": UsingWeaponCoreTracker.Value = MyConfigs.ParseBool(item.Value); break;
                        case "RangeMultiply": RangeMultiply = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), 1e-3f, 1f); break;
                        case "BlockEnabled": Enabled.Value = MyConfigs.ParseBool(item.Value); break;
                        default: break;
                    }
                }
            }
            if (!Configs.ContainsKey("DefaultTurretWeaponConfig")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig"), Configs, "DefaultTurretWeaponConfig");
            if (!Configs.ContainsKey("DefaultWeaponCoreWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.DefaultWeaponCore, Configs, "DefaultWeaponCoreWeapon");
            if (!Configs.ContainsKey("KeensRocketWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensRocket, Configs, "KeensRocketWeapon");
            if (!Configs.ContainsKey("KeensProjectile_SmallWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Small, Configs, "KeensProjectile_SmallWeapon");
            if (!Configs.ContainsKey("KeensProjectile_LargeWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Large, Configs, "KeensProjectile_LargeWeapon");
            if (!Configs.ContainsKey("EnergyWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.Energy, Configs, "EnergyWeapon");
        }
        protected override void SaveData()
        {
            if (Configs.ContainsKey("TurretState"))
            {
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "TurretEnabled", TurretEnabled.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "AutoFire", AutoFire.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "UsingWeaponCoreTracker", UsingWeaponCoreTracker.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "RangeMultiply", RangeMultiply.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "BlockEnabled", Enabled.Value.ToString());
            }
        }
      
      
      
        public void TriggerTurretEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) TurretEnabled.Value = !TurretEnabled.Value;
            }
            catch (Exception) { }
        }
        public bool GetterTurretEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return false;
                return TurretEnabled.Value;
            }
            catch (Exception) { return false; }
        }
        public void SetterTurretEnabled(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                TurretEnabled.Value = value;
            }
            catch (Exception) { }
        }
        public void TriggerUsingWeaponCoreTracker(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) UsingWeaponCoreTracker.Value = !UsingWeaponCoreTracker.Value;
            }
            catch (Exception) { }
        }
        public bool GetterUsingWeaponCoreTracker(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) return UsingWeaponCoreTracker.Value;
                return false;
            }
            catch (Exception) { return false; }
        }
        public void SetterUsingWeaponCoreTracker(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (EnabledGUI(Me)) UsingWeaponCoreTracker.Value = value;
            }
            catch (Exception) { }
        }
        public void TriggerAutoFire(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) AutoFire.Value = !AutoFire.Value;
            }
            catch (Exception) { }
        }
        public bool GetterAutoFire(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) return AutoFire.Value;
                return false;
            }
            catch (Exception) { return false; }
        }
        public void SetterAutoFire(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (EnabledGUI(Me)) AutoFire.Value = value;
            }
            catch (Exception) { }
        }
        public float RangeMultiply_Getter(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return 0;
                return RangeMultiply;
            }
            catch (Exception) { return 0; }
        }
        public void RangeMultiply_Setter(IMyTerminalBlock Me, float value)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                RangeMultiply = MathHelper.Clamp(value, 0.001f, 1f);
            }
            catch (Exception) { }
        }
        public void RangeMultiply_Inc(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                RangeMultiply = MathHelper.Clamp(RangeMultiply += 0.1f, 0.001f, 1f);
            }
            catch (Exception) { }
        }
        public void RangeMultiply_Dec(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                RangeMultiply = MathHelper.Clamp(RangeMultiply -= 0.1f, 0.001f, 1f);
            }
            catch (Exception) { }
        }
        public void RangeMultiply_Writter(IMyTerminalBlock Me, StringBuilder value)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                value.Clear();
                RangeMultiply = MathHelper.Clamp(RangeMultiply -= 0.1f, 0.001f, 1f);
                value.Append($"Range:{MathHelper.RoundOn2(RangeMultiply * Range)}m");
            }
            catch (Exception) { }
        }
    }
}
