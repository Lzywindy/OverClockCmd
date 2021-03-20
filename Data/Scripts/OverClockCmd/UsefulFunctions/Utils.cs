using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.Game.Entity;
using VRage.ModAPI;
using VRage.Game;
using Sandbox.Game.Entities;
using System.Linq;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class Common
        {
            public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero;
            public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero;
            public static bool IsNullCollection<T>(ICollection<T> Value) => (Value?.Count ?? 0) < 1;
            public static bool IsNullCollection<T>(IEnumerable<T> Value) => (Value?.Count() ?? 0) < 1;
            public static bool IsNull<T>(T Value) where T : class => Value == null;
            public static bool NullEntity<T>(T Ent) where T : IMyEntity => Ent == null || Ent.Closed || Ent.MarkedForClose;
            public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs<T>(gridTerminalSystem, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
            public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { if (gridTerminalSystem == null) return null; List<T> Items = new List<T>(); gridTerminalSystem.GetBlocksOfType<T>(Items, requst); return Items; }
            public static T GetT<T>(IMyTerminalBlock block, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs<T>(block, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
            public static List<T> GetTs<T>(IMyTerminalBlock block, Func<T, bool> requst = null) where T : class { if (block == null || block.CubeGrid == null) return null; List<T> Items = new List<T>(); MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(block.CubeGrid)?.GetBlocksOfType<T>(Items, requst); return Items; }
            public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0) => gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock;
            public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null) { if (gridTerminalSystem == null) return null; return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock); }
            public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class => GetTs(blockGroup, requst).FirstOrDefault();
            public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class { List<T> Items = new List<T>(); if (blockGroup == null) return Items; blockGroup.GetBlocksOfType<T>(Items, requst); return Items; }
            public static List<string> SpliteByQ(string context) => context?.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries)?.ToList() ?? (new List<string>());
            public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController) { Matrix me_matrix; ShipController.Orientation.GetMatrix(out me_matrix); return me_matrix; }
            public static List<IMySlimBlock> GetBlocksFromGrid(IMyCubeGrid Grid, Func<IMySlimBlock, bool> Filter = null)
            {
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                Grid?.GetBlocks(blocks, Filter);
                return blocks;
            }
            public static bool ExceptKeywords(IMyTerminalBlock block) { foreach (var item in BlackList_ShipController) { if (block.BlockDefinition.SubtypeId.Contains(item)) return false; } return true; }
            private static readonly string[] BlackList_ShipController = new string[] { "Hover", "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", };
        }



    }
    public static partial class Utils
    {
        public static Guid MyGuid { get; } = new Guid("5F1A43D3-02D3-C959-2413-5922F4EEB917");
        public static Dictionary<string, string> SolveLine(string configline)
        {
            var temp = configline.Split(new string[] { "{", "}", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (temp == null || temp.Length < 1) return null;
            Dictionary<string, string> Config_Pairs = new Dictionary<string, string>();
            foreach (var item in temp)
            {
                var config_pair = item.Split(new string[] { " ", "\t", "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (config_pair == null || config_pair.Length < 1) continue;
                if (config_pair.Length < 2) { Config_Pairs.Add(config_pair[0], ""); continue; }
                if (config_pair.Length < 3) { Config_Pairs.Add(config_pair[0], config_pair[1]); continue; }
            }
            if (Config_Pairs.Count < 1) return null;
            return Config_Pairs;
        }
        public static List<Dictionary<string, string>> GetConfigLines(string configlines)
        {
            var linesarray = configlines.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (linesarray == null || linesarray.Length < 1) return null;
            List<Dictionary<string, string>> configpaires = new List<Dictionary<string, string>>();
            foreach (var line in linesarray)
            {
                var temp = SolveLine(line);
                if (temp == null) continue;
                configpaires.Add(temp);
            }
            return configpaires;
        }

    }
    public static partial class Utils
    {

    }
    public static partial class Utils
    {




        public static bool BlockInTurretGroup(IMyBlockGroup group, IMyTerminalBlock Me)
        {
            if (group == null) return false;
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks); if (blocks.Count < 1) return false;
            if (!blocks.Contains(Me)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => block is IMyRemoteControl)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => block is IMyUserControllableGun)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => (block is IMyMotorStator) && (block.CustomName.Contains("Turret") || block.CustomName.Contains("turret") || block.CustomName.Contains("Gun") || block.CustomName.Contains("gun")))) return false;
            return true;
        }
        public static Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? CreateTarget(IMyEntity Target, IMyTerminalBlock SensorBlock, Vector3D? Position = null)
        {
            if (Target == null || SensorBlock == null) return null;
            return MyDetectedEntityInfoHelper.Create(MyEntities.GetEntityById(Target.EntityId), SensorBlock.OwnerId, Position ?? Target.GetPosition());
        }
        public static List<IMyTerminalBlock> GetTheNearlyBlock(IMyCubeGrid TargetGrid, IMyTerminalBlock TrackDevice, Func<IMyTerminalBlock, bool> AcceptBlockFilter = null)
        {
            if (TargetGrid == null || TrackDevice == null) return null;
            List<IMyTerminalBlock> blocks = Common.GetTs(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TargetGrid), (IMyTerminalBlock _block) => _block.IsFunctional && NotFriendlyBlock(_block, TrackDevice) && (AcceptBlockFilter?.Invoke(_block) ?? true));
            if (blocks == null || blocks.Count < 1) return null;
            if (blocks.Count < 2) return blocks;
            blocks.Sort((IMyTerminalBlock a, IMyTerminalBlock b) => Math.Sign(GetBlockP(b) - GetBlockP(a)));
            return blocks;
        }
        public static void UpdateBlocks(ref List<IMyTerminalBlock> blocks, IMyTerminalBlock TrackDevice, Func<IMyTerminalBlock, bool> AcceptBlockFilter = null)
        {
            if (blocks == null || blocks.Count < 1 || TrackDevice == null) { blocks = null; return; }
            blocks.RemoveAll((IMyTerminalBlock _block) => !_block.IsFunctional || !NotFriendlyBlock(_block, TrackDevice) || _block.Closed || _block.MarkedForClose || (!(AcceptBlockFilter?.Invoke(_block) ?? true)));
            if (blocks.Count < 1) blocks = null;
        }
        public static int GetBlockP(IMyTerminalBlock _block)
        {
            if (_block == null) return 8;
            if (_block is IMyUserControllableGun && _block.IsFunctional) return 0;
            if (_block is IMyThrust && _block.IsFunctional) return 1;
            if (_block is IMyGyro && _block.IsFunctional) return 2;
            if (_block is IMyMotorSuspension && _block.IsFunctional) return 3;
            if (_block is IMyReactor && _block.IsFunctional) return 4;
            if (_block is IMyBatteryBlock && _block.IsFunctional) return 5;
            if (_block is IMyCockpit && _block.IsFunctional) return 6;
            return 7;
        }
        public static bool NotFriendlyBlock(IMyTerminalBlock test, IMyTerminalBlock refer)
        {
            if (test == null || refer == null) return true;
            var relation = refer.GetUserRelationToOwner(test.OwnerId);
            return (relation != MyRelationsBetweenPlayerAndBlock.FactionShare) && (relation != MyRelationsBetweenPlayerAndBlock.Friends) && (relation != MyRelationsBetweenPlayerAndBlock.Owner);
        }



        public static int CycleInteger(int value, int lower, int upper)
        {
            return (value >= upper) ? lower : (value < lower) ? (upper - 1) : value;
        }


        public static bool NonTargetBlock(IMyTerminalBlock block)
        {
            return block == null || !block.IsFunctional || block.Closed || block.MarkedForClose;
        }

        public static List<IMyModelDummy> GetDummies(IMyEntity entity, string DummiesNMTag)
        {
            Dictionary<string, IMyModelDummy> muzzle_projectiles = new Dictionary<string, IMyModelDummy>();
            List<IMyModelDummy> muzzle_projectiles_l = new List<IMyModelDummy>();
            entity?.Model?.GetDummies(muzzle_projectiles);
            if (Common.IsNullCollection(muzzle_projectiles)) return muzzle_projectiles_l;
            var keys = muzzle_projectiles.Keys.Where(k => k.Contains(DummiesNMTag))?.ToList();
            if (Common.IsNullCollection(keys)) return muzzle_projectiles_l;
            foreach (var key in keys) { muzzle_projectiles_l.Add(muzzle_projectiles[key]); }
            return muzzle_projectiles_l;
        }
    }
    public static partial class Utils
    {
        public static class MyPlanetInfoAPI
        {
            public static MyPlanet GetNearestPlanet(Vector3D MyPosition) { return MyGamePruningStructure.GetClosestPlanet(MyPosition); }
            public static Vector3D? GetNearestPlanetPosition(Vector3D MyPosition) { return MyGamePruningStructure.GetClosestPlanet(MyPosition)?.PositionComp?.GetPosition(); }
            public static double? GetSealevel(Vector3D MyPosition)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                if (planet == null || GetCurrentGravity(MyPosition) == Vector3.Zero) return null;
                return (MyPosition - planet.PositionComp.GetPosition()).Length() - planet.AverageRadius;
            }
            public static double? GetSurfaceHight(Vector3D MyPosition)
            {
                var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                if (planet == null || GetCurrentGravity(MyPosition) == Vector3.Zero) return null;
                var position = planet.GetClosestSurfacePointGlobal(ref MyPosition);
                return Vector3D.Distance(position, MyPosition);
            }
            public static Vector3 GetCurrentGravity(Vector3D Position) { float mult; return MyAPIGateway.Physics.CalculateNaturalGravityAt(Position, out mult); }
            public static Vector3 GetCurrentGravity(Vector3D Position, out float mult) { return MyAPIGateway.Physics.CalculateNaturalGravityAt(Position, out mult); }
            public struct NearestPlanetInfo
            {
                public float PlanetRadius { get; private set; }
                public float AtmoAtt { get; private set; }
                public MyPlanet Planet { get; private set; }
                public static NearestPlanetInfo? CtreateNearestPlanetInfo(Vector3D MyPosition)
                {
                    var planet = MyGamePruningStructure.GetClosestPlanet(MyPosition);
                    if (planet == null) return null;
                    return new NearestPlanetInfo()
                    {
                        Planet = planet,
                        PlanetRadius = planet.AverageRadius,
                        AtmoAtt = planet.AtmosphereAltitude
                    };
                }
            }
        }
        public static class MyTargetEnsureAPI
        {
            public static IMyTerminalBlock GetTargetedBlock(IMyCubeGrid CubeGrid, Sandbox.ModAPI.Ingame.IMyTerminalBlock Detector, Func<IMyTerminalBlock, bool> OtherModBlockFilter = null)
            {
                if (Common.NullEntity(CubeGrid)) return null;
                List<IMyTerminalBlock> Targets = Common.GetTs<IMyTerminalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(CubeGrid), (block) => block.IsFunctional && ((block is IMyUserControllableGun) || (OtherModBlockFilter?.Invoke(block) ?? false)));
                if (Common.IsNullCollection(Targets)) return null;
                if (Targets.Count == 1) { return Targets.First(); }
                return Targets.MinBy((target) => (float)Vector3D.Distance(target.GetPosition(), Detector.GetPosition()));
            }
            public static bool CouldBeEnemy(IMyEntity target, Sandbox.ModAPI.Ingame.IMyTerminalBlock DetectorProcess)
            {
                if (Common.NullEntity(target)) return false;
                return 目标敌对(target, DetectorProcess);
            }
            private static bool 目标敌对(IMyEntity Ent, Sandbox.ModAPI.Ingame.IMyTerminalBlock DetectorProcess)
            {
                if (Ent == null || DetectorProcess == null) return false;
                if (Ent is IMyMeteor || 是否是导弹(Ent)) return 目标朝我靠近(Ent, DetectorProcess);
                if (Ent is IMyCubeGrid)
                {
                    var grid = Ent as IMyCubeGrid;
                    if (grid == null) return false;
                    bool HasHostileBlock = false;
                    foreach (var item in grid.BigOwners)
                    {
                        switch (DetectorProcess.GetUserRelationToOwner(item))
                        {
                            case MyRelationsBetweenPlayerAndBlock.Enemies:
                                HasHostileBlock = HasHostileBlock || true;
                                break;
                            default:
                                break;
                        }
                    }
                    return HasHostileBlock && 网络通电(grid);
                }
                return false;
            }
            private static bool 网络通电(IMyCubeGrid Grid)
            {
                var blocks = Common.GetTs<IMyFunctionalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid), 供能方块);
                return (blocks?.Any(b => b.Enabled && b.IsFunctional) ?? false);
            }
            private static bool 供能方块(IMyFunctionalBlock block)
            {
                if (block is IMyBatteryBlock || block is IMyReactor) return true;
                var str = block.GetType().ToString();
                if (str.Contains("HydrogenEngine") || str.Contains("WindTurbine") || str.Contains("SolarPanel")) return true;
                return false;
            }
            private static bool 目标朝我靠近(IMyEntity Ent, Sandbox.ModAPI.Ingame.IMyTerminalBlock DetectorProcess)
            {
                if (Ent?.Physics == null) return false;
                return Vector3D.Dot(Vector3D.Normalize((DetectorProcess.GetPosition() - Ent.GetPosition())), Ent.Physics.LinearVelocity) > 0.865;
            }
            public static int 统计网格中通电的方块(IMyCubeGrid cubeGrid)
            {
                if (cubeGrid == null) return 0;
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(blocks, (IMySlimBlock block) =>
                {
                    if (block == null) return false;
                    var b_t = block as IMyTerminalBlock;
                    if (b_t == null || !b_t.IsFunctional) return false;
                    var b_f = block as IMyFunctionalBlock;
                    if (b_f == null || !b_f.Enabled) return false;
                    return true;
                });
                return blocks.Count;
            }
            public static bool 是否是陨石(IMyEntity entity) => entity is IMyMeteor;
            public static bool 是否是体素或者行星(IMyEntity entity) => entity is IMyVoxelBase;
            public static bool 是否是导弹(IMyEntity entity) => (entity?.GetType()?.ToString() ?? "") == "Sandbox.Game.Weapons.MyMissile";
            public static bool 是否在范围里(IMyEntity Me, IMyEntity Target, double Range)
            {
                if (Me == null || Target == null) return false;
                return (Me.GetPosition() - Target.GetPosition()).Length() <= Range;
            }
        }
        public static class MyRotorAPI
        {
            public static bool DisabledTerminalBlock<T>(T Block) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock => (Block == null || !Block.IsFunctional);
            public static bool DisabledTerminalBlocks<T>(ICollection<T> Blocks) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock => (Blocks == null || Blocks.Count < 1 || Blocks.All(m => DisabledTerminalBlock(m)));
            public static bool DisabledMotorRotor<T>(T Motor) where T : Sandbox.ModAPI.Ingame.IMyMotorStator => (Motor?.TopGrid == null || !Motor.IsFunctional);
            public static bool DisabledMotorRotors<T>(ICollection<T> Motors) where T : Sandbox.ModAPI.Ingame.IMyMotorStator => (Motors == null || Motors.Count < 1 || Motors.All(m => DisabledMotorRotor(m)));
            public static void RotorSetDefault<T>(T Motor, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                if (Motor == null || Motor.TopGrid == null) return;
                Motor.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -Max_Speed, Max_Speed);
            }
            public static void RotorsSetDefault<T>(ICollection<T> Motors, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                if (Common.IsNullCollection(Motors)) return;
                foreach (var Motor in Motors) { RotorSetDefault(Motor, Max_Speed); }
            }
            public static float RotorRunning<T>(T Motor, float value) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                var upper = Motor.UpperLimitRad;
                var lower = Motor.LowerLimitRad;
                if (value > 0)
                {
                    if (upper >= float.MaxValue) return value;
                    if (Motor.Angle >= upper) return 0;
                    return value;
                }
                else if (value < 0)
                {
                    if (lower <= float.MinValue) return value;
                    if (Motor.Angle <= lower) return 0;
                    return value;
                }
                return 0;
            }
        }
    }
}
