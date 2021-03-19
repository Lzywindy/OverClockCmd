using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRageMath;
using System.Linq;
using SIngame = Sandbox.ModAPI.Ingame;

namespace SuperBlocks.Controller
{
    public class MyWeaponSystemBinding : UpdateableClass
    {
        public MyWeaponSystemBinding(IMyTerminalBlock Me) : base() { MyConfigs.CustomDataConfigRead_INI(Me, Configs); }




        public void SetWeaponAmmo(MyTuple<string, string> WANM)
        {
            TurretCtrl.SetWeaponAmmoConfigInfo(WANM.Item1, WANM.Item2);
            TargetPredictEnt.SetWeaponAmmoConfigInfo(FixedWeaponConfig, WANM.Item1, WANM.Item2);
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (!RadarOnline) return;
            if (updatecounts % 98 == 0) MyConfigs.CustomDataConfigRead_INI(CtrlBlock, Configs);
            RadarTargets.Update(CtrlBlock);
            TurretCtrl.Update(CtrlBlock);
            TargetPredictEnt.CalculateDirection(CtrlBlock, FixedWeapons);
        }


        public int ActiveTurrets => TurretCtrl.ActiveTurrets;
        public bool HasTarget => TurretCtrl.HasTarget;
        public bool AnyTurretsReady => TurretCtrl.AnyTurretsReady;
        public bool AllTurretsReady => TurretCtrl.AllTurretsReady;

        #region PrivateVariables
        private bool RadarOnline => RadarTargets.Range > 10;
        private Dictionary<string, Dictionary<string, string>> Configs { get; } = new Dictionary<string, Dictionary<string, string>>();
        public MyRadarTargets RadarTargets { get; } = new MyRadarTargets();
        private MyTargetPredict TargetPredictEnt { get; } = new MyTargetPredict();
        private MyTurretController TurretCtrl { get; } = new MyTurretController();
        private MyTurretConfig FixedWeaponConfig;
        private List<IMyTerminalBlock> FixedWeapons { get; } = new List<IMyTerminalBlock>();
        #endregion
        #region InitAndEnabledFunctions
        public void InitRadar(IMyTerminalBlock Me, float Range)
        {
            RadarTargets.ResetParameters(Me, Range);
        }
        public void InitFixedWeapons(IMyTerminalBlock Me, bool Enabled)
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
            EnabledFixedWeapon = Enabled && EnabledWeapons;
            TargetPredictEnt.Init();
            FixedWeaponConfig = new MyTurretConfig(Configs, FixedWeaponID);
        }
        public void InitTurret(IMyTerminalBlock Me, string TurretID)
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
            TurretCtrl.Init(Me, Configs, TurretID);
            TurretCtrl.SetTargetLocker(RadarTargets);
        }
        public void SetTurretEnabled(bool value) => TurretCtrl.TurretEnabled = value && EnabledWeapons;
        public void SetAutoFire(bool value) => TurretCtrl.AutoFire = value && EnabledWeapons;
        public void SetUsingWeaponCoreTracker(bool value) => TurretCtrl.UsingWeaponCoreTracker = value && EnabledWeapons;
        public void SetFixedWeaponsEnabled(bool value) => EnabledFixedWeapon = value;
        public bool GetFixedWeaponsEnabled => TargetPredictEnt.CanFireWeapon(FixedWeapons) && EnabledFixedWeapon;
        public bool TurretEnabled { get { return TurretCtrl.TurretEnabled && EnabledWeapons; } set { TurretCtrl.TurretEnabled = value && EnabledWeapons; } }
        public bool AutoFire { get { return TurretCtrl.AutoFire && EnabledWeapons; } set { TurretCtrl.AutoFire = value && EnabledWeapons; } }
        public bool UsingWeaponCoreTracker { get { return TurretCtrl.UsingWeaponCoreTracker; } set { TurretCtrl.UsingWeaponCoreTracker = value; } }
        public bool EnabledFixedWeapon { get; set; } = false;
        public bool EnabledWeapons { get; set; } = false;
        public string TurretID { get; private set; } = "TurretSetup";
        #endregion
        #region ConstValues
        private const string FixedWeaponID = "FixedWeapon";
        #endregion
        #region PublicFunctions
        public long GetTarget(IMyTerminalBlock CtrlBlock) => RadarTargets.得的最近向我靠近最快的目标(CtrlBlock)?.Entity?.EntityId ?? -1;
        public Vector3D? GetPredictInfo(IMyTerminalBlock CtrlBlock) => TargetPredictEnt.Direction;
        #endregion
    }
}