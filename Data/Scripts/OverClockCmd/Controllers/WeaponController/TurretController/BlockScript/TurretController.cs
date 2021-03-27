using VRage.Game;
using VRage.Game.Components;


namespace SuperBlocks.Controller
{
    using static SuperBlocks.Utils;
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TurretController", "SmallTurretController")]
    public partial class TurretController : MyGridProgram4ISConvert
    {
        protected override void Program()
        {
        }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource)
        {
        }
        protected override void ClosedBlock()
        {
        }
        protected override void CustomDataChangedProcess()
        {
        }

        public volatile bool TurretEnabled;
        public volatile bool AutoFire;
        public volatile bool UsingWeaponCoreTracker;
        public volatile float RangeMult;
        public volatile float GravityMult;
        public volatile float Ini;
        public volatile bool IsDirect;
        public volatile bool Ignore_speed_self;
        public volatile float Delta_t;
        public volatile float Delta_precious;
        public volatile float Calc_t;
        public volatile float TimeFixed;
        public volatile float InitialSpeed;
        public volatile float DesiredSpeed;
        public volatile float AccelPerSec;
        public volatile float GravityMultiplier;

        public volatile int MaxTime;
        public volatile float MaxTrajectory;
        public volatile uint MaxTrajectoryTime;

       

    }
}
