using Sandbox.Common.ObjectBuilders;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_BatteryBlock), false, "TestZEP")]
    public class SuperEnergyBattarys : MyGameLogicComponent
    {
        MyBatteryBlock block => Entity as MyBatteryBlock;
        public sealed override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            NeedsUpdate |= MyEntityUpdateEnum.EACH_FRAME;
        }
        public sealed override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            try
            {
                if (Utils.Common.NullEntity(block)) return;
                block.CurrentStoredPower = block.MaxStoredPower;
                //block.ChargeMode = Sandbox.ModAPI.Ingame.ChargeMode.Discharge;
            }
            catch (Exception) { }
        }
    }

}
