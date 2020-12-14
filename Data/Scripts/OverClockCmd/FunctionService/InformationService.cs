using VRage.Game.Components;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
namespace SuperBlocks.Controller
{
    /// <summary>
    /// 信息服务
    /// 便于探测设备发现和筛选目标
    /// </summary>
    [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation)]
    public class InformationService : MySessionComponentBase
    {
        /// <summary>
        /// 火控雷达筛选之后的物体
        /// </summary>
        public static HashSet<IMyEntity> Entities_WeaponRadar { get; } = new HashSet<IMyEntity>();
        /// <summary>
        /// 搜索雷达探测到的物体
        /// </summary>
        public static HashSet<IMyEntity> Entities_Scanner { get; } = new HashSet<IMyEntity>();
        /// <summary>
        /// 控制器注册
        /// 同一个网格中只允许一个活跃的控制器存在
        /// 其他控制器将作为备用控制器（炮塔、飞行器、载具的控制器都是互斥的）
        /// </summary>
        public static Dictionary<long, long> Grid_Controller { get; } = new Dictionary<long, long>();
        /// <summary>
        /// 干扰器注册
        /// 使用了干扰器
        /// 低一级别或者同等级别的通信和探测将被阻断
        /// 高一级别的通信和探测将无效
        /// </summary>
        public static Dictionary<long, Jammer> Grid_Jammer { get; } = new Dictionary<long, Jammer>();
        public override void UpdateBeforeSimulation()
        {
            if (!Initialized) return;
            if (!InitReady)
            {
                Init();
                return;
            }
            UpdateInformations();
            LAN_Network.UpdateInfos();
            //MyAPIGateway.Entities.OnEntityAdd   
        }
        private void Entities_OnEntityAdd(IMyEntity obj)
        {
            if (obj == null) return;            
        }
        private void Entities_OnEntityRemove(IMyEntity obj)
        {
            if (obj == null) return;           
        }
        private void UpdateInformations()
        {
            if (count > 0) { count--; return; }
            Entities_WeaponRadar.Clear();
            MyAPIGateway.Entities.GetEntities(Entities_Scanner);
            Entities_WeaponRadar.IntersectWith(Entities_Scanner);
            Entities_WeaponRadar.RemoveWhere((IMyEntity ent) =>
            {
                if (Utils.是否是体素或者行星(ent) || Utils.是否是角色或者陨石(ent))
                    return true;
                if (Utils.是否是导弹(ent))
                    return false;
                return Utils.统计网格中通电的方块(ent as IMyCubeGrid) < 5f;
            });
            count = 10;
        }
        private void Init()
        {
            //MyAPIGateway.Entities.OnEntityAdd += Entities_OnEntityAdd;
            //MyAPIGateway.Entities.OnEntityRemove += Entities_OnEntityRemove;
            InitReady = true;
            return;
        }
        protected override void UnloadData()
        {
            base.UnloadData();
            //MyAPIGateway.Entities.OnEntityAdd -= Entities_OnEntityAdd;
            //MyAPIGateway.Entities.OnEntityRemove -= Entities_OnEntityRemove;
        }
        private int count = 10;
        private bool InitReady = false;
    }
}
