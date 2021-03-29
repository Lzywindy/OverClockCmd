using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRageMath;

namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation| MyUpdateOrder.Simulation)]
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
                return;
            }
            //MyAPIGateway.Utilities.ShowNotification($"Weapon Defs:{WcApi.WeaponDefinitions.Count}");
            try { if (!WcApi.IsReady) WcApi.Load(null, true); } catch (Exception) { }
           

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


        #region ProgrammableBlock实用参数

        #endregion
    }
}