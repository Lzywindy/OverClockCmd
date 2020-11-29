using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.ModAPI;
using VRageMath;

namespace SuperBlocks
{
    public partial class CannonTurret : ControllerManageBase
    {
        public CannonTurret(IMyTerminalBlock refered_block) : base(refered_block) { }

        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
        }
    }
    public partial class CannonTurret
    {
        private void InitTurretGroup()
        {
            List<IMyBlockGroup> blockGroups = new List<IMyBlockGroup>();
            MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(Me.CubeGrid).GetBlockGroups(blockGroups);
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();

            foreach (var blockGroup in blockGroups)
            {
                blockGroup.GetBlocks(blocks, (IMyTerminalBlock block) => block == Me);
                if (blocks.Count < 1) { blocks.Clear(); continue; }
                TurretGroups.Add(blockGroup);
            }
        }
        private List<IMyBlockGroup> TurretGroups { get; } = new List<IMyBlockGroup>();
    }
}
