using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using VRage.ModAPI;
namespace SuperBlocks.Controller
{
    public struct MyTargetFilterManager
    {
        public string AimAtConfig
        {
            get
            {
                string cfg = "";
                if (AimAtAllFunctionalBlocks)
                    cfg += "AllFunctionalBlocks|";
                if (AimAtTurret)
                    cfg += "Turret|";
                if (AimAtGuns)
                    cfg += "Guns|";
                if (AimAtThrusts)
                    cfg += "Thrusts|";
                if (AimAtSuspends)
                    cfg += "Suspends|";
                if (AimAtGyros)
                    cfg += "Gyros|";
                if (AimAtCockpits)
                    cfg += "Cockpits|";
                if (AimAtReactors)
                    cfg += "Reactors|";
                if (AimAtBatteries)
                    cfg += "Batteries|";
                if (AimAtCharactors)
                    cfg += "Charactors|";
                if (AimAtMissiles)
                    cfg += "Missiles|";
                if (AimAtMeteros)
                    cfg += "Meteros|";
                return cfg;
            }
            private set
            {
                AimAtAllFunctionalBlocks = value.Contains("AllFunctionalBlocks");
                AimAtTurret = AimAtAllFunctionalBlocks || value.Contains("Turret");
                AimAtGuns = AimAtAllFunctionalBlocks || value.Contains("Guns");
                AimAtThrusts = AimAtAllFunctionalBlocks || value.Contains("Thrusts");
                AimAtSuspends = AimAtAllFunctionalBlocks || value.Contains("Suspends");
                AimAtGyros = AimAtAllFunctionalBlocks || value.Contains("Gyros");
                AimAtCockpits = AimAtAllFunctionalBlocks || value.Contains("Cockpits");
                AimAtReactors = AimAtAllFunctionalBlocks || value.Contains("Reactors");
                AimAtBatteries = AimAtAllFunctionalBlocks || value.Contains("Batteries");
                AimAtCharactors = value.Contains("Charactors");
                AimAtMissiles = value.Contains("Missiles");
                AimAtMeteros = value.Contains("Meteros");
            }
        }
        public bool EntityFilter(IMyEntity ent)
        {
            if (ent == null) return false;
            if (ent is IMyFloatingObject || ent is IMyVoxelBase) return false;
            if ((ent is IMyCharacter) && (!AimAtCharactors)) return false;
            if (Utils.MyTargetEnsureAPI.是否是导弹(ent) && (!AimAtMissiles)) return false;
            if ((ent is IMyMeteor) && (!AimAtMeteros)) return false;
            if ((ent is IMyCubeGrid) && (Utils.MyTargetEnsureAPI.统计网格中通电的方块(ent as IMyCubeGrid) < 4)) return false;
            return true;
        }
        public bool BlockFilter(IMyTerminalBlock block)
        {
            if (block == null) return false;
            if (!block.IsFunctional) return false;
            if (AimAtAllFunctionalBlocks) return true;
            if ((block is IMyLargeTurretBase) && (!AimAtTurret)) return false;
            if ((block is IMySmallGatlingGun || block is IMySmallMissileLauncher || block is IMySmallMissileLauncherReload) && (!AimAtGuns)) return false;
            if ((block is IMyThrust) && (!AimAtThrusts)) return false;
            if ((block is IMyMotorSuspension) && (!AimAtSuspends)) return false;
            if ((block is IMyGyro) && (!AimAtGyros)) return false;
            if ((block is IMyShipController) && (!AimAtCockpits)) return false;
            if ((block is IMyReactor) && (!AimAtReactors)) return false;
            if ((block is IMyBatteryBlock) && (!AimAtBatteries)) return false;
            return true;
        }
        private bool AimAtTurret { get; set; }
        private bool AimAtGuns { get; set; }
        private bool AimAtThrusts { get; set; }
        private bool AimAtSuspends { get; set; }
        private bool AimAtGyros { get; set; }
        private bool AimAtCockpits { get; set; }
        private bool AimAtReactors { get; set; }
        private bool AimAtBatteries { get; set; }
        private bool AimAtAllFunctionalBlocks { get; set; }
        private bool AimAtCharactors { get; set; }
        private bool AimAtMissiles { get; set; }
        private bool AimAtMeteros { get; set; }
        public static implicit operator MyTargetFilterManager(string config)
        {
            MyTargetFilterManager filterManager = new MyTargetFilterManager();
            filterManager.AimAtConfig = config;
            return filterManager;
        }
        public void SetConfig(string config)
        {
            AimAtConfig = config;
        }
    }
}