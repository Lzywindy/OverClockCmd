using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.Game.ModAPI.Interfaces;
using VRage.Game.Models;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageRender.Messages;
namespace SuperBlocks
{
    public partial class VitrualController
    {
        public VitrualController()
        {
        }
    }
    public partial class VitrualController
    {
        private IMyTerminalBlock Me;
        public IMyShipController LinkedShipController { get; set; }
        public IMyEntity Target { get; set; }
        public float Gun_Projector_Speed { get; set; }
        public bool EnabledOverrideByOtherblock { get; set; }
    }
}
