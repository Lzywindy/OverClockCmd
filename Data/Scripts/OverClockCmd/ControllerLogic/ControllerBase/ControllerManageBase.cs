using System;
using Sandbox.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;

    public class ControllerManageBase
    {
        public ControllerManageBase(IMyTerminalBlock refered_block)
        {
            InitBaseInfo(refered_block);
        }
        public void Restart(IMyTerminalBlock refered_block)
        {
            InitBaseInfo(refered_block);
        }
        public void Running1()
        {
            if (!IsReadyForControl) return;
            try { AppRunning1(); } catch (Exception e) { SetDefault(); Information += (e.Message + "\n"); IsReadyForControl = false; return; }
        }
        public void Running10()
        {
            if (!IsReadyForControl) return;
            try { AppRunning10(); } catch (Exception e) { SetDefault(); Information += (e.Message + "\n"); IsReadyForControl = false; return; }
        }
        public void Running100()
        {
            if (!IsReadyForControl) return;
            try { AppRunning100(); } catch (Exception e) { SetDefault(); Information += (e.Message + "\n"); IsReadyForControl = false; return; }
        }
        #region 可以通过重载来实现不同的控制功能的地方
        protected virtual void Init(IMyTerminalBlock refered_block) { }
        #endregion
        #region 内定的方法和变量
        public string Information { get; protected set; } = "";
        public bool IsReadyForControl { get; private set; } = false;
        protected IMyTerminalBlock Me;
        protected IMyShipController MainCtrl;
        protected Action AppRunning1 = () => { };
        protected Action AppRunning10 = () => { };
        protected Action AppRunning100 = () => { };
        protected IMyGridTerminalSystem GridTerminalSystem { get { try { return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid); } catch (Exception e) { SetDefault(); Information += (e.Message + "\n"); return null; } } }
        #endregion
        #region 类体初始化
        private void InitBaseInfo(IMyTerminalBlock refered_block)
        {
            try
            {
                Information = "";
                Me = refered_block;
                MainCtrl = GetT(GridTerminalSystem, (IMyShipController block) => block.IsMainCockpit);
                Init(refered_block);
                IsReadyForControl = true;
            }
            catch (Exception e)
            {
                SetDefault(); Information += (e.Message + "\n"); IsReadyForControl = false; return;
            }
        }
        #endregion
        #region 得到基础参数
        protected Vector3 LinearVelocity { get { return Me.CubeGrid.Physics.LinearVelocity; } }
        protected Vector3 AngularVelocity { get { return Me.CubeGrid.Physics.AngularVelocity; } }
        protected Vector3 Gravity { get { return Me.CubeGrid.Physics.Gravity; } }
        protected virtual void SetDefault() { }
        public virtual void SaveDatas() { }
        #endregion
    }
}
