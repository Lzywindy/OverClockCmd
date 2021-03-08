using System;
namespace SuperBlocks.Controller
{
    public class StepTimer
    {
        private readonly int initvalue;
        private int UpdateCount = 0;
        public StepTimer(int interval)
        {
            initvalue = Math.Max(1, interval);
            UpdateCount = 0;
        }
        public void Update()
        {
            if (UpdateCount > 0) { UpdateCount--; return; }
            UpdateCount = initvalue;
            _Action?.Invoke();
        }
        public Action _Action;
    }
}