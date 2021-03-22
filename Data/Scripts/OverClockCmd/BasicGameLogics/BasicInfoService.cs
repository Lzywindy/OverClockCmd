using VRage.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using System;
using VRageMath;
using VRage;
using VRage.Game.ModAPI;
using System.Linq;

namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
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
            try { UpdateWeaponCoreApis(); } catch (Exception) { }
            try { MyWeaponSystemManage.Update(); } catch (Exception) { }

        }
        public void Init()
        {
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_Gravity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.Gravity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_LinearVelocity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_AngularVelocity", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.AngularVelocity ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_LinearAcceleration", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.LinearAcceleration ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Vector3, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_AngularAcceleration", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => block?.CubeGrid?.Physics?.AngularAcceleration ?? Vector3.Zero, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, long?, MyTuple<long, Vector3D, Vector3D, Vector3D>>, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_Target_Tracker", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => Me_Target_Tracker, (block, value) => { });
            CreateProperty.CreateProperty_PB_IN<Func<MyTuple<Sandbox.ModAPI.Ingame.IMyTerminalBlock, ICollection<Sandbox.ModAPI.Ingame.IMyTerminalBlock>, MyTuple<long, float, float, float, float, float>, MyTuple<Vector3D?, double?>?>, MyTuple<Vector3D?, double?>?>, Sandbox.ModAPI.Ingame.IMyProgrammableBlock>("Me_PredictTarget", block => block is Sandbox.ModAPI.Ingame.IMyProgrammableBlock, block => CalculateDirection, (block, value) => { });
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
        private static Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, long?, MyTuple<long, Vector3D, Vector3D, Vector3D>> Me_Target_Tracker = (Sandbox.ModAPI.Ingame.IMyTerminalBlock Detector, long? TargetID) =>
        {
            var default_value = new MyTuple<long, Vector3D, Vector3D, Vector3D>(-1, default(Vector3D), default(Vector3D), default(Vector3D));
            var Entity = MyAPIGateway.Entities.GetEntityById(TargetID);
            if (Utils.Common.NullEntity(Entity) || Detector == null || !Detector.IsFunctional || !Utils.MyTargetEnsureAPI.CouldBeEnemy(Entity, Detector)) return default_value;
            default_value.Item2 = Utils.MyTargetEnsureAPI.GetTargetedBlock(Entity as IMyCubeGrid, Detector, WcApi.HasCoreWeapon)?.GetPosition() ?? Entity?.GetPosition() ?? Vector3.Zero;
            default_value.Item3 = Entity?.Physics?.LinearVelocity ?? Vector3.Zero;
            default_value.Item4 = Entity?.Physics?.LinearAcceleration ?? Vector3.Zero;
            return default_value;
        };
        private static Func<MyTuple<Sandbox.ModAPI.Ingame.IMyTerminalBlock, ICollection<Sandbox.ModAPI.Ingame.IMyTerminalBlock>, MyTuple<long, float, float, float, float, float>, MyTuple<Vector3D?, double?>?>, MyTuple<Vector3D?, double?>?> CalculateDirection
            = (MyTuple<Sandbox.ModAPI.Ingame.IMyTerminalBlock, ICollection<Sandbox.ModAPI.Ingame.IMyTerminalBlock>, MyTuple<long, float, float, float, float, float>, MyTuple<Vector3D?, double?>?> Parameters) => MyTargetPredict.CalculateDirection(Parameters.Item1 as IMyTerminalBlock,
                Parameters.Item2?.ToList()?.ConvertAll(b => b as IMyTerminalBlock),
                Parameters.Item3.Item1, Parameters.Item3.Item2, Parameters.Item3.Item3, Parameters.Item3.Item4, Parameters.Item3.Item5, Parameters.Item3.Item6, Parameters.Item4);

        #endregion
    }
}