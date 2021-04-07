using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Entity;
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
                var block = Utils.MyRadarSubtypeIdHelper.GetFarestDetectedBlock(grid);
                range = Utils.MyRadarSubtypeIdHelper.DetectedRangeBlock(block);
            }
            catch (Exception) { }
        }
        public MyTargetDetected GetTheMostThreateningTarget(IMyTerminalBlock RequstBlock)
        {
            if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList)) return null;
            var ent_tgs = TargetsList.AsParallel().Where(tg => tg.GetDistance(RequstBlock) < Range)?.ToList();
            if (Utils.Common.IsNullCollection(ent_tgs)) return null;
            if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)tg.Priority(RequstBlock));
            else if (TargetsList.Count == 1) return TargetsList.First();
            else return null;

        }
        public MyTargetDetected GetTheMostThreateningTarget(IMyTerminalBlock RequstBlock, Func<MyTargetDetected, bool> TargetFilter = null)
        {
            if (RequstBlock == null || Utils.Common.IsNullCollection(TargetsList)) return null;
            var ent_tgs = TargetsList.AsParallel().Where(tg => (tg.GetDistance(RequstBlock) < Range) && (TargetFilter?.Invoke(tg) ?? true))?.ToList();
            if (Utils.Common.IsNullCollection(ent_tgs)) return null;
            if (TargetsList.Count > 1) return TargetsList.ToList()?.MinBy(tg => (float)tg.Priority(RequstBlock));
            else if (TargetsList.Count == 1) return TargetsList.First();
            else return null;
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
            if (Utils.Common.NullEntity(CtrlBlock) || Range < 50) { _DetectedEntities = new ConcurrentBag<MyEntity>(); TargetsList = new ConcurrentBag<MyTargetDetected>(); return; }
            BoundingSphereD bounding = new BoundingSphereD(CtrlBlock.GetPosition(), Range);
            List<MyEntity> entities = new List<MyEntity>();
            MyGamePruningStructure.GetAllTopMostEntitiesInSphere(ref bounding, entities);
            if (Utils.Common.IsNullCollection(entities)) return;
            _DetectedEntities = new ConcurrentBag<MyEntity>(entities);
            var enm = entities.Where(t => Utils.MyTargetEnsureAPI.IsEnemy(t, CtrlBlock));
            if (Utils.Common.IsNullCollection(enm)) return;
            var Targets = enm.ToList()?.ConvertAll(t => new MyTargetDetected(t, CtrlBlock));
            if (Utils.Common.IsNullCollection(Targets)) return;
            TargetsList = new ConcurrentBag<MyTargetDetected>(Targets);
        }
        public void UpdateScanning(IMyEntity Me)
        {
            var grid = Me?.GetTopMostParent() as IMyCubeGrid;
            if (Utils.Common.NullEntity(grid)) return;
            var block = Utils.MyRadarSubtypeIdHelper.GetFarestDetectedBlock(grid);
            range = Utils.MyRadarSubtypeIdHelper.DetectedRangeBlock(block) * 1.5f;
            UpdateScanning(block);
        }
        public int EnemyCount => TargetsList?.Count ?? 0;
        public float Range => range;
        private volatile float range;
        private ConcurrentBag<MyTargetDetected> TargetsList;
        private ConcurrentBag<MyEntity> _DetectedEntities;

    }
}