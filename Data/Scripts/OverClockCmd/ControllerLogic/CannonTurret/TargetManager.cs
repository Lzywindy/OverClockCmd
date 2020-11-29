using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    public sealed class TargetManager
    {
        public IMyCameraBlock TargetLocker { get; set; }
        public IMyEntity Target { get; private set; }
        public double Range { get { return _Range; } set { _Range = MathHelper.Clamp(value, 1e3, 1e6); } }
        public void TriggerLockTarget()
        {
            Target = null;
            RelativePosition = Vector3.Zero;
            mode = 0;
            if (TargetLocker == null) return;
            VRage.Game.ModAPI.IHitInfo target;
            bool HasEntity = MyAPIGateway.Physics.CastLongRay(TargetLocker.GetPosition() + TargetLocker.WorldMatrix.Forward * 50, TargetLocker.GetPosition() + TargetLocker.WorldMatrix.Forward * Range, out target, true);
            if (!HasEntity || target == null) return;
            Target = target.HitEntity;
            RelativePosition = Vector3.Transform(target.Position, Matrix.Transpose(Target.WorldMatrix));
        }
        public void ResetLockTarget()
        {
            Target = null;
            RelativePosition = Vector3.Zero;
        }
        public void SetTarget(IMyEntity Target)
        {
            if (Target is IMyVoxelBase) return;
            if (Target == null || Vector3D.Distance(Target.GetPosition(), TargetLocker.GetPosition()) > Range) { mode = 0; this.Target = null; RelativePosition = Vector3.Zero; return; }
            if (Target is IMyCubeGrid)
            {
                mode = 0;
                var temp = Target as IMyCubeGrid;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(temp).GetBlocks(blocks);
                this.Target = blocks[random.Next(0, blocks.Count)];
                RelativePosition = Vector3.Zero;
                return;
            }
            else
            {
                mode = 0;
                this.Target = Target;
                RelativePosition = Vector3.Zero;
                return;
            }
        }
        public void SetGPS(Vector3D? Position)
        {
            if (!Position.HasValue || Vector3D.Distance(Target.GetPosition(), TargetLocker.GetPosition()) > Range)
            { mode = 0; RelativePosition = Vector3.Zero; return; }
            mode = 1;
            RelativePosition = Position.Value;
        }
        public Vector3? TargetPosition
        {
            get
            {
                switch (mode)
                {
                    case 1:
                        if (Vector3D.Distance(Target.GetPosition(), TargetLocker.GetPosition()) > Range)
                            return null;
                        return RelativePosition;
                    default:
                        if (Target == null)
                            return null;
                        return Vector3.Transform(RelativePosition, Target.WorldMatrix);
                }
            }
        }
        public Vector3? TargetVelocity
        {
            get
            {
                if (Target == null) return null;
                if (Target is IMyCubeGrid)
                {
                    if (Target.Physics == null)
                        return Vector3.Zero;
                    return Target.Physics.LinearVelocity;
                }
                else if (Target is IMyTerminalBlock)
                {
                    var target = (Target as IMyTerminalBlock);
                    if (target.CubeGrid.Physics == null)
                        return Vector3.Zero;
                    return target.CubeGrid.Physics.LinearVelocity;
                }
                else
                {
                    if (Target is IMyVoxelBase)
                        return Vector3.Zero;
                    if (Target.Physics == null)
                        return Vector3.Zero;
                    return Target.Physics.LinearVelocity;
                }
            }
        }
        private Vector3 RelativePosition = Vector3.Zero;
        private double _Range = 2e3;
        private int mode = 0;//0为实体，1为GPS
        private Random random { get; } = new Random(0x15834562);
    }
}
