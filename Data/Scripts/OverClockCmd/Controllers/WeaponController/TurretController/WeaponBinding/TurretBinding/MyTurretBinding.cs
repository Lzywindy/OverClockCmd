using ParallelTasks;
using Sandbox.ModAPI;
using SpaceEngineers.Game.ModAPI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public sealed partial class MyTurretBinding : IEquatable<MyTurretBinding>, IEqualityComparer<MyTurretBinding>
    {
        public MyTurretBinding(IMyMotorStator MotorAz)
        {
            if (Utils.Common.NullEntity(MotorAz)) return;
            this.MotorAz = MotorAz;
            HashCode = this.MotorAz.EntityId.GetHashCode();
            EntityID = this.MotorAz.EntityId;
            BasicInit(this.MotorAz);
            ReadConfig_Turret_Rotors();
        }
        public void SetConfig(ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs)
        {
            try
            {
                if (SlaveWeapons.UnabledFire || Configs.ContainsKey(_TurretWeaponConfigID))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, _TurretWeaponConfigID);
                else if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons.First()))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultWeaponCoreWeapon");
                else if (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Plasam") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("plasam"))
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "EnergyWeapon");
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeGatlingTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallGatlingGun))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case null:
                        case "":
                        case "SmallGatlingTurret":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_LargeWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeMissileTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncher) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncherReload))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case null:
                        case "":
                        case "LargeMissileLauncher":
                        case "SmallMissileTurret":
                        case "SmallRocketLauncherReload":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensRocketWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeInteriorTurret))
                {
                    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
                    {
                        case "LargeInteriorTurret":
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_SmallWeapon");
                            break;
                        default:
                            ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
                            break;
                    }
                }
                else
                    ModifiedConfig = DefaultConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
            }
            catch (Exception) { Clear(); }
        }
        public void Running()
        {
            try
            {
                RemoveEmptyBlocks();
                MotorAz.Enabled = RotorsEnabled;
                foreach (var motorEv in MotorEvs) motorEv.Enabled = RotorsEnabled;
                if (!Enabled || !CanRunning || !RotorsEnabled) { RunningDefault(); RunningAutoFire(false); return; }
                Vector3D? Direction = null;
                bool fire = AutoFire && Enabled && RotorsEnabled;
                if (UnderControl)
                {
                    RunningManual(Utils.Common.GetT<IMyShipController>(MotorAz, block => block.IsUnderControl)?.RotationIndicator);
                    RunningAutoFire(false);
                    return;
                }
                else if (ManuelOnly)
                {
                    RunningDefault();
                    RunningAutoFire(false);
                }
                else
                {
                    ReferWeapon();
                    SlaveWeapons.ResetFireRPM(DefaultConfig.RPM);
                    Direction = MyTargetPredict.CalculateDirection_TargetTest(MotorAz, Weapons, AimTarget, ref ModifiedConfig);
                    if (InRangeDirection(Direction))
                    {
                        fire = AutoFire && Enabled && RotorsEnabled && MyTargetPredict.CanFireWeapon(SlaveWeapons.CurrentWeapons, Direction, DefaultConfig.Delta_precious);
                        RunningDirection(Direction);
                        RunningAutoAimAt(MotorAz);
                        RunningAutoFire(fire); return;
                    }
                    RunningDefault();
                    RunningAutoFire(false);
                }
            }
            catch (Exception) { }
        }

        public void Restart()
        {
            BasicInit(MotorAz);
            ReadConfig_Turret_Rotors();
        }
        public MyTargetDetected AimTarget { get; set; }
        public volatile bool AutoFire = false;
        public volatile bool Enabled = false;
        public volatile bool RotorsEnabled = true;
        public override int GetHashCode()
        {
            return HashCode;
        }

        public override bool Equals(object obj)
        {
            return obj.GetHashCode() == HashCode;
        }

        public bool Equals(MyTurretBinding other)
        {
            return other.HashCode == HashCode;
        }

        public bool Equals(MyTurretBinding x, MyTurretBinding y)
        {
            return x.HashCode == y.HashCode;
        }

        public int GetHashCode(MyTurretBinding obj)
        {
            return obj.HashCode;
        }
        public readonly long EntityID = -1;
        private volatile int HashCode = -1;


        public void CycleWeapons() { }
    }
    public sealed partial class MyTurretBinding
    {
        private void BasicInit(IMyMotorStator motorAz)
        {
            try
            {
                Cameras.Clear(); Weapons.Clear(); MotorEvs.Clear();
                //motorEvs_WTs.Clear();
                if (MyWeaponAndTurretApi.ActiveRotorBasicFilter(motorAz))
                    MotorAz = motorAz;
                else
                    MotorAz = null;
                UpdateBinding();
            }
            catch (Exception) { Clear(); }
        }
        private void ReferWeapon()
        {
            SlaveWeapons.ResetFireRPM(DefaultConfig.RPM);
            if (SlaveWeapons.UnabledFire) SlaveWeapons.LoadCurrentWeapons(Weapons, DefaultConfig.RPM);
            if (!SlaveWeapons.UnabledFire)
            {
                foreach (var CurrentWeapon in SlaveWeapons.CurrentWeapons)
                {
                    if (!BasicInfoService.WcApi.HasCoreWeapon(CurrentWeapon)) continue;
                    if (Utils.Common.NullEntity(AimTarget?.Entity)) continue;
                    BasicInfoService.WcApi.SetWeaponTarget(CurrentWeapon, AimTarget.Entity, MyWeaponAndTurretApi.GetWeaponID(CurrentWeapon));
                }
            }
            SetWeaponAmmos();
        }
        private void SetWeaponAmmos()
        {
            if (Utils.Common.IsNullCollection(SlaveWeapons.CurrentWeapons)) return;
            var weapon = SlaveWeapons.CurrentWeapons.FirstOrDefault();
            if (Utils.Common.NullEntity(weapon) || !BasicInfoService.WcApi.HasCoreWeapon(weapon)) return;
            string AmmoName = BasicInfoService.WcApi.GetActiveAmmo(weapon, MyWeaponAndTurretApi.GetWeaponID(weapon));
            if (Utils.Common.IsNull(AmmoName)) return;
            foreach (var wp in SlaveWeapons.CurrentWeapons) BasicInfoService.WcApi.SetActiveAmmo(wp, MyWeaponAndTurretApi.GetWeaponID(wp), AmmoName);
            var value = MyWeaponAndTurretApi.GetWeaponCoreDefinition(weapon, AmmoName);
            if (value == null) return;
            ModifiedConfig = value.Value;
            ModifiedConfig.Trajectory.GravityMultiplier = DefaultConfig.Trajectory.GravityMultiplier * value.Value.Trajectory.GravityMultiplier;
        }
    }
    public sealed partial class MyTurretBinding
    {
        private MyWeaponParametersConfig DefaultConfig = default(MyWeaponParametersConfig);
        private MyWeaponParametersConfig ModifiedConfig = default(MyWeaponParametersConfig);
        public bool CanRunning => (!(Utils.Common.NullEntity(MotorAz) || Utils.Common.IsNullCollection(MotorEvs)));
        private bool ManuelOnly => Utils.Common.IsNullCollection(Weapons) && CanManuel;
        private bool UnderControl => CanManuel && !Utils.Common.NullEntity(ControlledCamera);
        private Utils.FireWeaponManage SlaveWeapons { get; } = new Utils.FireWeaponManage();
        private IMyCameraBlock ControlledCamera { get { if (Utils.Common.IsNullCollection(Cameras)) return null; IMyCameraBlock cameraBlock = Cameras.FirstOrDefault(b => b.IsActive); return cameraBlock; } }

        private bool CanManuel => !Utils.Common.IsNullCollection(Cameras);
        private float Mult { get { if (!UnderControl) return 0; return MathHelper.Clamp(MyAPIGateway.Session.Camera.FovWithZoom, 0.00001f, 0.5f); } }
    }
    public sealed partial class MyTurretBinding
    {
        private float _Max_AV_az = 3;
        private float _Max_AV_ev = 3;
        private float _Multipy = 8;
        private volatile bool _AzLimit = false;
        private volatile bool _EvLimit = true;
        private volatile float _AzUpperLimit = 170;
        private volatile float _AzLowerLimit = -170;
        private volatile float _EvUpperLimit = 80;
        private volatile float _EvLowerLimit = -5;
        private const float _AzUpperLimitMax = 170;
        private const float _AzLowerLimitMax = -170;
        private const float _EvUpperLimitMax = 89;
        private const float _EvLowerLimitMax = -89;
        private string _TurretWeaponConfigID = "DefaultSetup";
        private Vector2 _MaxSpeed => new Vector2(_Max_AV_ev, _Max_AV_az);
        public void ReadConfig_Turret_Rotors()
        {
            if (Utils.Common.IsNull(MotorAz)) return;
            Dictionary<string, Dictionary<string, string>> Config = MyConfigs.CustomDataConfigRead_INI(MotorAz.CustomData);
            if (Utils.Common.IsNullCollection(Config) || !Config.ContainsKey("TurretLimited")) { WriteTemplateConfig(); SetLimitedMotors(); return; }
            var value = Config["TurretLimited"];
            foreach (var item in value)
            {
                switch (item.Key)
                {
                    case "Max_AV_az": _Max_AV_az = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "Max_AV_ev": _Max_AV_ev = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "Multipy": _Multipy = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
                    case "AzLimit": _AzLimit = MyConfigs.ParseBool(item.Value); break;
                    case "EvLimit": _EvLimit = MyConfigs.ParseBool(item.Value); break;
                    case "AzUpperLimit": _AzUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
                    case "AzLowerLimit": _AzLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
                    case "EvUpperLimit": _EvUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
                    case "EvLowerLimit": _EvLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
                    case "TurretWeaponConfigID": _TurretWeaponConfigID = item.Value; break;
                    default: break;
                }
            }
            SetLimitedMotors();
        }
        private void WriteTemplateConfig()
        {
            if (Utils.Common.IsNull(MotorAz)) return;
            StringBuilder sb = new StringBuilder();
            sb.Clear();
            sb.AppendLine("[TurretLimited]");
            sb.AppendLine($"Max_AV_az={_Max_AV_az}");
            sb.AppendLine($"Max_AV_ev={_Max_AV_ev}");
            sb.AppendLine($"Multipy={_Multipy}");
            sb.AppendLine($"AzLimit={_AzLimit}");
            sb.AppendLine($"EvLimit={_EvLimit}");
            sb.AppendLine($"AzUpperLimit={_AzUpperLimit}");
            sb.AppendLine($"AzLowerLimit={_AzLowerLimit}");
            sb.AppendLine($"EvUpperLimit={_EvUpperLimit}");
            sb.AppendLine($"EvLowerLimit={_EvLowerLimit}");
            sb.AppendLine($"TurretWeaponConfigID={_TurretWeaponConfigID}");
            MotorAz.CustomData = sb.ToString();
        }
        private void SetLimitedMotors()
        {
            try
            {
                if (!Utils.Common.IsNull(MotorAz))
                {
                    if (_AzLimit)
                    {
                        MotorAz.LowerLimitRad = MathHelper.ToRadians(_AzLowerLimit);
                        MotorAz.UpperLimitRad = MathHelper.ToRadians(_AzUpperLimit);
                    }
                    else
                    {
                        MotorAz.LowerLimitRad = float.MinValue;
                        MotorAz.UpperLimitRad = float.MaxValue;
                    }
                }
                if (!Utils.Common.IsNullCollection(MotorEvs))
                {
                    if (_EvLimit && !Utils.Common.IsNull(MotorAz?.TopGrid))
                    {
                        foreach (var MotorEv in MotorEvs)
                        {
                            var sign = Math.Sign(MotorAz.TopGrid.WorldMatrix.Left.Dot(MotorEv.WorldMatrix.Up));
                            if (sign > 0)
                            {
                                MotorEv.LowerLimitRad = MathHelper.ToRadians(_EvLowerLimit);
                                MotorEv.UpperLimitRad = MathHelper.ToRadians(_EvUpperLimit);
                            }
                            else if (sign < 0)
                            {
                                MotorEv.LowerLimitRad = MathHelper.ToRadians(_EvUpperLimit) * sign;
                                MotorEv.UpperLimitRad = MathHelper.ToRadians(_EvLowerLimit) * sign;
                            }
                            else
                            {
                                MotorEv.LowerLimitRad = 0;
                                MotorEv.UpperLimitRad = 0;
                            }

                        }
                    }
                    else
                    {
                        foreach (var MotorEv in MotorEvs)
                        {
                            MotorEv.LowerLimitRad = float.MinValue;
                            MotorEv.UpperLimitRad = float.MaxValue;
                        }
                    }
                }
            }
            catch (Exception) { Clear(); }
        }
        public bool TargetInRange_Angle(MyTargetDetected _TargetDetected)
        {
            if (Utils.Common.NullEntity(_TargetDetected?.Entity)) return false;
            return InRangeDirection(_TargetDetected.GetEntityPosition(MotorAz));
        }
        private bool InRangeDirection(Vector3D? Direction)
        {
            if (Direction == null || Vector3D.IsZero(Direction.Value)) return false;
            var local_vector = Vector3D.TransformNormal(Direction.Value, MatrixD.Transpose(MotorAz.WorldMatrix));
            double az, ev;
            Vector3D.GetAzimuthAndElevation(local_vector, out az, out ev);
            if (_AzLimit && (az > MathHelper.ToRadians(_AzUpperLimit) || az < MathHelper.ToRadians(_AzLowerLimit))) return false;
            if (_EvLimit && (ev > MathHelper.ToRadians(_EvUpperLimit) || ev < MathHelper.ToRadians(_EvLowerLimit))) return false;
            return true;
        }
    }

    public sealed partial class MyTurretBinding
    {

        private IMyGridTerminalSystem GridTerminalSystem { get { if (Utils.Common.NullEntity(MotorAz?.CubeGrid)) return null; return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(MotorAz.CubeGrid); } }

        //private Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> motorEvs_WTs { get; } = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
        private Dictionary<string, HashSet<IMyTerminalBlock>> WeaponKinds { get; } = new Dictionary<string, HashSet<IMyTerminalBlock>>();
        private List<string> WeaponKindsNM => WeaponKinds.Keys.ToList();
        private string CurrentWeaponNM;


        private bool UsefulRotorEvs(IMyMotorStator MotorStatorEv)
        {
            if (Utils.Common.NullEntity(MotorStatorEv?.TopGrid) || Utils.Common.NullEntity(MotorAz?.TopGrid) || MotorAz.TopGrid != MotorStatorEv.CubeGrid) return false;
            return Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(MotorStatorEv.WorldMatrix.Up)) > 0.985;
        }
        private void Clear()
        {
            Cameras.Clear();
            Weapons.Clear();
            MotorEvs.Clear();
            //motorEvs_WTs.Clear();
            WeaponKinds.Clear();
            SlaveWeapons.CurrentWeapons?.Clear();

        }
        IMyProgrammableBlock FireManager;
    }
}