using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using SIngame = Sandbox.ModAPI.Ingame;
using System.Collections.Concurrent;
using VRageMath;
using System;
using static WeaponThread.Session;

namespace SuperBlocks.Controller
{
    public class MyTurretController : UpdateableClass
    {
        public MyTurretController() : base() { }
        public void Init(IMyTerminalBlock Me, Dictionary<string, Dictionary<string, string>> Config, string TurretID)
        {
            Configs = new MyTurretConfig(Config, TurretID);
            var AzName = Configs.TurretAzNM;
            var azs = Utils.Common.GetTs<IMyMotorStator>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid), b => b.CubeGrid == Me.CubeGrid && b.CustomName.Contains(AzName) && b.TopGrid != null);
            if (Utils.Common.IsNullCollection(azs)) return;
            Turrets.Clear();
            foreach (var az_motor in azs)
            {
                Turrets.AddOrUpdate(az_motor, new MyTurretBinding(), (key, value) => new MyTurretBinding());
                Turrets[az_motor].Init(az_motor, ref Configs);
            }
        }
        public void SetTargetLocker(MyRadarTargets RadarTargets) { this.RadarTargets = RadarTargets; if (RadarTargets == null) MyAPIGateway.Parallel.ForEach(Turrets, Turret => { Turret.Value.TargetPredict.TargetLocked = null; }); }
        public void SetWeaponAmmoConfigInfo(string weaponNM, string ammoNM) { if (Utils.Common.IsNullCollection(Turrets)) return; foreach (var Turret in Turrets) Turret.Value.TargetPredict.SetWeaponAmmoConfigInfo(Configs, weaponNM, ammoNM); }
        public bool TurretEnabled { get; set; }
        public bool AutoFire { get; set; }
        public bool UsingWeaponCoreTracker { get; set; }
        public int ActiveTurrets => Turrets.Count;
        public bool HasTarget => Turrets.Any(t => t.Value.HasTarget);
        public bool AnyTurretsReady => Turrets.Any(t => t.Value.Enabled);
        public bool AllTurretsReady => Turrets.All(t => t.Value.Enabled);
        public void CycleWeapons()
        {
        }
        #region 炮塔控制器
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            Controllers.RemoveWhere(Utils.Common.NullEntity);
            if (updatecounts % 13 == 0)
            {
                UpdateControllersInfo(CtrlBlock);
                UpdateBindings();
            }
            MyAPIGateway.Parallel.ForEach(Turrets, Turret =>
            {
                Turret.Value.Config = Configs;
                if (RadarTargets == null || !TurretEnabled) { Turret.Value.TargetPredict.TargetLocked = null; Turret.Value.RunningDefault(); return; }
                var range = Configs?.range ?? 3000;
                var target = UsingWeaponCoreTracker ? new MyTargetDetected(BasicInfoService.WcApi.GetAiFocus(CtrlBlock.CubeGrid), Turret.Key, true) : RadarTargets.得的最近向我靠近最快的目标(Turret.Key, range);
                try
                {
                    var position = target?.GetEntityPosition(CtrlBlock);
                    if (position != null && target.Entity?.EntityId != Turret.Value.TargetPredict.TargetLocked?.Entity?.EntityId
                    && Vector3D.Distance(position.Value, Turret.Key.GetPosition()) < range)
                        Turret.Value.TargetPredict.TargetLocked = target;
                }
                catch (Exception) { }
                if (Turret.Value.UnderControl)
                    Turret.Value.RunningManual(Controller?.RotationIndicator);
                else if (Turret.Value.ManuelOnly)
                    Turret.Value.RunningDefault();
                else
                {
                    Turret.Value.SetFire(AutoFire && TurretEnabled);
                    Turret.Value.RunningAutoAimAt(Turret.Key);
                    Turret.Value.RunningAutoFire();
                }
            });
        }
        private ConcurrentDictionary<IMyMotorStator, MyTurretBinding> Turrets { get; } = new ConcurrentDictionary<IMyMotorStator, MyTurretBinding>();
        private HashSet<IMyShipController> Controllers { get; } = new HashSet<IMyShipController>();
        private void UpdateControllersInfo(IMyTerminalBlock CtrlBlock)
        {
            Controllers.Clear();
            var list = Utils.Common.GetTs<IMyShipController>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(CtrlBlock.CubeGrid));
            if (Utils.Common.IsNullCollection(list)) return;
            Controllers.UnionWith(list);
        }
        private void UpdateBindings() { var removeable = Turrets.Keys?.ToList()?.Where(Utils.Common.NullEntity); if (Utils.Common.IsNullCollection(removeable)) return; MyAPIGateway.Parallel.ForEach(removeable, item => Turrets.Remove(item)); }
        private IMyShipController Controller { get { if (Utils.Common.IsNullCollection(Controllers) || !Controllers.Any(b => b.IsUnderControl)) return null; return Controllers.First(b => b.IsUnderControl); } }
        private MyRadarTargets RadarTargets;
        private MyTurretConfig Configs;
        #endregion
    }
    public class MyRotorTurretCtrl : UpdateableClass
    {
    }
}