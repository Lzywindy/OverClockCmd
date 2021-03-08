﻿using System;
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
        public static bool IsNull(Vector3? Value) => Value == null || Value.Value == Vector3.Zero;
        public static bool IsNull(Vector3D? Value) => Value == null || Value.Value == Vector3D.Zero;
        public static bool IsNullCollection<T>(ICollection<T> Value) => Value == null || Value.Count < 1;
        public static bool IsNull<T>(T Value) where T : class => Value == null;
        public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { List<T> Items = GetTs<T>(gridTerminalSystem, requst); if (IsNullCollection(Items)) return null; else return Items.First(); }
        public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class { if (gridTerminalSystem == null) return null; List<T> Items = new List<T>(); gridTerminalSystem.GetBlocksOfType<T>(Items, requst); return Items; }
        public static Matrix GetWorldMatrix(IMyTerminalBlock ShipController) { Matrix me_matrix; ShipController.Orientation.GetMatrix(out me_matrix); return me_matrix; }
        public static bool ExceptKeywords(IMyTerminalBlock block) { foreach (var item in BlackList_ShipController) { if (block.BlockDefinition.SubtypeId.Contains(item)) return false; } return true; }
        private static readonly string[] BlackList_ShipController = new string[] { "Hover", "Torpedo", "Torp", "Payload", "Missile", "At_Hybrid_Main_Thruster_Large", "At_Hybrid_Main_Thruster_Small", };
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
        public static IMyTerminalBlock GetBlock(IMyGridTerminalSystem gridTerminalSystem, long EntIds = 0)
        {
            return gridTerminalSystem?.GetBlockWithId(EntIds) as IMyTerminalBlock;
        }
        public static List<IMyTerminalBlock> GetBlocks(IMyGridTerminalSystem gridTerminalSystem, List<long> EntIds = null)
        {
            if (gridTerminalSystem == null) return null;
            return EntIds?.ConvertAll(id => gridTerminalSystem.GetBlockWithId(id) as IMyTerminalBlock);
        }
        public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class
        {
            List<T> Items = GetTs(blockGroup, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class
        {
            if (blockGroup == null) return null;
            List<T> Items = new List<T>();
            blockGroup.GetBlocksOfType<T>(Items, requst);
            return Items;
        }
    }
    public static partial class Utils
    {
        /// <summary>
        /// 火控弹道计算
        /// 不涉及风阻和风向的考虑
        /// </summary>
        /// <param name="TargetPosition">目标位置</param>
        /// <param name="SelfPosition">自己位置</param>
        /// <param name="TargetVelocity">目标速度</param>
        /// <param name="SelfVelocity">自己速度</param>
        /// <param name="Gravity">重力</param>
        /// <param name="V_project_length">炮弹初速度</param>
        /// <param name="V_project">炮弹指向</param>
        /// <param name="t_n">炮弹经历的时间</param>
        /// <param name="CannonDirection">炮口指向</param>
        /// <param name="CalcCount"></param>
        /// <param name="Delta_Time">时间乘倍</param>
        /// <param name="Delta_Precious"></param>
        public static void GunDirection(Vector3D? TargetPosition, Vector3D? SelfPosition, Vector3D? TargetVelocity, Vector3D? SelfVelocity, Vector3D? Gravity, double V_project_length, ref Vector3D? V_project_direction, ref double? t_n, Vector3D CannonDirection, int CalcCount = 4, float Delta_Time = 1, float Delta_Precious = 1)
        {
            if (!TargetPosition.HasValue || !SelfPosition.HasValue) return;
            var v_dis = TargetPosition.Value - SelfPosition.Value;
            if (v_dis == Vector3.Zero) return;
            if (!TargetVelocity.HasValue) { v_dis.Normalize(); V_project_direction = v_dis; return; }
            var v_relative = TargetVelocity.Value - (SelfVelocity ?? Vector3D.Zero);
            Delta_Precious = MathHelper.Clamp(Delta_Precious, 0.01f, 100f); V_project_length = Math.Max(V_project_length, 100f); Delta_Time = MathHelper.Clamp(Delta_Time, 0.9f, 1.1f);
            var min = v_dis.Length() / Math.Max(V_project_length + Vector3D.Dot(Vector3D.Normalize(v_dis), TargetVelocity.Value), 1f);
            if (Gravity.HasValue && Gravity != Vector3.Zero)
            {
                var g_l = Gravity.Value.LengthSquared(); var v_l = v_relative.LengthSquared();
                var b = -2 * v_relative.Dot(Gravity.Value) / g_l; var c = (4 * v_l - 2 * Gravity.Value.Dot(v_dis) - 4 * V_project_length * V_project_length) / g_l;
                var d = 4 * v_relative.Dot(v_dis) / g_l; var e = 4 * v_l / g_l;
                int count = 0;
                if (!V_project_direction.HasValue && CannonDirection != Vector3D.Zero)
                    V_project_direction = Vector3D.Normalize(CannonDirection);
                if (!t_n.HasValue || t_n.Value < 0)
                    t_n = Solve_Subfunction(min, b, c, d, e);
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
    }
    public static partial class Utils
    {
        /// <summary>
        /// 得的该模块的末端矩阵
        /// </summary>
        /// <param name="TerminalBlock">本模块</param>
        /// <returns>矩阵列表</returns>
        public static List<MatrixD> GetFinalMatrixs(IMyTerminalBlock TerminalBlock)
        {
            if (TerminalBlock == null) return null;
            var ent = TerminalBlock as MyEntity;
            if (ent == null) return null;
            List<MatrixD> matrixs = new List<MatrixD>();
            try
            {
                if (TerminalBlock is IMyLargeTurretBase)
                {
                    foreach (var key in ent.Subparts.Keys)
                        foreach (var keyB in ent.Subparts[key].Subparts.Keys)
                            matrixs.Add(ent.Subparts[key].Subparts[keyB].WorldMatrix);
                }
                else matrixs.Add(TerminalBlock.WorldMatrix);
            }
            catch (Exception) { }
            return matrixs;
        }
        public static Vector3 ProcessDampeners(IMyTerminalBlock TerminalBlock, Vector3? InitAngularDampener = null, Vector3? AngularDampeners = null)
        {
            var temp = Vector3.TransformNormal(GetAngularVelocity(TerminalBlock) ?? Vector3.Zero, Matrix.Transpose(TerminalBlock.WorldMatrix));
            var a_temp = Vector3.Abs(temp);
            var _InitAngularDampener = InitAngularDampener ?? (new Vector3(70, 50, 10));
            return Vector3.Clamp(a_temp * temp * _InitAngularDampener / 4, -_InitAngularDampener, _InitAngularDampener) * (AngularDampeners ?? Vector3.One);
        }
        public static Vector3? GetAngularVelocity(IMyTerminalBlock TerminalBlock)
        {
            if (TerminalBlock == null || TerminalBlock.CubeGrid == null || TerminalBlock.CubeGrid.Physics == null) return null;
            return TerminalBlock.CubeGrid.Physics.AngularVelocity;
        }
        public static Vector3? GetLinearVelocity(IMyTerminalBlock TerminalBlock, bool EnableToGet = true)
        {
            if (TerminalBlock == null || TerminalBlock.CubeGrid == null || TerminalBlock.CubeGrid.Physics == null || (!EnableToGet)) return null;
            return TerminalBlock.CubeGrid.Physics.LinearVelocity;
        }
        public static Vector3? GetGravity(IMyTerminalBlock TerminalBlock, bool EnableToGet = true)
        {
            if (TerminalBlock == null || TerminalBlock.CubeGrid == null || TerminalBlock.CubeGrid.Physics == null || (!EnableToGet) || TerminalBlock.CubeGrid.Physics.Gravity == Vector3.Zero) return null;
            return TerminalBlock.CubeGrid.Physics.Gravity;
        }
        public static Vector3 ProjectLinnerVelocity_CockpitForward(IMyTerminalBlock TerminalBlock, bool EnableToGet = true, bool IgnoreForwardVelocity = false)
        {
            if (TerminalBlock == null) return Vector3.Zero;
            var LinearVelocity = GetLinearVelocity(TerminalBlock, EnableToGet) ?? Vector3.Zero;
            if (IgnoreForwardVelocity)
                return PoseProcessFuncs.ProjectOnPlane(LinearVelocity, TerminalBlock.WorldMatrix.Forward);
            else
                return LinearVelocity;
        }
        public static double ErrorFunction(Vector3D V_target, Vector3D P_target, Vector3D V_me, Vector3D V_projector, Vector3D P_me, double t, Vector3D? Gravity = null)
        {
            var P_target_1 = V_target * t + P_target;
            var P_target_2 = (V_me + V_projector + (Gravity.HasValue ? (Gravity.Value * 0.5f * t) : Vector3D.Zero)) * t + P_me;
            return (P_target_1 - P_target_2).Length();
        }
        public static double Solve_Subfunction(double t, double b, double c, double d, double e)
        {
            var t_2 = t * t; var t_3 = t * t_2; var t_4 = t * t_3;
            var t_b = 4 * t_3 + 3 * t_2 * b + 2 * c * t + d;
            if (t_b == 0) return 0;
            var t_v = t_4 + b * t_3 + c * t_2 + d * t + e;
            return t - t_v / t_b;
        }
        public static Vector3D? Solve_Direction(double t, Vector3D v_r, Vector3D dis, Vector3D? g = null, double Delta_Time = 1, bool InSimulate = false)
        {
            if (t <= 0) return null; if (!InSimulate) t *= Delta_Time;
            var velocity = (dis / t + v_r - (g.HasValue ? (g.Value * t / 2) : Vector3D.Zero));
            if (velocity == Vector3D.Zero) return null; velocity = Vector3D.Normalize(velocity);
            return velocity;
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
        public static bool 是否是角色或者陨石(IMyEntity entity)
        {
            if (entity == null) return false;
            if (entity is IMyCharacter || entity is IMyMeteor) return true;
            return false;
        }
        public static bool 是否是体素或者行星(IMyEntity entity)
        {
            if (entity == null) return false;
            if (entity is IMyVoxelBase) return true;
            return false;
        }
        public static bool 是否是导弹(IMyEntity entity)
        {
            if (entity == null) return false;
            if (entity.GetType().ToString() == "Sandbox.Game.Weapons.MyMissile") return true;
            return false;
        }
        public static bool 是否在范围里(IMyEntity Me, IMyEntity Target, double Range)
        {
            if (Me == null || Target == null) return false;
            return (Me.GetPosition() - Target.GetPosition()).Length() <= Range;
        }
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
            List<IMyTerminalBlock> blocks = GetTs(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TargetGrid), (IMyTerminalBlock _block) => _block.IsFunctional && NotFriendlyBlock(_block, TrackDevice) && (AcceptBlockFilter?.Invoke(_block) ?? true));
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
        public static double CalcDistanceBetween(IMyEntity target, IMyEntity me)
        {
            return (target.GetPosition() - me.GetPosition()).Length();
        }
        public static double CalcSDistanceBetween(IMyEntity target, IMyEntity me)
        {
            return (target.GetPosition() - me.GetPosition()).Length();
        }
        public static List<IMyTerminalBlock> GetCurrentEntityBlocks(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> Filter = null)
        {
            HashSet<IMyCubeGrid> cubeGrids = new HashSet<IMyCubeGrid>();
            MyAPIGateway.GridGroups.GetGroup(Me.CubeGrid, GridLinkTypeEnum.Logical, cubeGrids);
            cubeGrids.Add(Me.CubeGrid);
            HashSet<IMySlimBlock> blocks = new HashSet<IMySlimBlock>();
            foreach (var cubeGrid in cubeGrids)
            {
                List<IMySlimBlock> slimBlocks = new List<IMySlimBlock>();
                cubeGrid.GetBlocks(slimBlocks, block => (block as IMyTerminalBlock) != null && (Filter?.Invoke(block as IMyTerminalBlock) ?? true));
                blocks.UnionWith(slimBlocks);
            }
            return blocks.ToList().ConvertAll(block => block as IMyTerminalBlock);
        }
        public static void RotorSetDefault<T>(T Motor, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
        {
            if (Motor == null || Motor.TopGrid == null) return;
            Motor.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -Max_Speed, Max_Speed);
        }
        public static void RotorsSetDefault<T>(List<T> Motors, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
        {
            if (Motors == null || Motors.Count < 1) return;
            foreach (var Motor in Motors) { RotorSetDefault(Motor, Max_Speed); }
        }
        public static bool DisabledTerminalBlock<T>(T Block) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock
        {
            return (Block == null || !Block.IsFunctional);
        }
        public static bool DisabledTerminalBlocks<T>(ICollection<T> Blocks) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock
        {
            return (Blocks == null || Blocks.Count < 1 || Blocks.All(m => DisabledTerminalBlock(m)));
        }
        public static bool DisabledMotorRotor<T>(T Motor) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
        {
            return (Motor == null || Motor.TopGrid == null || !Motor.IsFunctional);
        }
        public static bool DisabledMotorRotors<T>(ICollection<T> Motors) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
        {
            return (Motors == null || Motors.Count < 1 || Motors.All(m => DisabledMotorRotor(m)));
        }
        public static bool IsNullCollection<T>(IEnumerable<T> Value)
        {
            return Value == null || Value.Count() < 1;
        }
        public static int CycleInteger(int value, int lower, int upper)
        {
            return (value >= upper) ? lower : (value < lower) ? (upper - 1) : value;
        }
        public static void RemoveNullBlocks(List<IMyTerminalBlock> Blocks)
        {
            if (IsNullCollection(Blocks)) return;
            if (Blocks.Count > 0)
                Blocks.RemoveAll(block => block == null || block.Closed || block.MarkedForClose);
        }
        public static void RemoveNullConfigs<T>(List<IMyTerminalBlock> Blocks, Dictionary<long, T> BlockConfigs)
        {
            if (IsNullCollection(Blocks)) return;
            Blocks.RemoveAll(block => block == null || block.Closed || block.MarkedForClose);
            if (IsNullCollection(Blocks)) { BlockConfigs.Clear(); return; }
            var totalids = new HashSet<long>(BlockConfigs.Keys);
            totalids.ExceptWith(Blocks.ConvertAll(block => block.EntityId));
            foreach (var totalid in totalids) BlockConfigs.Remove(totalid);
        }
        public static void AddConfigs<T>(IMyTerminalBlock Me, Dictionary<long, T> BlockConfigs, T Config)
        {
            if (IsNull(Me) || IsNullCollection(BlockConfigs)) return;
            if (!BlockConfigs.ContainsKey(Me.EntityId))
                BlockConfigs.Add(Me.EntityId, Config);
        }
        public static void RemoveConfig<T>(IMyTerminalBlock Me, Dictionary<long, T> BlockConfigs)
        {
            if (IsNull(Me) || IsNullCollection(BlockConfigs)) return;
            if (BlockConfigs.ContainsKey(Me.EntityId))
                BlockConfigs.Remove(Me.EntityId);
        }
        public static int GetIndex<T>(List<T> entities, T entity, bool Inc, bool Invert) where T : IMyEntity
        {
            if (IsNullCollection(entities)) return -1;
            if (entity == null) return Invert ? entities.Count - 1 : 0;
            var index = entities.IndexOf(entity);
            if (index < 0) return -1;
            return CycleInteger(index + ((!Inc) ? 0 : Invert ? -1 : 1), 0, entities.Count);
        }
        public static bool NullEntity<T>(T Ent) where T : IMyEntity
        {
            return Ent == null || Ent.Closed || Ent.MarkedForClose;
        }
        public static void CycleEntity<T>(int mode, List<T> Collection, ref int index)
        {
            if (IsNullCollection(Collection)) return;
            switch (mode)
            {
                case 1: index = 0; return;
                case 2: index = Collection.Count - 1; return;
                case 3: index = CycleInteger(index + 1, 0, Collection.Count); return;
                case 4: index = CycleInteger(index - 1, 0, Collection.Count); return;
                default: index = CycleInteger(index, 0, Collection.Count); return;
            }
        }
        public static bool NonTargetBlock(IMyTerminalBlock block)
        {
            return block == null || !block.IsFunctional || block.Closed || block.MarkedForClose;
        }
        public static List<T> GetBlocksFromGrid<T>(IMyCubeGrid Grid, Func<IMySlimBlock, bool> Filter = null) where T : class
        {
            if (Grid == null) return null;
            List<IMySlimBlock> blocks = new List<IMySlimBlock>();
            Grid.GetBlocks(blocks, Filter);
            return blocks.ConvertAll(b => b as T);
        }
        public static List<IMyModelDummy> GetDummies(IMyEntity entity, string DummiesNMTag)
        {
            Dictionary<string, IMyModelDummy> muzzle_projectiles = new Dictionary<string, IMyModelDummy>();
            List<IMyModelDummy> muzzle_projectiles_l = new List<IMyModelDummy>();
            entity?.Model?.GetDummies(muzzle_projectiles);
            if (IsNullCollection(muzzle_projectiles)) return muzzle_projectiles_l;
            var keys = muzzle_projectiles.Keys.Where(k => k.Contains(DummiesNMTag));
            if (IsNullCollection(keys)) return muzzle_projectiles_l;
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
    }
}
