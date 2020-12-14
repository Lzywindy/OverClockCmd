using System;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using VRageMath;
using System.Text;
namespace SuperBlocks.Controller
{
    public class MyTargetPredictDevice : MySuperBlockProgram
    {
        #region 对象函数和成员

        protected virtual void CheckSystem() { }
        protected virtual void Running()
        {
            bool hastarget = CanApprochTarget;
            if (!EnabledRunning) return;
            if (!hastarget) V_project = null;
        }
        protected void InitWeapons(List<IMyUserControllableGun> Guns)
        {
            if (Guns == null || Guns.Count < 1) return;
            Guns.RemoveAll(block => block is IMyLargeTurretBase);
            WeaponManager.SetWeaponsHook(Guns);
        }
        protected override void FunctionAddOn()
        {
            base.FunctionAddOn();
            InitAction += InitSystem;
            Running1Action += Running;
            Running1Action += WeaponManager.Salvo;
            SetupInfos += (IMyTerminalBlock Me, StringBuilder Context) =>
            {
                if (Me.EntityId != this.Me.EntityId) return;
                if (WeaponManager.WeaponManagersOnline)
                    Context.Append($"Weapons Online\n\r");
                if (TargetFilterManager.EntityIds != null)
                    Context.Append($"Entity Count:{TargetFilterManager.EntityIds.Count}\n\r");
            };
        }
        private void InitSystem()
        {
            TargetPredict.Set_CalcCount(5);
            TargetPredict.Set_Delta_Time(1);
            TargetPredict.Set_Delta_Precious(1);
            TargetPredict.Set_V_project_length(500);
            TargetPredict.Set_Ignore_Gravity(true);
            TargetPredict.Set_Ignore_Self_Velocity(false);
            TargetPredict.Set_IsDirectWeapon(false);
            TargetPredict.Set_Cannon_Offset(0);
            WeaponManager.Salvo(true);
            SetTargetAimRange(2000);
            GetIgnoredEntities();
            CheckSystem();
        }
        public void FireWeapons()
        {
            WeaponManager.FiringWeapons();
        }
        public void SetTargetAimRange(float Range)
        {
            TargetAimRange = Range;
        }
        private void GetIgnoredEntities()
        {
            HashSet<IMyCubeGrid> IgnoredCubeGrid = new HashSet<IMyCubeGrid>();
            MyAPIGateway.GridGroups.GetGroup(Me.CubeGrid, GridLinkTypeEnum.Logical, IgnoredCubeGrid);
            IgnoredCubeGrid.Add(Me.CubeGrid);
            TargetFilterManager.SetIgnoredEntityList(IgnoredCubeGrid);
        }
        #endregion
        #region 模块
        protected MyWeaponManager WeaponManager { get; } = new MyWeaponManager(null);
        protected MyTargetManager TargetManager { get; } = new MyTargetManager();
        protected MyTargetFilterManager TargetFilterManager { get; } = "AllFunctionalBlocks|Missiles";
        protected MyTargetPredict TargetPredict { get; } = new MyTargetPredict(0.0001f, 1, 5, 900, 0);
        #endregion
        #region 变量和属性
        protected bool CanApprochTarget
        {
            get
            {
                if (!EnabledRunning || !TargetManager.IsNotNullTarget(Me, TargetFilterManager.BlockFilter)) { EntityID = null; t_n = null; V_project = null; return false; }
                Vector3? SelfPosition;
                Vector3? Current_CannonDirection;
                WeaponManager.CalcParameters(out SelfPosition, out Current_CannonDirection, TargetPredict.Cannon_Offset);
                if (TargetManager.GPSMode) { EntityID = null; t_n = null; V_project = null; }
                else if (EntityID == null || TargetManager.TargetID == null || EntityID.Value != TargetManager.TargetID.Value) { EntityID = TargetManager.TargetID; t_n = null; V_project = null; }
                if (TargetManager.Distance(CurrentGrid?.GetPosition()) < 1) { t_n = null; V_project = null; return false; }
                TargetPredict.Running(TargetManager.GetTargetPV(), CurrentGrid, SelfPosition, Current_CannonDirection, ref V_project, ref t_n);
                return true;
            }
        }
        protected Vector3D? Direction => V_project;
        public float TargetAimRange { get { return _TargetAimRange; } set { _TargetAimRange = Math.Max(value, 100f); } }
        private float _TargetAimRange = 100;
        protected Vector3D? V_project = null;
        protected double? t_n = null;
        protected long? EntityID = null;
        #endregion
        #region 纯属性
        public static void SetGPS_Target(IMyTerminalBlock Me, Vector3D? GPS)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetManager.SetGTarget(GPS);
        }
        public static void SetID_Target(IMyTerminalBlock Me, long EntityID)
        {
            var logic = GetLogic<MyTargetPredictDevice>(Me);
            if (logic == null) return;
            logic.TargetManager.SetITarget(EntityID, logic.TargetFilterManager.EntityFilter);
        }
        public static Vector3D? Get_PredictDirection(IMyTerminalBlock Me)
        {
            return GetLogic<MyTargetPredictDevice>(Me)?.V_project;
        }
        public static void Set_Ignore_Self_Velocity(IMyTerminalBlock Me, bool value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_Ignore_Self_Velocity(value);
        }
        public static void Set_Ignore_Gravity(IMyTerminalBlock Me, bool value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_Ignore_Gravity(value);
        }
        public static void Set_IsDirectWeapon(IMyTerminalBlock Me, bool value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_IsDirectWeapon(value);
        }
        public static void Set_CalcCount(IMyTerminalBlock Me, int value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_CalcCount(value);
        }
        public static void Set_V_project_length(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_V_project_length(value);
        }
        public static void Set_Delta_Precious(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_Delta_Precious(value);
        }
        public static void Set_Delta_Time(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_Delta_Time(value);
        }
        public static void Set_Cannon_Offset(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetPredict.Set_Cannon_Offset(value);
        }
        public static void Set_TargetAimRange(IMyTerminalBlock Me, float value)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.SetTargetAimRange(value);
        }
        public static List<long> Get_WeaponIds(IMyTerminalBlock Me)
        {
            return GetLogic<MyTargetPredictDevice>(Me)?.WeaponManager.GetWeaponIds();
        }
        #endregion
        #region UI控件和属性(可以在UI控制，也可以在Programmable Block进行控制)
        public static bool WeaponCtrlEnabled(IMyTerminalBlock Me)
        {
            return GetLogic<MyTargetPredictDevice>(Me) != null;
        }
        public static void Trigger_CycleSubparts(IMyTerminalBlock Me)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.TargetManager.CycleSubpart();
        }
        public static void Trigger_FireWeapons(IMyTerminalBlock Me)
        {
            GetLogic<MyTargetPredictDevice>(Me)?.FireWeapons();
        }
        public static void Trigger_SalvoEnabled(IMyTerminalBlock Me)
        {
            var logic = GetLogic<MyTargetPredictDevice>(Me);
            if (logic == null) return;
            logic.WeaponManager.Salvo();
        }
        public static bool Get_SalvoEnabled(IMyTerminalBlock Me)
        {
            var logic = GetLogic<MyTargetPredictDevice>(Me);
            if (logic == null) return false;
            return logic.WeaponManager.SalvoEnabled;
        }
        public static void Set_SalvoEnabled(IMyTerminalBlock Me, bool SalvoEnabled)
        {
            var logic = GetLogic<MyTargetPredictDevice>(Me);
            if (logic == null) return;
            logic.WeaponManager.Salvo(SalvoEnabled);
        }
        public static void Set_AimAtConfig(IMyTerminalBlock Me, StringBuilder config_aimat)
        {
            if (config_aimat == null) return;
            var logic = GetLogic<MyTargetPredictDevice>(Me);
            if (logic == null) return;
            GetLogic<MyTargetPredictDevice>(Me).TargetFilterManager.SetConfig(config_aimat.ToString());
        }
        public static StringBuilder Get_AimAtConfig(IMyTerminalBlock Me)
        {
            var value = GetLogic<MyTargetPredictDevice>(Me).TargetFilterManager.AimAtConfig;
            if (value == null)
                return new StringBuilder("");
            else
                return new StringBuilder(value);
        }
        #endregion
    }
}