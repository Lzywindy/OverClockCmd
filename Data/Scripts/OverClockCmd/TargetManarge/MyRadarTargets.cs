using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using System.Linq;
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
        public MyTargetDetected 得的最近向我靠近最快的目标(IMyTerminalBlock RequstBlock)
        {
            if (Utils.Common.IsNull(RequstBlock) || Utils.Common.IsNullCollection(TargetsList) || Range < 10) return null;
            if (TargetsList.Count > 1) return TargetsList.ToList().MinBy(tg => (float)tg.Priority(RequstBlock));
            else if (TargetsList.Count == 1) return TargetsList.FirstElement();
            else return null;
        }
        public MyTargetDetected 得的最近向我靠近最快的目标(IMyTerminalBlock RequstBlock, Func<IMyEntity, bool> Filter = null, Func<IMyEntity, double> FilterPriority = null)
        {
            if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList) || Range < 10) return null;
            var ent_tgs = TargetsList.Where(tg => Filter?.Invoke(tg.Entity) ?? true)?.ToList();
            if (Utils.Common.IsNullCollection(ent_tgs)) return null;
            if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)(tg.Priority(RequstBlock) - (FilterPriority?.Invoke(tg?.Entity) ?? 0)));
            else if (TargetsList.Count == 1) return TargetsList.FirstElement();
            else return null;
        }
        #endregion
        #region 外部可调用属性
        public MyTargetDetected 当前目标 { get; private set; }
        #endregion
        #region 更新函数       
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (updatecounts % 21 == 0)
            {
                TargetsList.RemoveWhere(t => t.InvalidTarget || t.TargetSafety());
                foreach (var item in TargetsList) { item.Update(CtrlBlock); }
                //MyAPIGateway.Utilities.ShowNotification($"Update21:{true}");
            }
            if (updatecounts % 11 == 0)
            {
                BoundingSphereD bounding = new BoundingSphereD(CtrlBlock.GetPosition(), Range);
                DetectedEntities.Clear();
                List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounding);
                if (Utils.Common.IsNullCollection(entities)) return;
                DetectedEntities.UnionWith(entities);
                List<IMyEntity> 敌人 = entities.Where(t => 可能敌对的目标(t, CtrlBlock))?.ToList();
                if (Utils.Common.IsNullCollection(敌人)) { 当前目标 = null; return; }
                Enemies.RemoveWhere(Utils.Common.NullEntity);
                Enemies.UnionWith(敌人);
                if (Utils.Common.IsNullCollection(Enemies)) { 当前目标 = null; return; }
                TargetsList.UnionWith(Enemies.ToList().ConvertAll(t => new MyTargetDetected(t, CtrlBlock)));
                if (TargetsList.Count > 1) 当前目标 = TargetsList.ToList()?.MinBy(tg => (float)tg.Priority(CtrlBlock));
                else if (TargetsList.Count == 1) 当前目标 = TargetsList.FirstElement();
                else 当前目标 = null;
                //MyAPIGateway.Utilities.ShowNotification($"Update11:{true}");
            }
            if (updatecounts % 4 == 0) 当前目标?.Update(CtrlBlock);
        }
        #endregion
        #region 私有函数
        private bool 可能敌对的目标(IMyEntity target, IMyTerminalBlock DetectorProcess)
        {
            if (Utils.Common.NullEntity(target)) return false;
            return 目标敌对(target, DetectorProcess);
        }
        private bool 目标敌对(IMyEntity Ent, IMyTerminalBlock DetectorProcess)
        {
            if (Ent == null || DetectorProcess == null) return false;
            if (Ent is IMyMeteor || Utils.MyTargetEnsureAPI.是否是导弹(Ent)) return 目标朝我靠近(Ent, DetectorProcess);
            if (Ent is IMyCubeGrid)
            {
                var grid = Ent as IMyCubeGrid;
                if (grid == null) return false;
                bool HasHostileBlock = false;
                foreach (var item in grid.BigOwners)
                {
                    switch (DetectorProcess.GetUserRelationToOwner(item))
                    {
                        case VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies:
                            HasHostileBlock = HasHostileBlock || true;
                            break;
                        default:
                            break;
                    }
                }
                return HasHostileBlock && 网络通电(grid, DetectorProcess);
            }
            return false;
        }
        private bool 网络通电(IMyCubeGrid Grid, IMyTerminalBlock DetectorProcess)
        {
            var blocks = Utils.Common.GetTs<IMyFunctionalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid), 供能方块);
            return (blocks?.Any(b => b.Enabled && b.IsFunctional) ?? false);
        }
        private bool 供能方块(IMyFunctionalBlock block)
        {
            if (block is IMyBatteryBlock || block is IMyReactor) return true;
            var str = block.GetType().ToString();
            if (str.Contains("HydrogenEngine") || str.Contains("WindTurbine") || str.Contains("SolarPanel")) return true;
            return false;
        }
        private bool 目标朝我靠近(IMyEntity Ent, IMyTerminalBlock DetectorProcess)
        {
            if (Ent?.Physics == null) return false;
            return Vector3D.Dot(Vector3D.Normalize((DetectorProcess.GetPosition() - Ent.GetPosition())), Ent.Physics.LinearVelocity) > 0.865;
        }
        private bool TargetInRange(MyTargetDetected target, IMyTerminalBlock DetectorProcess)
        {
            if (target == null || target.InvalidTarget || Range < 10 || !DetectorProcess.IsFunctional) return false;
            return (target.Entity.GetPosition() - DetectorProcess.GetPosition()).Length() <= Range;
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