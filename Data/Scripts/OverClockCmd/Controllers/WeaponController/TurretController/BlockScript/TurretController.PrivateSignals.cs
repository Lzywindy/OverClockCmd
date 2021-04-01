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
        private volatile float RangeMultiply = 1;
        private MyEventParameter_Bool TurretEnabled { get; } = new MyEventParameter_Bool(true);
        private MyEventParameter_Bool AutoFire { get; } = new MyEventParameter_Bool(false);
        private MyEventParameter_Bool UsingWeaponCoreTracker { get; } = new MyEventParameter_Bool(false);
        private volatile uint updatecounts = 0;
        private volatile float Range = 3000f;
        private MyRadarTargets RadarTargets { get; } = new MyRadarTargets();
        private ConcurrentBag<MyTurretBinding> Turrets;
        private MyBlockGroupService BlockGroupService { get; } = new MyBlockGroupService();

        private bool IsTurretBase(IMyMotorStator Motor)
        {
            var str = Motor.BlockDefinition.SubtypeId.ToLower();
            if (str.Contains("turret")) return true;
            str = Motor.CustomName.ToLower();
            if (str.Contains("turret")) return true;
            return BlockGroupService.TestBlockInGroups(Motor);
        }
        private void UpdateBindings()
        {
            if (Common.NullEntity(Me)) return;
            if (Common.IsNullCollection(Configs))
                LoadData(Me);
            if (Me.CustomData.Length < 1)
                SaveData(Me);
            Range = MyRadarSubtypeIdHelper.DetectedRangeBlock(MyRadarSubtypeIdHelper.GetFarestDetectedBlock(Me.CubeGrid));
            BlockGroupService.Init(Me);
            if (!Common.IsNullCollection(Turrets)) return;
            List<MyTurretBinding> list = Common.GetTs<IMyMotorStator>(Me, az => HasEvMotors(az) && InThisEntity(az) && IsTurretBase(az)).ConvertAll(az => new MyTurretBinding(az));
            if (Common.IsNullCollection(list)) { Turrets = null; return; }
            Turrets = new ConcurrentBag<MyTurretBinding>(list);
        }
        private void UpdateTurrets(MyTurretBinding Turret)
        {
            try
            {
                if (Common.NullEntity(Me)) return;
                Turret.SetConfig(Configs);
                Turret.AutoFire = AutoFire.Value;
                Turret.RotorsEnabled = Enabled.Value;
                Turret.Enabled = TurretEnabled.Value;
                if (!TurretEnabled.Value || !Enabled.Value) { Turret.AimTarget = null; }
                else
                {
                    Turret.AimTarget = UsingWeaponCoreTracker.Value ? new MyTargetDetected(BasicInfoService.WcApi.GetAiFocus(Me.CubeGrid), Me, true) : RadarTargets.GetTheMostThreateningTarget(Turret.MotorAz, Turret.TargetInRange_Angle);
                }

            }
            catch (Exception) { }
            Turret.Running();

        }
        private static bool HasEvMotors(IMyMotorStator MotorAz)
        {
            if (Common.IsNull(MotorAz?.TopGrid)) return false;
            return Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985).Count > 0;
        }
        protected override void UpdateState()
        {
            Color CurrentColor = Enabled.Value ? (TurretEnabled.Value ? (AutoFire.Value ? (UsingWeaponCoreTracker.Value ? Color.Blue : Color.Yellow) : Color.Cyan) : Color.Green) : Color.Black;
            float e = Enabled.Value ? 4 : 0;
            try { Me.SetEmissiveParts("Emissive", CurrentColor, e); } catch (Exception) { }
        }
        private void Try2AttachTops()
        {
            var rotors = Common.GetTs<IMyMotorStator>(Me, rt => rt.TopGrid == null && InThisEntity(rt));
            foreach (var rotor in rotors) rotor.GetActionWithName("Attach").Apply(rotor);
        }
    }
}
