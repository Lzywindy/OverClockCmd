using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks.Controller
{
    /// <summary>
    /// 目标检测，可以单独作为火控雷达使用
    /// 但是也可以做为主机：
    ///     来让其他隶属的火控系统进行锁定
    ///     可以通过不同炮塔位置、旋转范围来分配其优先级
    /// </summary>
    public sealed class MyRadarTargets : UpdateableClass
    {
        #region 初始化函数
        public MyRadarTargets() : base() { }
        #endregion
        #region 外部可调用函数
        public void ResetParameters(IMyTerminalBlock DetectorProcess, float distance)
        {
            if (Utils.Common.NullEntity(DetectorProcess)) return;
            Range = distance * 1.5;
        }
        public MyTargetDetected 得的最近向我靠近最快的目标(IMyTerminalBlock RequstBlock, double MaxRange = 1000)
        {
            if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList) || Range < 10) return null;
            var ent_tgs = TargetsList.Where(tg => tg.GetDistance(RequstBlock) < MaxRange)?.ToList();
            if (Utils.Common.IsNullCollection(ent_tgs)) return null;
            if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)(tg.Priority(RequstBlock)));
            else if (TargetsList.Count == 1) return TargetsList.FirstElement();
            else return null;
        }
        #endregion
        #region 更新函数       
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (updatecounts % 21 == 0)
            {
                TargetsList.RemoveWhere(t => t.InvalidTarget || t.TargetSafety());
                foreach (var item in TargetsList) { item.Update(CtrlBlock); }
            }
            if (updatecounts % 9 == 0)
            {
                BoundingSphereD bounding = new BoundingSphereD(CtrlBlock.GetPosition(), Range);
                DetectedEntities.Clear();
                List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounding);
                if (Utils.Common.IsNullCollection(entities)) return;
                DetectedEntities.UnionWith(entities);
                List<IMyEntity> 敌人 = entities.Where(t => Utils.MyTargetEnsureAPI.IsEnemy(t, CtrlBlock))?.ToList();
                if (Utils.Common.IsNullCollection(敌人)) return;
                Enemies.RemoveWhere(Utils.Common.NullEntity);
                Enemies.UnionWith(敌人);
                if (Utils.Common.IsNullCollection(Enemies)) return;
                TargetsList.UnionWith(Enemies.ToList().ConvertAll(t => new MyTargetDetected(t, CtrlBlock)));
            }
        }
        #endregion
        #region 私有变量
        private HashSet<IMyEntity> Enemies { get; } = new HashSet<IMyEntity>();
        public HashSet<IMyEntity> DetectedEntities { get; } = new HashSet<IMyEntity>();
        private HashSet<MyTargetDetected> TargetsList { get; } = new HashSet<MyTargetDetected>();
        public int EnemyCount => Enemies.Count;
        public double Range { get; private set; }
        #endregion
    }
}