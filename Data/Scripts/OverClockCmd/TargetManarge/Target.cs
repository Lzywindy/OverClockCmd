using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRage;
using VRageMath;
using System.Linq;
using VRage.Game.Components;

namespace SuperBlocks.Controller
{
    public sealed class Target : UpdateableClass, IComparable<Target>, IEquatable<Target>, IComparer<Target>, IEqualityComparer<Target>
    {
        public Target(IMyEntity Entity, IMyTerminalBlock Detector) : base()
        {
            this.Entity = Entity;
            HashCode = Entity.GetHashCode();
            if (!IsGrid) return;
            InitDatas(Detector);
            ObjectSize = Grid.GridSize;
        }
        #region 可访问的状态函数变量
        public bool TargetSafety()
        {
            if (Grid == null) return Utils.NullEntity(Entity);
            return Blocks.Count < 1;
        }
        public readonly IMyEntity Entity;
        public bool IsGrid => Grid != null;
        public bool InvalidTarget => Utils.NullEntity(Entity);
        public double Priority(IMyTerminalBlock Detector)
        {
            if (InvalidTarget) return float.MaxValue;
            if (Entity?.Physics == null) return (Entity.GetPosition() - Detector.GetPosition()).LengthSquared();
            return (Entity.GetPosition() - Detector.GetPosition() + Entity.Physics.LinearVelocity * 0.05f + 0.5f * Entity.Physics.LinearAcceleration * 0.00025f).LengthSquared() - Blocks.Count * 200;
        }
        public MyTuple<Vector3D?, Vector3D?>? GetTarget_PV(IMyTerminalBlock Detector)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(Detector)) return null;
            if (!IsGrid) return new MyTuple<Vector3D?, Vector3D?>(Entity?.GetPosition(), Entity?.Physics?.LinearVelocity);
            if (Utils.NonTargetBlock(TargetBlock))
                Blocks.Remove(TargetBlock);
            if (Utils.IsNullCollection(Blocks))
                return null;
            if (Utils.NonTargetBlock(TargetBlock))
                TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            return new MyTuple<Vector3D?, Vector3D?>(TargetBlock?.GetPosition(), Entity?.Physics?.LinearVelocity);
        }
        public void GetTarget_PV(IMyTerminalBlock Detector, out Vector3D? Position, out Vector3D Velocity)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(Detector)) { Position = null; Velocity = Vector3D.Zero; return; }
            if (!IsGrid) { Position = Entity?.GetPosition(); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) Blocks.Remove(TargetBlock);
            if (Utils.IsNullCollection(Blocks)) { Position = null; Velocity = Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            Position = TargetBlock?.GetPosition(); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero;
        }
        public void GetTarget_PV(IMyTerminalBlock Detector, out Vector3D Position, out Vector3D Velocity)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(Detector)) { Position = default(Vector3D); Velocity = Vector3D.Zero; return; }
            if (!IsGrid) { Position = Entity?.GetPosition() ?? default(Vector3D); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) Blocks.Remove(TargetBlock);
            if (Utils.IsNullCollection(Blocks)) { Position = default(Vector3D); Velocity = Vector3D.Zero; return; }
            if (Utils.NonTargetBlock(TargetBlock)) TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            Position = TargetBlock?.GetPosition() ?? default(Vector3D); Velocity = Entity?.Physics?.LinearVelocity ?? Vector3D.Zero;
        }
        public static float Priority(IMyEntity target, IMyTerminalBlock Detector)
        {
            if (Utils.NullEntity(target)) return int.MaxValue;
            if (target?.Physics == null) return (float)(target.GetPosition() - Detector.GetPosition()).LengthSquared();
            var value = (target.GetPosition() - Detector.GetPosition() + target.Physics.LinearVelocity + 0.5f * target.Physics.LinearAcceleration).LengthSquared();
            if (!(target is IMyCubeGrid)) return (float)value;
            List<IMyTerminalBlock> Blocks = new List<IMyTerminalBlock>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(target as IMyCubeGrid)?.GetBlocksOfType(Blocks, WeaponFilter);
            return (float)(value - Blocks.Count * 200);
        }
        #endregion
        #region 更新接口
        public void InitDatas(IMyTerminalBlock Detector)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(Detector)) return;
            if (Grid == null) return;
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid)?.GetBlocksOfType(Blocks, WeaponFilter);
            if (Blocks.Count > 1)
                TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - Detector.GetPosition()).LengthSquared());
            else if (Blocks.Count == 1)
                TargetBlock = Blocks.First();
        }
        protected override void UpdateFunctions(IMyTerminalBlock CtrlBlock)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(CtrlBlock)) return;
            if (!IsGrid) return;
            if (updatecounts % 4 == 0)
            {
                GridTerminalSystem.GetBlocksOfType(Blocks, WeaponFilter);
                if (Blocks.Count > 1)
                    TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - CtrlBlock.GetPosition()).LengthSquared());
                else if (Blocks.Count == 1)
                    TargetBlock = Blocks.First();
            }
        }
        public void ForceUpdate(IMyTerminalBlock CtrlBlock)
        {
            if (Utils.NullEntity(Entity) || Utils.NullEntity(CtrlBlock)) return;
            if (Grid == null) return;
            GridTerminalSystem.GetBlocksOfType(Blocks, WeaponFilter);
            Blocks.RemoveAll(Utils.NonTargetBlock);
            if (Blocks.Count > 1)
                TargetBlock = Blocks.MinBy(b => (float)(b.GetPosition() - CtrlBlock.GetPosition()).LengthSquared());
            else if (Blocks.Count == 1)
                TargetBlock = Blocks.First();
        }
        public int FunctionalWeapons => Blocks.Count;
        public double ObjectSize { get; private set; }
        #endregion
        #region 接口函数
        public int CompareTo(Target other)
        {
            if (other.Entity == Entity)
                return 0;
            return 1;
        }
        public bool Equals(Target other)
        {
            return other.Entity == Entity;
        }
        public int Compare(Target x, Target y)
        {
            if (x.Entity == y.Entity)
                return 0;
            return 1;
        }
        public override int GetHashCode()
        {
            return HashCode;
        }
        public bool Equals(Target x, Target y)
        {
            return x.Entity == y.Entity;
        }
        public int GetHashCode(Target obj)
        {
            return obj.HashCode;
        }
        #endregion
        #region 私有变量函数
        private List<IMyTerminalBlock> Blocks { get; } = new List<IMyTerminalBlock>();
        private IMyCubeGrid Grid => Entity as IMyCubeGrid;
        private IMyGridTerminalSystem GridTerminalSystem => (IsGrid ? MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid) : null);
        private IMyTerminalBlock TargetBlock;
        private readonly int HashCode;
        public MyPhysicsComponentBase TargetPhysical { get { if (Utils.NullEntity(Entity)) return null; return Entity?.Physics; } }
        public Vector3D? Position { get { if (Utils.NullEntity(Entity)) return null; return TargetBlock?.GetPosition() ?? Entity.GetPosition(); } }
        private static bool WeaponFilter(IMyTerminalBlock block)
        {
            return (block is IMyUserControllableGun) && block.IsFunctional;
        }
        #endregion
    }
}