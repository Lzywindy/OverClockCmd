using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyAutoCloseDoorController
        {
            private List<MyAutoCloseDoorTimer> Timers { get; } = new List<MyAutoCloseDoorTimer>();
            public void UpdateBlocks(IMyGridTerminalSystem GridTerminalSystem) { var doors_group = GridTerminalSystem.GetBlockGroupWithName(ACDoorsGroupNM); if (doors_group == null) return; var doors = Common.GetTs<IMyDoor>(doors_group); foreach (var door in doors) { Timers.Add(new MyAutoCloseDoorTimer(door)); } }
            public void Running(IMyGridTerminalSystem GridTerminalSystem) { try { if (Timers.Count == 0) UpdateBlocks(GridTerminalSystem); else { foreach (var Timer in Timers) { Timer.Running(); } } } catch (Exception) { Timers.Clear(); } }
        }
    }
}