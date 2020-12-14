using VRage.Game.Components;
using VRage.ObjectBuilders;
using VRage.ModAPI;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using System;
using VRage.Game.ModAPI;
using VRageMath;
using VRage;
using Sandbox.Game.Entities;
/*  
  Welcome to Modding API. This is second of two sample scripts that you can modify for your needs,
  in this case simple script is prepared that will alter behaviour of sensor block
  This type of scripts will be executed automatically  when sensor (or your defined) block is added to world
 */
namespace SuperBlocks.Controller
{
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "RadarCore_Block_Large", "RadarCore_Block_Small")]
    public partial class RadarBlockLogic : MyGameLogicComponent
    {
        public override void Init(MyObjectBuilder_EntityBase objectBuilder)
        {
            base.Init(objectBuilder);
            NeedsUpdate |= (MyEntityUpdateEnum.EACH_FRAME | MyEntityUpdateEnum.EACH_10TH_FRAME | MyEntityUpdateEnum.EACH_100TH_FRAME);
        }
        public override void UpdateBeforeSimulation()
        {
            base.UpdateBeforeSimulation();
            TryRegistMainBlock(Entity as IMyTerminalBlock);
        }
        public override void UpdateAfterSimulation10()
        {
            base.UpdateAfterSimulation10();
            if (!BlockOnOff || !MainBlock || GridTerminalSystem == null) return;
            try
            {
                Targets.RemoveWhere((IMyEntity entity) => !在探测器区域内(entity));
            }
            catch (Exception) { }
        }
        public override void UpdateAfterSimulation100()
        {
            base.UpdateAfterSimulation100();
            if (!BlockOnOff || !MainBlock || GridTerminalSystem == null) return;
            try
            {
                MyAPIGateway.Entities.GetEntities(Targets, 目标在范围内);
                Targets.RemoveWhere((IMyEntity entity) => entity == null || entity.MarkedForClose || entity.Closed);
            }
            catch (Exception) { }
        }
    }
    public partial class RadarBlockLogic
    {
        private static Dictionary<long, long> MainBlockRegister { get; } = new Dictionary<long, long>();
        private static void TryRegistMainBlock(IMyTerminalBlock block)
        {
            if (block == null || MainBlockRegister.ContainsKey(block.GetTopMostParent().EntityId)) return;
            MainBlockRegister.Add(block.GetTopMostParent().EntityId, block.EntityId);
        }
        private static bool IsMainBlock(IMyTerminalBlock block)
        {
            if (block == null || !MainBlockRegister.ContainsKey(block.GetTopMostParent().EntityId)) return false;
            return MainBlockRegister[block.GetTopMostParent().EntityId] == block.EntityId;
        }
        public static void UpdateRadarBlockLogicRegisters()
        {
            List<long> DeleteEntity = new List<long>();
            foreach (var item in MainBlockRegister)
            {
                if (MyAPIGateway.Entities.GetEntityById(item.Key) == null)
                { DeleteEntity.Add(item.Key); continue; }
                var grid = MyAPIGateway.Entities.GetEntityById(item.Key) as IMyCubeGrid;
                if (grid == null) continue;
                if (MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid)?.GetBlockWithId(item.Value) == null)
                { DeleteEntity.Add(item.Key); continue; }
            }
            foreach (var item in DeleteEntity) { MainBlockRegister.Remove(item); }
        }
    }
    public partial class RadarBlockLogic
    {
        /// <summary>
        /// 探测器
        /// 可以是无线电的：天线、探测器、信标
        /// 可以是光学的：激光天线、摄像头、信标
        /// 可以是其他高科技通信设备（待做）
        /// </summary>
        private HashSet<long> 探测器 { get; } = new HashSet<long>();
        private bool Enable => BlockOnOff && MainBlock && GridTerminalSystem != null && (Entity as IMyTerminalBlock).IsFunctional;
    }
    public partial class RadarBlockLogic
    {
        public static Dictionary<string, Delegate> GetRadarApis()
        {
            Dictionary<string, Delegate> APIs = new Dictionary<string, Delegate>()
            {
                ["SetToAttachRadar"] = new Action<Sandbox.ModAPI.Ingame.IMyTerminalBlock, Sandbox.ModAPI.Ingame.IMyTerminalBlock>(SetToAttachRadar),
                ["SetsToAttachRadar"] = new Action<Sandbox.ModAPI.Ingame.IMyTerminalBlock, List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>>(SetsToAttachRadar),
                ["IsAvailableForRadarCore"] = new Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool>(IsAvailableForRadarCore),
            };
            return APIs;
        }
        public static bool SetLockHookForMe(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            if (Me == null) return false;
            var me = (Me as IMyProgrammableBlock);
            if (me == null) return false;
            var id = me.GetTopMostParent().EntityId;
            if (!ProgrammableBlock_TargetLock.ContainsKey(id))
                ProgrammableBlock_TargetLock.Add(id, new Dictionary<long, List<long>>());
            if (!ProgrammableBlock_TargetLock[id].ContainsKey(me.EntityId))
                ProgrammableBlock_TargetLock[id].Add(me.EntityId, new List<long>());
            return true;
        }
        public static Vector3D? GetLockHookForMe_TargetPosition(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            if (!SetLockHookForMe(Me)) return null;
            return null;
        }
        public static List<long> GetEntityDetected_IDList(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            if (!SetLockHookForMe(Me)) return null;
            return ProgrammableBlock_TargetLock[(Me as IMyProgrammableBlock).GetTopMostParent().EntityId][Me.EntityId];
        }
        public static void SetEntityLocked(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me, List<long> TargetsID = null)
        {
        }
        public static List<Sandbox.ModAPI.Ingame.MyDetectedEntityInfo?> GetRadarMap(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            return null;
        }
        public static Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? GetCurrentLockedInfo(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            return null;
        }
        private static Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? ProcessTargetInfo(IMyTerminalBlock RadarBlock, long TargetID, string subsystem = "")
        {
            if (RadarBlock == null) return null;
            var Ent = MyEntities.GetEntityById(TargetID);
            if (Ent == null || TargetID == RadarBlock.GetTopMostParent().EntityId) return null;
            if (subsystem == "" || !(Ent is IMyCubeGrid))
                return MyDetectedEntityInfoHelper.Create(Ent, RadarBlock.OwnerId, null);
            Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? Target = null;
            try
            {
                var grid = Ent as IMyCubeGrid;
                List<IMySlimBlock> blocks = new List<IMySlimBlock>();
                switch (subsystem)
                {
                    case "weapons":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyUserControllableGun) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "thrust":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyThrust) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "gyro":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyGyro) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "production":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyProductionBlock || block is IMyGasGenerator) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "power":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyReactor || block is IMyBatteryBlock) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "wheel":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyWheel || block is IMyMotorSuspension) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    case "rotor":
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyMotorRotor || block is IMyMotorStator) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                    default:
                        grid.GetBlocks(blocks, (IMySlimBlock block) => (block is IMyTerminalBlock) && (block as IMyTerminalBlock).IsFunctional);
                        break;
                }
                if (blocks.Count < 1) return MyDetectedEntityInfoHelper.Create(Ent, RadarBlock.OwnerId, null);
                if (blocks.Count > 1)
                    blocks.Sort((IMySlimBlock a, IMySlimBlock b) => { return Math.Sign(Vector3D.Distance((b as IMyCubeBlock).GetPosition(), RadarBlock.GetPosition()) - Vector3D.Distance((a as IMyCubeBlock).GetPosition(), RadarBlock.GetPosition())); });
                var blockcenter = (blocks[0] as IMyCubeBlock).GetPosition();
                Target = MyDetectedEntityInfoHelper.Create(Ent, RadarBlock.OwnerId, blockcenter);
            }
            catch (Exception) { }
            return MyDetectedEntityInfoHelper.Create(Ent, RadarBlock.OwnerId, null);
        }
        private static IMyTerminalBlock GetActivityRadar(Sandbox.ModAPI.Ingame.IMyProgrammableBlock Me)
        {
            if (!SetLockHookForMe(Me)) return null;
            return null;
        }
        public static void SetToAttachRadar(Sandbox.ModAPI.Ingame.IMyTerminalBlock 雷达方块, Sandbox.ModAPI.Ingame.IMyTerminalBlock 信息探测方块)
        {
            var Logic = GetRadarBlockLogic(雷达方块 as IMyTerminalBlock);
            if (Logic == null || !IsAvailableForRadarCore(信息探测方块)) return;
            Logic.探测器.Add(信息探测方块.EntityId);
        }
        public static void SetsToAttachRadar(Sandbox.ModAPI.Ingame.IMyTerminalBlock 雷达方块, List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> 信息探测方块组)
        {
            var Logic = GetRadarBlockLogic(雷达方块 as IMyTerminalBlock);
            if (Logic == null || 信息探测方块组 == null || 信息探测方块组.Count < 1) return;
            foreach (var 信息探测方块 in 信息探测方块组)
            {
                if (!IsAvailableForRadarCore(信息探测方块)) continue;
                Logic.探测器.Add(信息探测方块.EntityId);
            }
        }
        private static HashSet<string> 可接受的子ID { get; } = new HashSet<string>() {
            "LargeBlockRadioAntenna",
            "LargeBlockBeacon",
            "SmallBlockRadioAntenna",
            "SmallBlockBeacon",
            "LargeBlockLaserAntenna",
            "SmallBlockLaserAntenna",
            "SmallCameraBlock",
            "LargeCameraBlock"
        };
        private static Dictionary<long, Dictionary<long, List<long>>> ProgrammableBlock_TargetLock { get; } = new Dictionary<long, Dictionary<long, List<long>>>();
        public static bool IsAvailableForRadarCore(Sandbox.ModAPI.Ingame.IMyTerminalBlock 信息探测方块)
        {
            if (信息探测方块 == null) return false;
            return 可接受的子ID.Contains(信息探测方块.BlockDefinition.SubtypeId);
        }
    }
    //GUI控制接口
    public partial class RadarBlockLogic
    {
        public static bool EnabledController(IMyTerminalBlock block)
        {
            var logic = GetRadarBlockLogic(block);
            if (logic == null) return false;
            return true;
        }
        public static bool Get_RadarWorking(IMyTerminalBlock 雷达方块)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return false;
            return logic.Enable;
        }
        public static void Set_RadarWorking(IMyTerminalBlock 雷达方块, bool Enabled)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return;
            logic.BlockOnOff = Enabled;
        }
        public static void TriggerRadarWorking(IMyTerminalBlock 雷达方块)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return;
            logic.BlockOnOff = !logic.BlockOnOff;
        }
        public static float Get_DetectRange(IMyTerminalBlock 雷达方块)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return 0;
            return logic.Get_DetectRange();
        }
        public static void Set_GridSize(IMyTerminalBlock 雷达方块, float blockcount)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return;
            logic.FunctionalBlockLowerLimited = MathHelper.Clamp((int)blockcount, -1, 2 ^ 20);
        }
        public static float Get_GridSize(IMyTerminalBlock 雷达方块)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return 0;
            return MathHelper.Clamp(logic.FunctionalBlockLowerLimited, -1, 2 ^ 20);
        }
        public static void Set_DetectRange(IMyTerminalBlock 雷达方块, float range)
        {
            var logic = GetRadarBlockLogic(雷达方块);
            if (logic == null) return;
            logic.Set_DetectRange(range);
        }
        public static RadarBlockLogic GetRadarBlockLogic(IMyTerminalBlock 雷达方块)
        {
            if (雷达方块 == null) return null;
            return 雷达方块?.GameLogic.GetAs<RadarBlockLogic>();
        }
    }
    //私有函数和成员
    public partial class RadarBlockLogic
    {
        public float Get_DetectRange()
        {
            return MathHelper.Clamp(Range, 5000, 200000);
        }
        public void Set_DetectRange(float value)
        {
            Range = MathHelper.Clamp(value, 5000, 200000);
            foreach (var id in 探测器)
            {
                var device = GridTerminalSystem.GetBlockWithId(id);
                if (device is IMySensorBlock)
                {
                    (device as IMySensorBlock).BackExtend = Range;
                    (device as IMySensorBlock).TopExtend = Range;
                    (device as IMySensorBlock).BottomExtend = Range;
                    (device as IMySensorBlock).FrontExtend = Range;
                    (device as IMySensorBlock).LeftExtend = Range;
                    (device as IMySensorBlock).RightExtend = Range;
                }
                else if (device is IMyLaserAntenna)
                {
                    (device as IMyLaserAntenna).Range = Range;
                }
                else if (device is IMyRadioAntenna)
                {
                    (device as IMyRadioAntenna).Radius = Range;
                }
                else if (device is IMyBeacon)
                {
                    (device as IMyBeacon).Radius = Range;
                }
            }
        }
        private bool MainBlock => IsMainBlock(Entity as IMyTerminalBlock);
        private HashSet<IMyEntity> Targets { get; } = new HashSet<IMyEntity>();
        private bool 目标在范围内(IMyEntity Target)
        {
            if (Utils.是否是体素或者行星(Target) || !Utils.是否在范围里(Entity.GetTopMostParent(), Target, Range) || !在探测器区域内(Target))
                return false;
            if (Utils.是否是角色或者陨石(Target))
                return true;
            if (Utils.是否是导弹(Target))
                return true;
            return Utils.统计网格中通电的方块(Target as IMyCubeGrid) > FunctionalBlockLowerLimited;
        }
        private bool 在摄像头的范围内(IMyCameraBlock device, IMyEntity Target)
        {
            if (device == null || Target == null) return false;
            return device.CanScan(Target.GetPosition());
        }
        private bool 在无线电的范围内(IMyRadioAntenna device, IMyEntity Target)
        {
            if (device == null || Target == null) return false;
            return device.Radius > Vector3D.Distance(device.GetTopMostParent().GetPosition(), Target.GetPosition());
        }
        private bool 在探测器的范围内(IMySensorBlock device, IMyEntity Target)
        {
            if (device == null || Target == null) return false;
            BoundingBoxD bounding = new BoundingBoxD(new Vector3D(device.LeftExtend, device.BackExtend, device.BackExtend), new Vector3D(device.RightExtend, device.TopExtend, device.FrontExtend));
            bounding.TransformSlow(device.WorldMatrix);
            return bounding.Contains(Target.WorldAABB) != ContainmentType.Disjoint;
        }
        private bool 在激光天线的范围内(IMyLaserAntenna device, IMyEntity Target)
        {
            if (device == null || Target == null) return false;
            return device.Range > Vector3D.Distance(device.GetTopMostParent().GetPosition(), Target.GetPosition());
        }
        private bool 在信标的范围内(IMyBeacon device, IMyEntity Target)
        {
            if (device == null || Target == null) return false;
            return device.Radius > Vector3D.Distance(device.GetTopMostParent().GetPosition(), Target.GetPosition());
        }
        private bool 在探测器区域内(IMyEntity Target)
        {
            if (GridTerminalSystem == null) return false;
            bool CanbeRay = false;
            foreach (var ids in 探测器)
            {
                var device = GridTerminalSystem.GetBlockWithId(ids);
                CanbeRay = CanbeRay || 在摄像头的范围内(device as IMyCameraBlock, Target) ||
                    在无线电的范围内(device as IMyRadioAntenna, Target) ||
                    在探测器的范围内(device as IMySensorBlock, Target) ||
                    在激光天线的范围内(device as IMyLaserAntenna, Target) ||
                    在信标的范围内(device as IMyBeacon, Target);
            }
            return CanbeRay;
        }
        private IMyGridTerminalSystem GridTerminalSystem { get { if (Entity == null) return null; var me = Entity as IMyTerminalBlock; if (me == null) return null; return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(me.CubeGrid); } }
        private int FunctionalBlockLowerLimited = 10;
        private bool BlockOnOff = true;
        private float Range = 10000;
    }
}
