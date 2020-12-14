using System;
using VRage.Game.Components;
using Sandbox.Common.ObjectBuilders;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using Sandbox.ModAPI.Interfaces;
using Sandbox;
using Sandbox.Game.Entities;
/*  
  Welcome to Modding API. This is second of two sample scripts that you can modify for your needs,
  in this case simple script is prepared that will alter behaviour of sensor block
  This type of scripts will be executed automatically  when sensor (or your defined) block is added to world
 */
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_RemoteControl), false, "PRC_Block_Large", "PRC_Block_Small")]
    public partial class TargetLocker_CameraBlock : MySuperBlockProgram
    {
        HashSet<IMyEntity> IgnoreItems = new HashSet<IMyEntity>();
        protected override void FunctionAddOn()
        {
            base.FunctionAddOn();
            InitAction += CheckEnabled;
            SetupInfos += RemoteControl_AppendingCustomInfo;
            Running100Action += CheckEnabled;
        }
        float distance = 5000;
        //public IHitInfo Target_Internal;
        IMyRemoteControl remoteControl;
        IMyCameraBlock cameraBlock;
        public void CheckEnabled()
        {
           
            if (remoteControl == null) { remoteControl = Entity as IMyRemoteControl; if (remoteControl == null) return; }
            else remoteControl.RefreshCustomInfo();
            var item = Entity.GetTopMostParent();
            if (item != null) IgnoreItems.Add(item);
            try { cameraBlock = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(remoteControl.CubeGrid).GetBlockWithId(remoteControl.GetValue<long>("CameraList")) as IMyCameraBlock; } catch (Exception) { }
        }
        private void RemoteControl_AppendingCustomInfo(IMyTerminalBlock arg1, System.Text.StringBuilder arg2)
        {
            if (arg1.EntityId != Me.EntityId) return;
            arg2.Clear();
            if (Target == null)
                arg2.Append($"No Target!\n\r");
            else
                arg2.Append($"Target:{Target.DisplayName}, ID:{Target.EntityId}\n\r");
            if (BasicInfoService.SetupComplete)
                arg2.Append($"Service Online\n\r");
        }
        private bool CheckDisabled(long entid)
        {
            var blocks = Utils.GetTs(GridTerminalSystem, (IMyCubeBlock block) => (block is IMyMechanicalConnectionBlock));
            HashSet<long> ForbiddenIDs = new HashSet<long>();
            if (CurrentGrid == null) return false;
            ForbiddenIDs.Add(CurrentGrid.EntityId);
            if (blocks == null) return ForbiddenIDs.Contains(entid);
            foreach (var block in blocks)
            {
                if (blocks is IMyMechanicalConnectionBlock)
                {
                    var topgrid = (blocks as IMyMechanicalConnectionBlock).TopGrid;
                    if (topgrid == null) continue;
                    ForbiddenIDs.Add(topgrid.EntityId);
                }
            }
            return ForbiddenIDs.Contains(entid);
        }
        HashSet<long> DisabledID { get; } = new HashSet<long>();
    }
    public partial class TargetLocker_CameraBlock
    {
        private IMyEntity Target;
        private List<IMyTerminalBlock> blocks => Utils.GetTheNearlyBlock(Target as IMyCubeGrid, Me);
        private int index = 0;
        public IMyCubeBlock GetCurrentBlock()
        {
            var list = blocks;
            if (list == null || list.Count < 1) return null;
            index = (index < 0) ? (list.Count - 1) : (index % list.Count);
            return list[index];
        }
        public IMyCubeBlock GetNextBlock()
        {
            var list = blocks;
            if (list == null || list.Count < 1) return null;
            index = (index < 0) ? (list.Count - 1) : (index + 1 % list.Count);
            return list[index];
        }
        private static Random random { get; } = new Random(34652223);
        public static void InitHitInfo(IHitInfo hitInfo, TargetLocker_CameraBlock targetlocker)
        {
            if (targetlocker == null) return;
            if (hitInfo == null) { targetlocker.Target = null; return; }
            targetlocker.Target = hitInfo.HitEntity;
        }
        public static bool EnabledTargetLocker(IMyTerminalBlock block)
        {
            if (block?.GameLogic?.GetAs<TargetLocker_CameraBlock>() == null) return false;
            return true;
        }
        public static float Get_DetectedDistance(IMyTerminalBlock block)
        {
            if (!EnabledTargetLocker(block)) return 0;
            return Math.Max(block.GameLogic.GetAs<TargetLocker_CameraBlock>().distance, 50);
        }
        public static void Set_DetectedDistance(IMyTerminalBlock block, float Distance)
        {
            if (!EnabledTargetLocker(block)) return;
            block.GameLogic.GetAs<TargetLocker_CameraBlock>().distance = Math.Max(Distance, 50);
        }
        public static void CycleTargetPart(IMyTerminalBlock block)
        {
            var logic = block?.GameLogic?.GetAs<TargetLocker_CameraBlock>();
            if (logic == null) return;
            var list = logic.blocks;
            if (list == null) { logic.index = -1; return; }
            logic.index = (logic.index + 1) % list.Count;
        }
        public static void LockOnTarget(IMyTerminalBlock block)
        {
            if (!EnabledTargetLocker(block)) return;
            var logic = block.GameLogic.GetAs<TargetLocker_CameraBlock>();
            var entities = GetDirectTarget(logic);
            if (entities == null) { InitHitInfo(null, logic); return; }
            logic.Target = entities[0];
        }
        private static List<IMyEntity> GetDirectTarget(TargetLocker_CameraBlock logic)
        {
            if (logic == null || logic.Me == null || logic.CurrentGrid == null || logic.cameraBlock == null || !logic.cameraBlock.Enabled) return null;
            HashSet<IMyEntity> entities = new HashSet<IMyEntity>();
            MyAPIGateway.Entities.GetEntities(entities, (IMyEntity ent) =>
            {
                if (ent is IMyVoxelBase) return false;
                if (ent is IMyFloatingObject) return false;
                var direct = ent.GetPosition() - logic.cameraBlock.GetPosition();
                var length = direct.Normalize();
                if (length < logic.CurrentGrid.GridSize) return false;
                if (MyMath.AngleBetween(direct, logic.cameraBlock.WorldMatrix.Forward) > MathHelper.ToRadians(2)) return false;
                if (logic.CheckDisabled(ent.EntityId)) return false;
                return true;
            });
            if (entities.Count < 1) return null;
            var list = new List<IMyEntity>(entities);
            if (list.Count < 2) return list;
            list.Sort((IMyEntity a, IMyEntity b) =>
            {
                return Math.Sign((Utils.CalcDistanceBetween(a, logic.cameraBlock) - Utils.CalcDistanceBetween(b, logic.cameraBlock))
                    + MathHelper.ToDegrees(MyMath.AngleBetween(a.GetPosition() - logic.cameraBlock.GetPosition(), logic.cameraBlock.WorldMatrix.Forward)
                    - MyMath.AngleBetween(b.GetPosition() - logic.cameraBlock.GetPosition(), logic.cameraBlock.WorldMatrix.Forward)) * logic.distance);
            });
            return list;
        }
        public static void UnlockTarget(IMyTerminalBlock block)
        {
            if (!EnabledTargetLocker(block)) return;
            InitHitInfo(null, block.GameLogic.GetAs<TargetLocker_CameraBlock>());
        }
        public static Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? Get_CurrentTarget(IMyTerminalBlock block)
        {
            var logic = block?.GameLogic?.GetAs<TargetLocker_CameraBlock>();
            var target = logic?.Target;
            var cameraBlock = logic?.cameraBlock;
            if (target == null || cameraBlock == null) return null;
            var ent = MyEntities.GetEntityById(target.EntityId);
            if (ent == null) return null;
            if (!(target is IMyCubeGrid)) { return MyDetectedEntityInfoHelper.Create(ent, block.OwnerId, target.GetPosition()); }
            return MyDetectedEntityInfoHelper.Create(ent, block.OwnerId, logic.GetCurrentBlock()?.GetPosition());
        }
        public static Vector3D? GetHitPosition(IMyTerminalBlock block)
        {
            if (!EnabledTargetLocker(block)) return null;
            var logic = block.GameLogic.GetAs<TargetLocker_CameraBlock>();
            if (logic == null || logic.cameraBlock == null || !logic.cameraBlock.Enabled) return null;
            var camera = logic.cameraBlock;
            Vector3D from = camera.GetPosition();
            Vector3D to = camera.GetPosition() + logic.distance * camera.WorldMatrix.Forward;
            IHitInfo hitInfo;
            if (MyAPIGateway.Physics.CastLongRay(from, to, out hitInfo, true)) { return hitInfo.Position; }
            return null;
        }
        public static Vector3D? TrackTargetHitPosition(IMyTerminalBlock block)
        {
            var logic = block?.GameLogic?.GetAs<TargetLocker_CameraBlock>();
            var target = logic?.Target;
            var cameraBlock = logic?.cameraBlock;
            if (target == null || cameraBlock == null) return null;
            if (!(target is IMyCubeGrid)) return target.GetPosition();
            return logic.GetCurrentBlock()?.GetPosition();
        }
    }
}
