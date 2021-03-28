using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class MyRadarSubtypeIdHelper
        {
            public static IMyTerminalBlock GetFarestDetectedBlock(IMyCubeGrid Grid)
            {
                if (Common.NullEntity(Grid)) return null;
                List<IMyTerminalBlock> RadarHooks = new List<IMyTerminalBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid)?.GetBlocksOfType<IMyTerminalBlock>(RadarHooks, AvaliableRadarBlocks);
                return RadarHooks.MaxBy(DetectedRangeBlock);
            }
            public static float CommunicateRangeBlock(IMyTerminalBlock block)
            {
                if (Common.NullEntity(block)) return 0;
                if (block is IMyRadioAntenna) return 4e4f;
                if (block is IMyLaserAntenna) return 4e7f;
                if (block is IMySensorBlock) return 2.5e3f;
                if (block is IMyBeacon) return 4e4f;
                if (block.BlockDefinition.SubtypeId.Contains("QuantumRadar")) return 1e8f;
                if (block.BlockDefinition.SubtypeId.Contains("SpaceRadar")) return 1e10f;
                if (block.BlockDefinition.SubtypeId.Contains("RegularRadar")) return 5e12f;
                return 0;
            }
            public static float DetectedRangeBlock(IMyTerminalBlock block)
            {
                if (Common.NullEntity(block)) return 0;
                if (block is IMyRadioAntenna) return Math.Min((block as IMyRadioAntenna).Radius, 5e4f);
                if (block is IMyLaserAntenna) return Math.Min((block as IMyLaserAntenna).Range, 5e6f);
                if (block is IMySensorBlock) return Math.Min((block as IMySensorBlock).MaxRange, 3e3f);
                if (block is IMyBeacon) return Math.Min((block as IMyBeacon).Radius, 5e4f);
                if (block.BlockDefinition.SubtypeId.Contains("QuantumRadar")) return 1e8f;
                if (block.BlockDefinition.SubtypeId.Contains("SpaceRadar")) return 1e10f;
                if (block.BlockDefinition.SubtypeId.Contains("RegularRadar")) return 5e12f;
                return 0;
            }
            public static bool AvaliableRadarBlocks(IMyTerminalBlock block)
            {
                if (Common.NullEntity(block)) return false;
                if (block is IMyRadioAntenna) return true;
                if (block is IMyLaserAntenna) return true;
                if (block is IMySensorBlock) return true;
                if (block is IMyBeacon) return true;
                if (block.BlockDefinition.SubtypeId.Contains("QuantumRadar")) return true;
                if (block.BlockDefinition.SubtypeId.Contains("SpaceRadar")) return true;
                if (block.BlockDefinition.SubtypeId.Contains("RegularRadar")) return true;
                return false;
            }
        }



    }
}
