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
        float Ev_max = 30;
        float Az_max = 30;
        float Length = 3;
        Program()
        {
            pbApi.Activate(Me);
            try
            {
                InitParameters();
                ProcessCustomData();
            }
            catch (Exception) { }
            finally
            {
                Runtime.UpdateFrequency = UpdateFrequency.Update1 | UpdateFrequency.Update100;
            }
        }
        void Main(string argument, UpdateType updateSource)
        {
            try
            {
                if (updateSource.HasFlag(UpdateType.Update1))
                {
                    if (InitComplete)
                    {
                        GetAmmoSpeed_Fake();
                        var target = pbApi.GetAiFocus(Turret.Weapon.EntityId, 0);
                        Turret.Running(target, V_project_length, _Cannon_Offset, Az_max, Ev_max, Length);
                        if (target.HasValue && !target.Value.IsEmpty() && Turret.Weapons != null && Turret.Weapons.Count >= 1)
                        {
                            foreach (var weapon in Turret.Weapons)
                            {
                                var weaponid = GetWeaponID(weapon);
                                pbApi.SetWeaponTarget(weapon, target.Value.EntityId, weaponid);
                            }
                        }
                    }
                    else
                    {
                        InitParameters();
                        ProcessCustomData();
                    }
                    Echo($"Projector Velocity:{V_project_length}");
                }
                if (updateSource.HasFlag(UpdateType.Update100))
                {
                    ProcessCustomData();
                }
                if (updateSource.HasFlag(UpdateType.Trigger) || updateSource.HasFlag(UpdateType.Terminal) || updateSource.HasFlag(UpdateType.Script))
                {
                    if (argument.Contains("readcfg")) { ProcessCustomData(); }
                }
            }
            catch (Exception)
            {
                Turret = null;
            }
        }
        private void InitParameters()
        {
            if (InitComplete) return;
            var turret_group = GetGroup_BlockInThisGroup();
            Turret = TurretManage.CreateTurretController(Me, GridTerminalSystem);
        }
        bool InitComplete => Turret != null;
        private void GetAmmoSpeed_Fake()
        {
            var ammo = pbApi.GetActiveAmmo(Turret.Weapon, GetWeaponID(Turret.Weapon));
            Echo($"Ammo Type:{ammo}");
            if (!AmmoSpeed.TryGetValue(ammo, out V_project_length))
                V_project_length = DV_project_length;
        }
        private int GetWeaponID(IMyUserControllableGun weapon_block)
        {
            if (weapon_block == null) return 0;
            Dictionary<string, int> weapons = new Dictionary<string, int>();
            pbApi.GetBlockWeaponMap(weapon_block, weapons);
            foreach (var weapon in weapons)
                return weapon.Value;
            return 0;
        }
        private float _Cannon_Offset = 0;
        private float V_project_length;
        private float DV_project_length;
        public void ProcessCustomData()
        {
            var linesarray = Me.CustomData.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (linesarray == null || linesarray.Length < 1) return;
            foreach (var line in linesarray)
            {
                var config = SolveLine(line);
                if (config.ContainsKey("name"))
                {
                    switch (config["name"])
                    {
                        case "setup": GetSetupConfig(config); break;
                        case "ammo": GetAmmoConfig(config); break;
                        default: break;
                    }
                }
            }
        }
        private void GetAmmoConfig(Dictionary<string, string> Config)
        {
            if (Config == null || Config.Count < 1 || Turret == null) return;
            AmmoSpeed.Clear();
            foreach (var configitem in Config)
            {
                if (configitem.Key == "ammo") continue;
                float speed;
                if (float.TryParse(configitem.Value, out speed))
                    AmmoSpeed.Add(configitem.Key, speed);
                else continue;
            }

        }
        private void GetSetupConfig(Dictionary<string, string> Config)
        {
            if (Config == null || Config.Count < 1 || Turret == null) return;
            foreach (var configitem in Config)
            {
                switch (configitem.Key)
                {
                    case "speed":
                        float speed;
                        if (float.TryParse(configitem.Value, out speed))
                            DV_project_length = Math.Abs(speed);
                        break;
                    case "gravity":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            Turret.Ignore_Gravity = false;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            Turret.Ignore_Gravity = true;
                        break;
                    case "ignore_speed_self":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            Turret.Ignore_Self_Velocity = true;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            Turret.Ignore_Self_Velocity = false;
                        break;
                    case "direct":
                        if (configitem.Value == "yes" || configitem.Value == "true")
                            Turret.IsDirectWeapon = true;
                        else if (configitem.Value == "no" || configitem.Value == "false")
                            Turret.IsDirectWeapon = false;
                        break;
                    case "offset":
                        float offset;
                        if (float.TryParse(configitem.Value, out offset))
                            _Cannon_Offset = MathHelper.Clamp(offset, -100, 100);
                        break;
                    case "delta_t":
                        float delta_t;
                        if (float.TryParse(configitem.Value, out delta_t))
                            Turret.Delta_Time = delta_t;
                        break;
                    case "delta_precious":
                        float delta_precious;
                        if (float.TryParse(configitem.Value, out delta_precious))
                            Turret.Delta_Precious = delta_precious;
                        break;
                    case "calc_t":
                        int calc_t;
                        if (int.TryParse(configitem.Value, out calc_t))
                            Turret.CalcCount = calc_t;
                        break;
                    default:
                        break;
                }
            }
        }
        private IMyBlockGroup GetGroup_BlockInThisGroup()
        {
            List<IMyBlockGroup> groups = new List<IMyBlockGroup>();
            GridTerminalSystem.GetBlockGroups(groups);
            return groups.Find((IMyBlockGroup group) =>
            {
                if (group == null) return false;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                group.GetBlocks(blocks); if (blocks.Count < 1) return false;
                if (!blocks.Contains(Me)) return false;
                if (!blocks.Exists((IMyTerminalBlock block) => block is IMyRemoteControl)) return false;
                if (!blocks.Exists((IMyTerminalBlock block) => block is IMyUserControllableGun)) return false;
                if (!blocks.Exists((IMyTerminalBlock block) => (block is IMyMotorStator) && (block.CustomName.Contains("Turret") || block.CustomName.Contains("turret") || block.CustomName.Contains("Gun") || block.CustomName.Contains("gun")))) return false;
                return true;
            });
        }
        public static Dictionary<string, string> SolveLine(string configline)
        {
            var temp = configline.Split(new string[] { "{", "}", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (temp == null || temp.Length < 1) return null;
            Dictionary<string, string> Config_Pairs = new Dictionary<string, string>();
            foreach (var item in temp)
            {
                var config_pair = item.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (config_pair == null || config_pair.Length < 1) continue;
                config_pair[0] = config_pair[0].TrimStart(' ', '\t').TrimEnd(' ', '\t');
                if (config_pair.Length < 2) { Config_Pairs.Add(config_pair[0], ""); continue; }
                if (config_pair.Length < 3) { Config_Pairs.Add(config_pair[0], config_pair[1]); continue; }
                if (config_pair.Length >= 3) { string value = ""; for (int index = 1; index < config_pair.Length; index++) value += (config_pair[index] + " "); Config_Pairs.Add(config_pair[0], value); continue; }
            }
            if (Config_Pairs.Count < 1) return null;
            return Config_Pairs;
        }
        public static WcPbApi pbApi { get; } = new WcPbApi();
        private Dictionary<string, float> AmmoSpeed { get; } = new Dictionary<string, float>();
        private TurretManage Turret { get; set; }
        public static T GetT<T>(IMyGridTerminalSystem GridTerminalSystem, Func<T, bool> requst) where T : class
        {
            List<T> Items = new List<T>();
            GridTerminalSystem.GetBlocksOfType(Items, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        public static List<T> GetTs<T>(IMyGridTerminalSystem GridTerminalSystem, Func<T, bool> requst) where T : class
        {
            List<T> Items = new List<T>();
            GridTerminalSystem.GetBlocksOfType(Items, requst);
            return Items;
        }
        public class TurretManage
        {
            public bool Ignore_Self_Velocity { get; set; }
            public bool Ignore_Gravity { get; set; }
            public bool IsDirectWeapon { get; set; }
            public int CalcCount { get { return _CalcCount; } set { _CalcCount = Math.Max(value, 1); } }
            public float V_project_length { get { return Math.Max(_V_project_length, 25f); } set { _V_project_length = Math.Max(value, 25f); } }
            public float Delta_Precious { get { return _Delta_Precious; } set { _Delta_Precious = Math.Abs(value); } }
            public float Delta_Time { get { return _Delta_Time; } set { _Delta_Time = MathHelper.Clamp(value, 0.9f, 1.1f); } }
            private TurretManage(IMyRemoteControl ctrl, IMyMotorStator turret_base, List<IMyMotorStator> turret_guns, List<IMyUserControllableGun> weapons, IMyProgrammableBlock ME)
            {
                Agent = ctrl; this.turret_base = turret_base; this.Weapons = new HashSet<IMyUserControllableGun>(weapons);
                Gun_Rotor_Groups = Get_Gun_Motor_Group(turret_guns, weapons); me = ME;
                FireWeapons = (bool Fire) => { };
                foreach (var Weapon in weapons) { FireWeapons += (bool Fire) => { if (Weapon == null) return; Weapon.ApplyAction("Shoot"); }; }
            }
            public void Running(MyDetectedEntityInfo? Target, float V_project_length = 200, float Cannon_Offset = 0, float Max_Az = 30, float Max_Ev = 30, float length = 3)
            {
                bool hastarget = PreCalculate(Target, V_project_length, Cannon_Offset);
                if (DisabledFunction) return; if (!hastarget) V_project = null;
                RotationMotors(turret_base, Gun_Rotor_Groups, V_project, length, Max_Az, Max_Ev);
            }
            private bool PreCalculate(MyDetectedEntityInfo? Target, float V_project_length = 200, float Cannon_Offset = 0)
            {
                if (DisabledFunction) return false;
                Vector3? SelfPosition;
                Vector3? Current_CannonDirection;
                CalcParameters(out SelfPosition, out Current_CannonDirection, Cannon_Offset);
                if (!Target.HasValue || Target.Value.IsEmpty()) { EntityID = null; return false; }
                if (EntityID == null || EntityID.Value != Target.Value.EntityId) { EntityID = Target.Value.EntityId; t_n = null; V_project = null; }
                var target_ent = Target.Value;
                Vector3D? TargetPosition = target_ent.HitPosition ?? target_ent.Position;
                Vector3D? TargetVelocity = IsDirectWeapon ? null : (Vector3D?)target_ent.Velocity;
                Vector3D? SelfVelocity = Ignore_Self_Velocity ? null : (Vector3D?)Agent.GetShipVelocities().LinearVelocity;
                Vector3D? Gravity = Ignore_Gravity ? null : (Vector3D?)Agent.GetNaturalGravity();
                GunDirection(TargetPosition, SelfPosition, TargetVelocity, SelfVelocity, Gravity, V_project_length, ref V_project, ref t_n, Current_CannonDirection.Value, CalcCount, Delta_Time, Delta_Precious);
                return true;
            }
            private void CalcParameters(out Vector3? FirePosition, out Vector3? Current_CannonDirection, float Cannon_Offset)
            {
                if (DisabledFunction) { FirePosition = null; Current_CannonDirection = null; return; }
                Vector3 vector = Vector3.Zero;
                Vector3 position = Vector3.Zero;
                foreach (var weapon in Weapons)
                {
                    vector += weapon.WorldMatrix.Forward;
                    position += weapon.GetPosition() + Cannon_Offset * weapon.WorldMatrix.Forward;
                }
                if (vector == Vector3.Zero) Current_CannonDirection = null;
                else { vector.Normalize(); Current_CannonDirection = vector; }
                position /= Weapons.Count;
                FirePosition = position;
            }
            private Vector3D? V_project = null;
            private double? t_n = null;
            private long? EntityID = null;
            private IMyRemoteControl Agent = null;
            private IMyMotorStator turret_base = null;
            private IMyProgrammableBlock me = null;
            public HashSet<IMyUserControllableGun> Weapons { get; private set; } = null;
            private Dictionary<IMyMotorStator, HashSet<IMyUserControllableGun>> Gun_Rotor_Groups = null;
            private float _Delta_Precious;
            private float _Delta_Time;
            private int _CalcCount;
            private float _V_project_length;
            public Action<bool> FireWeapons { get; private set; }
            public IMyUserControllableGun Weapon { get { if (Weapons == null) return null; return Weapons.FirstElement(); } }
            private bool DisabledFunction => (Agent == null || turret_base == null || Gun_Rotor_Groups == null || Gun_Rotor_Groups.Count < 1 || Weapons == null || Weapons.Count < 1);
            public static TurretManage CreateTurretController(IMyTerminalBlock Me, IMyGridTerminalSystem GridTerminalSystem)
            {
                if (Me == null) return null;
                var turret_grid = Me.CubeGrid;
                if (GridTerminalSystem == null) return null;
                var Agent = GetT(GridTerminalSystem, (IMyRemoteControl ctrl) => ctrl.CustomName.Contains("Agent") && (ctrl is IMyRemoteControl));
                if (Agent == null) return null;
                var turret_base = GetT(GridTerminalSystem, (IMyMotorStator rotor) => (rotor.TopGrid != null && (rotor.TopGrid == turret_grid || rotor.CubeGrid == turret_grid)) && (rotor.CustomName.Contains("Turret") || rotor.CustomName.Contains("turret")));
                if (turret_base == null) return null;
                var turret_guns = GetTs(GridTerminalSystem, (IMyMotorStator rotor) => (rotor.CustomName.Contains("Gun") || rotor.CustomName.Contains("gun")) && (rotor.TopGrid != null && rotor.CubeGrid == turret_grid));
                if (turret_guns == null || turret_guns.Count < 1) return null;
                HashSet<IMyCubeGrid> GunGrids = new HashSet<IMyCubeGrid>();
                foreach (var turret_gun in turret_guns)
                    GunGrids.Add(turret_gun.TopGrid);
                var Weapons = GetTs(GridTerminalSystem, (IMyUserControllableGun weapon) => (!(weapon is IMyLargeTurretBase) && GunGrids.Contains(weapon.CubeGrid)));
                if (Weapons == null || Weapons.Count < 1) return null;
                return new TurretManage(Agent, turret_base, turret_guns, Weapons, Me as IMyProgrammableBlock);
            }
            public static void RotationMotors(IMyMotorStator Turret, Dictionary<IMyMotorStator, HashSet<IMyUserControllableGun>> Gun_Rotor_Groups, Vector3D? Direction, double ForceLength, float Max_Az = 30, float Max_Ev = 30)
            {
                if (Turret == null || Gun_Rotor_Groups == null || Gun_Rotor_Groups.Count < 1) return;
                Max_Az = Math.Abs(Max_Az); Max_Ev = Math.Abs(Max_Ev);
                float TurretRotor_Torque = 0;
                foreach (var Gun_Rotor_Group in Gun_Rotor_Groups)
                {
                    if (Direction == null)
                    {
                        Gun_Rotor_Group.Key.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Gun_Rotor_Group.Key.Angle), -Max_Ev, Max_Ev);
                    }
                    else
                    {
                        var data = Get_ArmOfForce_Point(Gun_Rotor_Group);
                        TurretRotor_Torque += GetTorque(Turret, data, Direction, ForceLength);
                        Gun_Rotor_Group.Key.TargetVelocityRad = MathHelper.Clamp(GetTorque(Gun_Rotor_Group.Key, data, Direction, ForceLength), -Max_Ev, Max_Ev);
                    }
                }
                TurretRotor_Torque /= Gun_Rotor_Groups.Count;
                if (Direction == null)
                    Turret.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Turret.Angle), -Max_Az, Max_Az);
                else
                    Turret.TargetVelocityRad = MathHelper.Clamp(TurretRotor_Torque, -Max_Az, Max_Az);
            }
            public static float GetTorque(IMyMotorStator Motor, MyTuple<Vector3D?, Vector3D?> ArmOfForce_Point, Vector3D? Direction, double ForceLength)
            {
                if (Motor == null || Motor.TopGrid == null) return 0;
                if (ArmOfForce_Point.Item1 == null || ArmOfForce_Point.Item2 == null || Direction == null || Direction.Value == Vector3D.Zero || ForceLength == 0)
                    return -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -30, 30);
                var arm = ArmOfForce_Point.Item1.Value;
                var force = Direction.Value * ForceLength;
                return (float)Vector3D.Dot(Vector3D.Cross(force, arm), Motor.Top.WorldMatrix.Up);
            }
            public static MyTuple<Vector3D?, Vector3D?> Get_ArmOfForce_Point(KeyValuePair<IMyMotorStator, HashSet<IMyUserControllableGun>> Gun_Rotor_Group)
            {
                if (Gun_Rotor_Group.Key == null || Gun_Rotor_Group.Value == null || Gun_Rotor_Group.Value.Count < 1) return new MyTuple<Vector3D?, Vector3D?>(null, null);
                Vector3D center = Vector3D.Zero;
                Vector3D direction = Vector3D.Zero;
                foreach (var gun in Gun_Rotor_Group.Value)
                {
                    center += gun.GetPosition();
                    if (direction == Vector3D.Zero)
                        direction = gun.WorldMatrix.Forward;
                }
                center /= Gun_Rotor_Group.Value.Count;
                return new MyTuple<Vector3D?, Vector3D?>(direction * Vector3D.Distance(center, Gun_Rotor_Group.Key.GetPosition()), center);//Get arm of force and it force point
            }
            public static Dictionary<IMyMotorStator, HashSet<IMyUserControllableGun>> Get_Gun_Motor_Group(List<IMyMotorStator> GunMotors, List<IMyUserControllableGun> guns)
            {
                if (GunMotors == null || guns == null || GunMotors.Count < 1 || guns.Count < 1) return null;
                Dictionary<IMyMotorStator, HashSet<IMyUserControllableGun>> Group = new Dictionary<IMyMotorStator, HashSet<IMyUserControllableGun>>();
                foreach (var GunMotor in GunMotors)
                {
                    var grid = GunMotor.TopGrid;
                    if (grid == null) continue;
                    var weapons = guns.FindAll((IMyUserControllableGun gun) => gun.CubeGrid == grid);
                    if (weapons == null || weapons.Count < 1) continue;
                    if (!Group.ContainsKey(GunMotor))
                        Group.Add(GunMotor, new HashSet<IMyUserControllableGun>(weapons));
                    else
                        Group[GunMotor].UnionWith(weapons);
                }
                return Group;
            }
            public static void GunDirection(Vector3D? TargetPosition, Vector3D? SelfPosition, Vector3D? TargetVelocity, Vector3D? SelfVelocity, Vector3D? Gravity, double V_project_length, ref Vector3D? V_project_direction, ref double? t_n, Vector3D CannonDirection, int CalcCount = 4, float Delta_Time = 1, float Delta_Precious = 1)
            {
                if (!TargetPosition.HasValue || !SelfPosition.HasValue) return;
                var v_dis = TargetPosition.Value - SelfPosition.Value;
                if (v_dis == Vector3.Zero) return;
                if (!TargetVelocity.HasValue) { v_dis.Normalize(); V_project_direction = v_dis; return; }
                var v_relative = TargetVelocity.Value - (SelfVelocity ?? Vector3D.Zero);
                Delta_Precious = MathHelper.Clamp(Delta_Precious, 0.01f, 100f); V_project_length = Math.Max(V_project_length, 100f); Delta_Time = MathHelper.Clamp(Delta_Time, 0.9f, 1.1f);
                var min = v_dis.Length() / V_project_length;
                if (Gravity.HasValue && Gravity != Vector3.Zero)
                {
                    var g_l = Gravity.Value.LengthSquared(); var v_l = v_relative.LengthSquared();
                    var b = -2 * v_relative.Dot(Gravity.Value) / g_l; var c = (4 * v_l - 2 * Gravity.Value.Dot(v_dis) - 4 * V_project_length * V_project_length) / g_l;
                    var d = 4 * v_relative.Dot(v_dis) / g_l; var e = 4 * v_l / g_l;
                    int count = 0;
                    if (!t_n.HasValue || t_n.Value < 0)
                        t_n = Solve_Subfunction(min, b, c, d, e);
                    if (!V_project_direction.HasValue && CannonDirection != Vector3D.Zero)
                        V_project_direction = Vector3D.Normalize(CannonDirection);
                    while (count < CalcCount && ErrorFunction(TargetVelocity.Value, TargetPosition.Value, SelfVelocity.Value, V_project_direction.Value * V_project_length, SelfPosition.Value, t_n.Value, Gravity) > Delta_Precious)
                    {
                        t_n = Math.Max(Solve_Subfunction(t_n.Value, b, c, d, e), min);
                        V_project_direction = Solve_Direction(t_n.Value, v_relative, v_dis, Gravity, Delta_Time, true);
                        if (!V_project_direction.HasValue && CannonDirection != Vector3D.Zero)
                            V_project_direction = Vector3D.Normalize(CannonDirection);
                        count++;
                    }
                }
                else
                {
                    var a = v_relative.LengthSquared() - V_project_length * V_project_length;
                    var b = v_relative.Dot(v_dis);
                    var c = v_dis.LengthSquared();
                    var k = b * b - 4 * a * c;
                    if (k < 0) { return; }
                    var sqrt_k = (float)Math.Sqrt(k);
                    t_n = Math.Max(Math.Max((-b - sqrt_k) / (2 * a), min), Math.Max((-b + sqrt_k) / (2 * a), min));
                }
                if (t_n == null) return;
                V_project_direction = Solve_Direction(t_n.Value, v_relative, v_dis, Gravity, Delta_Time, false);
            }
            private static double ErrorFunction(Vector3D V_target, Vector3D P_target, Vector3D V_me, Vector3D V_projector, Vector3D P_me, double t, Vector3D? Gravity = null)
            {
                var P_target_1 = V_target * t + P_target;
                var P_target_2 = (V_me + V_projector + (Gravity.HasValue ? (Gravity.Value * 0.5f * t) : Vector3D.Zero)) * t + P_me;
                return (P_target_1 - P_target_2).Length();
            }
            private static double Solve_Subfunction(double t, double b, double c, double d, double e)
            {
                var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
                var t_b = 4 * t_3 + 3 * t_2 * b + 2 * c * t + d;
                if (t_b == 0) return 0;
                var t_v = t_4 + b * t_3 + c * t_2 + d * t + e;
                return t - t_v / t_b;
            }
            private static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null, double Delta_Time = 1, bool InSimulate = false)
            {
                if (t <= 0) return null; if (!InSimulate) t *= Delta_Time;
                var velocity = (dis / t + v_r - (g.HasValue ? (g.Value * t / 2) : Vector3D.Zero));
                if (velocity == Vector3D.Zero) return null; velocity = Vector3D.Normalize(velocity);
                return velocity;
            }
        }
    }
    public partial class CannonTurret
    {
        private void InitTurretGroup()
        {
            List<IMyBlockGroup> blockGroups = new List<IMyBlockGroup>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid).GetBlockGroups(blockGroups);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            foreach (var blockGroup in blockGroups)
            {
                blockGroup.GetBlocks(blocks, (IMyTerminalBlock block) => block == Me);
                if (blocks.Count < 1) { blocks.Clear(); continue; }
                TurretGroups.Add(blockGroup);
            }
        }
        private List<IMyBlockGroup> TurretGroups { get; } = new List<IMyBlockGroup>();
    }
}
