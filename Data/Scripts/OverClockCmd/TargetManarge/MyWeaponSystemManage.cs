using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using SIngame = Sandbox.ModAPI.Ingame;
using VRage;
using VRageMath;
using System.Linq;
using System.Collections.Concurrent;
namespace SuperBlocks.Controller
{
    public static class MyWeaponSystemManage
    {
        public static bool 初始化完成 { get; private set; } = false;
        #region 创建对应的应用     
        #endregion
        #region 更新函数
        public static void Update()
        {
            try
            {
                if (!初始化完成) { CreateProperties(); return; }
                count = (count + 1) % count_max;
                if (count == 0) { RemoveNulls(); }
                MyAPIGateway.Parallel.ForEach(BlockBindings_WeaponSystem, bound => { if (bound.Key.IsFunctional && (bound.Key is IMyFunctionalBlock) && (bound.Key as IMyFunctionalBlock).Enabled) bound.Value.Update(bound.Key); });
            }
            catch (Exception) { 初始化完成 = false; BlockBindings_WeaponSystem.Clear(); /*MyAPIGateway.Utilities.ShowNotification(e.Message);*/ }
        }
        static int count = 0;
        const int count_max = 100;
        #endregion
        private static ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding> BlockBindings_WeaponSystem { get; } = new ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding>();
        private static void CreateProperties()
        {
            初始化完成 = true;
            #region CommonFunctions
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_Setup", EnabledBlock, GetWeaponSysBindings, SetWeaponSysBindings);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_Enabled", EnabledBlock, GetWeaponSysEnabled, SetWeaponSysEnabled);
            CreateProperty.CreateProperty_PB_CN<float, SIngame.IMyProgrammableBlock>($"LZY_WS_InitRadar", EnabledBlock, GetRadarRange, SetRadarRange);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_AutoFire", EnabledBlock, AutoFire, AutoFire);
            CreateProperty.CreateProperty_PB_CN<MyTuple<string, string>, SIngame.IMyProgrammableBlock>($"LZY_WS_SetWeaponAmmo", EnabledBlock, b => new MyTuple<string, string>("DefaultWeapon", "DefaultAmmo"), WeaponAmmo);
            #endregion
            #region FixedWeaponConfig
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_InitFixedWeapons", EnabledBlock, EnabledFixedWeapon, EnabledFixedWeapon);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_FixedWeaponsEnabled", EnabledBlock, FixedWeaponsEnabled, FixedWeaponsEnabled);
            #endregion
            #region TurretConfig
            CreateProperty.CreateProperty_PB_CN<string, SIngame.IMyProgrammableBlock>($"LZY_WS_InitTurrets", EnabledBlock, TurretSetup, TurretSetup);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_TurretEnabled", EnabledBlock, TurretEnabled, TurretEnabled);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_UsingWeaponCoreTracker", EnabledBlock, UsingWeaponCoreTracker, UsingWeaponCoreTracker);
            #endregion
        }
        #region CommonFunctions
        private static void SetWeaponSysBindings(IMyTerminalBlock Me, bool Enabled)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock))) return;
                if (!Enabled)
                {
                    MyWeaponSystemBinding WS;
                    BlockBindings_WeaponSystem.TryRemove(Me, out WS);
                    return;
                }
                BlockBindings_WeaponSystem.AddOrUpdate(Me, new MyWeaponSystemBinding(Me), (key, oldvalue) => { return oldvalue; });
                BlockBindings_WeaponSystem[Me].EnabledWeapons = Enabled;
            }
            catch (Exception) { }
        }
        private static bool GetWeaponSysBindings(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock))) return false;
                return BlockBindings_WeaponSystem.ContainsKey(Me);
            }
            catch (Exception) { return false; }
        }
        private static void SetWeaponSysEnabled(IMyTerminalBlock Me, bool Enabled)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].EnabledWeapons = Enabled;
            }
            catch (Exception) { }
        }
        private static bool GetWeaponSysEnabled(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.EnabledWeapons ?? false; } catch (Exception) { return false; }
        }
        private static void SetRadarRange(IMyTerminalBlock Me, float Range)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].RadarTargets.ResetParameters(Me, Range);
            }
            catch (Exception) { }
        }
        private static float GetRadarRange(IMyTerminalBlock Me)
        {
            try { return (float)(GetWeaponSystemBinding(Me)?.RadarTargets?.Range ?? 1500) / 1.5f; } catch (Exception) { return 1000; }
        }
        private static MyWeaponSystemBinding GetWeaponSystemBinding(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me];
            }
            catch (Exception) { return null; }
        }
        private static bool AutoFire(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.AutoFire ?? false; } catch (Exception) { return false; }
        }
        private static void AutoFire(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].AutoFire = value;
            }
            catch (Exception) { }
        }
        private static void WeaponAmmo(IMyTerminalBlock Me, MyTuple<string, string> WANM)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].SetWeaponAmmo(WANM);
            }
            catch (Exception) { }
        }
        #endregion
        #region FixedWeaponConfig
        private static bool EnabledFixedWeapon(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.EnabledFixedWeapon ?? false; } catch (Exception) { return false; }
        }
        private static void EnabledFixedWeapon(IMyTerminalBlock Me, bool value)
        {
            try
            {
                if (Utils.Common.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].EnabledFixedWeapon = value;
            }
            catch (Exception) { }
        }
        private static bool FixedWeaponsEnabled(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.GetFixedWeaponsEnabled ?? false; } catch (Exception) { return false; }
        }
        private static void FixedWeaponsEnabled(IMyTerminalBlock Me, bool value)
        {
            try
            {
                GetWeaponSystemBinding(Me)?.SetFixedWeaponsEnabled(value);
            }
            catch (Exception) { }
        }
        #endregion


        private static string TurretSetup(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.TurretID ?? "TurretSetup"; } catch (Exception) { return "TurretSetup"; }
        }
        private static void TurretSetup(IMyTerminalBlock Me, string value)
        {
            try { GetWeaponSystemBinding(Me)?.InitTurret(Me, value); } catch (Exception) { }
        }
        private static bool TurretEnabled(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.TurretEnabled ?? false; } catch (Exception) { return false; }
        }
        private static void TurretEnabled(IMyTerminalBlock Me, bool value)
        {
            try { GetWeaponSystemBinding(Me)?.SetTurretEnabled(value); } catch (Exception) { }
        }
        private static bool UsingWeaponCoreTracker(IMyTerminalBlock Me)
        {
            try { return GetWeaponSystemBinding(Me)?.UsingWeaponCoreTracker ?? false; } catch (Exception) { return false; }
        }
        private static void UsingWeaponCoreTracker(IMyTerminalBlock Me, bool value)
        {
            try { GetWeaponSystemBinding(Me)?.SetUsingWeaponCoreTracker(value); } catch (Exception) { }
        }
        private static void RemoveNulls()
        {
            if (Utils.Common.IsNullCollection(BlockBindings_WeaponSystem)) return;
            var removeble = BlockBindings_WeaponSystem.Keys.Where(Utils.Common.NullEntity)?.ToList();
            if (Utils.Common.IsNullCollection(removeble)) return;
            MyAPIGateway.Parallel.ForEach(removeble, key => { MyWeaponSystemBinding value; BlockBindings_WeaponSystem.TryRemove(key, out value); });
        }




        private static bool EnabledBlock(IMyTerminalBlock block) => block is SIngame.IMyProgrammableBlock && block.IsFunctional && (block as SIngame.IMyProgrammableBlock).Enabled;
    }
}