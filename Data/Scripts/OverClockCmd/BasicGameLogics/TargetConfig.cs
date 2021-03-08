using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRageMath;
using VRage.Game.ModAPI;
using VRage.ModAPI;
namespace SuperBlocks.Controller
{
    public class TargetConfig
    {
        public bool IsNull => Target == null;
        public bool IsGrid => TargetGrid != null;
        public bool HasSubpartTarget => TargetBlock != null;
        public MyTargetFilterManager TargetFilter { get; } = "AllFunctionalBlocks|Missiles|Meteros";
        public void SetTarget(IMyEntity Target)
        {
            if (Target == null || !TargetFilter.EntityFilter(Target)) return;
            if (this.Target != Target)
            {
                TargetBlock = null;
                TargetBlocks.Clear();
            }
            this.Target = Target;
        }
        public void Update(IMyTerminalBlock Me)
        {
            if (Target == null || Target.Closed || Target.MarkedForClose || !TargetFilter.EntityFilter(Target)) { Target = null; TargetBlock = null; return; }
            CycleBlock(Me, false, false);
        }
        public void CycleBlock(IMyTerminalBlock Me, bool Enabled = false, bool Invert = false)
        {
            if (TargetGrid == null || Me == null) return;
            if (Utils.IsNullCollection(TargetBlocks))
                MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TargetGrid)?.GetBlocksOfType(TargetBlocks, TargetFilter.BlockFilter);
            TargetBlocks.RemoveAll(b => !b.IsFunctional);
            if (Utils.IsNullCollection(TargetBlocks)) { TargetBlock = null; return; }
            if (TargetBlocks.Count > 1)
                TargetBlocks.Sort((a, b) => Math.Sign(Vector3D.Distance(a.GetPosition(), Me.GetPosition()) - Vector3D.Distance(b.GetPosition(), Me.GetPosition())));
            if (TargetBlock == null || !TargetBlock.IsFunctional) { TargetBlock = TargetBlocks[0]; return; }
            if (!Enabled) return;
            TargetBlock = TargetBlocks[Utils.CycleInteger(Utils.CycleInteger(TargetBlocks.FindIndex(b => b == TargetBlock), 0, TargetBlocks.Count) + (Invert ? -1 : 1), 0, TargetBlocks.Count)];
        }
        public MyTuple<Vector3D?, Vector3D?>? TrackTarget_PV()
        {
            if (IsNull)
                return null;
            if (!IsGrid)
                return new MyTuple<Vector3D?, Vector3D?>(Target?.GetPosition(), Target?.Physics.LinearVelocity);
            if (HasSubpartTarget)
                return new MyTuple<Vector3D?, Vector3D?>(TargetBlock.GetPosition(), TargetGrid?.Physics?.LinearVelocity);
            return null;
        }
        public string GetTargetDetails()
        {
            string dt = "";
            if (IsNull) return "Null Target!\n\r";
            dt += $"Name:{Target.DisplayName}\n\r";
            if (IsGrid)
                dt += $"Is Grid:{IsGrid}\n\rGrid Size:{TargetGrid.GridSize}\n\rGrid Type:{TargetGrid.GridSizeEnum}\n\r";
            if (HasSubpartTarget)
                dt += $"Current Target Block:\n\r{TargetBlock.DetailedInfo}\n\r";
            return dt;
        }
        IMyEntity Target;
        IMyCubeGrid TargetGrid => Target as IMyCubeGrid;
        IMyTerminalBlock TargetBlock;
        List<IMyTerminalBlock> TargetBlocks { get; } = new List<IMyTerminalBlock>();
    }
    //public class TargetPredictAndTrack
    //{
    //    TargetConfig target { get; } = new TargetConfig();
    //    List<IMyTerminalBlock>
    //}
}