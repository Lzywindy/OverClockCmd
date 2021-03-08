using Sandbox.ModAPI;
using System;
using VRage;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using System.Collections.Generic;
using SIngame = Sandbox.ModAPI.Ingame;
namespace SuperBlocks.Controller
{
    public static partial class RadarSystemAPIs
    {
        public static void RemoveAllEmptyEntities(Dictionary<long, MyTuple<Vector3D, float, Color>> EntitiesInRadar)
        {
            if (EntitiesInRadar == null) return;
            List<long> EntitiesRemoveable = new List<long>();
            foreach (var item in EntitiesInRadar)
                if (MyAPIGateway.Entities.EntityExists(item.Key))
                    EntitiesRemoveable.Add(item.Key);
            foreach (var item in EntitiesRemoveable)
                EntitiesInRadar.Remove(item);
        }
        public static void GetEntityRadarShowList(IMyEntity ent, SIngame.IMyTerminalBlock Me, Dictionary<long, MyTuple<Vector3D, float, Color>> EntitiesInRadar)
        {
            var IMe = (Me as IMyTerminalBlock);
            if (EntitiesInRadar == null || ent == null || IMe == null) return;
            if (ent is Sandbox.Game.Entities.MyPlanet && EntitiesInRadar.ContainsKey(ent.EntityId))
            {
                var planet = ent as Sandbox.Game.Entities.MyPlanet; if (planet == null) return;
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), planet.AverageRadius, Color.Blue));
                return;
            }
            if (ent is IMyVoxelBase)
            {
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 1, Color.Gold));
                return;
            }
            if (Utils.是否是导弹(ent))
            {
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 0, Color.White));
                return;
            }
            if (ent is IMyFloatingObject)
            {
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 0, Color.Yellow));
                return;
            }
            if (ent is IMyMeteor)
            {
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 0, Color.OrangeRed));
                return;
            }
            if (ent is IMyCharacter)
            {
                var ch = ent as IMyCharacter; if (ch == null) return;
                var relate = IMe.GetUserRelationToOwner(ch.EntityId);
                Color color = Color.Gray;
                if (relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner)
                    color = Color.DeepSkyBlue;
                else if (relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.Friends || relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare)
                    color = Color.DarkCyan;
                else if (relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies)
                    color = Color.Red;
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 0, color));
                return;
            }
            if (ent is IMyCubeGrid)
            {
                var grid = ent as IMyCubeGrid;
                EntitiesInRadar.Add(ent.EntityId, new MyTuple<Vector3D, float, Color>(ent.GetPosition(), 0, State2Color(TargetGridState_All(grid, IMe))));
                return;
            }
            return;
        }
    }
    public static partial class RadarSystemAPIs
    {
        /// <summary>
        /// 用于火控雷达筛选目标
        /// </summary>
        /// <param name="ent">被发现的目标</param>
        /// <param name="Me">当前雷达</param>
        /// <param name="EntFilter">
        /// EntFilter.Item1:Meteor,Missile,Charactor,Grid
        /// EntFilter.Item2:Hostile,Netrual
        /// EntFilter.Item3:Small,Large,Static,IgnoreNonFunctional,FunctionalBlocksThreahold
        /// </param>
        /// <returns>
        /// Result.Item1:Trackable
        /// Result.Item2:Fireable
        /// </returns>
        public static bool EntityFilter(IMyEntity ent, SIngame.IMyTerminalBlock Me, MyTuple<MyTuple<bool, bool, bool, bool>, MyTuple<bool, bool>, MyTuple<bool, bool, bool, bool, int>> EntFilter)
        {
            if (Me == null) return false; var IMe = (Me as IMyTerminalBlock);
            if (IMe == null || ent == null || Utils.是否是体素或者行星(ent)) return false;
            if (ent is IMyMeteor && EntFilter.Item1.Item1 && EntitySubFilter_EnabledTarget(ent, Me, 45)) return true;
            if (Utils.是否是导弹(ent) && EntFilter.Item1.Item2 && EntitySubFilter_EnabledTarget(ent, Me, 60)) return true;
            if (ent is IMyCharacter && EntFilter.Item1.Item3)
            {
                var ch = ent as IMyCharacter; if (ch == null) return false;
                var relate = IMe.GetUserRelationToOwner(ch.EntityId);
                if (relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies && EntFilter.Item2.Item1) return true;
                if (relate == VRage.Game.MyRelationsBetweenPlayerAndBlock.Neutral && EntFilter.Item2.Item2) return true;
                return false;
            }
            if (ent is IMyCubeGrid && EntFilter.Item1.Item4)
            {
                var grid = ent as IMyCubeGrid;
                if (EntFilter.Item3.Item4 || Utils.统计网格中通电的方块(grid) < EntFilter.Item3.Item5) return false;
                var Fact_State = TargetGridState(grid, IMe);
                var HasEnmTarget = Fact_State.Item1 && EntFilter.Item2.Item1;
                var HasNeuTarget = Fact_State.Item2 && EntFilter.Item2.Item2;
                if (!(HasEnmTarget && HasNeuTarget)) return false;
                if (EntFilter.Item3.Item1 && grid.GridSizeEnum == VRage.Game.MyCubeSize.Small) return true;
                if (grid.GridSizeEnum == VRage.Game.MyCubeSize.Large)
                {
                    if (EntFilter.Item3.Item2 && grid.IsStatic) return false;
                    if (EntFilter.Item3.Item3 && !grid.IsStatic) return false;
                    return true;
                }
            }
            return false;
        }
        public static bool EntitySubFilter_EnabledTarget(IMyEntity ent, SIngame.IMyTerminalBlock me, double angle)
        {
            if (ent?.Physics == null) return false;
            var k = ent.Physics.LinearVelocity.Dot(me.GetPosition() - ent.GetPosition());
            if (k <= 0) return false;
            k /= ent.Physics.LinearVelocity.Length();
            if (k > Math.Cos(MathHelper.ToRadians(angle))) return true;
            return false;
        }
        public static MyTuple<bool, bool> TargetGridState(IMyCubeGrid Grid, IMyTerminalBlock Me)
        {
            if (Grid == null) return new MyTuple<bool, bool>(false, false);
            bool HasHostileBlock = false;
            bool HasNeutralBlock = false;
            foreach (var item in Grid.BigOwners)
            {
                switch (Me.GetUserRelationToOwner(item))
                {
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        HasNeutralBlock = HasNeutralBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Neutral:
                        HasNeutralBlock = HasNeutralBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies:
                        HasHostileBlock = HasHostileBlock || true;
                        break;
                    default:
                        break;
                }
            }
            return new MyTuple<bool, bool>(HasHostileBlock, HasNeutralBlock);
        }
        public static MyTuple<bool, bool, bool, bool> TargetGridState_All(IMyCubeGrid Grid, IMyTerminalBlock Me)
        {
            if (Grid == null) return default(MyTuple<bool, bool, bool, bool>);
            bool IsOwn = false;
            bool HasFriendlyBlock = false;
            bool HasHostileBlock = false;
            bool HasNeutralBlock = false;
            foreach (var item in Grid.BigOwners)
            {
                switch (Me.GetUserRelationToOwner(item))
                {
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.NoOwnership:
                        HasNeutralBlock = HasNeutralBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Neutral:
                        HasNeutralBlock = HasNeutralBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Enemies:
                        HasHostileBlock = HasHostileBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.FactionShare:
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Friends:
                        HasNeutralBlock = HasFriendlyBlock || true;
                        break;
                    case VRage.Game.MyRelationsBetweenPlayerAndBlock.Owner:
                        HasNeutralBlock = IsOwn || true;
                        break;
                    default:
                        break;
                }
            }
            return new MyTuple<bool, bool, bool, bool>(IsOwn, HasFriendlyBlock, HasHostileBlock, HasNeutralBlock);
        }
        public static Color State2Color(MyTuple<bool, bool, bool, bool> GridState)
        {
            Color color = Color.White;
            if (GridState.Item1)
                color.B = 0;
            if (GridState.Item2)
                color.G = 0;
            if (GridState.Item3)
                color.R = 0;
            if (GridState.Item4)
                color.A /= 2;
            return color;
        }
    }
}