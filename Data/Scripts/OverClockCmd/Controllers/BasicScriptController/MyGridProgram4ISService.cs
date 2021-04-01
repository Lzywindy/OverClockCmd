using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public sealed class MyGridProgram4ISConvertService : MySessionComponentBase
    {
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            try
            {
                if (!Initialized) return;
                if (!BasicInfoService.SetupComplete) return;
                if (!SetupComplete) { Init(); SetupComplete = true;  return; }
            }
            catch (Exception) { }
        }
        public void Init()
        {
            MyGridProgram4ISConvert.LoadupInterface();
        }
        protected override void UnloadData()
        {
            MyGridProgram4ISConvert.UnloadInterface();
            base.UnloadData();
        }       
    }
}
