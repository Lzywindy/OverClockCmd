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
        public MyWeaponSystemBinding(IMyTerminalBlock Me) : base()
        {
            MyConfigs.CustomDataConfigRead_INI(Me, Configs);
        }
        public void SetWeaponSysEnabled(bool Enabled) => EnabledFunction[0] = Enabled;
        public void InitRadar(IMyTerminalBlock Me, float Range)
        {
            EnabledFunction[1] = Range > 10;
            RadarTargets.ResetParameters(Me, Range);
        }
        public void InitFixedWeapons(IMyTerminalBlock Me, bool Enabled)
        {
            EnabledFunction[3] = Enabled;
            TargetPredictEnt.Init();
        }
        public void InitTurret(IMyTerminalBlock Me, string TurretID)
        {
            EnabledFunction[2] = (TurretID != "");
            //MyAPIGateway.Utilities.ShowNotification($"Turret:{ EnabledFunction[2]}");
            TurretCtrl.Init(Me, Configs, TurretID);
            if (EnabledFunction[2]) TurretCtrl.SetTargetLocker(RadarTargets);
        }
        public bool TurretEnabled { get { return EnabledFunction[2]; } set { EnabledFunction[2] = value; if (value) TurretCtrl.SetTargetLocker(RadarTargets); else TurretCtrl.SetTargetLocker(null); } }
        public void SetTimeOffset(float TimeOffset)
        {
            TurretCtrl.SetTimeFixed(TimeOffset);
            TargetPredictEnt.时间补偿 = TimeOffset;
        }
        public void SetWeaponAmmo(MyTuple<string, string> WANM)
        {
            TurretCtrl.SetWeaponAmmoConfigInfo(Configs, WANM.Item1, WANM.Item2);
            TargetPredictEnt.SetWeaponAmmoConfigInfo(Configs, WANM.Item1, WANM.Item2);
        }
        public List<VRage.Game.ModAPI.Ingame.IMyEntity> GetEntities()
        {
            if (!EnabledFunction[1]) return null;
            return RadarTargets.DetectedEntities.ToList()?.ConvertAll(b => b as VRage.Game.ModAPI.Ingame.IMyEntity);
        }
        public MyTuple<Vector3D?, Vector3D?>? GetTargetPV(IMyTerminalBlock CtrlBlock)
        {
            return RadarTargets?.当前目标?.GetTarget_PV(CtrlBlock);
        }
        public List<SIngame.IMyFunctionalBlock> GetFixedWeapons()
        {
            return FixedWeapons.ConvertAll(b => b as SIngame.IMyFunctionalBlock);
        }
        public bool GetFixedWeaponsEnabled()
        {
            return TargetPredictEnt.CanFireWeapon(FixedWeapons);
        }
        public Dictionary<SIngame.IMyTerminalBlock, List<SIngame.IMyTerminalBlock>> GetTurretWeapons()
        {
            return TurretCtrl.GetTurretWeapons();
        }
        public Dictionary<SIngame.IMyTerminalBlock, bool> GetTurretWeaponsEnabled()
        {
            return TurretCtrl.GetTurretWeaponsEnabled();
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (!EnabledFunction[0]) return;
            if (updatecounts % 98 == 0) MyConfigs.CustomDataConfigRead_INI(CtrlBlock, Configs);
            RadarTargets.Update(CtrlBlock);
            TurretCtrl.Update(CtrlBlock);
            TargetPredictEnt.CalculateDirection(CtrlBlock, FixedWeapons);
        }
        private Dictionary<string, Dictionary<string, string>> Configs { get; } = new Dictionary<string, Dictionary<string, string>>();
        private MyRadarTargets RadarTargets { get; } = new MyRadarTargets();
        private MyTargetPredictEnt TargetPredictEnt { get; } = new MyTargetPredictEnt();
        private MyTurretCtrl TurretCtrl { get; } = new MyTurretCtrl();
        private List<IMyTerminalBlock> FixedWeapons { get; } = new List<IMyTerminalBlock>();
        public bool[] EnabledFunction { get; } = new bool[4];
    }
}