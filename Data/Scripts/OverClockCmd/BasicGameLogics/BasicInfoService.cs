using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation | MyUpdateOrder.Simulation)]
    public sealed class BasicInfoService : MySessionComponentBase
    {
        public static HashSet<IMyTerminalBlock> Register { get; } = new HashSet<IMyTerminalBlock>();
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!SetupComplete)
            {
                SetupComplete = true;
                Init();
                if (!WcApi.IsReady)
                {
                    WcApi.Load(null, true);
                    foreach (var weapondefs in WcApi.WeaponDefinitions)
                    {
                        var ammos = weapondefs.Ammos;
                        if (Utils.Common.IsNullCollection(ammos)) continue;
                        ConcurrentDictionary<string, TrajectoryDef> _tammos = new ConcurrentDictionary<string, TrajectoryDef>();
                        foreach (var item in ammos)
                            _tammos.AddOrUpdate(item.AmmoRound, TrajectoryDef.CreateFromWeaponCoreDatas(item), (k, v) => v);
                        if (Utils.Common.IsNullCollection(weapondefs.Assignments.MountPoints)) continue;
                        foreach (var item in weapondefs.Assignments.MountPoints)
                            WeaponInfos.AddOrUpdate(item.SubtypeId, _tammos, (k, v) => v);
                    }
                }
                return;
            }
            //MyAPIGateway.Utilities.ShowNotification($"Weapon Defs:{WcApi.WeaponDefinitions.Count}");
            //try { } catch (Exception) { }
            


        }
        public override void Simulate()
        {
            base.Simulate();
           
            //try { MyWeaponSystemManage.Update(); } catch (Exception) { }
            //try { MyWeaponSystemManage.Update(); } catch (Exception) { }
        }
        public void Init()
        {
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_Gravity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.Gravity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_LinearVelocity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_AngularVelocity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.AngularVelocity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_LinearAcceleration", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.LinearAcceleration ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_AngularAcceleration", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.AngularAcceleration ?? Vector3.Zero, (block, value) => { });
        }
        protected sealed override void UnloadData()
        {
            try { WcApi.Unload(); } catch (Exception) { }
        }
        private void UpdateWeaponCoreApis()
        {
            try { if (!WcApi.IsReady) WcApi.Load(null, true); } catch (Exception) { }
            //MyAPIGateway.Utilities.ShowNotification($"Weapon Defs:{WcApi.WeaponDefinitions.Count}");
        }
        public static WeaponCore.Api.WcApi WcApi { get; } = new WeaponCore.Api.WcApi();

        public static ConcurrentDictionary<string, ConcurrentDictionary<string, TrajectoryDef>> WeaponInfos { get; } = new ConcurrentDictionary<string, ConcurrentDictionary<string, TrajectoryDef>>();
        #region ProgrammableBlock实用参数

        #endregion
    }
}