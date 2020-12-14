using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks
{
    using static Utils;
    public sealed partial class BallisticComputer : ControllerManageBase, ICombot
    {
        public int 牛顿迭代次数 { get { return _PredictionPosition.牛顿迭代次数; } set { _PredictionPosition.牛顿迭代次数 = value; } }
        public float 炮口位置偏移 { get { return _炮口位置偏移; } set { _炮口位置偏移 = MathHelper.Clamp(value, -100, 100); } }
        public float 弹头速度 { get { return _PredictionPosition.弹头速度; } set { _PredictionPosition.弹头速度 = value; } }
        public float 瞄准微调 { get { return _PredictionPosition.瞄准微调; } set { _PredictionPosition.瞄准微调 = value; } }
        public double 锁定距离 { get { return _TargetManager.Range; } set { _TargetManager.Range = value; } }
        public bool 忽略自己的速度 { get { return _PredictionPosition.忽略自己的速度; } set { _PredictionPosition.忽略自己的速度 = value; } }
        public bool 忽略重力影响 { get { return _PredictionPosition.忽略重力影响; } set { _PredictionPosition.忽略重力影响 = value; } }
        public bool 是否是直瞄武器 { get { return _PredictionPosition.是否是直瞄武器; } set { _PredictionPosition.是否是直瞄武器 = value; } }
        public IMyBlockGroup CurrentWeapons { get; set; }
        public IMyCameraBlock TargetLocker { get; private set; }
        public BallisticComputer(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            _PredictionPosition.Me = refered_block;
            AppRunning1 += () =>
            {
                _PredictionPosition.GunFirePoint = 开火位置;
                _PredictionPosition.TargetPosition = _TargetManager.TargetPosition;
                _PredictionPosition.TargetVelocity = _TargetManager.TargetVelocity;
            };
            AppRunning10 += () =>
            {
                _TargetManager.TargetLocker = TargetLocker;
            };
        }
        public Vector3? 目标炮口方向 { get { return _PredictionPosition.炮口方向; } set { } }
        public Vector3? 当前炮口指向
        {
            get
            {
                if (CurrentWeapons == null) return null;
                var Weapons = GetTs<IMyUserControllableGun>(CurrentWeapons);
                if (Weapons == null || Weapons.Count < 1) return null;
                Vector3 vector = Vector3.Zero;
                foreach (var weapon in Weapons)
                    vector += weapon.WorldMatrix.Forward;
                if (vector == Vector3.Zero)
                    return null;
                vector.Normalize();
                return vector;
            }
            set { }
        }
        public void CycleGroup()
        {
            var current_config = _ConfigManager.CycleConfig();
            if (current_config == null || current_config.Count < 1) return;
            foreach (var configitem in current_config)
            {
                switch (configitem.Key)
                {
                    case "name":
                        CurrentWeapons = 找到包含了这个方块的组(configitem.Value);
                        break;
                    case "speed":
                        float speed;
                        if (float.TryParse(configitem.Value, out speed))
                            弹头速度 = speed;
                        break;
                    case "gravity":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            忽略重力影响 = false;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            忽略重力影响 = true;
                        break;
                    case "ignore_speed_self":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            忽略自己的速度 = true;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            忽略自己的速度 = false;
                        break;
                    case "direct":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            是否是直瞄武器 = true;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            是否是直瞄武器 = false;
                        break;
                    case "offset":
                        float offset;
                        if (float.TryParse(configitem.Value, out offset))
                            炮口位置偏移 = offset;
                        break;
                    case "delta_t":
                        float delta_t;
                        if (float.TryParse(configitem.Value, out delta_t))
                            瞄准微调 = delta_t;
                        break;
                    case "calc_t":
                        int calc_t;
                        if (int.TryParse(configitem.Value, out calc_t))
                            牛顿迭代次数 = calc_t;
                        break;
                    case "distance":
                        double distance;
                        if (double.TryParse(configitem.Value, out distance))
                            锁定距离 = distance;
                        break;
                    case "camera":
                        if (CurrentWeapons == null) break;
                        TargetLocker = GetT(CurrentWeapons, (IMyCameraBlock camera) => camera.CustomName == configitem.Value);
                        break;
                    default:
                        break;
                }
            }
        }
        public void FireWeapon()
        {
            if (CurrentWeapons == null) return;
            var Weapons = GetTs<IMyUserControllableGun>(CurrentWeapons);
            if (Weapons == null || Weapons.Count < 1) return;
            foreach (var Weapon in Weapons)
            {
                var action = Weapon.GetActionWithName("Shoot");
                if (action == null) continue;
                action.Apply(Weapon);
            }
        }
        public void LockTarget()
        {
            _TargetManager.TriggerLockTarget();
        }
        public void UnlockTarget()
        {
            _TargetManager.ResetLockTarget();
        }
        public string Configs { get { return configs; } set { configs = value; _ConfigManager.ReadConfigs(configs); } }
    }
    public sealed partial class BallisticComputer
    {
        private PredictionPosition _PredictionPosition { get; } = new PredictionPosition();
        private TargetManager _TargetManager { get; } = new TargetManager();
        private ConfigManager _ConfigManager { get; } = new ConfigManager();
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
        private string configs;
        private IMyTerminalBlock _ReferedControlBlock;
        private IMyBlockGroup 找到包含了这个方块的组(string name)
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);
            if (!groups.Exists((IMyBlockGroup group) => (是否包含了这个方块的组(group) && (group.Name == name))))
                return null;
            return groups.Find((IMyBlockGroup group) => (是否包含了这个方块的组(group) && (group.Name == name)));
        }
        private bool 是否包含了这个方块的组(IMyBlockGroup group)
        {
            if (group == null) return false;
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks);
            if (blocks.Count < 1) return false;
            return blocks.Contains(Me);
        }
    }
}
