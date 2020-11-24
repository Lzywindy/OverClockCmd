using Sandbox.ModAPI;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;
    public partial class BallisticComputer : ControllerManageBase
    {
        public float 炮口位置偏移 { get { return _炮口位置偏移; } set { _炮口位置偏移 = MathHelper.Clamp(value, -100, 100); } }
        public float 弹头速度 { get { return _predictionPosition.弹头速度; } set { _predictionPosition.弹头速度 = value; } }
        public float 瞄准微调 { get { return _predictionPosition.瞄准微调; } set { _predictionPosition.瞄准微调 = value; } }
        public double 锁定距离 { get { return _TargetManager.Range; } set { _TargetManager.Range = value; } }
        public bool 忽略自己的速度 { get { return _predictionPosition.忽略自己的速度; } set { _predictionPosition.忽略自己的速度 = value; } }
        public bool 忽略重力影响 { get { return _predictionPosition.忽略重力影响; } set { _predictionPosition.忽略重力影响 = value; } }
        public bool 是否是直瞄武器 { get { return _predictionPosition.是否是直瞄武器; } set { _predictionPosition.是否是直瞄武器 = value; } }
        public IMyBlockGroup CurrentWeapons { get; set; }
        public IMyCameraBlock TargetLocker { get; set; }
        public BallisticComputer(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            _predictionPosition.Me = refered_block;
            AppRunning1 += () => { _predictionPosition.GunFirePoint = 开火位置; _predictionPosition.TargetPosition = _TargetManager.TargetPosition; _predictionPosition.TargetVelocity = _TargetManager.TargetVelocity; };
            AppRunning10 += () => { _TargetManager.TargetLocker = TargetLocker; };
        }
        public void LockTarget()
        {
            _TargetManager.TriggerLockTarget();
        }
        public void UnlockTarget()
        {
            _TargetManager.ResetLockTarget();
        }
        private PredictionPosition _predictionPosition { get; } = new PredictionPosition();
        private TargetManager _TargetManager { get; } = new TargetManager();
        private Vector3? 开火位置
        {
            get
            {
                if (CurrentWeapons == null) return null;
                var Weapons = GetTs<IMyUserControllableGun>(CurrentWeapons);
                if (Weapons == null || Weapons.Count < 1) return null;
                Vector3 position = Vector3.Zero;
                foreach (var weapon in Weapons)
                    position += weapon.GetPosition() + 炮口位置偏移 * weapon.WorldMatrix.Forward;
                position /= Weapons.Count;
                return position;
            }
        }
        private float _炮口位置偏移 = 0;
    }
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
