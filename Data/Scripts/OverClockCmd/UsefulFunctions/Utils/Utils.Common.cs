using Sandbox.ModAPI;
using SuperBlocks.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class Common
        {
            public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero;
            public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero;
            public static bool IsNullCollection<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1;
            public static bool IsNullCollection<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1;
            public static bool IsNull<T>(T Value) where T : class => Value == null;
            public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null || Ent.Closed || Ent.MarkedForClose;
            public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs<T>(gridTerminalSystem, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
            public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (gridTerminalSystem == null) return Items; gridTerminalSystem.GetBlocksOfType<T>(Items, requst); return Items; }
            public static T GetT<T>(IMyTerminalBlock block, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs<T>(block, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
            public static List<T> GetTs<T>(IMyTerminalBlock block, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (block == null || block.CubeGrid == null) return Items; MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid)?.GetBlocksOfType<T>(Items, requst); return Items; }
            public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0) => gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock;
            public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null) { if (gridTerminalSystem == null) return null; return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock); }
            public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class => GetTs(blockGroup, requst).FirstOrDefault();
            public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (blockGroup == null) return Items; blockGroup.GetBlocksOfType<T>(Items, requst); return Items; }
            public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>());
            public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController) { Matrix me_matrix; ShipController.Orientation.GetMatrix(out me_matrix); return me_matrix; }
            public static List<IMySlimBlock> GetBlocksFromGrid(IMyCubeGrid Grid, Func<IMySlimBlock, bool> Filter = null)
            {
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                Grid?.GetBlocks(blocks, Filter);
                return blocks;
            }
            public static Sandbox.ModAPI.Ingame.IMyBlockGroup GetBg(IMyTerminalBlock block, Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, bool> requst = null) { List<IMyBlockGroup> Items = GetTs<IMyBlockGroup>(block, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
            public static List<Sandbox.ModAPI.Ingame.IMyBlockGroup> GetBgs(IMyTerminalBlock block, Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, bool> requst = null)  { List<Sandbox.ModAPI.Ingame.IMyBlockGroup> Items = new List<Sandbox.ModAPI.Ingame.IMyBlockGroup>(); if (block == null || block.CubeGrid == null) return Items; MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid)?.GetBlockGroups(Items, requst); return Items; }
            public static bool IsStaticWeapon(IMyTerminalBlock block)
            {
                if (NullEntity(block) || !block.IsFunctional || (block is IMyLargeTurretBase)) return false;
                return (block is IMySmallGatlingGun) | (block is IMySmallMissileLauncher) || (block is IMySmallMissileLauncherReload) || BasicInfoService.WeaponInfos.ContainsKey(block.BlockDefinition.SubtypeId);
            }
            public static bool ExceptKeywords(IMyTerminalBlock block) { foreach (var item in BlackList_ShipController) { if (block.BlockDefinition.SubtypeId.Contains(item)) return false; } return true; }
            private static readonly string[] BlackList_ShipController = new string[] { "Hover", "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", };
        }



    }
}
