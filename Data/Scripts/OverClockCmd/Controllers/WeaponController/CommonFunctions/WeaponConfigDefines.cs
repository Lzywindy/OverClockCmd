using Sandbox.ModAPI;
using SuperBlocks.Controller;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks
{
    public interface IClear
    {
        void Clear();
    }
    public class MyAmmoConfig : IClear
    {
        public float Speed { get; private set; }
        public float Acc { get; private set; }
        public float Gravity_mult { get; private set; }
        public float Trajectory { get; private set; }
        public MyAmmoConfig(Dictionary<string, string> Config)
        {
            if (Config == null) { Clear(); return; }
            foreach (var configitem in Config)
            {
                switch (configitem.Key)
                {
                    case "speed": Speed = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "acc": Acc = MyConfigs.ParseFloat(configitem.Value); break;
                    case "gravity_mult": Gravity_mult = MyConfigs.ParseFloat(configitem.Value); break;
                    case "trajectory": Trajectory = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    default: break;
                }
            }
        }
        public void Clear()
        {
            Speed = 3e8f;
            Acc = 0;
            Gravity_mult = 0;
            Trajectory = 10000;
        }
    }
    public class MyWeaponConfig : IClear
    {
        public const string EnergyWeaponID = @"EnergyWeapon";
        public bool IsDirect { get; private set; } = false;
        public bool Ignore_speed_self { get; private set; } = false;
        public float Delta_t { get; private set; }
        public float Delta_precious { get; private set; }
        public int Calc_t { get; private set; }
        public float Offset { get; private set; }
        public float TimeFixed { get; private set; }
        private Dictionary<string, MyAmmoConfig> AmmoNMs { get; } = new Dictionary<string, MyAmmoConfig>();
        public MyWeaponConfig(Dictionary<string, Dictionary<string, string>> ConfigTree, string WPNM)
        {
            if (Utils.Common.IsNullCollection(ConfigTree) || WPNM == EnergyWeaponID || !ConfigTree.ContainsKey(WPNM)) { Clear(); return; }
            var Config = ConfigTree[WPNM];
            foreach (var configitem in Config)
            {
                switch (configitem.Key)
                {
                    case "delta_t": Delta_t = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "delta_precious": Delta_precious = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "calc_t": Calc_t = Math.Abs(MyConfigs.ParseInt(configitem.Value)); break;
                    case "offset": Offset = MyConfigs.ParseFloat(configitem.Value); break;
                    case "ammo":
                        var ammos = configitem.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (ammos == null) continue;
                        foreach (var ammo in ammos)
                        {
                            if (!ConfigTree.ContainsKey(ammo)) continue;
                            AmmoNMs.Add(ammo, new MyAmmoConfig(ConfigTree[ammo]));
                        }
                        break;
                    case "timefixed": TimeFixed = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    default: break;
                }
            }
        }
        public void Clear() { TimeFixed = 1; IsDirect = true; Ignore_speed_self = false; Delta_t = 0; Delta_precious = 0; Calc_t = 0; Offset = 0; AmmoNMs.Clear(); AmmoNMs.Add("DefaultAmmo", new MyAmmoConfig(null)); }
        public void GetConfig(string AmmoName, out MyWeaponParametersConfig Parameters)
        {
            Parameters.IsDirect = IsDirect; Parameters.Ignore_speed_self = Ignore_speed_self; Parameters.Delta_t = Delta_t;
            Parameters.Delta_precious = Delta_precious; Parameters.Calc_t = Calc_t; Parameters.Offset = Offset;
            Parameters.TimeFixed = TimeFixed;
            if (!AmmoNMs.ContainsKey(AmmoName)) { Parameters.Speed = 3e8f; Parameters.Acc = 0; Parameters.Gravity_mult = 0; Parameters.Trajectory = 10000; return; }
            Parameters.Speed = AmmoNMs[AmmoName].Speed; Parameters.Acc = AmmoNMs[AmmoName].Acc; Parameters.Gravity_mult = AmmoNMs[AmmoName].Gravity_mult; Parameters.Trajectory = AmmoNMs[AmmoName].Trajectory;
        }
    }
    public class MyTurretConfig : IClear
    {
        public float max_az { get; private set; }
        public float max_ev { get; private set; }
        public float mult { get; private set; }
        public float range { get; private set; }
        public int firegap { get; private set; }
        public string turretNM { get; private set; }
        public string weapon_tag { get; private set; }
        public Dictionary<string, MyWeaponConfig> TurretWeaponConfigs { get; } = new Dictionary<string, MyWeaponConfig>();
        private const string azNM = "Az";
        private const string evNM = "Ev";
        public string TurretAzNM => $"{turretNM}{azNM}";
        public string TurretEzNM => $"{turretNM}{evNM}";
        public Vector2 Max_Speed => new Vector2(max_ev, max_az);
        public MyTurretConfig(Dictionary<string, Dictionary<string, string>> ConfigTree, string ConfigID)
        {
            if (ConfigTree == null || ConfigID == null || ConfigID == "" || !ConfigTree.ContainsKey(ConfigID)) { Clear(); return; }
            var Config = ConfigTree[ConfigID];
            foreach (var configitem in Config)
            {
                switch (configitem.Key)
                {
                    case "max_az": max_az = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "max_ev": max_ev = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "mult": mult = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "range": range = Math.Abs(MyConfigs.ParseFloat(configitem.Value)); break;
                    case "firegap": firegap = Math.Abs(MyConfigs.ParseInt(configitem.Value)); break;
                    case "turretNM": turretNM = configitem.Value; break;
                    case "weapon_tag": weapon_tag = configitem.Value; break;
                    case "weapons":
                        var weapon_nms = configitem.Value.Split(new string[] { "," }, StringSplitOptions.RemoveEmptyEntries);
                        if (weapon_nms == null) continue;
                        foreach (var weapon_nm in weapon_nms)
                        {
                            if (!ConfigTree.ContainsKey(weapon_nm)) continue;
                            TurretWeaponConfigs.Add(weapon_nm, new MyWeaponConfig(ConfigTree, weapon_nm));
                        }
                        break;
                    default: break;
                }
            }
        }
        public void GetConfig(string WeaponName, string AmmoName, out MyWeaponParametersConfig Parameters)
        {
            if (!TurretWeaponConfigs.ContainsKey(WeaponName))
            {
                TurretWeaponConfigs["DefaultWeapon"].GetConfig(AmmoName, out Parameters);
                Parameters.Trajectory = Math.Min(range, Math.Max(0, Parameters.Trajectory));
                return;
            }
            TurretWeaponConfigs[WeaponName].GetConfig(AmmoName, out Parameters);
            Parameters.Trajectory = Math.Min(range, Math.Max(0, Parameters.Trajectory));
        }
        public void Clear() { max_az = 1; max_ev = 1; mult = 3; range = 1000; turretNM = "Turret"; weapon_tag = "Gun"; TurretWeaponConfigs.Clear(); TurretWeaponConfigs.Add("DefaultWeapon", new MyWeaponConfig(null, "DefaultWeapon")); }
    }
    public class MyTurretControllerConfig : IClear
    {
        public const string ConfigID = @"TCConfig";
        public ConcurrentDictionary<string, MyTurretConfig> TurretConfigs { get; } = new ConcurrentDictionary<string, MyTurretConfig>();
        public void UpdateConfigs(Dictionary<string, Dictionary<string, string>> ConfigTree)
        {
            if (ConfigTree == null || !ConfigTree.ContainsKey(ConfigID)) { Clear(); return; }
            foreach (var config in ConfigTree[ConfigID])
            {
                switch (config.Key)
                {
                    case ConfigID:
                        var turrets = Utils.Common.SpliteByQ(config.Value);
                        foreach (var ConfigNM in turrets)
                        {
                            if (!ConfigTree.ContainsKey(ConfigNM)) continue;
                            TurretConfigs.AddOrUpdate(ConfigNM, new MyTurretConfig(ConfigTree, ConfigNM), (cfNM, value) => new MyTurretConfig(ConfigTree, ConfigNM));
                        }
                        break;
                    default: break;
                }
            }
        }
        public void ForceReUpdateConfigs(Dictionary<string, Dictionary<string, string>> ConfigTree)
        {
            Clear();
            if (ConfigTree == null || !ConfigTree.ContainsKey(ConfigID)) return;
            foreach (var config in ConfigTree[ConfigID])
            {
                switch (config.Key)
                {
                    case ConfigID:
                        var turrets = Utils.Common.SpliteByQ(config.Value);
                        foreach (var ConfigNM in turrets)
                        {
                            if (!ConfigTree.ContainsKey(ConfigNM)) continue;
                            TurretConfigs.AddOrUpdate(ConfigNM, new MyTurretConfig(ConfigTree, ConfigNM), (cfNM, value) => new MyTurretConfig(ConfigTree, ConfigNM));
                        }
                        break;
                    default: break;
                }
            }
        }
        public void Clear() { TurretConfigs.Clear(); }
        public override string ToString()
        {
            string str = $"[{ConfigID}]/r/n" + "turrets=";
            foreach (var TurretConfig in TurretConfigs)
                str += TurretConfig.Key + ",";
            return str;
        }
    }
    public class MyFixedWeaponsConfig : IClear
    {
        public const string ConfigID = @"FWConfig";
        public void ForceReUpdateConfigs(Dictionary<string, Dictionary<string, string>> ConfigTree)
        {
            if (ConfigTree == null || !ConfigTree.ContainsKey(ConfigID)) { Clear(); return; }
            foreach (var config in ConfigTree[ConfigID])
            {
                switch (config.Key)
                {
                    case ConfigID:
                        var weapons = Utils.Common.SpliteByQ(config.Value);
                        foreach (var ConfigNM in weapons)
                        {
                            if (!ConfigTree.ContainsKey(ConfigNM)) continue;
                            WeaponConfigs.AddOrUpdate(ConfigNM, new MyWeaponConfig(ConfigTree, ConfigNM), (cfNM, value) => new MyWeaponConfig(ConfigTree, ConfigNM));
                        }
                        break;
                    default: break;
                }
            }
        }
        public void UpdateConfigs(Dictionary<string, Dictionary<string, string>> ConfigTree)
        {
            Clear();
            if (ConfigTree == null || !ConfigTree.ContainsKey(ConfigID)) return;
            foreach (var config in ConfigTree[ConfigID])
            {
                switch (config.Key)
                {
                    case ConfigID:
                        var weapons = Utils.Common.SpliteByQ(config.Value);
                        foreach (var ConfigNM in weapons)
                        {
                            if (!ConfigTree.ContainsKey(ConfigNM)) continue;
                            WeaponConfigs.AddOrUpdate(ConfigNM, new MyWeaponConfig(ConfigTree, ConfigNM), (cfNM, value) => new MyWeaponConfig(ConfigTree, ConfigNM));
                        }
                        break;
                    default: break;
                }
            }
        }
        public ConcurrentDictionary<string, MyWeaponConfig> WeaponConfigs { get; } = new ConcurrentDictionary<string, MyWeaponConfig>();
        public void Clear() => WeaponConfigs.Clear();
        public override string ToString()
        {
            string str = $"[{ConfigID}]/r/n" + "fixedweapons=";
            foreach (var WeaponConfig in WeaponConfigs)
                str += WeaponConfig.Key + ",";
            return str;
        }
    }
    public struct MyWeaponParametersConfig
    {
        public volatile bool IsDirect;
        public volatile bool Ignore_speed_self;
        public volatile float Delta_t;
        public volatile float Delta_precious;
        public volatile float Calc_t;
        public volatile float Offset;
        public volatile float TimeFixed;
        public volatile float Speed;
        public volatile float Acc;
        public volatile float Gravity_mult;
        public volatile float Trajectory;
    }
    public class MyTurretScript
    {
        public MyTurretScript(IMyTerminalBlock Me, MyTurretConfig ThisConfig) { Init(Me, ThisConfig); }
        public bool NeedRestart => Utils.Common.NullEntity(Me) || ThisConfig == null;
        public void Init(IMyTerminalBlock Me, MyTurretConfig ThisConfig)
        {
            this.Me = Me; this.ThisConfig = ThisConfig;
            if (Utils.Common.NullEntity(Me) || ThisConfig == null) return;
        }
        private IMyTerminalBlock Me;
        private MyTurretConfig ThisConfig;
    }
    public class MyFixedWeaponScript
    {
        public MyFixedWeaponScript(IMyTerminalBlock Me, MyTurretConfig ThisConfig) { InitBasicParameters(Me, ThisConfig); }
        public virtual void Init(IMyTerminalBlock Me, MyTurretConfig ThisConfig)
        {
            if (!InitBasicParameters(Me, ThisConfig)) return;
        }
        public void SetWeaponAmmo(string WeaponNM, string AmmoNM)
        {

        }
        protected bool InitBasicParameters(IMyTerminalBlock Me, MyTurretConfig ThisConfig)
        {
            this.Me = Me; this.ThisConfig = ThisConfig;
            WeaponBlocks.Clear();
            if (NonBasicParametersReady) return false;
            GetWeapons();
            if (Utils.Common.IsNullCollection(WeaponBlocks)) return false;
            return true;
        }
        private void GetWeapons()
        {
            if (NonBasicParametersReady) return;
            WeaponBlocks.AddRange(Utils.Common.GetTs(Me, (IMyTerminalBlock block) => block.CustomName.Contains(Weapon_Tag)));
        }
        private IMyTerminalBlock Me;
        private MyTurretConfig ThisConfig;
        private bool NonBasicParametersReady => Utils.Common.NullEntity(Me) || ThisConfig == null;
        private MyTargetPredict TargetPredict { get; } = new MyTargetPredict();
        private string Weapon_Tag => ThisConfig?.weapon_tag ?? "Gun";
        private List<IMyTerminalBlock> WeaponBlocks { get; } = new List<IMyTerminalBlock>();
    }
}