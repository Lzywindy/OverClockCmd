using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.Components;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks.Controller
{
    public static class UniversalControllerManage
    {
        private static Dictionary<IMyEntity, IMyTerminalBlock> Register { get; } = new Dictionary<IMyEntity, IMyTerminalBlock>();
        public static bool RegistControllerBlock(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return false;
                if (Register.ContainsKey(TopmostEnt)) return false;
                Register.Add(TopmostEnt, Block);
                return true;
            }
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"UniversalControllerManage"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); return false; }
        }
        public static bool IsMainController(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return false;
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return true; }
                if (Utils.Common.NullEntity(Register[TopmostEnt])) Register.Remove(TopmostEnt);
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return true; }
                return Register[TopmostEnt] == Block;
            }
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"UniversalControllerManage"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); return false; }
        }
        public static string GetRegistControllerBlockConfig(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return "";
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return Block.CustomData; }
                if (Utils.Common.NullEntity(Register[TopmostEnt])) Register.Remove(TopmostEnt);
                if (!Register.ContainsKey(TopmostEnt)) { Register.Add(TopmostEnt, Block); return Block.CustomData; }
                return Register[TopmostEnt].CustomData;
            }
            catch (Exception) { return ""; }
        }
        public static void UnRegistControllerBlock(IMyTerminalBlock Block)
        {
            try
            {
                var TopmostEnt = Block?.GetTopMostParent();
                if (TopmostEnt == null) return;
                if (Register.ContainsKey(TopmostEnt) && Register[TopmostEnt] == Block) { Register.Remove(TopmostEnt); return; }
            }
            catch (Exception e) { MyAPIGateway.Utilities.CreateNotification($"{"UniversalControllerManage"}:{e.Message}"); if (Block?.GetTopMostParent() != null && Register.ContainsKey(Block.GetTopMostParent())) Register.Remove(Block.GetTopMostParent()); }
        }

        public static void SaveDatasExit()
        {
            foreach (var KV in Register)
            {
                try
                {
                    if (Utils.Common.NullEntity(KV.Value)) return;
                    KV.Value?.GameLogic?.GetAs<TurretController>()?.SaveData(KV.Value);
                }
                catch (Exception) { }
            }
        }
    }


    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public sealed class UniversalControllerService : MySessionComponentBase
    {
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!Initialized) return;
                if (!MyGridProgram4ISConvertService.SetupComplete) return;
                if (!SetupComplete) { SetupComplete = true; Init(); return; }
            }
            catch (Exception) { }
        }
        public void Init()
        {
            UniversalController.LoadupInterface_UniversalController();
        }
        protected override void UnloadData()
        {
            UniversalController.UnloadInterface_UniversalController();
            UniversalControllerManage.SaveDatasExit();
            base.UnloadData();
        }
    }
}
