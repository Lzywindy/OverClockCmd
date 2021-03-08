using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using SIngame = Sandbox.ModAPI.Ingame;
using System.Collections.Concurrent;
using VRageMath;
namespace SuperBlocks.Controller
{
    public class MyTurretCtrl : UpdateableClass
    {
        public MyTurretCtrl() : base() { }
        public void Init(IMyTerminalBlock Me, Dictionary<string, Dictionary<string, string>> Config, string TurretID)
        {
            Configs.ResetValues();
            Configs.GetDataFromConfig(Config, TurretID);
            var AzName = $"{Configs.turretNM}{MyTurretConfig.azNM}";
            var azs = Utils.GetTs<IMyMotorStator>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid), b => b.CubeGrid == Me.CubeGrid && b.CustomName.Contains(AzName) && b.TopGrid != null);
            if (Utils.IsNullCollection(azs)) return;
            Turrets.Clear();
            foreach (var az_motor in azs)
            {
                Turrets.TryAdd(az_motor, new MyTurretBinding());
                Turrets[az_motor].Init(az_motor, ref this.Configs);
                if (Turrets[az_motor].ManuelOnly) continue;
                TargetPs.TryAdd(az_motor, new MyTargetPredictEnt());
            }
        }
        public void SetTargetLocker(MyRadarTargets RadarTargets)
        {
            this.RadarTargets = RadarTargets;
            if (RadarTargets == null) MyAPIGateway.Parallel.ForEach(TargetPs, TargetP => { TargetP.Value.TargetLocked = null; });
            //string enabled = (RadarTargets == null) ? "Enabled" : "Disabled";
            //MyAPIGateway.Utilities.ShowNotification($"Turret:{enabled}");
        }
        public void SetWeaponAmmoConfigInfo(Dictionary<string, Dictionary<string, string>> Config, string weaponNM, string ammoNM)
        {
            if (Utils.IsNullCollection(TargetPs)) return;
            foreach (var TargetP in TargetPs)
                TargetP.Value.SetWeaponAmmoConfigInfo(Config, weaponNM, ammoNM);
        }
        public void SetTimeFixed(float TimeFixed)
        {
            if (Utils.IsNullCollection(TargetPs)) return;
            foreach (var TargetP in TargetPs)
                TargetP.Value.时间补偿 = TimeFixed;
        }
        public Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>> GetTurretWeapons()
        {
            Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>> Weapons = new Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>>();
            foreach (var item in Turrets)
            {
                if (Utils.IsNullCollection(item.Value.Weapons)) continue;
                Weapons.Add(item.Key, item.Value.Weapons.ToList()?.ConvertAll(b => b as SIngame.IMyTerminalBlock));
            }
            return Weapons;
        }
        public Dictionary<SIngame.IMyTerminalBlock, bool> GetTurretWeaponsEnabled()
        {
            Dictionary<SIngame.IMyTerminalBlock, bool> Weapons = new Dictionary<SIngame.IMyTerminalBlock, bool>();
            foreach (var item in TargetPs)
            {
                if (!Turrets.ContainsKey(item.Key)) continue;
                Weapons.Add(item.Key, item.Value.CanFireWeapon(Turrets[item.Key].Weapons));
            }
            return Weapons;
        }
        #region 炮塔控制器
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            Controllers.RemoveWhere(Utils.NullEntity);
            if (updatecounts % 13 == 0)
            {
                UpdateControllersInfo(CtrlBlock);
                UpdateBindings();
                UpdateTargets();
            }
            var CurrentCtrl = Controller;
            MyAPIGateway.Parallel.ForEach(Turrets, Turret =>
            {
                Turret.Value.Config = Configs;
                if (Turret.Value.UnderControl)
                {
                    Turret.Value.RunningManual(CurrentCtrl?.RotationIndicator);
                }
                else if (Turret.Value.ManuelOnly)
                {
                    Turret.Value.RunningDefault();
                }
                else if (TargetPs.ContainsKey(Turret.Key))
                {
                    TargetPs[Turret.Key].CalculateDirection(Turret.Key, Turret.Value.Weapons, Configs.range);
                    Turret.Value.RunningDirection(TargetPs[Turret.Key].Direction);
                }
            });
        }
        private ConcurrentDictionary<IMyMotorStator, MyTurretBinding> Turrets { get; } = new ConcurrentDictionary<IMyMotorStator, MyTurretBinding>();
        private ConcurrentDictionary<IMyMotorStator, MyTargetPredictEnt> TargetPs { get; } = new ConcurrentDictionary<IMyMotorStator, MyTargetPredictEnt>();
        private HashSet<IMyShipController> Controllers { get; } = new HashSet<IMyShipController>();
        private void UpdateControllersInfo(IMyTerminalBlock CtrlBlock)
        {
            Controllers.Clear();
            var list = Utils.GetTs<IMyShipController>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(CtrlBlock.CubeGrid));
            if (Utils.IsNullCollection(list)) return;
            Controllers.UnionWith(list);
        }
        private void UpdateBindings()
        {
            var removeable = Turrets.Keys?.ToList()?.Where(Utils.NullEntity);
            if (Utils.IsNullCollection(removeable)) return;
            MyAPIGateway.Parallel.ForEach(removeable, item => { Turrets.Remove(item); TargetPs.Remove(item); });
        }
        private void UpdateTargets()
        {
            if (Utils.IsNullCollection(TargetPs)) return;
            MyAPIGateway.Parallel.ForEach(TargetPs, item =>
            {
                if (RadarTargets == null) { item.Value.TargetLocked = null; return; }
                lock (RadarTargets)
                {
                    var target = RadarTargets.得的最近向我靠近最快的目标(item.Key);
                    if (item.Value.TargetLocked == null || item.Value.TargetLocked.InvalidTarget || item.Value.TargetLocked.TargetSafety() || target == null || target?.Position == null || Vector3D.Distance(target.Position.Value, item.Key.GetPosition()) > Configs.range)
                    {
                        item.Value.TargetLocked = target;
                        return;
                    }
                }
                /*if (item.Value.TargetLocked == null || item.Value.TargetLocked.InvalidTarget || item.Value.TargetLocked.TargetSafety())*/
            });
        }
        private IMyShipController Controller { get { if (Utils.IsNullCollection(Controllers) || !Controllers.Any(b => b.IsUnderControl)) return null; return Controllers.First(b => b.IsUnderControl); } }
        private MyRadarTargets RadarTargets;
        private MyTurretConfig Configs;
        #endregion
    }
    public class MyRotorTurretCtrl : UpdateableClass
    {
    }
}