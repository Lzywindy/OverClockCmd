using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    public partial class CannonTurret : ControllerManageBase
    {
        public CannonTurret(IMyTerminalBlock refered_block) : base(refered_block) { }

        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
        }
    }
    public partial class CannonTurret
    {
        private const string 瞄准镜组ID = @"TargetFinder";
        private const string 炮塔组ID = @"TurretGroup";
        private IMyEntity Target;
        private IMyTerminalBlock TargetLocker;
        private Vector3 RelativePosition;
        private double _Range = 2e3;
        public double Range { get { return _Range; } set { _Range = MathHelper.Clamp(value, 1e3, 1e6); } }
        public void TriggerLockTarget()
        {
            Target = null;
            if (TargetLocker == null) return;
            VRage.Game.ModAPI.IHitInfo target;
            bool HasEntity = MyAPIGateway.Physics.CastLongRay(TargetLocker.GetPosition(), TargetLocker.GetPosition() + TargetLocker.WorldMatrix.Forward * Range, out target, true);
            if (!HasEntity || target == null) return;
            if (target.HitEntity is IMyVoxelBase) return;
            Target = target.HitEntity;
            RelativePosition = Vector3.TransformNormal(target.Position - Target.GetPosition(), Matrix.Transpose(Target.WorldMatrix));
        }
        public void ResetLockTarget()
        {
            Target = null;
            RelativePosition = Vector3.Zero;
        }
    }
}
