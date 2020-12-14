using System;
using VRage.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks.Controller
{
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public sealed class WeaponInfoService : MySessionComponentBase
    {
        public static bool SetupComplete { get; private set; } = false;
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized || !BasicInfoService.SetupComplete) return;
            if (!SetupComplete)
            {
                SetupComplete = true;
                Init();
                return;
            }
        }
        public void Init()
        {
            #region 控件函数连接MyTargetPredictDevice基类
            开火.TriggerFunc = MyTargetPredictDevice.Trigger_FireWeapons;
            是否齐射.TriggerFunc = MyTargetPredictDevice.Trigger_SalvoEnabled;
            是否齐射.GetterFunc = MyTargetPredictDevice.Get_SalvoEnabled;
            是否齐射.SetterFunc = MyTargetPredictDevice.Set_SalvoEnabled;
            瞄准配置.GetterFunc = MyTargetPredictDevice.Get_AimAtConfig;
            瞄准配置.SetterFunc = MyTargetPredictDevice.Set_AimAtConfig;
            切换瞄准部位.TriggerFunc = MyTargetPredictDevice.Trigger_CycleSubparts;
            #endregion
            #region 属性函数连接MyTargetPredictDevice基类
            目标设置 = new CreateProperty<long, IMyTerminalBlock>("SetID_Target", MyTargetPredictDevice.WeaponCtrlEnabled, block => -1, MyTargetPredictDevice.SetID_Target);
            坐标设置 = new CreateProperty<Vector3D?, IMyTerminalBlock>("SetGPS_Target", MyTargetPredictDevice.WeaponCtrlEnabled, block => null, MyTargetPredictDevice.SetGPS_Target);
            得的预测方向 = new CreateProperty<Vector3D?, IMyTerminalBlock>("PredictDirection", MyTargetPredictDevice.WeaponCtrlEnabled, MyTargetPredictDevice.Get_PredictDirection, (IMyTerminalBlock block, Vector3D? pos) => { });
            预测忽略重力 = new CreateProperty<bool, IMyTerminalBlock>("Ignore_Gravity", MyTargetPredictDevice.WeaponCtrlEnabled, block => true, MyTargetPredictDevice.Set_Ignore_Gravity);
            预测忽略自己速度 = new CreateProperty<bool, IMyTerminalBlock>("Ignore_Self_Velocity", MyTargetPredictDevice.WeaponCtrlEnabled, block => false, MyTargetPredictDevice.Set_Ignore_Self_Velocity);
            是否是直瞄武器 = new CreateProperty<bool, IMyTerminalBlock>("IsDirectWeapon", MyTargetPredictDevice.WeaponCtrlEnabled, block => false, MyTargetPredictDevice.Set_IsDirectWeapon);
            设置迭代次数 = new CreateProperty<int, IMyTerminalBlock>("CalcCount", MyTargetPredictDevice.WeaponCtrlEnabled, block => 5, MyTargetPredictDevice.Set_CalcCount);
            设置炮口初速度 = new CreateProperty<float, IMyTerminalBlock>("V_project_length", MyTargetPredictDevice.WeaponCtrlEnabled, block => 900, MyTargetPredictDevice.Set_V_project_length);
            设置迭代精度 = new CreateProperty<float, IMyTerminalBlock>("Delta_Precious", MyTargetPredictDevice.WeaponCtrlEnabled, block => 0.0001f, MyTargetPredictDevice.Set_Delta_Precious);
            设置时间加倍 = new CreateProperty<float, IMyTerminalBlock>("Delta_Time", MyTargetPredictDevice.WeaponCtrlEnabled, block => 1, MyTargetPredictDevice.Set_Delta_Time);
            设置炮口偏移 = new CreateProperty<float, IMyTerminalBlock>("Cannon_Offset", MyTargetPredictDevice.WeaponCtrlEnabled, block => 0, MyTargetPredictDevice.Set_Cannon_Offset);
            设置瞄准范围 = new CreateProperty<float, IMyTerminalBlock>("TargetAimRange", MyTargetPredictDevice.WeaponCtrlEnabled, block => 1000, MyTargetPredictDevice.Set_TargetAimRange);
            得到所有武器ID = new CreateProperty<List<long>, IMyTerminalBlock>("WeaponIds", MyTargetPredictDevice.WeaponCtrlEnabled, MyTargetPredictDevice.Get_WeaponIds, (IMyTerminalBlock block, List<long> ids) => { });
            #endregion
            #region 纯属性MyTargetPredictDevice
            最大水平响应率 = new CreateProperty<float, IMyTerminalBlock>("Max_Az_Speed", MyTurretController.TurretControllerEnabled, block => 30, MyTurretController.Set_Max_Az_Speed);
            最大垂直响应率 = new CreateProperty<float, IMyTerminalBlock>("Max_Ev_Speed", MyTurretController.TurretControllerEnabled, block => 30, MyTurretController.Set_Max_Ev_Speed);
            炮塔灵敏度响应 = new CreateProperty<float, IMyTerminalBlock>("RotationMult", MyTurretController.TurretControllerEnabled, block => 1, MyTurretController.Set_RotationMult);
            #endregion
            MyAPIGateway.TerminalControls.CustomControlGetter += Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 开火.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 是否齐射.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 瞄准配置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter += 切换瞄准部位.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter += 开火.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 是否齐射.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter += 切换瞄准部位.CreateAction;
        }
        protected sealed override void UnloadData()
        {
            MyAPIGateway.TerminalControls.CustomControlGetter -= Fence_0.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 开火.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 是否齐射.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 瞄准配置.CreateController;
            MyAPIGateway.TerminalControls.CustomControlGetter -= 切换瞄准部位.CreateController;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 开火.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 是否齐射.CreateAction;
            MyAPIGateway.TerminalControls.CustomActionGetter -= 切换瞄准部位.CreateAction;
        }
        #region UI控件（自动属性）MyTargetPredictDevice基类
        private CreateTerminalFence<IMyTerminalBlock> Fence_0 { get; } = new CreateTerminalFence<IMyTerminalBlock>(MyTargetPredictDevice.WeaponCtrlEnabled);
        private CreateTerminalButton<IMyTerminalBlock> 开火 { get; } = new CreateTerminalButton<IMyTerminalBlock>("ControlFireWeaponsID", "Fire Weapons", MyTargetPredictDevice.WeaponCtrlEnabled);
        private CreateTerminalSwitch<IMyTerminalBlock> 是否齐射 { get; } = new CreateTerminalSwitch<IMyTerminalBlock>("ControlSalvoEnabledID", "Salvo Enabled", MyTargetPredictDevice.WeaponCtrlEnabled);
        private CreateTerminalTextBox<IMyTerminalBlock> 瞄准配置 { get; } = new CreateTerminalTextBox<IMyTerminalBlock>("ControlAimAtConfigID", "Aim At Config", MyTargetPredictDevice.WeaponCtrlEnabled);
        private CreateTerminalButton<IMyTerminalBlock> 切换瞄准部位 { get; } = new CreateTerminalButton<IMyTerminalBlock>("ControlCycleSubpartsID", "Cycle Subparts", MyTargetPredictDevice.WeaponCtrlEnabled);
        #endregion
        #region 纯属性MyTargetPredictDevice基类
        public CreateProperty<long, IMyTerminalBlock> 目标设置 { get; private set; }
        public CreateProperty<Vector3D?, IMyTerminalBlock> 坐标设置 { get; private set; }
        public CreateProperty<Vector3D?, IMyTerminalBlock> 得的预测方向 { get; private set; }
        public CreateProperty<bool, IMyTerminalBlock> 预测忽略重力 { get; private set; }
        public CreateProperty<bool, IMyTerminalBlock> 预测忽略自己速度 { get; private set; }
        public CreateProperty<bool, IMyTerminalBlock> 是否是直瞄武器 { get; private set; }
        public CreateProperty<int, IMyTerminalBlock> 设置迭代次数 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 设置炮口初速度 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 设置迭代精度 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 设置时间加倍 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 设置炮口偏移 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 设置瞄准范围 { get; private set; }
        public CreateProperty<List<long>, IMyTerminalBlock> 得到所有武器ID { get; private set; }
        #endregion
        #region 纯属性MyTargetPredictDevice
        public CreateProperty<float, IMyTerminalBlock> 最大水平响应率 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 最大垂直响应率 { get; private set; }
        public CreateProperty<float, IMyTerminalBlock> 炮塔灵敏度响应 { get; private set; }
        #endregion
    }
}