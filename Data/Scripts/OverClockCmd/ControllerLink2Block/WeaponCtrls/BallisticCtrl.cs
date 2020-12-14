using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRage.Game;
using VRage.Game.Components;
namespace SuperBlocks.Controller
{
    //[MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "BallisticComputer", "SmallBallisticComputer")]
    public sealed class BallisticCtrl : ControlBase
    {
        protected override void InitController()
        {
            (Entity as IMyTerminalBlock).CustomDataChanged += BallisticCtrl_CustomDataChanged;
        }
        private void BallisticCtrl_CustomDataChanged(IMyTerminalBlock obj)
        {
            var ctrl = Control as BallisticComputer;
            if (ctrl == null) return;
            ctrl.Configs = obj.CustomData;
        }
        protected override void InitParameters()
        {
            Control = new BallisticComputer(Entity as IMyTerminalBlock);
        }
        private void DeleteMessages()
        {
            try
            {
                (Entity as IMyTerminalBlock).CustomDataChanged -= BallisticCtrl_CustomDataChanged;
                //(Entity as IMyTerminalBlock).CustomDataChanged
            }
            catch (Exception) { }
        }
        public override void Close()
        {
            base.Close();
            DeleteMessages();
        }
        public override void MarkForClose()
        {
            base.MarkForClose();
            DeleteMessages();
        }
    }
    [MyEntityComponentDescriptor(typeof(MyObjectBuilder_TerminalBlock), false, "TurretController", "SmallTurretController")]
    public sealed class TurretCtrl : ControlBase
    {
        protected override void InitController()
        {
            (Entity as IMyTerminalBlock).CustomDataChanged += BallisticCtrl_CustomDataChanged;
        }
        private void BallisticCtrl_CustomDataChanged(IMyTerminalBlock obj)
        {
            var ctrl = Control as BallisticComputer;
            if (ctrl == null) return;
            ctrl.Configs = obj.CustomData;
        }
        protected override void InitParameters()
        {
            Control = new BallisticComputer(Entity as IMyTerminalBlock);
        }
        private void DeleteMessages()
        {
            try
            {
                (Entity as IMyTerminalBlock).CustomDataChanged -= BallisticCtrl_CustomDataChanged;
                //(Entity as IMyTerminalBlock).CustomDataChanged
            }
            catch (Exception) { }
        }
        public override void Close()
        {
            base.Close();
            DeleteMessages();
        }
        public override void MarkForClose()
        {
            base.MarkForClose();
            DeleteMessages();
        }
    }
}
