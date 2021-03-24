using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;

namespace SuperBlocks.Controller
{
    public class MyWeaponSystemBinding : UpdateableClass
    {
        public MyWeaponSystemBinding(IMyTerminalBlock Me) : base() { MyConfigs.CustomDataConfigRead_INI(Me, Configs); }




        public void SetWeaponAmmo(MyTuple<string, string> WANM)
        {
            TurretCtrl.SetWeaponAmmoConfigInfo(WANM.Item1, WANM.Item2);
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (!RadarOnline) return;
            if (updatecounts % 98 == 0) { MyConfigs.CustomDataConfigRead_INI(CtrlBlock, Configs); TurretCtrl.Configs = new MyTurretConfig(Configs, TurretID); }
            RadarTargets.Update(CtrlBlock);
            TurretCtrl.Update(CtrlBlock);
        }


        public int ActiveTurrets => TurretCtrl.ActiveTurrets;
        public bool HasTarget => TurretCtrl.HasTarget;
        public bool AnyTurretsReady => TurretCtrl.AnyTurretsReady;
        public bool AllTurretsReady => TurretCtrl.AllTurretsReady;

        #region PrivateVariables
        private bool RadarOnline => RadarTargets.Range > 10;
        private Dictionary<string, Dictionary<string, string>> Configs { get; } = new Dictionary<string, Dictionary<string, string>>();
        public MyRadarTargets RadarTargets { get; } = new MyRadarTargets();
        private MyTurretController TurretCtrl { get; } = new MyTurretController();
        #endregion
        #region InitAndEnabledFunctions
        public void InitTurret(IMyTerminalBlock Me)
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
            TurretCtrl.Init(Me, Configs, TurretID);
            TurretCtrl.SetTargetLocker(RadarTargets);
        }
        public void SetTurretEnabled(bool value) => TurretCtrl.TurretEnabled = value && EnabledWeapons;
        public void SetAutoFire(bool value) => TurretCtrl.AutoFire = value && EnabledWeapons;
        public void SetUsingWeaponCoreTracker(bool value) => TurretCtrl.UsingWeaponCoreTracker = value && EnabledWeapons;
        public void SetFixedWeaponsEnabled(bool value) => EnabledFixedWeapon = value;
        public bool TurretEnabled { get { return TurretCtrl.TurretEnabled && EnabledWeapons; } set { TurretCtrl.TurretEnabled = value && EnabledWeapons; } }
        public bool AutoFire { get { return TurretCtrl.AutoFire && EnabledWeapons; } set { TurretCtrl.AutoFire = value && EnabledWeapons; } }
        public bool UsingWeaponCoreTracker { get { return TurretCtrl.UsingWeaponCoreTracker; } set { TurretCtrl.UsingWeaponCoreTracker = value; } }
        public bool EnabledFixedWeapon { get; set; } = false;
        public bool EnabledWeapons { get; set; } = false;
        public const string TurretID = "TurretSetup";
        #endregion
        #region ConstValues
        private const string FixedWeaponID = "FixedWeapon";
        #endregion
    }
}