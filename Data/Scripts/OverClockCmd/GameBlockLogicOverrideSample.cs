using System;
using VRage.Game.Components;
using Sandbox.Definitions;
using VRage.Game;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage;
using Sandbox.ModAPI;
using System.Collections.Generic;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
/*  
  Welcome to Modding API. This is second of two sample scripts that you can modify for your needs,
  in this case simple script is prepared that will alter behaviour of sensor block
  This type of scripts will be executed automatically  when sensor (or your defined) block is added to world
 */
namespace TestScript
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false)]
    public class OverClock : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            thisBlock = Entity as IMyTerminalBlock;
            ObjectBuilder = objectBuilder;
            overClockConsle_Process = new OverClockConsle_Process();
            base.Init(objectBuilder);
            thisBlock.CustomNameChanged += ThisBlock_CustomNameChanged;
            thisBlock.CustomDataChanged += ThisBlock_CustomDataChanged;
            NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        private void ThisBlock_CustomDataChanged(IMyTerminalBlock obj)
        {
            if (overClockConsle_Process == null) { overClockConsle_Process = new OverClockConsle_Process(); return; }
            if (!overClockConsle_Process.EnabledOC) return;
            overClockConsle_Process.CommendLines(obj.CustomData);
            UpdateDevice();
        }
        private void ThisBlock_CustomNameChanged(IMyTerminalBlock obj)
        {
            if (overClockConsle_Process == null) { overClockConsle_Process = new OverClockConsle_Process(); return; }
            overClockConsle_Process.CheckConsle_Function(obj.CustomName);
            UpdateDevice();
        }
        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (IsLoadingData) return;
            if (overClockConsle_Process == null)
                overClockConsle_Process = new OverClockConsle_Process();
            overClockConsle_Process.CheckConsle_Function(thisBlock.CustomName);
            overClockConsle_Process.CommendLines(thisBlock.CustomData);
            UpdateDevice();
        }
        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
        }
        private bool IsLoadingData = false;
        private IMyTerminalBlock thisBlock;
        private MyObjectBuilder_EntityBase ObjectBuilder;
        private OverClockConsle_Process overClockConsle_Process;
        private void UpdateDevice()
        {
            if (overClockConsle_Process == null) { overClockConsle_Process = new OverClockConsle_Process(); return; }
            if (overClockConsle_Process.EnabledOC)
            {
                if (thisBlock.CubeGrid == null) return;
                List<IMyReactor> Reactors = new List<IMyReactor>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(thisBlock.CubeGrid).GetBlocksOfType(Reactors);
                foreach (var Reactor in Reactors)
                    Reactor.PowerOutputMultiplier = overClockConsle_Process.Reactor_Multiply;
                List<IMyThrust> Thrusts = new List<IMyThrust>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(thisBlock.CubeGrid).GetBlocksOfType(Thrusts);
                foreach (var Thrust in Thrusts)
                {
                    if (Thrust.BlockDefinition.SubtypeId.Contains("HoverEngine")) continue;
                    if (Thrust.BlockDefinition.SubtypeId.Contains("Hover")) continue;
                    if (Thrust.BlockDefinition.SubtypeId.Contains("Hover Engine")) continue;
                    Thrust.PowerConsumptionMultiplier = overClockConsle_Process.Thrust_Multiply;
                    Thrust.ThrustMultiplier = overClockConsle_Process.Thrust_Multiply;
                }
                List<IMyGyro> Gyros = new List<IMyGyro>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(thisBlock.CubeGrid).GetBlocksOfType(Gyros);
                foreach (var Gyro in Gyros)
                {
                    Gyro.PowerConsumptionMultiplier = overClockConsle_Process.Gyro_Multiply;
                    Gyro.GyroStrengthMultiplier = overClockConsle_Process.Gyro_Multiply;
                }
                List<IMyGasGenerator> gasGenerators = new List<IMyGasGenerator>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(thisBlock.CubeGrid).GetBlocksOfType(gasGenerators);
                foreach (var gasGenerator in gasGenerators)
                {
                    gasGenerator.PowerConsumptionMultiplier = overClockConsle_Process.Production_Multipy;
                    gasGenerator.ProductionCapacityMultiplier = overClockConsle_Process.Production_Multipy;
                }
            }
        }
    }
    public class OverClockConsle_Process
    {
        private static string[] line_Split = new string[] { "\n", "\r", ";" };
        private static string[] cmd_Split = new string[] { " ", "\t", "-", "=" };
        public float Reactor_Multiply { get; private set; } = 1.0f;
        public float Thrust_Multiply { get; private set; } = 1.0f;
        public float Gyro_Multiply { get; private set; } = 1.0f;
        public float Production_Multipy { get; private set; } = 1.0f;
        public bool EnabledOC { get; private set; } = false;
        public void CommendLines(string cmd)
        {
            string[] commendlines = cmd.Split(line_Split, StringSplitOptions.RemoveEmptyEntries);
            if (commendlines == null || commendlines.Length == 0) return;
            foreach (var commendline in commendlines)
            {
                var parameters = commendline.Split(cmd_Split, StringSplitOptions.RemoveEmptyEntries);
                if (parameters == null || parameters.Length == 0) return;
                CommendLine_Reactor(parameters);
                CommendLine_Thrust(parameters);
                CommendLine_Gyro(parameters);
                CommendLine_Production(parameters);
            }
        }
        private void CommendLine_Reactor(string[] parameters)
        {
            if (parameters[0] != "RE") return;
            float _Multiply = 0f;
            if (parameters.Length == 2)
                float.TryParse(parameters[1], out _Multiply);
            Reactor_Multiply = Math.Max(1f, _Multiply);
        }
        private void CommendLine_Thrust(string[] parameters)
        {
            if (parameters[0] != "TR") return;
            float _Multiply = 0f;
            if (parameters.Length == 2)
                float.TryParse(parameters[1], out _Multiply);
            Thrust_Multiply = Math.Max(1f, _Multiply);
        }
        private void CommendLine_Gyro(string[] parameters)
        {
            if (parameters[0] != "GY") return;
            float _Multiply = 0f;
            if (parameters.Length == 2)
                float.TryParse(parameters[1], out _Multiply);
            Gyro_Multiply = Math.Max(1f, _Multiply);
        }
        private void CommendLine_Production(string[] parameters)
        {
            if (parameters[0] != "PD") return;
            float _Multiply = 0f;
            if (parameters.Length == 2)
                float.TryParse(parameters[1], out _Multiply);
            Production_Multipy = Math.Max(1f, _Multiply);
        }
        public void CheckConsle_Function(string CustomName)
        {
            EnabledOC = CustomName.Contains("-OC");
        }
    }
}
