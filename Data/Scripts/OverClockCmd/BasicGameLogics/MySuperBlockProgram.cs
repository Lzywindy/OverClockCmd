using System;
using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Text;
namespace SuperBlocks.Controller
{
    public partial class MySuperBlockProgram : MyGameLogicComponent
    {
        /// <summary>
        /// 得的当前方块
        /// </summary>
        protected IMyTerminalBlock Me { get; private set; }
        /// <summary>
        /// 得的当前方块所在的网络
        /// </summary>
        protected IMyCubeGrid CurrentGrid { get; private set; }
        /// <summary>
        /// 得的该网络的信息注册系统
        /// </summary>
        protected IMyGridTerminalSystem GridTerminalSystem { get; private set; }
        protected bool InitFinished { get; set; }
        public bool EnabledRunning { get; set; } = true;
        public sealed override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            LoadData();
            InitFinished = false;
            InitLogics();
            Me.AppendingCustomInfo += Me_AppendingCustomInfo;
            NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        /// <summary>
        /// 显示方块信息
        /// </summary>
        private void Me_AppendingCustomInfo(IMyTerminalBlock arg1, StringBuilder arg2)
        {
            if (arg1.EntityId != Me.EntityId) return;
            try
            {
                arg2.Clear();
                arg2.AppendStringBuilder(ExceptionInfo);
                SetupInfos(arg1, arg2);
            }
            catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        /// <summary>
        /// 供外部调用该函数以达到保存数据的目的
        /// </summary>
        public void SaveData()
        {
            try
            {
                var enabled = Me?.Storage?.ContainsKey(Utils.MyGuid);
                if (enabled == null) return;
                if (enabled.Value == false)
                    Me.Storage.Remove(Utils.MyGuid);
                Me.Storage.Add(Utils.MyGuid, SerializeConfig());
            }
            catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        /// <summary>
        /// 初始化逻辑
        /// </summary>
        private void InitLogics()
        {
            if (InitFinished) return;
            try
            {
                Me = Entity as IMyTerminalBlock;
                CurrentGrid = Me.GetTopMostParent(typeof(IMyCubeGrid)) as IMyCubeGrid;
                GridTerminalSystem = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(CurrentGrid);
                FunctionAddOn();
                InitAction?.Invoke();
                InitFinished = true;
            }
            catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        private void LoadData()
        {
            BasicInfoService.Register.Add(Me);
            string value = "";
            var enabled = Me?.Storage?.TryGetValue(Utils.MyGuid, out value);
            if (enabled.HasValue && enabled.Value) ConfigProcess(value);
        }
        /// <summary>
        /// 重载此函数，以序列化参数
        /// </summary>
        /// <returns>序列化之后的字符串</returns>
        protected virtual string SerializeConfig() { return ""; }
        /// <summary>
        /// 重载此项目以添加更新函数
        /// </summary>
        protected virtual void FunctionAddOn()
        {
            SetupInfos = (IMyTerminalBlock block, StringBuilder Context) =>
            {
                if (block.EntityId != Me.EntityId) return;
                if (EnabledRunning)
                    Context.Append($"System Online\n\r");
                if (InitFinished)
                    Context.Append($"Init Finished\n\r");
            };
        }
    }
    public partial class MySuperBlockProgram
    {
        public sealed override void Close()
        {
            base.Close();
            try { BasicInfoService.Register.Remove(Me); Me.AppendingCustomInfo -= Me_AppendingCustomInfo; OnClose(); } catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        public sealed override void MarkForClose()
        {
            base.MarkForClose();
            try { BasicInfoService.Register.Remove(Me); OnClose(); } catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        public sealed override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (!EnabledRunning) return;
            if (!BasicInfoService.SetupComplete) return;
            try { Running1Action(); } catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        public sealed override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            Me?.RefreshCustomInfo();
            InitLogics();
            if (!EnabledRunning) return;
            if (!BasicInfoService.SetupComplete) return;
            try { Running10Action(); } catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
        public sealed override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (!EnabledRunning) return;
            if (!BasicInfoService.SetupComplete) return;
            try { Running100Action(); } catch (Exception e) { ExceptionInfo.Clear(); ExceptionInfo.Append(e); InitFinished = false; }
        }
    }
    public partial class MySuperBlockProgram
    {
        protected StringBuilder ExceptionInfo { get; } = new StringBuilder();
        protected Action InitAction = () => { };
        protected Action Running1Action = () => { };
        protected Action Running10Action = () => { };
        protected Action Running100Action = () => { };
        protected Action OnClose = () => { };
        protected Action<string> ConfigProcess = (string cfg) => { };
        protected Action<IMyTerminalBlock, StringBuilder> SetupInfos = (IMyTerminalBlock block, StringBuilder Context) => { };
    }
    public partial class MySuperBlockProgram
    {
        public static T GetLogic<T>(IMyTerminalBlock Me) where T : MySuperBlockProgram
        {
            return Me?.GameLogic?.GetAs<T>();
        }
        public static bool ActionsEnabled(IMyTerminalBlock Me)
        {
            return GetLogic<MySuperBlockProgram>(Me) != null;
        }
        public static bool Get_Enabled(IMyTerminalBlock Me)
        {
            var logic = GetLogic<MySuperBlockProgram>(Me);
            if (logic == null) return false;
            return logic.EnabledRunning;
        }
        public static void Set_Enabled(IMyTerminalBlock Me, bool Enabled)
        {
            var logic = GetLogic<MySuperBlockProgram>(Me);
            if (logic == null) return;
            logic.EnabledRunning = Enabled;
        }
        public static void TriggeEnabled(IMyTerminalBlock Me)
        {
            var logic = GetLogic<MySuperBlockProgram>(Me);
            if (logic == null) return;
            logic.EnabledRunning = !logic.EnabledRunning;
        }
        public static void Trigger_Restart(IMyTerminalBlock Me)
        {
            var logic = GetLogic<MySuperBlockProgram>(Me);
            if (logic == null) return;
            logic.InitFinished = false;
        }
        public const string ActionID = @"ProgramRunningEnabledID";
        public const string ActionNM = @"Program Running Enabled";
    }
}