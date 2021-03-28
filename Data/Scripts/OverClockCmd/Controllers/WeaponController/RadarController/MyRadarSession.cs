using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace SuperBlocks.Controller
{
    /// <summary>
    /// 雷达系统
    /// 绑定至网格上面
    /// 每个网格只有一个该脚本
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.AfterSimulation | MyUpdateOrder.Simulation)]
    public class MyRadarSession : MySessionComponentBase
    {
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation() { if (!Initialized) return; if (!SetupComplete) { SetupComplete = true; Init(); return; } count = (count + 1) % 100; UpdateCubeGrids(); }
        public static MyRadarTargets GetRadarTargets(IMyTerminalBlock Me)
        {
            try
            {
                var grid = Me?.GetTopMostParent() as IMyCubeGrid;
                if (Utils.Common.NullEntity(grid)) return null;
                return RadarScripts[grid];
            }
            catch (Exception) { return null; }
        }

        public override void Simulate()
        {
            if (!Initialized) return; 
            if (!SetupComplete) return;
            count_s = (count_s + 1) % 100;
            try
            {               
                if (count_s % 18 == 0)
                {
                    var removeable = RadarScripts.Keys.Where(g => Utils.Common.NullEntity(g) || Utils.Common.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(g), Utils.RadarSubtypeId.AvaliableRadarBlocks).Count < 1);
                    if (!Utils.Common.IsNullCollection(removeable))
                    {
                        MyAPIGateway.Parallel.ForEach(removeable, cubeGrid => { MyRadarTargets RadarTargets; RadarScripts.TryRemove(cubeGrid, out RadarTargets); });
                    }
                }
                if (count_s % 19 == 0)
                {
                    HashSet<IMyEntity> Entities = new HashSet<IMyEntity>();
                    MyAPIGateway.Entities.GetEntities(Entities, e => !Utils.Common.NullEntity(e) && (e is IMyCubeGrid));
                    if (Utils.Common.IsNullCollection(Entities)) return;
                    var Grids = Entities.ToList().ConvertAll(e => e as IMyCubeGrid)?.Where(g => Utils.Common.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(g), Utils.RadarSubtypeId.AvaliableRadarBlocks).Count > 0);
                    if (Utils.Common.IsNullCollection(Grids)) return;
                    MyAPIGateway.Parallel.ForEach(Grids, cubeGrid => { RadarScripts.TryAdd(cubeGrid, new MyRadarTargets()); });
                }
                if (Utils.Common.IsNullCollection(RadarScripts)) return;
                if (count_s % 7 == 0)
                    MyAPIGateway.Parallel.ForEach(RadarScripts, RadarScript => { RadarScript.Value.UpdateScanning(RadarScript.Key); });
            }
            catch (Exception) { }
        }

        private uint count_s = 0;
        private uint count = 0;
        private static ConcurrentDictionary<IMyCubeGrid, MyRadarTargets> RadarScripts { get; } = new ConcurrentDictionary<IMyCubeGrid, MyRadarTargets>();
        private void Init()
        {
            count = 0;
            count_s = 0;
            try { RadarScripts.Clear(); } catch (Exception) { }

        }
        protected sealed override void UnloadData()
        {
            try { RadarScripts.Clear(); } catch (Exception) { }
        }
        private void UpdateCubeGrids()
        {
            try
            {
               
            }
            catch (Exception) { }
        }
    }
}
