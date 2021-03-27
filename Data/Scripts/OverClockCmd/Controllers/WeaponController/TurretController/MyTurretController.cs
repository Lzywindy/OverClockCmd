using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public class MyTurretController : UpdateableClass
    {
        public MyTurretController() : base() { }
        public void Init(IMyTerminalBlock Me) => UpdateBindings(Me);
        public volatile bool TurretEnabled;
        public volatile bool AutoFire;
        public volatile bool UsingWeaponCoreTracker;
        public void CycleWeapons()
        {
        }
        #region 炮塔控制器
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (updatecounts % 9 == 0) { if (!MyRadarSession.SetupComplete) RadarTargets = null; else RadarTargets = MyRadarSession.GetRadarTargets(CtrlBlock); }
            if (updatecounts % 8 == 0) { UpdateBindings(CtrlBlock); }
            if (updatecounts % 7 == 0) { MyAPIGateway.Parallel.ForEach(Turrets, Turret => Turret.ReadConfig_Turret_Rotors()); }
            foreach (var Turret in Turrets) { UpdateTurrets(Turret, CtrlBlock); }
            //MyAPIGateway.Utilities.ShowNotification($"Turrets:{Turrets?.Count ?? 0}");
        }
        private void UpdateTurrets(MyTurretBinding Turret, IMyTerminalBlock CtrlBlock)
        {
            if (!Utils.Common.NullEntity(Me))
                Turret.SetConfig(Configs);
            if (RadarTargets == null || !TurretEnabled) { Turret.AimTarget = null; }
            else
            {
                try
                {
                    Turret.AimTarget = UsingWeaponCoreTracker ? new MyTargetDetected(BasicInfoService.WcApi.GetAiFocus(CtrlBlock.CubeGrid), CtrlBlock, true) : RadarTargets.GetTheMostThreateningTarget(Turret.MotorAz, DefaultConfig.Range, Turret.TargetInRange_Angle);// target;
                }
                catch (Exception) { }
            }
            Turret.AutoFire = AutoFire;
            Turret.Enabled = TurretEnabled;
            Turret.Running();
        }
        private IMyTerminalBlock Me;
        private MyRadarTargets RadarTargets;
        private volatile string ConfigsStrs;
        private ConcurrentBag<MyTurretBinding> Turrets;
        private MyWeaponParametersConfig DefaultConfig;

        private IMyGridTerminalSystem GridTerminalSystem { get { if (Utils.Common.NullEntity(Me?.CubeGrid)) return null; return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid); } }
        ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private void UpdateBindings(IMyTerminalBlock Me)
        {
            this.Me = Me;
            if (Utils.Common.NullEntity(this.Me)) return;
            ConfigsStrs = Me.CustomData;
            MyConfigs.Concurrent.CustomDataConfigRead_INI(ConfigsStrs, Configs);
            DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
            if (!Configs.ContainsKey("DefaultTurretWeaponConfig")) MyWeaponParametersConfig.SaveConfig(DefaultConfig, Configs, "DefaultTurretWeaponConfig");
            if (!Configs.ContainsKey("DefaultWeaponCoreWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.DefaultWeaponCore, Configs, "DefaultWeaponCoreWeapon");
            if (!Configs.ContainsKey("KeensRocketWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensRocket, Configs, "KeensRocketWeapon");
            if (!Configs.ContainsKey("KeensProjectile_SmallWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Small, Configs, "KeensProjectile_SmallWeapon");
            if (!Configs.ContainsKey("KeensProjectile_LargeWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Large, Configs, "KeensProjectile_LargeWeapon");
            if (!Configs.ContainsKey("EnergyWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.Energy, Configs, "EnergyWeapon");
            ConfigsStrs = Me.CustomData = MyConfigs.Concurrent.CustomDataConfigSave_INI(Configs);
            List<MyTurretBinding> list;
            if (Utils.Common.IsNullCollection(Turrets))
                list = Utils.Common.GetTs<IMyMotorStator>(Me, HasEvMotors).ConvertAll(az => new MyTurretBinding(az));
            else
                list = Turrets.Where(t => t.CanRunning).ToList();
            if (Utils.Common.IsNullCollection(list)) { Turrets = null; return; }
            Turrets = new ConcurrentBag<MyTurretBinding>(list);
        }
        private static bool HasEvMotors(IMyMotorStator MotorAz)
        {
            if (Utils.Common.IsNull(MotorAz?.TopGrid)) return false;
            return Utils.Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985).Count > 0;
        }
        #endregion
    }
}