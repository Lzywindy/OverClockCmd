using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage;
using Sandbox.ModAPI;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using System.Collections.Generic;
using System;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using VRage.GameServices;
using System.Text;
using VRageMath;

namespace SuperBlocks.Controller
{
    //[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TankTrackControl", "SmallTankTrackControl")]
    //public class TankTrackControl : ControlBase
    //{
    //    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    //    {
    //        base.Init(objectBuilder);
    //        Control = new TankController(Entity as IMyTerminalBlock);
    //        NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
    //    }
    //}
    //[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "HeilcopterControl", "SmallHeilcopterControl")]
    //public class HeilcopterControl : ControlBase
    //{
    //    public override void Init(MyObjectBuilder_EntityBase objectBuilder)
    //    {
    //        base.Init(objectBuilder);
    //        Control = new HelicopterController(Entity as IMyTerminalBlock);
    //        NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
    //    }
    //}
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]

    public class ControllerService : MySessionComponentBase
    {
        private CreateTerminalButton 重启程序 { get; } = new CreateTerminalButton(@"ControlResetID", @"Restart Program", (ControllerManageBase logic) => logic is ControllerManageBase);
        private CreateTerminalSwitch 开关引擎 { get; } = new CreateTerminalSwitch(@"ControlTriggerThrustID", @"Thrust", (ControllerManageBase logic) => logic is ICtrlDevCtrl);
        private CreateTerminalSwitch 巡航开启 { get; } = new CreateTerminalSwitch(@"ControlTriggerCruiserID", @"Cruise Mode", (ControllerManageBase logic) => logic is IPlaneController);
        private CreateTerminalSwitch 垂直起降 { get; } = new CreateTerminalSwitch(@"ControlTriggerVTOLID", @"Hover Mode", (ControllerManageBase logic) => logic is IHeilController);
        private CreateTerminalSwitch 是否有飞翼 { get; } = new CreateTerminalSwitch(@"ControlTriggerWingsID", @"Wings Mode", (ControllerManageBase logic) => logic is IWingModeController);
        private CreateTerminalSliderBar 安全偏转角度 { get; } = new CreateTerminalSliderBar(@"ControlAnglePercentageID", @"Angle Percentage", (ControllerManageBase logic) => logic is IPoseParamAdjust, 0, 1);

        public Guid guid { get; } = new Guid("5F1A43D3-02D3-C959-2413-5922F4EEB917");
        public bool SetupComplete = false;
        public List<IMyTerminalBlock> GameLogicsBlocks { get; } = new List<IMyTerminalBlock>();
        public bool ControlsLoaded;
        public override void UpdateBeforeSimulation()
        {
            if (!SetupComplete)
            {
                FunctionSetup();
                Init();
                SetupComplete = true;
                return;
            }
            //if (MyAPIGateway.Multiplayer.IsServer && MyAPIGateway.Utilities.IsDedicated) Init();
            //else if (MyAPIGateway.Session.Player != null) Init();
        }
        public void FunctionSetup()
        {
            {
                重启程序.TriggerFunc = (IMyTerminalBlock block) =>
                {
                    if (!重启程序.IsinEnabledList(block)) return;
                    var logic = block.GameLogic.GetAs<ControlBase>();
                    if (logic == null) return;
                    logic.RunningRestart();
                };
            }
            巡航开启.GetterFunc = (IMyTerminalBlock block) =>
            {
                if (!巡航开启.IsinEnabledList(block)) return false;
                return block.GameLogic.GetAs<FlyMachineCtrl>().EnabledCrusier_Getter(block);
            };
            巡航开启.SetterFunc = (IMyTerminalBlock block, bool Value) =>
            {
                if (!巡航开启.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledCrusier_Setter(block, Value);
            };
            巡航开启.TriggerFunc = (IMyTerminalBlock block) =>
            {
                if (!巡航开启.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledCrusier_Setter(block, !block.GameLogic.GetAs<FlyMachineCtrl>().EnabledCrusier_Getter(block));
            };
            垂直起降.GetterFunc = (IMyTerminalBlock block) =>
            {
                if (!垂直起降.IsinEnabledList(block)) return false;
                return block.GameLogic.GetAs<FlyMachineCtrl>().EnabledHover_Getter(block);
            };
            垂直起降.SetterFunc = (IMyTerminalBlock block, bool Value) =>
            {
                if (!垂直起降.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledHover_Setter(block, Value);
            };
            垂直起降.TriggerFunc = (IMyTerminalBlock block) =>
            {
                if (!垂直起降.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledHover_Setter(block, !block.GameLogic.GetAs<FlyMachineCtrl>().EnabledHover_Getter(block));
            };
            是否有飞翼.GetterFunc = (IMyTerminalBlock block) =>
            {
                if (!是否有飞翼.IsinEnabledList(block)) return false;
                return block.GameLogic.GetAs<FlyMachineCtrl>().EnabledWings_Getter(block);
            };
            是否有飞翼.SetterFunc = (IMyTerminalBlock block, bool Value) =>
            {
                if (!是否有飞翼.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledWings_Setter(block, Value);
            };
            是否有飞翼.TriggerFunc = (IMyTerminalBlock block) =>
            {
                if (!是否有飞翼.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().EnabledWings_Setter(block, !block.GameLogic.GetAs<FlyMachineCtrl>().EnabledWings_Getter(block));
            };
            安全偏转角度.GetterFunc = (IMyTerminalBlock block) =>
            {
                if (!安全偏转角度.IsinEnabledList(block)) return 0.5f;
                return block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Getter(block);
            };
            安全偏转角度.SetterFunc = (IMyTerminalBlock block, float Value) =>
            {
                if (!安全偏转角度.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Setter(block, Value);
            };
            安全偏转角度.IncreaseFunc = (IMyTerminalBlock block) =>
            {
                if (!安全偏转角度.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Setter(block, block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Getter(block) + 0.1f);
            };
            安全偏转角度.DecreaseFunc = (IMyTerminalBlock block) =>
            {
                if (!安全偏转角度.IsinEnabledList(block)) return;
                block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Setter(block, block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Getter(block) - 0.1f);
            };
            安全偏转角度.WriterFunc = (IMyTerminalBlock block, StringBuilder value) =>
            {
                if (!安全偏转角度.IsinEnabledList(block)) return;
                value.Clear();
                value.Append($"{MathHelper.RoundOn2(block.GameLogic.GetAs<FlyMachineCtrl>().安全偏转角度_Getter(block) * 100f)}%");
            };
            {
                开关引擎.GetterFunc = (IMyTerminalBlock block) =>
                {
                    if (!开关引擎.IsinEnabledList(block)) return false;
                    return block.GameLogic.GetAs<VehcCtrl>().EnabledThrusters_Getter(block);
                };
                开关引擎.SetterFunc = (IMyTerminalBlock block, bool Value) =>
                {
                    if (!开关引擎.IsinEnabledList(block)) return;
                    block.GameLogic.GetAs<VehcCtrl>().EnabledThrusters_Setter(block, Value);
                };
                开关引擎.TriggerFunc = (IMyTerminalBlock block) =>
                {
                    if (!开关引擎.IsinEnabledList(block)) return;
                    block.GameLogic.GetAs<VehcCtrl>().EnabledThrusters_Setter(block, !block.GameLogic.GetAs<VehcCtrl>().EnabledThrusters_Getter(block));
                };
            }
        }
        public void Init()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += 重启程序.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 重启程序.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 巡航开启.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 巡航开启.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 垂直起降.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 垂直起降.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 是否有飞翼.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 是否有飞翼.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 安全偏转角度.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 安全偏转角度.CreateAction;
        }
        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= 重启程序.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 重启程序.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 巡航开启.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 巡航开启.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 垂直起降.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 垂直起降.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 是否有飞翼.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 是否有飞翼.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 安全偏转角度.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 安全偏转角度.CreateAction;
        }
    }
}
