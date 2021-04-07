//using Sandbox.Common.ObjectBuilders;
//using Sandbox.ModAPI;
//using SpaceEngineers.Game.ModAPI;
//using SuperBlocks.Controller;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using VRage.Game;
//using VRage.Game.Components;
//using VRage.ObjectBuilders;
//using VRageMath;
//using static SuperBlocks.Definitions.Structures;
//using static SuperBlocks.Utils;

//namespace SuperBlocks.Controllers
//{
//    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorStator), false, "SmallStatorTurretAzimuth", "LargeStatorTurretAzimuth")]
//    public class TurretScript : MyGameLogicComponent
//    {
//        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
//        {
//            base.Init(objectBuilder);
//        }
//        public override void UpdateBeforeSimulation()
//        {
//            base.UpdateBeforeSimulation();
//        }
//    }
//    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_MotorAdvancedStator), false, "SmallTurretAzimuthAdvancedStator", "LargeTurretAzimuthAdvancedStator")]
//    public class AdvancedTurretScript : MyGameLogicComponent
//    {
//        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
//        {
//            base.Init(objectBuilder);
//        }
//        public override void UpdateBeforeSimulation()
//        {
//            base.UpdateBeforeSimulation();
//        }

//        public void Restart()
//        {
//            BasicInit(MotorAz);
//            ReadConfig_Turret_Rotors();

//        }
//        public void RunningAimAtTarget()
//        {
//            RemoveEmptyBlocks();
//            ReferWeapon();
//            MotorAz.Enabled = RotorsEnabled;
//            foreach (var motorEv in MotorEvs) { motorEv.Enabled = RotorsEnabled; }
//            var fire = AutoFire && Enabled && RotorsEnabled;
//            if (!Enabled || !CanRunning) { RunningDefault(); fire = false; }
//            else if (UnderControl)
//            {
//                RunningManual(Common.GetT<IMyShipController>(MotorAz, block => block.IsUnderControl)?.RotationIndicator);
//                fire = false;
//            }
//            else if (ManuelOnly) { RunningDefault(); fire = false; }
//            else
//            {
//                if ((Designer?.HasTarget ?? false) || Designer.Target != AimTarget.Entity)
//                    AimTarget.SetTarget(Designer.Target, MotorAz, false);
//                //AimTarget.SetTarget(BasicInfoService.WcApi.GetAiFocus(MotorAz.CubeGrid), MotorAz, false);
//                MotorsRunningDefault_NotCurrent();
//                ////AimTarget?.Update(MotorAz);
//                //var direction = AimTarget.GetDistance(MotorAz)<WeaponConfig.Trajectory? AimTarget.getpo         /*Vector3D.Normalize(MotorAz.WorldMatrix.Right + MotorAz.WorldMatrix.Forward + MotorAz.WorldMatrix.Up);*/// MyTargetPredict.CalculateDirection_TargetTest(MotorAz, SlaveWeapons.CurrentWeapons, AimTarget, ref WeaponConfig);
//                Vector3D? direction = null;
//                if (AimTarget.GetDistance(MotorAz) < WeaponConfig.Trajectory.MaxTrajectory)
//                {
//                    var p = AimTarget.GetEntityPosition(MotorAz);
//                    if (p.HasValue)
//                        direction = Vector3D.Normalize(p.Value - MotorAz.GetPosition());
//                }
//                fire = fire && MyTargetPredict.CanFireWeapon(Weapons, direction, WeaponConfig.Delta_precious);
//                //MyTargetPredict.CalculateDirection(MotorAz, SlaveWeapons.CurrentWeapons, _AimTarget, ref WeaponConfig, ref VpD_Tn);
//                if (InRangeDirection(direction)) RunningDirection(direction);
//                else RunningDefault();
//            }
//            if (BasicInfoService.WcApi.HasCoreWeapon(Weapons.FirstOrDefault()))
//            {
//                //SetFire(fire);
//                //RunningAutoFire(fire);
//            }
//            else
//            {
//                var block = Common.GetT<IMyTimerBlock>(MotorAz, b => b.CustomName.Contains("weapon") && b.CubeGrid == MotorAz.TopGrid);
//                if (block == null) return;
//                block.Enabled = fire; if (fire) block.Trigger();
//            }
//        }
       
//        public volatile bool IsUpdatingTarget = false;
//        public MyTargetDetected AimTarget { get; } = new MyTargetDetected();
//        public volatile bool AutoFire = false;
//        public volatile bool Enabled = false;
//        public volatile bool RotorsEnabled = true;
//        private void ReferWeapon()
//        {
//            try
//            {
//                //SlaveWeapons.ReferWeapon();
//                //SlaveWeapons.ResetFireRPM(WeaponConfig.RPM);
//                //SetWeaponAmmos();
//                //if (!SlaveWeapons.UnabledFire && AimTarget != null && !Common.NullEntity(AimTarget.Entity))
//                //{
//                //    foreach (var CurrentWeapon in SlaveWeapons.CurrentWeapons)
//                //    {
//                //        if (!BasicInfoService.WcApi.HasCoreWeapon(CurrentWeapon)) continue;
//                //        BasicInfoService.WcApi.SetWeaponTarget(CurrentWeapon, AimTarget.Entity, MyWeaponAndTurretApi.GetWeaponID(CurrentWeapon));
//                //    }
//                //}
//            }
//            catch (Exception) { }
//        }
//        private void SetWeaponAmmos()
//        {
//            //if (SlaveWeapons.UnabledFire) return;
//            //var weapon = SlaveWeapons.CurrentWeapons.FirstOrDefault();
//            //string AmmoName = SlaveWeapons.GetCurrentAmmoNM();
//            //if (Common.NullEntity(weapon) || !BasicInfoService.WcApi.HasCoreWeapon(weapon) || Common.IsNull(AmmoName)) return;
//            //foreach (var wp in SlaveWeapons.CurrentWeapons) BasicInfoService.WcApi.SetActiveAmmo(wp, MyWeaponAndTurretApi.GetWeaponID(wp), AmmoName);
//            //var value = MyWeaponAndTurretApi.GetWeaponCoreDefinition(weapon, AmmoName);
//            //if (value == null) return;
//            //var RPM = WeaponConfig.RPM;
//            //var Delta_precious = WeaponConfig.Delta_precious;
//            //var TimeFixed = WeaponConfig.TimeFixed;
//            //WeaponConfig = value.Value;
//            //WeaponConfig.RPM = RPM;
//            //WeaponConfig.Delta_precious = Delta_precious;
//            //WeaponConfig.TimeFixed = TimeFixed;
//            //WeaponConfig.Trajectory.GravityMultiplier = value.Value.Trajectory.GravityMultiplier * _GravityMultipy;
//        }
//        public void ReadConfig_Turret_Rotors()
//        {
//            if (Common.IsNull(MotorAz)) return;
//            Dictionary<string, Dictionary<string, string>> Config = MyConfigs.CustomDataConfigRead_INI(MotorAz.CustomData);
//            if (Common.IsNullCollection(Config) || !Config.ContainsKey("TurretLimited")) { WriteTemplateConfig(); SetLimitedMotors(); return; }
//            var value = Config["TurretLimited"];
//            foreach (var item in value)
//            {
//                switch (item.Key)
//                {
//                    case "Max_AV_az": _Max_AV_az = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
//                    case "Max_AV_ev": _Max_AV_ev = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
//                    case "Multipy": _Multipy = Math.Max(MyConfigs.ParseFloat(item.Value), 0); break;
//                    case "AzLimit": _AzLimit = MyConfigs.ParseBool(item.Value); break;
//                    case "EvLimit": _EvLimit = MyConfigs.ParseBool(item.Value); break;
//                    case "AzUpperLimit": _AzUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
//                    case "AzLowerLimit": _AzLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _AzLowerLimitMax, _AzUpperLimitMax); break;
//                    case "EvUpperLimit": _EvUpperLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
//                    case "EvLowerLimit": _EvLowerLimit = MathHelper.Clamp(MyConfigs.ParseFloat(item.Value), _EvLowerLimitMax, _EvUpperLimitMax); break;
//                    case "GravityMultipy": _GravityMultipy = MyConfigs.ParseFloat(item.Value); break;
//                    case "TurretWeaponConfigID": _TurretWeaponConfigID = item.Value; break;
//                    default: break;
//                }
//            }
//            SetLimitedMotors();
//        }
//        public bool TargetInRange_Angle(MyTargetDetected _TargetDetected)
//        {
//            if (Common.NullEntity(_TargetDetected?.Entity)) return false;
//            //return InRangeDirection(MyTargetPredict.CalculateDirection_TargetTest(MotorAz, SlaveWeapons.CurrentWeapons, _TargetDetected, ref WeaponConfig));
//            return InRangeDirection(_TargetDetected.GetEntityPosition(MotorAz));
//        }
//        public void SetConfig(ConcurrentDictionary<string, ConcurrentDictionary<string, string>> Configs)
//        {
//            try
//            {
//                //if (SlaveWeapons.UnabledFire || Configs.ContainsKey(_TurretWeaponConfigID))
//                //    WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, _TurretWeaponConfigID);
//                //else if (BasicInfoService.WcApi.HasCoreWeapon(SlaveWeapons.CurrentWeapons.First()))
//                //    WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultWeaponCoreWeapon");
//                //else if (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("direct") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("Plasam") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("laser") || SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId.Contains("plasam"))
//                //    WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "EnergyWeapon");
//                //else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeGatlingTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallGatlingGun))
//                //{
//                //    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
//                //    {
//                //        case null:
//                //        case "":
//                //        case "SmallGatlingTurret":
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_LargeWeapon");
//                //            break;
//                //        default:
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
//                //            break;
//                //    }
//                //}
//                //else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeMissileTurret) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncher) || (SlaveWeapons.CurrentWeapons.First() is IMySmallMissileLauncherReload))
//                //{
//                //    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
//                //    {
//                //        case null:
//                //        case "":
//                //        case "LargeMissileLauncher":
//                //        case "SmallMissileTurret":
//                //        case "SmallRocketLauncherReload":
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensRocketWeapon");
//                //            break;
//                //        default:
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
//                //            break;
//                //    }
//                //}
//                //else if ((SlaveWeapons.CurrentWeapons.First() is IMyLargeInteriorTurret))
//                //{
//                //    switch (SlaveWeapons.CurrentWeapons.First().BlockDefinition.SubtypeId)
//                //    {
//                //        case "LargeInteriorTurret":
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "KeensProjectile_SmallWeapon");
//                //            break;
//                //        default:
//                //            WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
//                //            break;
//                //    }
//                //}
//                //else
//                //    WeaponConfig = MyWeaponParametersConfig.CreateFromConfig(Configs, "DefaultTurretWeaponConfig");
//            }
//            catch (Exception) { }
//        }
//        private float _Max_AV_az = 3;
//        private float _Max_AV_ev = 3;
//        private float _Multipy = 8;
//        private volatile bool _AzLimit = false;
//        private volatile bool _EvLimit = true;
//        private volatile float _AzUpperLimit = 170;
//        private volatile float _AzLowerLimit = -170;
//        private volatile float _EvUpperLimit = 80;
//        private volatile float _EvLowerLimit = -5;
//        private volatile float _GravityMultipy = 1.2f;
//        private const float _AzUpperLimitMax = 170;
//        private const float _AzLowerLimitMax = -170;
//        private const float _EvUpperLimitMax = 89;
//        private const float _EvLowerLimitMax = -89;
//        private string _TurretWeaponConfigID = "DefaultSetup";
//        private Vector2 _MaxSpeed => new Vector2(_Max_AV_ev, _Max_AV_az);
//        private void WriteTemplateConfig()
//        {
//            if (Common.IsNull(MotorAz)) return;
//            StringBuilder sb = new StringBuilder();
//            sb.Clear();
//            sb.AppendLine("[TurretLimited]");
//            sb.AppendLine($"Max_AV_az={_Max_AV_az}");
//            sb.AppendLine($"Max_AV_ev={_Max_AV_ev}");
//            sb.AppendLine($"Multipy={_Multipy}");
//            sb.AppendLine($"AzLimit={_AzLimit}");
//            sb.AppendLine($"EvLimit={_EvLimit}");
//            sb.AppendLine($"AzUpperLimit={_AzUpperLimit}");
//            sb.AppendLine($"AzLowerLimit={_AzLowerLimit}");
//            sb.AppendLine($"EvUpperLimit={_EvUpperLimit}");
//            sb.AppendLine($"EvLowerLimit={_EvLowerLimit}");
//            sb.AppendLine($"GravityMultipy={_GravityMultipy}");
//            sb.AppendLine($"TurretWeaponConfigID={_TurretWeaponConfigID}");
//            MotorAz.CustomData = sb.ToString();
//        }
//        private void SetLimitedMotors()
//        {
//            if (!Common.IsNull(MotorAz))
//            {
//                if (_AzLimit)
//                {
//                    MotorAz.LowerLimitRad = MathHelper.ToRadians(_AzLowerLimit);
//                    MotorAz.UpperLimitRad = MathHelper.ToRadians(_AzUpperLimit);
//                }
//                else
//                {
//                    MotorAz.LowerLimitRad = float.MinValue;
//                    MotorAz.UpperLimitRad = float.MaxValue;
//                }
//            }
//            if (!Common.IsNullCollection(MotorEvs))
//            {
//                if (_EvLimit && !Common.IsNull(MotorAz?.TopGrid))
//                {
//                    foreach (var motorEv in MotorEvs)
//                    {
//                        var sign = Math.Sign(MotorAz.TopGrid.WorldMatrix.Left.Dot(motorEv.WorldMatrix.Up));
//                        if (sign > 0)
//                        {
//                            motorEv.LowerLimitRad = MathHelper.ToRadians(_EvLowerLimit);
//                            motorEv.UpperLimitRad = MathHelper.ToRadians(_EvUpperLimit);
//                        }
//                        else if (sign < 0)
//                        {
//                            motorEv.LowerLimitRad = MathHelper.ToRadians(_EvUpperLimit) * sign;
//                            motorEv.UpperLimitRad = MathHelper.ToRadians(_EvLowerLimit) * sign;
//                        }
//                        else
//                        {
//                            motorEv.LowerLimitRad = 0;
//                            motorEv.UpperLimitRad = 0;
//                        }

//                    }
//                }
//                else
//                {
//                    foreach (var motorEv in MotorEvs)
//                    {
//                        motorEv.LowerLimitRad = float.MinValue;
//                        motorEv.UpperLimitRad = float.MaxValue;
//                    }
//                }
//            }
//        }
//        private bool InRangeDirection(Vector3D? Direction)
//        {
//            if (Direction == null || Vector3D.IsZero(Direction.Value)) return false;
//            var local_vector = Vector3D.TransformNormal(Direction.Value, MatrixD.Transpose(MotorAz.WorldMatrix));
//            double az, ev;
//            Vector3D.GetAzimuthAndElevation(local_vector, out az, out ev);
//            if (_AzLimit && (az > MathHelper.ToRadians(_AzUpperLimit) * 1.01f || az < MathHelper.ToRadians(_AzLowerLimit) * 1.01f)) return false;
//            if (_EvLimit && (ev > MathHelper.ToRadians(_EvUpperLimit) * 1.01f || ev < MathHelper.ToRadians(_EvLowerLimit) * 1.01f)) return false;
//            return true;
//        }
//        private MyWeaponParametersConfig WeaponConfig = default(MyWeaponParametersConfig);
//        private void BasicInit(IMyMotorStator motorAz)
//        {
//            try
//            {
//                Cameras.Clear(); Weapons.Clear(); MotorEvs.Clear(); MotorEvs_WTs.Clear();
//                if (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(motorAz)) return;
//                if (Common.NullEntity(MotorAz)) return;
//                MotorEvs.UnionWith(Common.GetTs<IMyMotorStator>(MotorAz, b => b.TopGrid != null && b.CubeGrid == MotorAz.TopGrid && Math.Abs(MotorAz.TopGrid.WorldMatrix.Left.Dot(b.WorldMatrix.Up)) > 0.985));
//                if (Common.IsNullCollection(MotorEvs)) return;
//                foreach (var ev in MotorEvs)
//                {
//                    if (Common.NullEntity(ev) || (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(ev))) continue;
//                    var weapons = Common.GetTs<IMyTerminalBlock>(ev, b => b.CubeGrid == ev.TopGrid && Common.IsStaticWeapon(b));
//                    if (!Common.IsNullCollection(weapons)) Weapons.UnionWith(weapons);
//                    var cameras = Common.GetTs<IMyCameraBlock>(ev, b => b.CubeGrid == ev.TopGrid);
//                    if (!Common.IsNullCollection(cameras)) Cameras.UnionWith(cameras);
//                }
//                //SlaveWeapons.AllWeapons = Weapons.ToList();
//                ReMountWeapons();
//                Designer = Common.GetT<IMyLargeTurretBase>(motorAz, b => b.CustomName.Contains("Designator"));
//            }
//            catch (Exception) { }
//        }

//        private void RunningDefault() => MotorsRunningDefault();
//        private void RunningDirection(Vector3D? Direction)
//        {
//            if (!CanRunning || Direction == null) { MotorsRunningDefault(); return; }
//            float TurretRotor_Torque = 0;
//            foreach (var Gun_Rotor_Group in MotorEvs_WTs)
//            {
//                var data = MyWeaponAndTurretApi.Get_ArmOfForce_Point(Gun_Rotor_Group);
//                TurretRotor_Torque += MyWeaponAndTurretApi.GetTorque(MotorAz, data, Direction, _Multipy);
//                Gun_Rotor_Group.Key.TargetVelocityRad = MyRotorAPI.RotorRunning(Gun_Rotor_Group.Key, MyWeaponAndTurretApi.RotorDampener(MyWeaponAndTurretApi.GetTorque(Gun_Rotor_Group.Key, data, Direction, _Multipy * 1.4f), Gun_Rotor_Group.Key.TargetVelocityRad, _Max_AV_ev));
//            }
//            TurretRotor_Torque /= MotorEvs_WTs.Count;
//            MotorAz.TargetVelocityRad = MyRotorAPI.RotorRunning(MotorAz, MyWeaponAndTurretApi.RotorDampener(TurretRotor_Torque, MotorAz.TargetVelocityRad, _Max_AV_az));
//        }
//        private void RunningManual(Vector2? Rotation)
//        {
//            if (!CanRunning || Rotation == null) { MotorsRunningDefault(); return; }
//            var finalCtrl = Vector2.Clamp(Rotation.Value * Mult, -_MaxSpeed, _MaxSpeed);
//            MotorAz.TargetVelocityRad = finalCtrl.Y;
//            var MD = MotorAz.TopGrid.WorldMatrix.Left;
//            var TurretEvs = MotorEvs_WTs.Keys.ToList();
//            if (Common.IsNullCollection(TurretEvs)) return;
//            var MountEv = TurretEvs[0];
//            MountEv.TargetVelocityRad = MyRotorAPI.RotorRunning(MountEv, MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -_Max_AV_ev, _Max_AV_ev));// MathHelper.Clamp(-finalCtrl.X * (float)MD.Dot(TurretEvs[0].WorldMatrix.Up), -Config.max_ev, Config.max_ev);
//            for (int index = 1; index < TurretEvs.Count; index++)
//                TurretEvs[index].TargetVelocityRad = MyRotorAPI.RotorRunning(TurretEvs[index], MathHelper.Clamp((MathHelper.WrapAngle(Math.Sign(TurretEvs[0].WorldMatrix.Up.Dot(TurretEvs[index].WorldMatrix.Up)) * MountEv.Angle) - MathHelper.WrapAngle(TurretEvs[index].Angle)) * 35, _Max_AV_ev, _Max_AV_ev));

//        }
//        private void MotorsRunningDefault()
//        {
//            MyRotorAPI.RotorSetDefault(MotorAz, _Max_AV_az);
//            MyRotorAPI.RotorsSetDefault(MotorEvs_WTs?.Keys?.ToList(), _Max_AV_ev);
//        }
//        private void MotorsRunningDefault_NotCurrent() => MyRotorAPI.RotorsSetDefault(MotorEvs.Where(b => !MotorEvs_WTs.ContainsKey(b))?.ToList(), _Max_AV_ev);
//        private void RemoveEmptyMotorEvs()
//        {
//            var motors = MotorEvs_WTs.Keys.Where(Common.NullEntity);
//            if (Common.IsNullCollection(motors)) return;
//            foreach (var motor in motors) MotorEvs_WTs.Remove(motor);

//        }
//        private void RemoveEmptyBlocks()
//        {
//            RemoveEmptyMotorEvs();
//            Cameras.RemoveWhere(Common.NullEntity);
//            Weapons.RemoveWhere(Common.NullEntity);
//        }
//        private void ReMountWeapons()
//        {
//            //if (CurrentWeaponNM == SlaveWeapons.GetCurrentWeaponNM()) return;
//            //CurrentWeaponNM = SlaveWeapons.GetCurrentWeaponNM();
//            //MotorEvs_WTs.Clear();
//            //foreach (var ev in MotorEvs)
//            //{
//            //    if (!MyWeaponAndTurretApi.ActiveRotorBasicFilter(ev)) continue;
//            //    var weapons = SlaveWeapons.CurrentWeapons.Where(b => b.CubeGrid == ev.TopGrid);
//            //    if (Common.IsNullCollection(weapons)) continue;
//            //    MotorEvs_WTs.Add(ev, new HashSet<IMyTerminalBlock>(weapons));
//            //}
//        }
//        public void Clear()
//        {
//            Cameras.Clear();
//            Weapons.Clear();
//            MotorEvs.Clear();
//            MotorEvs_WTs.Clear();
//            //SlaveWeapons.Clear();
//        }
//        private IMyMotorStator MotorAz => Entity as IMyMotorStator;
//        private HashSet<IMyMotorStator> MotorEvs { get; } = new HashSet<IMyMotorStator>();
//        private HashSet<IMyCameraBlock> Cameras { get; } = new HashSet<IMyCameraBlock>();
//        private HashSet<IMyTerminalBlock> Weapons { get; } = new HashSet<IMyTerminalBlock>();
//        private Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> MotorEvs_WTs { get; } = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
//        public bool CanRunning => (!(Common.NullEntity(MotorAz) || Common.IsNullCollection(MotorEvs_WTs)));
//        private bool ManuelOnly => Common.IsNullCollection(Weapons) && CanManuel;
//        private bool UnderControl => CanManuel && !Common.NullEntity(ControlledCamera);
//        private IMyCameraBlock ControlledCamera { get { if (Common.IsNullCollection(Cameras)) return null; IMyCameraBlock cameraBlock = Cameras.FirstOrDefault(b => b.IsActive); return cameraBlock; } }
//        private bool CanManuel => !Common.IsNullCollection(Cameras);
//        private float Mult { get { if (!UnderControl) return 0; return MyAPIGateway.Session.Camera.FovWithZoom; } }

//        private IMyLargeTurretBase Designer;
//        private string CurrentWeaponNM = "";
//    }
//}
