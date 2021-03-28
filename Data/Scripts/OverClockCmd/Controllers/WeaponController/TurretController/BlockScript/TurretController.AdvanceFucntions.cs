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
        public bool EnabledGUI(IMyTerminalBlock Me) { return Me == this.Me; }
        public void TriggerReadConfigs(IMyTerminalBlock Me)
        {
            if (!EnabledGUI(Me)) return;
            MyConfigs.Concurrent.CustomDataConfigRead_INI(Me.CustomData, Configs);
            if (!Configs.ContainsKey("DefaultTurretWeaponConfig")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig"), Configs, "DefaultTurretWeaponConfig");
            if (!Configs.ContainsKey("DefaultWeaponCoreWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.DefaultWeaponCore, Configs, "DefaultWeaponCoreWeapon");
            if (!Configs.ContainsKey("KeensRocketWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensRocket, Configs, "KeensRocketWeapon");
            if (!Configs.ContainsKey("KeensProjectile_SmallWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Small, Configs, "KeensProjectile_SmallWeapon");
            if (!Configs.ContainsKey("KeensProjectile_LargeWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Large, Configs, "KeensProjectile_LargeWeapon");
            if (!Configs.ContainsKey("EnergyWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.Energy, Configs, "EnergyWeapon");
            if (!Configs.ContainsKey("TurretState"))
            {
                Configs.AddOrUpdate("TurretState", new ConcurrentDictionary<string, string>(), (k, v) => v);
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "TurretEnabled", TurretEnabled.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "AutoFire", AutoFire.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "UsingWeaponCoreTracker", UsingWeaponCoreTracker.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "RangeMultiply", RangeMultiply.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "BlockEnabled", BlockEnabled.ToString());
            }
            else
            {
                foreach (var item in Configs["TurretState"])
                {
                    switch (item.Key)
                    {
                        case "TurretEnabled": TurretEnabled = MyConfigs.ParseBool(item.Value); break;
                        case "AutoFire": AutoFire = MyConfigs.ParseBool(item.Value); break;
                        case "UsingWeaponCoreTracker": UsingWeaponCoreTracker = MyConfigs.ParseBool(item.Value); break;
                        case "RangeMultiply": RangeMultiply = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), 1e-3f, 1f); break;
                        case "BlockEnabled": BlockEnabled = MyConfigs.ParseBool(item.Value); break;
                        default: break;
                    }
                }
            }
        }
        public void TriggerSaveConfigs(IMyTerminalBlock Me)
        {
            if (!EnabledGUI(Me)) return;
            if (Configs.ContainsKey("TurretState"))
            {
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "TurretEnabled", TurretEnabled.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "AutoFire", AutoFire.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "UsingWeaponCoreTracker", UsingWeaponCoreTracker.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "RangeMultiply", RangeMultiply.ToString());
                MyConfigs.Concurrent.ModifyProperty(Configs["TurretState"], "BlockEnabled", BlockEnabled.ToString());
            }
            Me.CustomData = MyConfigs.Concurrent.CustomDataConfigSave_INI(Configs);
        }
        public void TriggerRestart(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                TriggerReadConfigs(Me);
                TriggerSaveConfigs(Me);
                Range = MyRadarSubtypeIdHelper.DetectedRangeBlock(MyRadarSubtypeIdHelper.GetFarestDetectedBlock(Me.CubeGrid));
                List<MyTurretBinding> list = Common.GetTs<IMyMotorStator>(Me, HasEvMotors).ConvertAll(az => new MyTurretBinding(az));
                if (Common.IsNullCollection(list)) { Turrets = null; return; }
                Turrets = new ConcurrentBag<MyTurretBinding>(list);
            }
            catch (Exception) { }
        }
        public void TriggerCycleWeapons(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) MyAPIGateway.Parallel.ForEach(Turrets, Turret => Turret.CycleWeapons());
            }
            catch (Exception) { }
        }
        public void TriggerBlockEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) BlockEnabled = !BlockEnabled;
            }
            catch (Exception) { }
        }
        public bool GetterBlockEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) return BlockEnabled;
                return false;
            }
            catch (Exception) { return false; }
        }
        public void SetterBlockEnabled(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (EnabledGUI(Me)) BlockEnabled = value;
            }
            catch (Exception) { }
        }
        public void TriggerTurretEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) TurretEnabled = !TurretEnabled;
            }
            catch (Exception) { }
        }
        public bool GetterTurretEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (!EnabledGUI(Me)) return false;
                return TurretEnabled;
            }
            catch (Exception) { return false; }
        }
        public void SetterTurretEnabled(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (!EnabledGUI(Me)) return;
                TurretEnabled = value;
            }
            catch (Exception) { }
        }
        public void TriggerUsingWeaponCoreTracker(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) UsingWeaponCoreTracker = !UsingWeaponCoreTracker;
            }
            catch (Exception) { }
        }
        public bool GetterUsingWeaponCoreTracker(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) return UsingWeaponCoreTracker;
                return false;
            }
            catch (Exception) { return false; }
        }
        public void SetterUsingWeaponCoreTracker(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (EnabledGUI(Me)) UsingWeaponCoreTracker = value;
            }
            catch (Exception) { }
        }
        public void TriggerAutoFire(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) AutoFire = !AutoFire;
            }
            catch (Exception) { }
        }
        public bool GetterAutoFire(IMyTerminalBlock Me)
        {
            try
            {
                if (EnabledGUI(Me)) return AutoFire;
                return false;
            }
            catch (Exception) { return false; }
        }
        public void SetterAutoFire(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (EnabledGUI(Me)) AutoFire = value;
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
