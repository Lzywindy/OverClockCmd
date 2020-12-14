using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using VRage.Game;
using VRage;
using Sandbox.ModAPI;
using Sandbox.Game.EntityComponents;
using SpaceEngineers.Game.Entities.Blocks;
using SpaceEngineers.Game.ModAPI;
using System;
using VRage.ObjectBuilders;
using VRage.GameServices;
using System.Text;
using VRageMath;
namespace SuperBlocks.Controller
{
    /// <summary>
    /// 这个是控件控制服务
    /// 用于服务设备的界面的
    /// 方便用户操作
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public partial class ControllerService : MySessionComponentBase
    {
        public const string ModNM = "LZYMod_FlyCtrls";
        private bool SetupComplete { get; set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!BasicInfoService.SetupComplete) return;
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
            安全偏转角度.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!安全偏转角度.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.SafetyStage_Getter(block)) }"); };
            偏转位置灵敏度.GetterFunc = FlyMachineCtrl.LocationSensetive_Getter;
            偏转位置灵敏度.SetterFunc = FlyMachineCtrl.LocationSensetive_Setter;
            偏转位置灵敏度.IncreaseFunc = FlyMachineCtrl.LocationSensetive_Inc;
            偏转位置灵敏度.DecreaseFunc = FlyMachineCtrl.LocationSensetive_Dec;
            偏转位置灵敏度.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!偏转位置灵敏度.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.LocationSensetive_Getter(block)) }"); };
            最大角速度设置.GetterFunc = FlyMachineCtrl.MaxReactions_AngleV_Getter;
            最大角速度设置.SetterFunc = FlyMachineCtrl.MaxReactions_AngleV_Setter;
            最大角速度设置.IncreaseFunc = FlyMachineCtrl.MaxReactions_Inc;
            最大角速度设置.DecreaseFunc = FlyMachineCtrl.MaxReactions_Dec;
            最大角速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大角速度设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxReactions_AngleV_Getter(block)) }"); };
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
            最大悬浮速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大悬浮速度设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxiumHoverSpeed_Getter(block)) }"); };
            最大巡航速度设置.GetterFunc = FlyMachineCtrl.MaxiumFlightSpeed_Getter;
            最大巡航速度设置.SetterFunc = FlyMachineCtrl.MaxiumFlightSpeed_Setter;
            最大巡航速度设置.IncreaseFunc = FlyMachineCtrl.MaxiumFlightSpeed_Inc;
            最大巡航速度设置.DecreaseFunc = FlyMachineCtrl.MaxiumFlightSpeed_Dec;
            最大巡航速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大巡航速度设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(FlyMachineCtrl.MaxiumFlightSpeed_Getter(block)) }"); };
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
            最大辅助速度设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大辅助速度设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(PlanetVech.MaximumCruiseSpeed_Getter(block)) }"); };
            滚转角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_R_Getter;
            滚转角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_R_Setter;
            滚转角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_R_Inc;
            滚转角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_R_Dec;
            滚转角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!滚转角速度阻尼设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_R_Getter(block)) }x"); };
            俯仰角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_P_Getter;
            俯仰角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_P_Setter;
            俯仰角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_P_Inc;
            俯仰角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_P_Dec;
            俯仰角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!俯仰角速度阻尼设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_P_Getter(block)) }x"); };
            摇摆角速度阻尼设置.GetterFunc = VehcCtrl.AngularDampener_Y_Getter;
            摇摆角速度阻尼设置.SetterFunc = VehcCtrl.AngularDampener_Y_Setter;
            摇摆角速度阻尼设置.IncreaseFunc = VehcCtrl.AngularDampener_Y_Inc;
            摇摆角速度阻尼设置.DecreaseFunc = VehcCtrl.AngularDampener_Y_Dec;
            摇摆角速度阻尼设置.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!摇摆角速度阻尼设置.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundOn2(VehcCtrl.AngularDampener_Y_Getter(block)) }x"); };
            锁定目标.TriggerFunc = TargetLocker_CameraBlock.LockOnTarget;
            解除锁定.TriggerFunc = TargetLocker_CameraBlock.UnlockTarget;
            最大锁定距离.GetterFunc = TargetLocker_CameraBlock.Get_DetectedDistance;
            最大锁定距离.SetterFunc = TargetLocker_CameraBlock.Set_DetectedDistance;
            最大锁定距离.IncreaseFunc = (IMyTerminalBlock block) => { TargetLocker_CameraBlock.Set_DetectedDistance(block, TargetLocker_CameraBlock.Get_DetectedDistance(block) + 100); };
            最大锁定距离.DecreaseFunc = (IMyTerminalBlock block) => { TargetLocker_CameraBlock.Set_DetectedDistance(block, TargetLocker_CameraBlock.Get_DetectedDistance(block) - 100); };
            最大锁定距离.WriterFunc = (IMyTerminalBlock block, StringBuilder value) => { if (!最大锁定距离.Filter(block)) return; value.Clear(); value.Append($"{MathHelper.RoundToInt(TargetLocker_CameraBlock.Get_DetectedDistance(block)) }"); };
            循环锁定部件.TriggerFunc = TargetLocker_CameraBlock.CycleTargetPart;
        }
        public void Init()
        {
            if (得的当前的目标 == null)
                得的当前的目标 = new CreateProperty<Sandbox.ModAPI.Ingame.MyDetectedEntityInfo?, IMyRemoteControl>(@"CurrentTargetLockedID", TargetLocker_CameraBlock.EnabledTargetLocker, TargetLocker_CameraBlock.Get_CurrentTarget, (IMyTerminalBlock block, Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? value) => { });
            if (得的当前的坐标 == null)
                得的当前的坐标 = new CreateProperty<Vector3D?, IMyRemoteControl>(@"CurrentTargetPositionID", TargetLocker_CameraBlock.EnabledTargetLocker, TargetLocker_CameraBlock.GetHitPosition, (IMyTerminalBlock block, Vector3D? value) => { });
            if (得的跟踪目标的当前 == null)
                得的跟踪目标的当前 = new CreateProperty<Vector3D?, IMyRemoteControl>(@"CurrentTargetTrackPositionID", TargetLocker_CameraBlock.EnabledTargetLocker, TargetLocker_CameraBlock.TrackTargetHitPosition, (IMyTerminalBlock block, Vector3D? value) => { });
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
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 锁定目标.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 解除锁定.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 最大锁定距离.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 锁定目标.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 解除锁定.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 最大锁定距离.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter += 循环锁定部件.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 循环锁定部件.CreateAction;
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
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_1.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 锁定目标.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 解除锁定.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 最大锁定距离.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 锁定目标.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 解除锁定.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 最大锁定距离.CreateAction;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 循环锁定部件.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 循环锁定部件.CreateAction;
        }
    }
    public partial class ControllerService
    {
        private CreateTerminalFence<IMyTerminalBlock> Fence_0 { get; } = new CreateTerminalFence<IMyTerminalBlock>(Filter_基本);
        private CreateTerminalButton<IMyTerminalBlock> 重启程序 { get; } = new CreateTerminalButton<IMyTerminalBlock>(@"ControlResetID", @"Restart Program", Filter_基本);
        private CreateTerminalSwitch<IMyTerminalBlock> 开关引擎 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTriggerThrustID", @"Thrust", Filter_基本设备);
        private CreateTerminalSwitch<IMyTerminalBlock> 开关动量轮 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTriggerGyroID", @"Gyro", Filter_基本设备);
        private CreateTerminalSliderBar<IMyTerminalBlock> 安全偏转角度 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlAngleStageID", @"Angle Stage", Filter_飞行基本, VehicleControllerBase.SafetyStageMin, VehicleControllerBase.SafetyStageMax);
        private CreateTerminalSliderBar<IMyTerminalBlock> 偏转位置灵敏度 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlLocalSensitiveID", @"Local Sensitive Stage", Filter_飞行基本, 0.5f, 4f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 最大角速度设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlMaxAngularVelocityID", @"Max Angular Velocity Stage", Filter_飞行基本, 1f, 90f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 最大悬浮速度设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlMaxHoverSpeedID", @"Max Hover Speed", Filter_飞行_直升机, 0f, 300f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 最大巡航速度设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlMaxCruiseSpeedID", @"Max Cruise Speed", Filter_飞行_固定翼, 0f, 1e9f);
        private CreateTerminalSwitch<IMyTerminalBlock> 是否有飞翼 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTriggerWingsID", @"Wings Mode", Filter_飞行_固定翼_空气动力);
        private CreateTerminalSwitch<IMyTerminalBlock> 巡航开启 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTriggerCruiserID", @"Cruise Mode", Filter_飞行_固定翼);
        private CreateTerminalSwitch<IMyTerminalBlock> 垂直起降 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTriggerVTOLID", @"Hover Mode", Filter_飞行_垂直起降);
        private CreateTerminalSwitch<IMyTerminalBlock> 坦克驱动模式 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlTankModeID", @"Tank Mode", Filter_陆地);
        private CreateTerminalSwitch<IMyTerminalBlock> 潜艇驱动模式 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>(@"ControlSubModID", @"Submarine Mode", Filter_海洋);
        private CreateTerminalSliderBar<IMyTerminalBlock> 最大辅助速度设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlCruiseSpeedID", @"Cruise Speed", Filter_星球基本, 0f, 360f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 滚转角速度阻尼设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlRollDampenerID", @"Roll Dampener", Filter_飞行_姿态, 0.1f, 10f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 俯仰角速度阻尼设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlPitchDampenerID", @"Pitch Dampener", Filter_飞行_姿态, 0.1f, 10f);
        private CreateTerminalSliderBar<IMyTerminalBlock> 摇摆角速度阻尼设置 { get; } = new CreateTerminalSliderBar<IMyTerminalBlock>(@"ControlYawDampenerID", @"Yaw Dampener", Filter_飞行_姿态, 0.1f, 10f);
        private CreateTerminalFence<IMyRemoteControl> Fence_1 { get; } = new CreateTerminalFence<IMyRemoteControl>(TargetLocker_CameraBlock.EnabledTargetLocker);
        private CreateTerminalButton<IMyRemoteControl> 锁定目标 { get; } = new CreateTerminalButton<IMyRemoteControl>(@"ControlLockOnTargetID", @"Lock On Target", TargetLocker_CameraBlock.EnabledTargetLocker);
        private CreateTerminalButton<IMyRemoteControl> 解除锁定 { get; } = new CreateTerminalButton<IMyRemoteControl>(@"ControlUnlockTargetID", @"Unlock Target", TargetLocker_CameraBlock.EnabledTargetLocker);
        private CreateTerminalButton<IMyRemoteControl> 循环锁定部件 { get; } = new CreateTerminalButton<IMyRemoteControl>(@"ControlCycleTargetPartID", @"Cycle Target Part", TargetLocker_CameraBlock.EnabledTargetLocker);
        private CreateTerminalSliderBar<IMyRemoteControl> 最大锁定距离 { get; } = new CreateTerminalSliderBar<IMyRemoteControl>(@"ControlDistanceID", @"Distance", TargetLocker_CameraBlock.EnabledTargetLocker, 50, 50000);
        protected static CreateProperty<Sandbox.ModAPI.Ingame.MyDetectedEntityInfo?, IMyRemoteControl> 得的当前的目标 { get; private set; }
        protected static CreateProperty<Vector3D?, IMyRemoteControl> 得的当前的坐标 { get; private set; }
        protected static CreateProperty<Vector3D?, IMyRemoteControl> 得的跟踪目标的当前 { get; private set; }
    }
    public partial class ControllerService
    {
        private static Func<IMyTerminalBlock, ControllerManageBase> GetGameCtrlLogic { get; } = (IMyTerminalBlock block) => { if (block == null) return null; var logic = block.GameLogic.GetAs<ControlBase>(); if (logic == null) return null; return logic.Control; };
        private static Func<IMyTerminalBlock, bool> Filter_基本 { get; } = (IMyTerminalBlock block) => { return GetGameCtrlLogic(block) != null; };
        private static Func<IMyTerminalBlock, bool> Filter_基本设备 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is ICtrlDevCtrl); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行基本 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IPoseParamAdjust) && !(ctrl is ILandVehicle) && !(ctrl is ISeaVehicle); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行_固定翼 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IPlaneController); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行_固定翼_空气动力 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IPlaneController); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行_直升机 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IHeilController); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行_垂直起降 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IHeilController) && !(ctrl is HelicopterController); };
        private static Func<IMyTerminalBlock, bool> Filter_星球基本 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is IPlanetVehicle); };
        private static Func<IMyTerminalBlock, bool> Filter_陆地 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is ILandVehicle); };
        private static Func<IMyTerminalBlock, bool> Filter_海洋 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is ISeaVehicle); };
        private static Func<IMyTerminalBlock, bool> Filter_飞行_姿态 { get; } = (IMyTerminalBlock block) => { var ctrl = GetGameCtrlLogic(block); if (ctrl == null) return false; return (ctrl is FlyingMachineCtrl_Base); };
    }
}
