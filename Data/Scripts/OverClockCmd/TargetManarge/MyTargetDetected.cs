using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRage;
using VRageMath;
using System.Linq;
using VRage.Game.Components;
using VRage.Game;

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
            return Blocks.Count < 1;
        }
        public readonly IMyEntity Entity;
        public bool IsGrid => Grid != null;
        public bool InvalidTarget => Utils.Common.NullEntity(Entity);
        public double Priority(IMyTerminalBlock Detector)
        {
            if (InvalidTarget) return float.MaxValue;
            var vector = Entity.GetPosition() - Detector.GetPosition();
            var direction = vector.LengthSquared() == 0 ? Vector3D.Normalize(vector) : Vector3D.Zero;
            if (Entity?.Physics == null) return vector.LengthSquared();
            var tend = (vector + (Entity.Physics.LinearVelocity * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS + 0.5f * Entity.Physics.LinearAcceleration * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS * MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS) * 40).Dot(direction);
            return tend - Blocks.Count * 20;
        }
        public MyTuple<Vector3D?, Vector3D?>? GetTarget_PV(IMyTerminalBlock Detector)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(Detector)) return null;
            if (!IsGrid) return new MyTuple<Vector3D?, Vector3D?>(Entity?.GetPosition(), Entity?.Physics?.LinearVelocity);
            if (Utils.NonTargetBlock(TargetBlock))
                Blocks.Remove(TargetBlock);
            if (Utils.Common.IsNullCollection(Blocks))
                return null;
            if (Utils.NonTargetBlock(TargetBlock))
            {
                if (Blocks.Count > 1)
                    TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
                else if (Blocks.Count == 1)
                    TargetBlock = Blocks.First();
                else TargetBlock = null;
            }
            return new MyTuple<Vector3D?, Vector3D?>(TargetBlock?.GetPosition(), Entity?.Physics?.LinearVelocity);
        }
        public void GetTarget_PV(IMyTerminalBlock Detector, out Vector3D? Position, out Vector3D Velocity)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(Detector)) { Position = null; Velocity = Vector3D.Zero; return; }
            if (!IsGrid) { Position = Entity?.GetPosition(); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) Blocks.Remove(TargetBlock);
            if (Utils.Common.IsNullCollection(Blocks)) { Position = null; Velocity = Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            Position = TargetBlock?.GetPosition(); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero;
        }
        public bool GetTarget_PV(IMyTerminalBlock Detector, out Vector3D Position, out Vector3D Velocity)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(Detector)) { Position = default(Vector3D); Velocity = Vector3D.Zero; return false; }
            if (!IsGrid) { Position = Entity?.GetPosition() ?? default(Vector3D); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero; return false; }
            if (Utils.NonTargetBlock(TargetBlock)) Blocks.Remove(TargetBlock);
            if (Utils.Common.IsNullCollection(Blocks)) { Position = default(Vector3D); Velocity = Vector3D.Zero; return false; }
            if (Utils.NonTargetBlock(TargetBlock))
            {
                if (Blocks.Count > 1)
                    TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
                else if (Blocks.Count == 1)
                    TargetBlock = Blocks.First();
                else TargetBlock = null;
            }
            Position = TargetBlock?.GetPosition() ?? default(Vector3D); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero;
            return TargetBlock != null;
        }
        public static float Priority(IMyEntity target, IMyTerminalBlock Detector, bool IgnoreFilter = false)
        {
            if (Utils.Common.NullEntity(target)) return int.MaxValue;
            if (target?.Physics == null) return (float)(target.GetPosition() - Detector.GetPosition()).LengthSquared();
            var value = (target.GetPosition() - Detector.GetPosition() + target.Physics.LinearVelocity + 0.5f * target.Physics.LinearAcceleration).LengthSquared();
            if (!(target is IMyCubeGrid)) return (float)value;
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            if (IgnoreFilter)
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(target as IMyCubeGrid)?.GetBlocksOfType(Blocks);
            else
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(target as IMyCubeGrid)?.GetBlocksOfType(Blocks, WeaponFilter);
            return (float)(value - Blocks.Count * 200);
        }
        #endregion
        #region 更新接口
        public void InitDatas(IMyTerminalBlock Detector)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(Detector) || Utils.Common.IsNull(Grid)) return;
            if (IgnoreFilter)
                GridTerminalSystem?.GetBlocksOfType(Blocks);
            else
                GridTerminalSystem?.GetBlocksOfType(Blocks, WeaponFilter);
            if (Blocks.Count > 1)
                TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            else if (Blocks.Count == 1)
                TargetBlock = Blocks.First();
            else
                TargetBlock = null;
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(CtrlBlock)) return;
            if (!IsGrid) return;
            if (updatecounts % 4 == 0)
            {
                if (IgnoreFilter)
                    GridTerminalSystem?.GetBlocksOfType(Blocks);
                else
                    GridTerminalSystem?.GetBlocksOfType(Blocks, WeaponFilter);
                if (Blocks.Count > 1)
                    TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - CtrlBlock.GetPosition()).LengthSquared());
                else if (Blocks.Count == 1)
                    TargetBlock = Blocks.First();
                else
                    TargetBlock = null;
            }
        }
        public void ForceUpdate(IMyTerminalBlock CtrlBlock)
        {
            if (Utils.Common.NullEntity(Entity) || Utils.Common.NullEntity(CtrlBlock)) return;
            if (Grid == null) return;
            if (IgnoreFilter)
                GridTerminalSystem?.GetBlocksOfType(Blocks);
            else
                GridTerminalSystem?.GetBlocksOfType(Blocks, WeaponFilter);
            Blocks.RemoveAll(Utils.NonTargetBlock);
            if (Blocks.Count > 1)
                TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - CtrlBlock.GetPosition()).LengthSquared());
            else if (Blocks.Count == 1)
                TargetBlock = Blocks.First();
            else
                TargetBlock = null;
        }
        public int FunctionalWeapons => Blocks.Count;
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
        private List<IMyTerminalBlock> Blocks { get; } = new List<IMyTerminalBlock>();
        private IMyCubeGrid Grid => Entity as IMyCubeGrid;
        private IMyGridTerminalSystem GridTerminalSystem => (IsGrid ? MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid) : null);
        private IMyTerminalBlock TargetBlock;
        private readonly int HashCode;
        public MyPhysicsComponentBase TargetPhysical { get { if (Utils.Common.NullEntity(Entity)) return null; return Entity?.Physics; } }
        public Vector3D? Position { get { if (Utils.Common.NullEntity(Entity)) return null; return TargetBlock?.GetPosition() ?? Entity.GetPosition(); } }
        private static bool WeaponFilter(IMyTerminalBlock block) => (BasicInfoService.WcApi.HasCoreWeapon(block) || (block is IMyUserControllableGun));
        #endregion
    }
}