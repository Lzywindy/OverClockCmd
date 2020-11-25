using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    public class TargetManager
    {
        public IMyCameraBlock TargetLocker { get; set; }
        public IMyEntity Target { get; private set; }
        public double Range { get { return _Range; } set { _Range = MathHelper.Clamp(value, 1e3, 1e6); } }
        public void TriggerLockTarget()
        {
            Target = null;
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
        public Vector3? TargetPosition { get { if (Target == null) return null; return Vector3.Transform(RelativePosition, Target.WorldMatrix); } }
        public Vector3? TargetVelocity
        {
            get
            {
                if (Target == null) return null;
                if (Target is IMyTerminalBlock)
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
    }
}
