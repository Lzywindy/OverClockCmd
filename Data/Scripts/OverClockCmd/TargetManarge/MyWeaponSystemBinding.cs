using Sandbox.ModAPI;
using System.Collections.Generic;

namespace SuperBlocks.Controller
{
    public class MyWeaponSystemBinding : UpdateableClass
    {
        public MyWeaponSystemBinding(IMyTerminalBlock Me) : base()
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
            try
            {
                TurretCtrl.Init(Me);
            }
            catch (System.Exception e) { MyAPIGateway.Utilities.ShowNotification($"Error:{e.Message}"); }
           
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (updatecounts % 98 == 0) { MyConfigs.CustomDataConfigRead_INI(CtrlBlock, Configs); }
            TurretCtrl.UpdateFunctions(CtrlBlock);
        }


        #region PrivateVariables
        private Dictionary<string, Dictionary<string, string>> Configs { get; } = new Dictionary<string, Dictionary<string, string>>();
        private MyTurretController TurretCtrl { get; } = new MyTurretController();
        #endregion
        #region InitAndEnabledFunctions
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