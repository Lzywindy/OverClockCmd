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
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class ControllerService : MySessionComponentBase
    {
        public static Guid guid { get; } = new Guid("5F1A43D3-02D3-C959-2413-5922F4EEB917");
        private bool SetupComplete { get; set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!SetupComplete)
            {
                FunctionSetup();
                Init();
                SetupComplete = true;
                return;
            }
        }
        public void FunctionSetup()
        {
            重启程序.TriggerFunc = ControlBase.RunningRestart;
            开关引擎.TriggerFunc = VehcCtrl.EnabledThrusters_Triggler;
            开关引擎.GetterFunc = VehcCtrl.EnabledThrusters_Getter;
            开关引擎.SetterFunc = VehcCtrl.EnabledThrusters_Setter;
            开关动量轮.TriggerFunc = VehcCtrl.EnabledGyros_Triggler;
            开关动量轮.GetterFunc = VehcCtrl.EnabledGyros_Getter;
            开关动量轮.SetterFunc = VehcCtrl.EnabledGyros_Setter;
            安全偏转角度.GetterFunc = FlyMachineCtrl.SafetyStage_Getter;
            安全偏转角度.SetterFunc = FlyMachineCtrl.SafetyStage_Setter;
            安全偏转角度.IncreaseFunc = FlyMachineCtrl.SafetyStage_Inc;
            安全偏转角度.DecreaseFunc = FlyMachineCtrl.SafetyStage_Dec;
            安全偏转角度.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!安全偏转角度.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.SafetyStage_Getter(block)) }"); };
            偏转位置灵敏度.GetterFunc = FlyMachineCtrl.LocationSensetive_Getter;
            偏转位置灵敏度.SetterFunc = FlyMachineCtrl.LocationSensetive_Setter;
            偏转位置灵敏度.IncreaseFunc = FlyMachineCtrl.LocationSensetive_Inc;
            偏转位置灵敏度.DecreaseFunc = FlyMachineCtrl.LocationSensetive_Dec;
            偏转位置灵敏度.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!偏转位置灵敏度.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.LocationSensetive_Getter(block)) }"); };
            最大角速度设置.GetterFunc = FlyMachineCtrl.MaxReactions_AngleV_Getter;
            最大角速度设置.SetterFunc = FlyMachineCtrl.MaxReactions_AngleV_Setter;
            最大角速度设置.IncreaseFunc = FlyMachineCtrl.MaxReactions_Inc;
            最大角速度设置.DecreaseFunc = FlyMachineCtrl.MaxReactions_Dec;
            最大角速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大角速度设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxReactions_AngleV_Getter(block)) }"); };
            巡航开启.GetterFunc = FlyMachineCtrl.EnabledCuriser_Getter;
            巡航开启.SetterFunc = FlyMachineCtrl.EnabledCuriser_Setter;
            巡航开启.TriggerFunc = FlyMachineCtrl.EnabledCuriser_Trigger;
            垂直起降.GetterFunc = FlyMachineCtrl.HoverMode_Getter;
            垂直起降.SetterFunc = FlyMachineCtrl.HoverMode_Setter;
            垂直起降.TriggerFunc = FlyMachineCtrl.HoverMode_Trigger;
            最大悬浮速度设置.GetterFunc = FlyMachineCtrl.MaxiumHoverSpeed_Getter;
            最大悬浮速度设置.SetterFunc = FlyMachineCtrl.MaxiumHoverSpeed_Setter;
            最大悬浮速度设置.IncreaseFunc = FlyMachineCtrl.MaxiumHoverSpeed_Inc;
            最大悬浮速度设置.DecreaseFunc = FlyMachineCtrl.MaxiumHoverSpeed_Dec;
            最大悬浮速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大悬浮速度设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxiumHoverSpeed_Getter(block)) }"); };
            最大巡航速度设置.GetterFunc = FlyMachineCtrl.MaxiumFlightSpeed_Getter;
            最大巡航速度设置.SetterFunc = FlyMachineCtrl.MaxiumFlightSpeed_Setter;
            最大巡航速度设置.IncreaseFunc = FlyMachineCtrl.MaxiumFlightSpeed_Inc;
            最大巡航速度设置.DecreaseFunc = FlyMachineCtrl.MaxiumFlightSpeed_Dec;
            最大巡航速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大巡航速度设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxiumFlightSpeed_Getter(block)) }"); };
            是否有飞翼.GetterFunc = FlyMachineCtrl.HasWings_Getter;
            是否有飞翼.SetterFunc = FlyMachineCtrl.HasWings_Setter;
            是否有飞翼.TriggerFunc = FlyMachineCtrl.HasWings_Trigger;
            坦克驱动模式.GetterFunc = TankCtrl.IsTank_Getter;
            坦克驱动模式.SetterFunc = TankCtrl.IsTank_Setter;
            坦克驱动模式.TriggerFunc = TankCtrl.IsTank_Trigger;
            潜艇驱动模式.GetterFunc = ShipCtrl.IsSubmarine_Getter;
            潜艇驱动模式.SetterFunc = ShipCtrl.IsSubmarine_Setter;
            潜艇驱动模式.TriggerFunc = ShipCtrl.IsSubmarine_Trigger;
            最大辅助速度设置.GetterFunc = PlanetVech.MaximumCruiseSpeed_Getter;
            最大辅助速度设置.SetterFunc = PlanetVech.MaximumCruiseSpeed_Setter;
            最大辅助速度设置.IncreaseFunc = PlanetVech.MaximumCruiseSpeed_Inc;
            最大辅助速度设置.DecreaseFunc = PlanetVech.MaximumCruiseSpeed_Dec;
            最大辅助速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大辅助速度设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(PlanetVech.MaximumCruiseSpeed_Getter(block)) }"); };
            滚转角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_R_Getter;
            滚转角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_R_Setter;
            滚转角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_R_Inc;
            滚转角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_R_Dec;
            滚转角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!滚转角速度阻尼设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_R_Getter(block)) }x"); };
            俯仰角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_P_Getter;
            俯仰角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_P_Setter;
            俯仰角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_P_Inc;
            俯仰角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_P_Dec;
            俯仰角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!俯仰角速度阻尼设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_P_Getter(block)) }x"); };
            摇摆角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_Y_Getter;
            摇摆角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_Y_Setter;
            摇摆角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_Y_Inc;
            摇摆角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_Y_Dec;
            摇摆角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!摇摆角速度阻尼设置.IsinEnabledList(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_Y_Getter(block)) }x"); };
        }
        public void Init()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 重启程序.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 开关引擎.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 开关动量轮.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 安全偏转角度.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 偏转位置灵敏度.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 最大角速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 巡航开启.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 垂直起降.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 最大悬浮速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 最大巡航速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 是否有飞翼.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 坦克驱动模式.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 潜艇驱动模式.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 最大辅助速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 滚转角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 俯仰角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 摇摆角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 重启程序.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 开关引擎.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 开关动量轮.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 安全偏转角度.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 偏转位置灵敏度.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 最大角速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 巡航开启.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 垂直起降.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 最大悬浮速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 最大巡航速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 是否有飞翼.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 坦克驱动模式.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 潜艇驱动模式.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 最大辅助速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 滚转角速度阻尼设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 俯仰角速度阻尼设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 摇摆角速度阻尼设置.CreateAction;
        }
        protected override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 重启程序.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 开关引擎.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 开关动量轮.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 安全偏转角度.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 偏转位置灵敏度.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 最大角速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 巡航开启.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 垂直起降.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 最大悬浮速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 最大巡航速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 是否有飞翼.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 坦克驱动模式.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 潜艇驱动模式.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 最大辅助速度设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 滚转角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 俯仰角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 摇摆角速度阻尼设置.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 重启程序.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 开关引擎.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 开关动量轮.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 安全偏转角度.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 偏转位置灵敏度.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 最大角速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 巡航开启.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 垂直起降.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 最大悬浮速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 最大巡航速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 是否有飞翼.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 坦克驱动模式.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 潜艇驱动模式.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 最大辅助速度设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 滚转角速度阻尼设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 俯仰角速度阻尼设置.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 摇摆角速度阻尼设置.CreateAction;
        }

        private CreateTerminalFence Fence_0 { get; } = new CreateTerminalFence();
        private CreateTerminalButton 重启程序 { get; } = new CreateTerminalButton(@"ControlResetID", @"Restart Program", (ControllerManageBase logic) => logic is ControllerManageBase);
        private CreateTerminalSwitch 开关引擎 { get; } = new CreateTerminalSwitch(@"ControlTriggerThrustID", @"Thrust", (ControllerManageBase logic) => logic is ICtrlDevCtrl);
        private CreateTerminalSwitch 开关动量轮 { get; } = new CreateTerminalSwitch(@"ControlTriggerGyroID", @"Gyro", (ControllerManageBase logic) => logic is ICtrlDevCtrl);
        private CreateTerminalSliderBar 安全偏转角度 { get; } = new CreateTerminalSliderBar(@"ControlAngleStageID", @"Angle Stage", (ControllerManageBase logic) => (logic is IPoseParamAdjust) && !(logic is ILandVehicle) && !(logic is ISeaVehicle), VehicleControllerBase.SafetyStageMin, VehicleControllerBase.SafetyStageMax);
        private CreateTerminalSliderBar 偏转位置灵敏度 { get; } = new CreateTerminalSliderBar(@"ControlLocalSensitiveID", @"Local Sensitive Stage", (ControllerManageBase logic) => (logic is IPoseParamAdjust) && !(logic is ILandVehicle) && !(logic is ISeaVehicle), 0.5f, 4f);
        private CreateTerminalSliderBar 最大角速度设置 { get; } = new CreateTerminalSliderBar(@"ControlMaxAngularVelocityID", @"Max Angular Velocity Stage", (ControllerManageBase logic) => (logic is IPoseParamAdjust) && !(logic is ILandVehicle) && !(logic is ISeaVehicle), 1f, 90f);
        private CreateTerminalSliderBar 最大悬浮速度设置 { get; } = new CreateTerminalSliderBar(@"ControlMaxHoverSpeedID", @"Max Hover Speed", (ControllerManageBase logic) => logic is IHeilController, 0f, 300f);
        private CreateTerminalSliderBar 最大巡航速度设置 { get; } = new CreateTerminalSliderBar(@"ControlMaxCruiseSpeedID", @"Max Cruise Speed", (ControllerManageBase logic) => logic is IPlaneController, 0f, 1e9f);
        private CreateTerminalSwitch 是否有飞翼 { get; } = new CreateTerminalSwitch(@"ControlTriggerWingsID", @"Wings Mode", (ControllerManageBase logic) => logic is IWingModeController);
        private CreateTerminalSwitch 巡航开启 { get; } = new CreateTerminalSwitch(@"ControlTriggerCruiserID", @"Cruise Mode", (ControllerManageBase logic) => logic is IPlaneController);
        private CreateTerminalSwitch 垂直起降 { get; } = new CreateTerminalSwitch(@"ControlTriggerVTOLID", @"Hover Mode", (ControllerManageBase logic) => logic is IHeilController && !(logic is HelicopterController));
        private CreateTerminalSwitch 坦克驱动模式 { get; } = new CreateTerminalSwitch(@"ControlTankModeID", @"Tank Mode", (ControllerManageBase logic) => logic is ILandVehicle);
        private CreateTerminalSwitch 潜艇驱动模式 { get; } = new CreateTerminalSwitch(@"ControlSubModID", @"Submarine Mode", (ControllerManageBase logic) => logic is ISeaVehicle);
        private CreateTerminalSliderBar 最大辅助速度设置 { get; } = new CreateTerminalSliderBar(@"ControlCruiseSpeedID", @"Cruise Speed", (ControllerManageBase logic) => logic is IPlanetVehicle, 0f, 360f);
        private CreateTerminalSliderBar 滚转角速度阻尼设置 { get; } = new CreateTerminalSliderBar(@"ControlRollDampenerID", @"Roll Dampener", (ControllerManageBase logic) => logic is FlyingMachineCtrl_Base, 0.1f, 10f);
        private CreateTerminalSliderBar 俯仰角速度阻尼设置 { get; } = new CreateTerminalSliderBar(@"ControlPitchDampenerID", @"Pitch Dampener", (ControllerManageBase logic) => logic is FlyingMachineCtrl_Base, 0.1f, 10f);
        private CreateTerminalSliderBar 摇摆角速度阻尼设置 { get; } = new CreateTerminalSliderBar(@"ControlYawDampenerID", @"Yaw Dampener", (ControllerManageBase logic) => logic is FlyingMachineCtrl_Base, 0.1f, 10f);
    }
}
