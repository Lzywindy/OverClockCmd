using VRage.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
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
            WeaponSystemManage.Update();
        }
        public void Init()
        {
           
        }
        protected sealed override void UnloadData()
        {
           
        }
    }
}