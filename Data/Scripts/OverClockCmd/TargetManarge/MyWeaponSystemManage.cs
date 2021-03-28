using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Linq;
using SIngame = Sandbox.ModAPI.Ingame;
namespace SuperBlocks.Controller
{
    public static class MyWeaponSystemManage
    {
        public static bool 初始化完成 { get; private set; } = false;
        public const string ModApiProperty_Start = @"LZY_WS_";
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
                //MyAPIGateway.Utilities.ShowNotification($"Weapon Systems:{BlockBindings_WeaponSystem.Count}");
                //MyAPIGateway.Utilities.ShowNotification($"Weapon Systems:{BlockBindings_WeaponSystem.Count}
            }
            catch (Exception) { 初始化完成 = false; try { BlockBindings_WeaponSystem.Clear(); } catch (Exception) { } /*MyAPIGateway.Utilities.ShowNotification(e.Message);*/ }
        }
        static int count = 0;
        const int count_max = 100;
        #endregion
        private static ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding> BlockBindings_WeaponSystem { get; } = new ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding>();
        private static void CreateProperties()
        {
            初始化完成 = true;
            #region CommonFunctions
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"{ModApiProperty_Start}Setup", EnabledBlock, GetWeaponSysBindings, SetWeaponSysBindings);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"{ModApiProperty_Start}AutoFire", EnabledBlock, AutoFire, AutoFire);
            #endregion
            #region TurretConfig
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"{ModApiProperty_Start}TurretEnabled", EnabledBlock, TurretEnabled, TurretEnabled);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"{ModApiProperty_Start}UsingWeaponCoreTracker", EnabledBlock, UsingWeaponCoreTracker, UsingWeaponCoreTracker);
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
        #endregion
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