using Sandbox.ModAPI;
using System;
namespace SuperBlocks.Controller
{
    public class UpdateableClass
    {
        protected int updatecounts { get; private set; }
        public UpdateableClass()
        {
            updatecounts = 100;
        }
        public void Update(IMyTerminalBlock CtrlBlock)
        {
            if (CtrlBlock == null) return;
            if (updatecounts <= 0) { updatecounts = 100; }
            try
            {
                //MyAPIGateway.Utilities.ShowNotification($"Type:{this.GetType()}");
                UpdateFunctions(CtrlBlock);
            }
            //catch (Exception) { }
            catch (Exception)
            {
                //MyAPIGateway.Utilities.ShowNotification($"{e.Source}");
                //MyAPIGateway.Utilities.ShowNotification($"{e.StackTrace}");
                //MyAPIGateway.Utilities.ShowNotification($"{e.Message}");
            }
            updatecounts--;
        }
        protected virtual void UpdateFunctions(IMyTerminalBlock CtrlBlock) { }
    }
}