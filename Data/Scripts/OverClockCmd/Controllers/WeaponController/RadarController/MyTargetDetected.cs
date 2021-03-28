using Sandbox.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks.Controller
{
    public sealed class MyTargetDetected : UpdateableClass, IComparable<MyTargetDetected>, IEquatable<MyTargetDetected>, IComparer<MyTargetDetected>, IEqualityComparer<MyTargetDetected>
    {
        public MyTargetDetected(IMyEntity Entity, IMyTerminalBlock Detector, bool IgnoreFilter = false) : base()
        {
            this.Entity = Entity;
            HashCode = Entity?.GetHashCode() ?? -1;
            this.IgnoreFilter = IgnoreFilter;
            if (!IsGrid) return;
            InitDatas(Detector);
            ObjectSize = Grid?.GridSize ?? -1;
        }
        #region 可访问的状态函数变量
        public bool TargetSafety()
        {
            if (Grid == null) return Utils.Common.NullEntity(Entity);
            return Utils.Common.IsNullCollection(Blocks);
        }
        public readonly IMyEntity Entity;
        public bool IsGrid => Grid != null;
        public bool InvalidTarget => Utils.Common.NullEntity(Entity);
        public double Priority(IMyTerminalBlock Detector)
        {
            if (InvalidTarget || Detector == null) return float.MaxValue;
            var vector = Entity.GetPosition() - Detector.GetPosition();
            var direction = vector.LengthSquared() == 0 ? Vector3D.Normalize(vector) : Vector3D.Zero;
            if (Entity?.Physics == null) return vector.LengthSquared();
            var tend = vector.LengthSquared();// (vector + (Entity.Physics.LinearVelocity * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS + 0.5f * Entity.Physics.LinearAcceleration * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS)).Dot(direction);
            if (!IsGrid) return tend;
            if (Utils.Common.IsNullCollection(Blocks)) return float.MaxValue;
            return tend;
        }
        public bool GetTarget_PV(IMyTerminalBlock Detector, out Vector3D Position, out Vector3 Velocity, out Vector3 Acc)
        {
            Position = Vector3D.Zero; Velocity = Vector3.Zero; Acc = Vector3.Zero;
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(Detector)) return false;
            var position = GetEntityPosition(Detector);
            if (position == null) return false;
            Position = position.Value;
            Velocity = Entity?.Physics?.LinearVelocity ?? Vector3.Zero;
            Acc = Entity?.Physics?.LinearAcceleration ?? Vector3.Zero;
            return true;
        }
        public double GetDistance(IMyTerminalBlock Detector)
        {
            if (InvalidTarget || Detector == null) return float.MaxValue;
            var position = GetEntityPosition(Detector);
            if (position == null) return float.MaxValue;
            return Vector3D.Distance(Detector.GetPosition(), position.Value);
        }
        public Vector3D? GetEntityPosition(IMyTerminalBlock Detector)
        {
            if (Detector == null || InvalidTarget) return null;
            if (!IsGrid) return Entity?.GetPosition();
            if (Utils.Common.IsNullCollection(Blocks)) return null;
            return Blocks.AsParallel().MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared())?.GetPosition();
            //if (Vector3D.Distance(Entity.GetPosition(), Detector.GetPosition()) > 3000)
            //    return new Vector3D(Blocks.Average(b => b.GetPosition().X), Blocks.Average(b => b.GetPosition().Y), Blocks.Average(b => b.GetPosition().Z));
            //else
            //    return Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared())?.GetPosition();
        }
        #endregion
        #region 更新接口
        public void InitDatas(IMyTerminalBlock Detector)
        {
            UpdateFunctions(Detector);
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(CtrlBlock)) return;
            if (!IsGrid) return;
            List<IMyTerminalBlock> _Blocks = new List<IMyTerminalBlock>();
            if (IgnoreFilter)
                GridTerminalSystem?.GetBlocksOfType(_Blocks);
            else
                GridTerminalSystem?.GetBlocksOfType(_Blocks, WeaponFilter);
            Blocks = new ConcurrentBag<IMyTerminalBlock>(_Blocks);
        }
        public double ObjectSize { get; private set; }
        #endregion
        #region 接口函数
        public int CompareTo(MyTargetDetected other)
        {
            if (other.Entity == Entity)
                return 0;
            return 1;
        }
        public bool Equals(MyTargetDetected other)
        {
            return other.Entity == Entity;
        }
        public int Compare(MyTargetDetected x, MyTargetDetected y)
        {
            if (x.Entity == y.Entity)
                return 0;
            return 1;
        }
        public override int GetHashCode()
        {
            return HashCode;
        }
        public bool Equals(MyTargetDetected x, MyTargetDetected y)
        {
            return x.Entity == y.Entity;
        }
        public int GetHashCode(MyTargetDetected obj)
        {
            return obj.HashCode;
        }
        #endregion
        #region 私有变量函数
        private readonly bool IgnoreFilter;
        private ConcurrentBag<IMyTerminalBlock> Blocks;
        //private List<IMyTerminalBlock> Blocks { get; } = new List<IMyTerminalBlock>();
        private IMyCubeGrid Grid => Entity as IMyCubeGrid;
        private IMyGridTerminalSystem GridTerminalSystem => (IsGrid ? MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid) : null);
        private readonly int HashCode;
        public MyPhysicsComponentBase TargetPhysical { get { if (Utils.Common.NullEntity(Entity)) return null; return Entity?.Physics; } }
        private static bool WeaponFilter(IMyTerminalBlock block) => (BasicInfoService.WcApi.HasCoreWeapon(block) || (block is IMyUserControllableGun));
        #endregion
    }
}