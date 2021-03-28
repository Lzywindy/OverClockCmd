using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
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
            private static bool IsPowerProvideBlock(IMyFunctionalBlock block) => (block is IMyBatteryBlock || block is IMyReactor || block.GetType().ToString().Contains("HydrogenEngine") || block.GetType().ToString().Contains("WindTurbine") || block.GetType().ToString().Contains("SolarPanel"));
            public static int StatisticPoweredBlocks(IMyCubeGrid cubeGrid)
            {
                if (cubeGrid == null) return 0;
                List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(cubeGrid)?.GetBlocksOfType(blocks, (IMyTerminalBlock block) => { if (block == null || !block.IsFunctional) return false; var b_f = block as IMyFunctionalBlock; if (b_f == null || !b_f.Enabled) return false; return true; });
                return blocks.Count;
            }
            public static bool IsMeteor(IMyEntity entity) => entity is IMyMeteor;
            public static bool IsVoxel(IMyEntity entity) => entity is IMyVoxelBase;
            public static bool IsMissile(IMyEntity entity) => (entity?.GetType()?.ToString() ?? "") == "Sandbox.Game.Weapons.MyMissile";
            public static bool IsInRange(IMyEntity Me, IMyEntity Target, double Range) { if (Me == null || Target == null) return false; return (Me.GetPosition() - Target.GetPosition()).Length() <= Range; }
            public static bool CouldBeEnemy(IMyEntity target, IMyTerminalBlock DetectorProcess)
            {
                if (Common.NullEntity(target)) return false;
                return IsEnemy(target, DetectorProcess);
            }
            public static bool IsEnemy(IMyEntity Ent, IMyTerminalBlock DetectorProcess)
            {
                if (Ent == null || DetectorProcess == null) return false;
                if (Ent is IMyMeteor || IsMissile(Ent)) return ClosingToMe(Ent, DetectorProcess);
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
                    return HasHostileBlock && PoweredGrid(grid);
                }
                return false;
            }
            public static bool PoweredGrid(IMyCubeGrid Grid) => (Common.GetTs<IMyFunctionalBlock>(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Grid), IsPowerProvideBlock)?.Any(b => b.Enabled && b.IsFunctional) ?? false);
            public static bool ClosingToMe(IMyEntity Ent, IMyTerminalBlock DetectorProcess)
            {
                if (Ent?.Physics == null) return false;
                return Vector3D.Dot(Vector3D.Normalize((DetectorProcess.GetPosition() - Ent.GetPosition())), Ent.Physics.LinearVelocity) > 0.865;
            }
        }
    }
}
