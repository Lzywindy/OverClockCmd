using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRage.ModAPI;
namespace SuperBlocks.Controller
{
    public sealed partial class LAN_Network
    {
        private HashSet<long> my_mod_blocks { get; } = new HashSet<long>();
        private LAN_Network(IMyTerminalBlock my_mod_block)
        {
            if (my_mod_block == null) return;
            var controller_logic = my_mod_block.GameLogic.GetAs<ControlBase>();
            var informationer_logic = my_mod_block.GameLogic.GetAs<InformationDeviceBase>();
            if (informationer_logic == null || controller_logic == null) return;
            my_mod_blocks.Add(my_mod_block.EntityId);
        }
        private void AddBlock(IMyTerminalBlock my_mod_block)
        {
            if (my_mod_block == null) return;
            my_mod_blocks.Add(my_mod_block.EntityId);
        }
        private void RemoveBlock(IMyTerminalBlock my_mod_block)
        {
            if (my_mod_block == null) return;
            my_mod_blocks.Remove(my_mod_block.EntityId);
        }
        private bool Empty()
        {
            return my_mod_blocks.Count == 0;
        }
    }
    public sealed partial class LAN_Network
    {
        /// <summary>
        /// 局域网注册
        /// 用于网格内部的网络通讯
        /// 主要用于本MOD中内部方块信息的交换
        /// 信息交换的方块需要与之对等的通讯方块
        /// 需求：所属一致
        /// </summary>
        private static Dictionary<long, LAN_Network> Grid_LAN_Networks { get; } = new Dictionary<long, LAN_Network>();
        /// <summary>
        /// 注册这个方块
        /// </summary>
        /// <param name="block">方块</param>
        public static void RegistBlock(IMyTerminalBlock block)
        {
            if (block == null) return;
            if (block is ControlBase || block is InformationDeviceBase)
            {
                if (!Grid_LAN_Networks.ContainsKey(block.GetTopMostParent().EntityId))
                    Grid_LAN_Networks.Add(block.GetTopMostParent().EntityId, new LAN_Network(block));
                else
                {
                    var ent = MyAPIGateway.Entities.GetEntityById(block.GetTopMostParent().EntityId) as IMyCubeGrid;
                    if (ent == null) return;
                    if (ent.BigOwners.Contains(block.OwnerId) || ent.SmallOwners.Contains(block.OwnerId))
                        Grid_LAN_Networks[block.GetTopMostParent().EntityId].AddBlock(block);
                }
            }
        }
        /// <summary>
        /// 解除注册这个方块
        /// </summary>
        /// <param name="block">方块</param>
        public static void UnregistBlock(IMyTerminalBlock block)
        {
            if (block == null) return;
            if (!Grid_LAN_Networks.ContainsKey(block.GetTopMostParent().EntityId)) return;
            Grid_LAN_Networks[block.GetTopMostParent().EntityId].RemoveBlock(block);
            if (Grid_LAN_Networks[block.GetTopMostParent().EntityId].Empty())
                Grid_LAN_Networks.Remove(block.GetTopMostParent().EntityId);
        }
        /// <summary>
        /// 更新存储的信息
        /// </summary>
        public static void UpdateInfos()
        {
            List<long> RemoveList = new List<long>();
            foreach (var Grid_LAN_Network in Grid_LAN_Networks)
            {
                if (!MyAPIGateway.Entities.EntityExists(Grid_LAN_Network.Key) || Grid_LAN_Network.Value.Empty())
                    RemoveList.Add(Grid_LAN_Network.Key);
            }
            foreach (var item in RemoveList)
            {
                Grid_LAN_Networks.Remove(item);
            }
        }
        /// <summary>
        /// 得到该方块所在的局域网中的所有方块ID
        /// </summary>
        /// <param name="block">该方块</param>
        /// <returns>所在的局域网中的所有方块ID</returns>
        public static List<long> GetAllBlocksInLAN(IMyTerminalBlock block)
        {
            if (block == null) return null;
            if (block is ControlBase || block is InformationDeviceBase)
            {
                if (!Grid_LAN_Networks.ContainsKey(block.GetTopMostParent().EntityId)) return null;
                return Grid_LAN_Networks[block.GetTopMostParent().EntityId].my_mod_blocks.ToList();
            }
            return null;
        }
    }
}
