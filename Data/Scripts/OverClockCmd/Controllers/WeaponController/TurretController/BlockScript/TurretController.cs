using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.Components;


namespace SuperBlocks.Controller
{
    using static SuperBlocks.Utils;
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TurretController", "SmallTurretController")]
    public partial class TurretController : MyGridProgram4ISConvert
    {
        protected override void Program()
        {
            OnRestart?.Invoke();
            UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency.Update1 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update10 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update100;
        }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource) { }
        protected override void ClosedBlock()
        {
            TurretRegister.UnRegistControllerBlock(Me);
            Enabled.OnValueChanged -= Enabled_OnValueChanged;
            TurretEnabled.OnValueChanged -= TurretEnabled_OnValueChanged;
            TurretEnabled.OnValueChanged -= UpdateState;
            AutoFire.OnValueChanged -= AutoFire_OnValueChanged;
            AutoFire.OnValueChanged -= UpdateState;
            UsingWeaponCoreTracker.OnValueChanged -= UpdateState;
            Configs.Clear();
            Turrets = null;
        }
        protected override void CustomDataChangedProcess()
        {
            if (Common.NullEntity(Me)) return;
            LoadData(Me);
        }
        protected override void InitBlock()
        {
            OnRestart += () => { 
                TurretRegister.RegistControllerBlock(Me); 
                UpdateState();
                if (Common.NullEntity(Me)) return;
                if (Common.IsNullCollection(Configs))
                    LoadData(Me);
                if (Me.CustomData.Length < 1)
                    SaveData(Me);
                Range = MyRadarSubtypeIdHelper.DetectedRangeBlock(MyRadarSubtypeIdHelper.GetFarestDetectedBlock(Me.CubeGrid));
                BlockGroupService.Init(Me);
                List<MyTurretBinding> list = Common.GetTs<IMyMotorStator>(Me, az => HasEvMotors(az) && InThisEntity(az) && IsTurretBase(az)).ConvertAll(az => new MyTurretBinding(az));
                if (Common.IsNullCollection(list)) { Turrets = null; return; }
                Turrets = new ConcurrentBag<MyTurretBinding>(list);
            };
            OnRunning1 += () =>
            {
                if (!TurretRegister.IsMainController(Me)) return;
                MyAPIGateway.Parallel.ForEach(Turrets, Turret => UpdateTurrets(Turret));
            };
            OnRunning10 += () =>
            {
                if (!TurretRegister.IsMainController(Me)) return;
                updatecounts = (updatecounts + 1) % 10;
                RadarTargets.UpdateScanning(Me?.GetTopMostParent());
                if (updatecounts % 8 == 0) { UpdateBindings(); }
                if (updatecounts % 9 == 0) { foreach (var Turret in Turrets) Turret.ReadConfig_Turret_Rotors(); }
            };
            OnRunning100 += () =>
             {
                 if (TurretRegister.IsMainController(Me))
                     foreach (var Turret in Turrets) Turret.Restart();
                 else
                     Me.CustomData = TurretRegister.GetRegistControllerBlockConfig(Me);
                 Try2AttachTops();
             };
            Enabled.OnValueChanged += Enabled_OnValueChanged;
            TurretEnabled.OnValueChanged += TurretEnabled_OnValueChanged;
            TurretEnabled.OnValueChanged += UpdateState;
            AutoFire.OnValueChanged += AutoFire_OnValueChanged;
            AutoFire.OnValueChanged += UpdateState;
            UsingWeaponCoreTracker.OnValueChanged += UpdateState;
        }
        private void AutoFire_OnValueChanged()
        {
            try
            {
                if (Common.IsNullCollection(Turrets)) return;
                foreach (var Turret in Turrets) { Turret.AutoFire = AutoFire.Value; }
            }
            catch (System.Exception) { }
        }
        private void Enabled_OnValueChanged()
        {
            try
            {
                if (Common.IsNullCollection(Turrets)) return;
                foreach (var Turret in Turrets) { Turret.RotorsEnabled = Enabled.Value; }
            }
            catch (System.Exception) { }
        }
        private void TurretEnabled_OnValueChanged()
        {
            try
            {
                if (Common.IsNullCollection(Turrets)) return;
                foreach (var Turret in Turrets) { Turret.Enabled = TurretEnabled.Value; }
            }
            catch (Exception) { }
        }
    }
}
