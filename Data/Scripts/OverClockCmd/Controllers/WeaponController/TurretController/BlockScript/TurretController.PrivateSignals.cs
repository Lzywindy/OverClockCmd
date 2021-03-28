using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
using static SuperBlocks.Utils;

namespace SuperBlocks.Controller
{
    public partial class TurretController
    {
        private volatile bool BlockEnabled = true;
        private volatile float RangeMultiply = 1;
        private volatile bool TurretEnabled;
        private volatile bool AutoFire;
        private volatile bool UsingWeaponCoreTracker;
        private volatile uint updatecounts = 0;
        private volatile float Range = 3000f;
        private MyRadarTargets RadarTargets;
        private ConcurrentBag<MyTurretBinding> Turrets;
        private ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs = new ConcurrentDictionary<string, ConcurrentDictionary<string, string>>();
        private void UpdateBindings()
        {
            if (Common.NullEntity(Me)) return;
            if (Common.IsNullCollection(Configs))
                TriggerReadConfigs(Me);
            if (Me.CustomData.Length < 1)
                TriggerSaveConfigs(Me);
            Range = RadarSubtypeId.DetectedRangeBlock(RadarSubtypeId.GetFarestDetectedBlock(Me.CubeGrid));
            List<MyTurretBinding> list;
            if (Common.IsNullCollection(Turrets))
                list = Common.GetTs<IMyMotorStator>(Me, az => HasEvMotors(az) && InThisEntity(az)).ConvertAll(az => new MyTurretBinding(az));
            else
                list = Turrets.Where(t => t.CanRunning).ToList();
            if (Common.IsNullCollection(list)) { Turrets = null; return; }
            Turrets = new ConcurrentBag<MyTurretBinding>(list);
        }
        private void UpdateTurrets(MyTurretBinding Turret)
        {
            if (!Common.NullEntity(Me))
                Turret.SetConfig(Configs);
            Turret.RotorsEnabled = BlockEnabled;
            Turret.AutoFire = AutoFire;
            Turret.Enabled = TurretEnabled;
            if (RadarTargets == null || !TurretEnabled || !BlockEnabled) { Turret.AimTarget = null; }
            else
            {
                try
                {
                    Turret.AimTarget = UsingWeaponCoreTracker ? new MyTargetDetected(BasicInfoService.WcApi.GetAiFocus(Me.CubeGrid), Me, true) : RadarTargets.GetTheMostThreateningTarget(Turret.MotorAz, Range, Turret.TargetInRange_Angle);// target;
                }
                catch (Exception) { }
            }
            Turret.Running();
        }
        private static bool HasEvMotors(IMyMotorStator MotorAz)
        {
            if (Common.IsNull(MotorAz?.TopGrid)) return false;
            return Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985).Count > 0;
        }
        private void UpdateState()
        {
            Color CurrentColor = BlockEnabled ? (TurretEnabled ? (AutoFire ? (UsingWeaponCoreTracker ? Color.Blue : Color.Yellow) : Color.Cyan) : Color.Green) : Color.Black;
            float e = BlockEnabled ? 4 : 0;
            try { Me.SetEmissiveParts("Emissive", CurrentColor, e); } catch (Exception) { }
        }
        private void Try2AttachTops()
        {
            var rotors = Common.GetTs<IMyMotorStator>(Me, rt => rt.TopGrid == null && InThisEntity(rt));
            foreach (var rotor in rotors)
                rotor.GetActionWithName("Attach").Apply(rotor);
        }
    }
}
