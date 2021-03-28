using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
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
    public sealed class MyRadarTargets
    {
        public MyRadarTargets() : base() { }
        public void InitOrRestart(IMyEntity Me)
        {
            try
            {
                var grid = Me?.GetTopMostParent() as IMyCubeGrid;
                if (Utils.Common.NullEntity(grid)) return;
                var block = Utils.RadarSubtypeId.GetFarestDetectedBlock(grid);
                range = Utils.RadarSubtypeId.DetectedRangeBlock(block);
            }
            catch (Exception) { }
        }
        public MyTargetDetected GetTheMostThreateningTarget(IMyTerminalBlock RequstBlock, double MaxRange = 3000)
        {
            try
            {
                if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList)) return null;
                var ent_tgs = TargetsList.AsParallel().Where(tg => tg.GetDistance(RequstBlock) < MaxRange)?.ToList();
                if (Utils.Common.IsNullCollection(ent_tgs)) return null;
                if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)tg.Priority(RequstBlock));
                else return TargetsList.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }

        }
        public MyTargetDetected GetTheMostThreateningTarget(IMyTerminalBlock RequstBlock, double MaxRange = 3000, Func<MyTargetDetected, bool> TargetFilter = null)
        {
            try
            {
                if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList)) return null;
                var ent_tgs = TargetsList.AsParallel().Where(tg => (tg.GetDistance(RequstBlock) < MaxRange) && (TargetFilter?.Invoke(tg) ?? true))?.ToList();
                if (Utils.Common.IsNullCollection(ent_tgs)) return null;
                if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)tg.Priority(RequstBlock));
                else return TargetsList.FirstOrDefault();
            }
            catch (Exception)
            {
                return null;
            }
        }
        public List<Sandbox.ModAPI.Ingame.MyDetectedEntityInfo> GetDetectedEntities(IMyTerminalBlock RequstBlock)
        {
            try
            {
                if (Utils.Common.IsNullCollection(_DetectedEntities)) return null;
                return _DetectedEntities.ToList().ConvertAll(b => MyDetectedEntityInfoHelper.Create(MyEntities.GetEntityById(b.EntityId, false), RequstBlock.OwnerId, null));
            }
            catch (Exception)
            {
                return null;
            }
        }
        public void UpdateScanning(IMyTerminalBlock CtrlBlock)
        {
            try
            {
                if (Utils.Common.NullEntity(CtrlBlock) || Range < 50)
                {
                    _DetectedEntities = new ConcurrentBag<IMyEntity>();
                    TargetsList = new ConcurrentBag<MyTargetDetected>();
                    return;
                }
                BoundingSphereD bounding = new BoundingSphereD(CtrlBlock.GetPosition(), Range);
                List<IMyEntity> entities = MyAPIGateway.Entities.GetTopMostEntitiesInSphere(ref bounding);
                if (Utils.Common.IsNullCollection(entities)) return;
                _DetectedEntities = new ConcurrentBag<IMyEntity>(entities);
                var 敌人 = entities.Where(t => Utils.MyTargetEnsureAPI.IsEnemy(t, CtrlBlock));
                if (Utils.Common.IsNullCollection(敌人)) return;
                TargetsList = new ConcurrentBag<MyTargetDetected>(敌人.ToList().ConvertAll(t => new MyTargetDetected(t, CtrlBlock)));
            }
            catch (Exception) { }
        }
        public void UpdateScanning(IMyEntity Me)
        {
            try
            {
                var grid = Me?.GetTopMostParent() as IMyCubeGrid;
                if (Utils.Common.NullEntity(grid)) return;
                var block = Utils.RadarSubtypeId.GetFarestDetectedBlock(grid);
                range = Utils.RadarSubtypeId.DetectedRangeBlock(block);
                UpdateScanning(block);
            }
            catch (Exception) { }
        }
        public int EnemyCount => TargetsList?.Count ?? 0;
        public float Range => range;
        private volatile float range;
        private ConcurrentBag<MyTargetDetected> TargetsList;
        private ConcurrentBag<IMyEntity> _DetectedEntities;

    }
}