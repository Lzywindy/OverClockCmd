using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public class MyTurretController
    {
        public MyTurretController() : base() { }
        public void Init(IMyTerminalBlock Me) => UpdateBindings(Me);
        public volatile bool TurretEnabled;
        public volatile bool AutoFire;
        public volatile bool UsingWeaponCoreTracker;
        public void CycleWeapons() { try { MyAPIGateway.Parallel.ForEach(Turrets, Turret => Turret.CycleWeapons()); } catch (Exception) { } }
        #region 炮塔控制器
        public void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            updatecounts = (updatecounts + 1) % 100;
            try
            {
                if (updatecounts % 7 == 0) { if (!MyRadarSession.SetupComplete) RadarTargets = null; else RadarTargets = MyRadarSession.GetRadarTargets(CtrlBlock); }
                if (updatecounts % 8 == 0) { UpdateBindings(CtrlBlock); }
                if (updatecounts % 9 == 0) { MyAPIGateway.Parallel.ForEach(Turrets, Turret => Turret.ReadConfig_Turret_Rotors()); }
                MyAPIGateway.Parallel.ForEach(Turrets, Turret => UpdateTurrets(Turret, CtrlBlock));
                //foreach (var Turret in Turrets) { UpdateTurrets(Turret, CtrlBlock); }
            }
            catch (Exception) { }
        }

        private volatile uint updatecounts = 0;
        private volatile float Range = 3000f;
        private IMyTerminalBlock Me;
        private MyRadarTargets RadarTargets;
        private volatile string ConfigsStrs;
        private ConcurrentBag<MyTurretBinding> Turrets;

        private IMyGridTerminalSystem GridTerminalSystem { get { if (Utils.Common.NullEntity(Me?.CubeGrid)) return null; return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid); } }
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private void UpdateBindings(IMyTerminalBlock Me)
        {
            try
            {
                this.Me = Me;
                if (Utils.Common.NullEntity(this.Me)) return;
                ConfigsStrs = Me.CustomData;
                MyConfigs.Concurrent.CustomDataConfigRead_INI(ConfigsStrs, Configs);
                if (!Configs.ContainsKey("DefaultTurretWeaponConfig")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig"), Configs, "DefaultTurretWeaponConfig");
                if (!Configs.ContainsKey("DefaultWeaponCoreWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.DefaultWeaponCore, Configs, "DefaultWeaponCoreWeapon");
                if (!Configs.ContainsKey("KeensRocketWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensRocket, Configs, "KeensRocketWeapon");
                if (!Configs.ContainsKey("KeensProjectile_SmallWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Small, Configs, "KeensProjectile_SmallWeapon");
                if (!Configs.ContainsKey("KeensProjectile_LargeWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.KeensProjectile_Large, Configs, "KeensProjectile_LargeWeapon");
                if (!Configs.ContainsKey("EnergyWeapon")) MyWeaponParametersConfig.SaveConfig(MyWeaponParametersConfig.Energy, Configs, "EnergyWeapon");
                ConfigsStrs = Me.CustomData = MyConfigs.Concurrent.CustomDataConfigSave_INI(Configs);
                Range = Utils.RadarSubtypeId.DetectedRangeBlock(Utils.RadarSubtypeId.GetFarestDetectedBlock(this.Me.CubeGrid));
                List<MyTurretBinding> list;
                if (Utils.Common.IsNullCollection(Turrets))
                    list = Utils.Common.GetTs<IMyMotorStator>(Me, HasEvMotors).ConvertAll(az => new MyTurretBinding(az));
                else
                    list = Turrets.Where(t => t.CanRunning).ToList();
                if (Utils.Common.IsNullCollection(list)) { Turrets = null; return; }
                Turrets = new ConcurrentBag<MyTurretBinding>(list);
            }
            catch (Exception) { }
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
                    Turret.AimTarget = UsingWeaponCoreTracker ? new MyTargetDetected(BasicInfoService.WcApi.GetAiFocus(CtrlBlock.CubeGrid), CtrlBlock, true) : RadarTargets.GetTheMostThreateningTarget(Turret.MotorAz, Range, Turret.TargetInRange_Angle);// target;
                }
                catch (Exception) { }
            }
            Turret.AutoFire = AutoFire;
            Turret.Enabled = TurretEnabled;
            Turret.Running();
        }
        private static bool HasEvMotors(IMyMotorStator MotorAz)
        {
            if (Utils.Common.IsNull(MotorAz?.TopGrid)) return false;
            return Utils.Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985).Count > 0;
        }
        #endregion
    }
}