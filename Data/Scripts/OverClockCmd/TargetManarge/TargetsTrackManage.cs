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
    public static class WeaponSystemManage
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
            catch (Exception e) { MyAPIGateway.Utilities.ShowNotification(e.Message); }
        }
        static int count = 0;
        const int count_max = 100;
        #endregion
        private static ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding> BlockBindings_WeaponSystem { get; } = new ConcurrentDictionary<IMyTerminalBlock, MyWeaponSystemBinding>();
        private static void CreateProperties()
        {
            初始化完成 = true;
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_Setup", EnabledBlock, b => BlockBindings_WeaponSystem.ContainsKey(b), SetWeaponSysBindings);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_Enabled", EnabledBlock, b => BlockBindings_WeaponSystem.ContainsKey(b) && BlockBindings_WeaponSystem[b].EnabledFunction[0], SetWeaponSysEnabled);
            CreateProperty.CreateProperty_PB_CN<float, SIngame.IMyProgrammableBlock>($"LZY_WS_InitRadar", EnabledBlock, b => 1000, InitRadar);
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_InitFixedWeapons", EnabledBlock, b => false, InitFixedWeapons);
            CreateProperty.CreateProperty_PB_CN<string, SIngame.IMyProgrammableBlock>($"LZY_WS_InitTurrets", EnabledBlock, b => "", InitTurrets);
            CreateProperty.CreateProperty_PB_CN<float, SIngame.IMyProgrammableBlock>($"LZY_WS_SetTimeOffset", EnabledBlock, b => 0, SetTimeOffset);
            CreateProperty.CreateProperty_PB_CN<MyTuple<string, string>, SIngame.IMyProgrammableBlock>($"LZY_WS_SetWeaponAmmo", EnabledBlock, b => default(MyTuple<string, string>), SetWeaponAmmo);
            CreateProperty.CreateProperty_PB_CN<List<VRage.Game.ModAPI.Ingame.IMyEntity>, SIngame.IMyProgrammableBlock>($"LZY_WS_GetDetectedEntities", EnabledBlock, GetDetectedEntities, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<MyTuple<Vector3D?, Vector3D?>?, SIngame.IMyProgrammableBlock>($"LZY_WS_GetTargetPV", EnabledBlock, GetTargetPV, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<List<SIngame.IMyFunctionalBlock>, SIngame.IMyProgrammableBlock>($"LZY_WS_GetFixedWeapons", EnabledBlock, GetFixedWeapons, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_GetFixedWeaponsEnabled", EnabledBlock, GetFixedWeaponsEnabled, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>>, SIngame.IMyProgrammableBlock>($"LZY_WS_GetTurretWeapons", EnabledBlock, GetTurretWeapons, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<Dictionary<SIngame.IMyTerminalBlock, bool>, SIngame.IMyProgrammableBlock>($"LZY_WS_GetTurretWeaponsEnabled", EnabledBlock, GetTurretWeaponsEnabled, (b, l) => { });
            CreateProperty.CreateProperty_PB_CN<bool, SIngame.IMyProgrammableBlock>($"LZY_WS_TurretEnabled", EnabledBlock,
            Me =>
            {
                try
                {
                    if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return false;
                    return BlockBindings_WeaponSystem[Me].TurretEnabled;
                }
                catch (Exception) { }
                return false;
            },
            (Me, value) =>
            {
                try
                {
                    if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                    BlockBindings_WeaponSystem[Me].TurretEnabled = value;
                }
                catch (Exception) { }
            });
        }
        private static void RemoveNulls()
        {
            if (Utils.IsNullCollection(BlockBindings_WeaponSystem)) return;
            var removeble = BlockBindings_WeaponSystem.Keys.Where(Utils.NullEntity);
            if (Utils.IsNullCollection(removeble)) return;
            MyAPIGateway.Parallel.ForEach(removeble, key => { MyWeaponSystemBinding value; BlockBindings_WeaponSystem.TryRemove(key, out value); });
        }
        private static void SetWeaponSysBindings(IMyTerminalBlock Me, bool Enabled)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock))) return;
                if (!Enabled)
                {
                    MyWeaponSystemBinding WS;
                    BlockBindings_WeaponSystem.TryRemove(Me, out WS);
                    return;
                }
                BlockBindings_WeaponSystem.AddOrUpdate(Me, new MyWeaponSystemBinding(Me), (key, oldvalue) => { return oldvalue; });
                BlockBindings_WeaponSystem[Me].SetWeaponSysEnabled(Enabled);
            }
            catch (Exception) { }
        }
        private static void SetWeaponSysEnabled(IMyTerminalBlock Me, bool Enabled)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].SetWeaponSysEnabled(Enabled);
            }
            catch (Exception) { }
        }
        private static void InitRadar(IMyTerminalBlock Me, float Range)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].InitRadar(Me, Range);
            }
            catch (Exception) { }
        }
        private static void InitFixedWeapons(IMyTerminalBlock Me, bool Enabled)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].InitFixedWeapons(Me, Enabled);
            }
            catch (Exception) { }
        }
        private static void InitTurrets(IMyTerminalBlock Me, string TurretID)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].InitTurret(Me, TurretID);
            }
            catch (Exception) { }
        }
        private static void SetTimeOffset(IMyTerminalBlock Me, float TimeOffset)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].SetTimeOffset(TimeOffset);
            }
            catch (Exception) { }
        }
        private static void SetWeaponAmmo(IMyTerminalBlock Me, MyTuple<string, string> WANM)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return;
                BlockBindings_WeaponSystem[Me].SetWeaponAmmo(WANM);
            }
            catch (Exception) { }
        }
        private static List<VRage.Game.ModAPI.Ingame.IMyEntity> GetDetectedEntities(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me].GetEntities();
            }
            catch (Exception) { }
            return null;
        }
        private static MyTuple<Vector3D?, Vector3D?>? GetTargetPV(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me].GetTargetPV(Me);
            }
            catch (Exception) { }
            return null;
        }
        private static List<SIngame.IMyFunctionalBlock> GetFixedWeapons(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me].GetFixedWeapons();
            }
            catch (Exception) { }
            return null;
        }
        private static bool GetFixedWeaponsEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return false;
                return BlockBindings_WeaponSystem[Me].GetFixedWeaponsEnabled();
            }
            catch (Exception) { }
            return false;
        }
        private static Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>> GetTurretWeapons(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me].GetTurretWeapons();
            }
            catch (Exception) { }
            return null;
        }
        private static Dictionary<SIngame.IMyTerminalBlock, bool> GetTurretWeaponsEnabled(IMyTerminalBlock Me)
        {
            try
            {
                if (Utils.NullEntity(Me) || (!(Me is IMyProgrammableBlock)) || !BlockBindings_WeaponSystem.ContainsKey(Me)) return null;
                return BlockBindings_WeaponSystem[Me].GetTurretWeaponsEnabled();
            }
            catch (Exception) { }
            return null;
        }
        private static bool EnabledBlock(IMyTerminalBlock block)
        {
            return block is SIngame.IMyProgrammableBlock && block.IsFunctional && (block as SIngame.IMyProgrammableBlock).Enabled;
        }
    }
}