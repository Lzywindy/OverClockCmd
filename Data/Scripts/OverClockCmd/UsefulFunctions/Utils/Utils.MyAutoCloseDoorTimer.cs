using Sandbox.ModAPI;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyAutoCloseDoorTimer
        {
            public MyAutoCloseDoorTimer(IMyDoor Door) { this.Door = Door; }
            public void Running()
            {
                if (Door == null) return;
                switch (Door.Status)
                {
                    case Sandbox.ModAPI.Ingame.DoorStatus.Opening: Count = Gap; return;
                    case Sandbox.ModAPI.Ingame.DoorStatus.Open: if (Count > 0) Count--; else Door.CloseDoor(); return;
                    default: break;
                }
            }
            private readonly IMyDoor Door;
            private const int Gap = 25;
            private int Count;
        }
    }
}