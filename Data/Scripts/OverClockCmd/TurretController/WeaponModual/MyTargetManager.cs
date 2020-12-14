using System;
using VRage.ModAPI;
using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage;
namespace SuperBlocks.Controller
{
    public struct MyTargetManager
    {
        public bool GPSMode { get { return _GPSMode; } private set { _GPSMode = value; if (_GPSMode) { Target = null; EntityBlockCache = null; } else { GPS = null; } } }
        private bool _GPSMode;
        private Vector3D? GPS;
        public long? TargetID => Target?.EntityId;
        private IMyEntity Target;
        private List<IMyTerminalBlock> EntityBlockCache;
        private int index;
        private Vector3D? EntTargetPosition => Target?.GetPosition();
        public double Distance(Vector3D? Current)
        {
            Vector3D? TPos = _GPSMode ? GPS : EntTargetPosition;
            if (TPos == null || Current == null) return -1;
            return Vector3D.Distance(TPos.Value, Current.Value);
        }
        public bool IsNotNullTarget(IMyTerminalBlock block, Func<IMyTerminalBlock, bool> AcceptBlockFilter = null)
        {
            if (GPSMode) return GPS.HasValue;
            if (Target == null) return false;
            if (Target.Closed || Target.MarkedForClose) { Target = null; EntityBlockCache = null; return false; }
            if (!(Target is IMyCubeGrid)) return true;
            var grid = Target as IMyCubeGrid; if (grid == null) return false;
            if (EntityBlockCache == null) EntityBlockCache = Utils.GetTheNearlyBlock(grid, block, AcceptBlockFilter);
            Utils.UpdateBlocks(ref EntityBlockCache, block, AcceptBlockFilter); UpdateIndex();
            return EntityBlockCache != null;
        }
        public void SetITarget(long EntityID, Func<IMyEntity, bool> AcceptEntity = null)
        {
            if (EntityID < 0) { Target = null; EntityBlockCache = null; return; }
            var target = MyAPIGateway.Entities.GetEntityById(EntityID);
            if (!(AcceptEntity?.Invoke(target) ?? true)) return;
            Target = target;
            GPSMode = false;
        }
        public long GetITarget()
        {
            if (!TargetID.HasValue) return -1;
            return TargetID.Value;
        }
        public void SetGTarget(Vector3D? GPS)
        {
            this.GPS = GPS;
            GPSMode = true;
        }
        public Vector3D? GetGTarget()
        {
            return GPS;
        }
        public MyTuple<Vector3D?, Vector3D?> GetTargetPV()
        {
            if (GPSMode) return new MyTuple<Vector3D?, Vector3D?>(GPS, null);
            if (Target == null) return new MyTuple<Vector3D?, Vector3D?>(null, null);
            if (EntityBlockCache == null) return new MyTuple<Vector3D?, Vector3D?>(Target.GetPosition(), Target?.Physics?.LinearVelocity);
            return new MyTuple<Vector3D?, Vector3D?>(EntityBlockCache[index].GetPosition(), Target?.Physics?.LinearVelocity);
        }
        public void CycleSubpart()
        {
            if (EntityBlockCache == null || EntityBlockCache.Count < 1) { index = -1; return; }
            if (EntityBlockCache.Count < 2) { index = 0; return; }
            index = Math.Max(index++, 0) % EntityBlockCache.Count;
        }
        private void UpdateIndex()
        {
            if (EntityBlockCache == null || EntityBlockCache.Count < 1) { index = -1; return; }
            if (EntityBlockCache.Count < 2) { index = 0; return; }
            index = Math.Max(index, 0) % EntityBlockCache.Count;
        }
    }
}