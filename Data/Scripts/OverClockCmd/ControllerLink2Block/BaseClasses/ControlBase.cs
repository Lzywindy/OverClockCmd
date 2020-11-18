using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI;

namespace SuperBlocks.Controller
{
    /// <summary>
    /// 控制模组
    /// 炮塔控制
    /// {   
    ///     带目标跟踪（弹道计算，可选（直射武器不用））
    ///     不带目标跟踪（弹道计算，可选）
    ///     自动索敌（需要引用雷达数据,但是数据则取自于一台信息服务器，引用方块指定为Data From:XXXX）
    ///     机械手控制（待做）
    ///     需要在炮塔子网格中
    /// }
    /// 载具控制
    /// {
    ///     太空飞船（轴稳定、重力环境下水平（上、左、右的推进器可减少），巡航飞行（稳定态，重力依赖，速度弱依赖）、战斗飞行（超机动，重力依赖，速度强依赖），速度限制，曲率飞行（需要有目标点，以及曲速引擎，并无位置要求（大气层跃迁也行）））
    ///     垂降飞机（轴稳定、重力环境下水平（上、左、右的推进器可减少），巡航飞行（稳定态，重力依赖，速度弱依赖）、战斗飞行（超机动，重力依赖，速度强依赖），速度限制）
    ///     地面坦克（轴稳定，差速转向，速度限制）
    ///     水面船舶（轴稳定，巡航模式（定速））
    ///     潜艇（轴稳定，巡航模式（定速））
    ///     机械腿控制（待做）
    /// }
    /// 雷达数据获取
    /// {
    ///     雷达需要1个或者多个计算机（可编程模块或者控制台）来进行信号处理（可以通过多个雷达聚合到一个内存集合里）
    ///     雷达指定的时候可以采用雷达的CustomData中指定IP来指定网络中的计算机进行处理     
    ///     数据可视化通过指定计算机来进行，数据会发送到CustomData中，指定现实内容为CustomName为XXXXX对应的可编程模块（HDMI:XXXXX）
    ///     雷达的距离设定可以通过数据控制台实现，但是距离越远越耗电，需要的算力越多（不然的话，目标就显示不全）
    /// }
    /// </summary>
    public class ControlBase : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            CreateController();
            NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        protected virtual void CreateController()
        {
            Control = new ControllerManageBase(Entity as IMyTerminalBlock);
        }
        public override void Close()
        {
            base.Close();
        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            if (!Control.IsReadyForControl) return;
            Control.Running1();
        }
        public override void UpdateBeforeSimulation10()
        {
            base.UpdateBeforeSimulation10();
            if (!Control.IsReadyForControl) return;
            Control.Running10();
        }
        public override void UpdateBeforeSimulation100()
        {
            base.UpdateBeforeSimulation100();
            if (Control.IsReadyForControl) Control.Running100();
            else Control.Restart(Entity as IMyTerminalBlock);
        }
        public ControllerManageBase Control { get; protected set; }
        public void RunningRestart()
        {
            Control.Restart(Entity as IMyTerminalBlock);
        }
    }
}
