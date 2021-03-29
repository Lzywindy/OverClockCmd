using System.Collections.Concurrent;
using System.Collections.Generic;
using VRage.Game;
using VRageMath;
using static SuperBlocks.Utils;

namespace SuperBlocks
{
    public static class Definitions
    {
        public const float TimeGap = MyEngineConstants.PHYSICS_STEP_SIZE_IN_SECONDS;
        public static class ConfigName
        {
            public static class ProjectileDef
            {
                public const string DefaultAmmo = @"DefaultAmmo";
                public const string InitialSpeed = "InitialSpeed";
                public const string DesiredSpeed = "DesiredSpeed";
                public const string Acceleration = "Acceleration";
                public const string GravityMultipy = "GravityMultipy";
                public const string Trajectory = "Trajectory";
            }
            public static class WeaponDef
            {
                public const string DefaultWeapon = @"DefaultWeapon";
                public const string IsDirect = "IsDirect";
                public const string Ignore_speed_self = "Ignore_speed_self";
                public const string Delta_t = "Delta_t";
                public const string Delta_precious = "Delta_precious";
                public const string Calc_t = "Calc_t";
                public const string TimeFixed = "TimeFixed";
                public const string FireAngle = "FireAngle";
                public const string Ammos = "Ammos";
            }
            public static class FunctionalGroupDef
            {
                public const string Weapons = "Weapons";
                public const string Range = "Range";
                public const string Firegap = "Firegap";
                public const string Weapon_tag = "Weapon_tag";
            }
            public static class Turret
            {
                public const string TurretNM = "TurretNM";
                public const string azNM = "Az";
                public const string evNM = "Ev";
                public const string AzLimit = "AzLimit";
                public const string EvLimit = "EvLimit";
                public const string AzUpperLimit = "AzUpperLimit";
                public const string AzLowerLimit = "AzLowerLimit";
                public const string EvUpperLimit = "EvUpperLimit";
                public const string EvLowerLimit = "EvLowerLimit";
            }
            public static class FixedWeapon
            {
                public const string DetectedAngle = "DetectedAngle";
            }
        }
        public static class Structures
        {
            public struct TrajectoryDef
            {
                public volatile string SubtypeId;
                public volatile bool IsDirect;
                public volatile float InitialSpeed;
                public volatile float DesiredSpeed;
                public volatile float AccelPerSec;
                public volatile float GravityMultiplier;
                public volatile int MaxTrajectoryTime;
                public volatile float MaxTrajectory;
                public WeaponType Type => (IsDirect || DesiredSpeed >= 3e8f) ? WeaponType.Energy : AccelPerSec > 0 ? WeaponType.Rocket : WeaponType.Projectile;
                public static TrajectoryDef DefaultWeaponCore => new TrajectoryDef() { IsDirect = false, InitialSpeed = 900, DesiredSpeed = 900, AccelPerSec = 0, GravityMultiplier = 1.02f, MaxTrajectoryTime = 3000, MaxTrajectory = 3000 };
                public static TrajectoryDef DefaultValue => new TrajectoryDef() { IsDirect = false, InitialSpeed = 100, DesiredSpeed = 200, AccelPerSec = 600, GravityMultiplier = 0, MaxTrajectoryTime = 1000, MaxTrajectory = 800 };
                public static TrajectoryDef KeensRocket => new TrajectoryDef() { IsDirect = false, InitialSpeed = 100, DesiredSpeed = 200, AccelPerSec = 600, GravityMultiplier = 0, MaxTrajectoryTime = 1000, MaxTrajectory = 800 };
                public static TrajectoryDef KeensProjectile_Small => new TrajectoryDef() { IsDirect = false, InitialSpeed = 300, DesiredSpeed = 300, AccelPerSec = 0, GravityMultiplier = 0, MaxTrajectoryTime = 1000, MaxTrajectory = 800 };
                public static TrajectoryDef KeensProjectile_Large => new TrajectoryDef() { IsDirect = false, InitialSpeed = 400, DesiredSpeed = 400, AccelPerSec = 0, GravityMultiplier = 0, MaxTrajectoryTime = 1000, MaxTrajectory = 800 };
                public static TrajectoryDef Energy => new TrajectoryDef() { IsDirect = true, InitialSpeed = 3e8f, DesiredSpeed = 3e8f, AccelPerSec = 0, GravityMultiplier = 0, MaxTrajectoryTime = 10, MaxTrajectory = 1e6f };
                public static TrajectoryDef CreateFromWeaponCoreDatas(WeaponCore.Api.WcApiDef.WeaponDefinition.AmmoDef.TrajectoryDef WCTrajectoryDef)
                {
                    return new TrajectoryDef()
                    {
                        IsDirect = false,
                        InitialSpeed = 0,
                        DesiredSpeed = WCTrajectoryDef.DesiredSpeed,
                        AccelPerSec = WCTrajectoryDef.AccelPerSec,
                        GravityMultiplier = WCTrajectoryDef.GravityMultiplier,
                        MaxTrajectoryTime = (int)WCTrajectoryDef.MaxTrajectoryTime,
                        MaxTrajectory = WCTrajectoryDef.MaxTrajectory
                    };
                }
                public static TrajectoryDef CreateFromWeaponCoreDatas(WeaponCore.Api.WcApiDef.WeaponDefinition.AmmoDef WCTrajectoryDef)
                {
                    return new TrajectoryDef()
                    {
                        IsDirect = false,
                        InitialSpeed = 0,
                        SubtypeId = WCTrajectoryDef.AmmoMagazine,
                        DesiredSpeed = WCTrajectoryDef.Trajectory.DesiredSpeed,
                        AccelPerSec = WCTrajectoryDef.Trajectory.AccelPerSec,
                        GravityMultiplier = WCTrajectoryDef.Trajectory.GravityMultiplier,
                        MaxTrajectoryTime = (int)WCTrajectoryDef.Trajectory.MaxTrajectoryTime,
                        MaxTrajectory = WCTrajectoryDef.Trajectory.MaxTrajectory
                    };
                }
            }
            public struct MyWeaponParametersConfig
            {
                public volatile float Delta_t;
                public volatile float Delta_precious;
                public volatile float Calc_t;
                public volatile float TimeFixed;
                public volatile float RPM;
                public volatile float Range;
                public TrajectoryDef Trajectory;
                public static MyWeaponParametersConfig KeensRocket => new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 5, TimeFixed = 5, RPM = 600, Range = float.MaxValue, Trajectory = TrajectoryDef.KeensRocket };
                public static MyWeaponParametersConfig DefaultWeaponCore => new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 5, TimeFixed = 5, RPM = 600, Range = float.MaxValue, Trajectory = TrajectoryDef.DefaultWeaponCore };
                public static MyWeaponParametersConfig KeensProjectile_Small => new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 5, TimeFixed = 5, RPM = 600, Range = float.MaxValue, Trajectory = TrajectoryDef.KeensProjectile_Small };
                public static MyWeaponParametersConfig KeensProjectile_Large => new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 5, TimeFixed = 5, RPM = 600, Range = float.MaxValue, Trajectory = TrajectoryDef.KeensProjectile_Large };
                public static MyWeaponParametersConfig Energy => new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 5, TimeFixed = 5, RPM = 600, Range = float.MaxValue, Trajectory = TrajectoryDef.Energy };
                public static MyWeaponParametersConfig CreateFromConfig(ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Config, string ConfigID)
                {
                    if (Common.IsNullCollection(Config) || ConfigID == null || ConfigID == "" || !Config.ContainsKey(ConfigID))
                        return new MyWeaponParametersConfig() { Delta_t = 1, Delta_precious = 0.0005f, Calc_t = 1, TimeFixed = 3, RPM = float.MaxValue, Range = float.MaxValue, Trajectory = TrajectoryDef.DefaultValue };
                    var value = Config[ConfigID];
                    var data = new MyWeaponParametersConfig();
                    foreach (var item in value)
                    {
                        switch (item.Key)
                        {
                            case "delta_t": data.Delta_t = MyConfigs.ParseFloat(item.Value); break;
                            case "delta_precious": data.Delta_precious = MyConfigs.ParseFloat(item.Value); break;
                            case "calc_t": data.Calc_t = MyConfigs.ParseFloat(item.Value); break;
                            case "timefixed": data.TimeFixed = MyConfigs.ParseFloat(item.Value); break;
                            case "range": data.Range = MyConfigs.ParseFloat(item.Value); break;
                            case "rpm": data.RPM = MyConfigs.ParseFloat(item.Value); break;
                            case "direct": data.Trajectory.IsDirect = MyConfigs.ParseBool(item.Value); break;
                            case "initialspeed": data.Trajectory.InitialSpeed = MyConfigs.ParseFloat(item.Value); break;
                            case "desiredspeed": data.Trajectory.DesiredSpeed = MyConfigs.ParseFloat(item.Value); break;
                            case "accelpersec": data.Trajectory.AccelPerSec = MyConfigs.ParseFloat(item.Value); break;
                            case "gravitymultiplier": data.Trajectory.GravityMultiplier = MyConfigs.ParseFloat(item.Value); break;
                            case "maxtrajectory": data.Trajectory.MaxTrajectory = MyConfigs.ParseFloat(item.Value); break;
                            case "maxtrajectorytime": data.Trajectory.MaxTrajectoryTime = MyConfigs.ParseInt(item.Value); break;
                            default: break;
                        }
                    }
                    return data;
                }
                public static void SaveConfig(MyWeaponParametersConfig data, ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Config, string ConfigID)
                {
                    if (ConfigID == null || ConfigID == "" || Config == null) return;
                    if (Config.TryAdd(ConfigID, new ConcurrentDictionary<string, string>()) || Config.ContainsKey(ConfigID))
                    {
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "delta_t", data.Delta_t.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "delta_precious", data.Delta_precious.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "calc_t", data.Calc_t.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "timefixed", data.TimeFixed.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "range", data.Range.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "rpm", data.RPM.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "direct", data.Trajectory.IsDirect.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "initialspeed", data.Trajectory.InitialSpeed.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "desiredspeed", data.Trajectory.DesiredSpeed.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "accelpersec", data.Trajectory.AccelPerSec.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "gravitymultiplier", data.Trajectory.GravityMultiplier.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "maxtrajectory", data.Trajectory.MaxTrajectory.ToString());
                        MyConfigs.Concurrent.ModifyProperty(Config[ConfigID], "maxtrajectorytime", data.Trajectory.MaxTrajectoryTime.ToString());
                        return;
                    }
                }
            }
            public struct FunctionalGroupDef
            {
                public Dictionary<string, WeaponDef> Weapons;
                public float Range;
                public int Firegap;
                public string Weapon_tag;
            }
            public struct ProjectileDef
            {
                public float InitialSpeed;
                public float DesiredSpeed;
                public float Acceleration;
                public float GravityMultipy;
                public float Trajectory;
            }
            public struct WeaponDef
            {
                public string IsDirect;
                public string Ignore_speed_self;
                public float Delta_t;
                public float Delta_precious;
                public int Calc_t;
                public int TimeFixed;
                public float FireAngle;
                public Dictionary<string, ProjectileDef> Ammos;
            }
            public struct FixedWeaponDef
            {
                public float DetectedAngle;
                public FunctionalGroupDef BasicWeaponSetup;
            }
            public struct TurretDef
            {
                public string TurretNM;
                public float Max_AV_az;
                public float Max_AV_ev;
                public float Mult;
                public FunctionalGroupDef BasicWeaponSetup;
                public bool AzLimit;
                public bool EvLimit;
                public float AzUpperLimit;
                public float AzLowerLimit;
                public float EvUpperLimit;
                public float EvLowerLimit;
                public string TurretAzNM => $"{TurretNM}{ConfigName.Turret.azNM}";
                public string TurretEzNM => $"{TurretNM}{ConfigName.Turret.evNM}";
                public Vector2 Max_Speed => new Vector2(Max_AV_ev, Max_AV_az);
            }
        }

    }
}