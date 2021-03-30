using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;

namespace SuperBlocks
{
    public static partial class Utils
    {
        public sealed class MyBlockGroupService
        {
            private List<IMyBlockGroup> blockGroups { get; } = new List<IMyBlockGroup>();
            public void Init(IMyTerminalBlock Me)
            {
                if (Common.NullEntity(Me?.CubeGrid)) return;
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid)?.GetBlockGroups(blockGroups);
                blockGroups.RemoveAll(g => !Common.BlockInTurretGroup(g, Me));
            }
            public bool TestBlockInGroups(IMyTerminalBlock Block) => blockGroups.Any(g => Common.BlockInTurretGroup(g, Block));
        }
    }
}
