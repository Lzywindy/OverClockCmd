using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
namespace SuperBlocks.Controller
{
    public sealed partial class WAN_Network
    {
        /// <summary>
        /// 通讯器的类型
        /// 无线电（需求为天线、网关）
        /// 量子通信（需求为天线、量子网关，确认连接完成之后可以关闭天线）
        /// 空间通信（需求为一个空间网关）
        /// 规则通信（需求为一个规则网关）
        /// </summary>
        [Flags]
        public enum CommunicationType { None = 0x00, Radio = 0x01, Quantum = 0x02, Space = 0x04, Regular = 0x08 }
        /// <summary>
        /// 广域网注册
        /// 用于网格之间的网络通讯
        /// 主要用于本MOD中内部方块信息的交换
        /// 信息交换的方块需要与之对等的通讯方块
        /// 需求：所属一致、种类一致（量子通信收发器只能和带有量子通信方式的收发器进行通信）
        /// 键值为网络发起方EntityID
        /// </summary>
        private static Dictionary<string, WAN_Network> Grid_WAN_Networks { get; } = new Dictionary<string, WAN_Network>();
        /// <summary>
        /// 广域网权限
        /// </summary>
        private static Dictionary<string, List<long>> Grid_Authority_Users { get; } = new Dictionary<string, List<long>>();
        public static void RegistBlock(IMyTerminalBlock block, string NetworkName)
        {
            if (block == null) return;
            var gridlogic = block.GameLogic.GetAs<InformationDeviceBase>();
            if (gridlogic == null) return;
            if (!gridlogic.CanCommunicate) return;
            if (!Grid_Authority_Users.ContainsKey(NetworkName))
                Grid_Authority_Users.Add(NetworkName, new List<long>(block.CubeGrid.SmallOwners));
            else
            {
                var list = Grid_Authority_Users[NetworkName];
                if (!list.Contains(block.OwnerId)) return;
            }
            if (!Grid_WAN_Networks.ContainsKey(NetworkName))
                Grid_WAN_Networks.Add(NetworkName, new WAN_Network(block));
            else
                Grid_WAN_Networks[NetworkName].AddBlock(block);
        }
        public static void UnregistBlock(IMyTerminalBlock block)
        {
            if (block == null) return;
            var gridlogic = block.GameLogic.GetAs<InformationDeviceBase>();
            if (gridlogic == null) return;
            foreach (var Grid_WAN_Network in Grid_WAN_Networks)
            {
                if (Grid_WAN_Network.Value != null && Grid_WAN_Network.Value.my_mod_blocks.Contains(block.EntityId))
                    Grid_WAN_Network.Value.my_mod_blocks.Remove(block.EntityId);
            }
        }
        public static void UpdateInfos()
        { 
        }
        //public static List<long> GetAllWANs(IMyTerminalBlock block)
        //{
        //    if (block == null) return null;
        //    if
        //}
        //public static List<long> GetAllEntityInWAN(IMyTerminalBlock block)
        //{ 
        //}
    }
    public sealed partial class WAN_Network
    {
        private HashSet<long> my_mod_blocks { get; } = new HashSet<long>();
        private WAN_Network(IMyTerminalBlock block)
        {
            if (block == null) return;
            my_mod_blocks.Add(block.EntityId);
        }
        private void AddBlock(IMyTerminalBlock my_mod_block)
        {
        }
        private void RemoveBlock(IMyTerminalBlock my_mod_block)
        {
        }
        private bool Empty()
        {
            if (my_mod_blocks.Count < 1) return true;
            return false;
        }
    }
}
