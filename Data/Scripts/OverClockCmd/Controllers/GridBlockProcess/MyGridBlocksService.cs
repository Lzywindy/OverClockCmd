using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
namespace SuperBlocks.Controller
{
    public class MyGridBlocksService : MyGameLogicComponent
    {
        public sealed override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);

            NeedsUpdate |= (MyEntityUpdateEnum.BEFORE_NEXT_FRAME | MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        public sealed override void UpdateOnceBeforeFrame()
        {
            base.UpdateOnceBeforeFrame();
            try
            {
            }
            catch (Exception) { }
        }
        public sealed override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            try
            {
               
            }
            catch (Exception) { }
        }
        public sealed override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            try
            {
               
            }
            catch (Exception) { }
        }
        public sealed override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            try
            {
              
            }
            catch (Exception) { }
        }
        public sealed override void Close()
        {
            base.Close(); 
            try
            {
                
            }
            catch (Exception)
            {
               
            }
        }

        IMyCubeGrid ThisGrid => Entity as IMyCubeGrid;
    }
   
}
