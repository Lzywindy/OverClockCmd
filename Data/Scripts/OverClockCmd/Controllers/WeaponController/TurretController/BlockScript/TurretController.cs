﻿using Sandbox.ModAPI;
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
            UpdateBindings();
            TurretRegister.RegistControllerBlock(Me);
            UpdateFrequency = Sandbox.ModAPI.Ingame.UpdateFrequency.Update1 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update10 | Sandbox.ModAPI.Ingame.UpdateFrequency.Update100;
        }
        protected override void Main(string argument, Sandbox.ModAPI.Ingame.UpdateType updateSource)
        {
            if (TurretRegister.IsMainController(Me))
            {
                switch (updateSource)
                {

                    case Sandbox.ModAPI.Ingame.UpdateType.Update1:
                        MyAPIGateway.Parallel.ForEach(Turrets, Turret => UpdateTurrets(Turret));
                        UpdateState();
                        break;
                    case Sandbox.ModAPI.Ingame.UpdateType.Update10:
                        updatecounts = (updatecounts + 1) % 10;
                        RadarTargets.UpdateScanning(Me?.GetTopMostParent());
                        if (updatecounts % 8 == 0) { UpdateBindings(); }
                        if (updatecounts % 9 == 0) { MyAPIGateway.Parallel.ForEach(Turrets, Turret => Turret.ReadConfig_Turret_Rotors()); }
                        break;
                    case Sandbox.ModAPI.Ingame.UpdateType.Update100:
                        Try2AttachTops();
                        break;
                    default:
                        break;
                }
            }
        }
        protected override void ClosedBlock()
        {
            TurretRegister.UnRegistControllerBlock(Me);
            Configs.Clear();
            Turrets = null;
        }
        protected override void CustomDataChangedProcess()
        {
            if (Common.NullEntity(Me)) return;
            TriggerReadConfigs(Me);
        }
    }
}
