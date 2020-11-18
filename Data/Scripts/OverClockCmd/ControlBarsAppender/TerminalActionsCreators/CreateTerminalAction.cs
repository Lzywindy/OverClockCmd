﻿using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces.Terminal;
using VRage.Utils;
using System.Collections.Generic;
using System.Text;
using System;

namespace SuperBlocks.Controller
{
    public class CreateTerminalAction<ValueType>
    {
        public CreateTerminalAction(string CtrlID, string Title, Func<ControllerManageBase, bool> Filter = null)
        {
            ControlID = CtrlID;
            if (Filter != null) this.Filter = Filter;
            this.Title = Title;
        }
        public virtual void CreateController(IMyTerminalBlock block, List<IMyTerminalControl> controls) { }
        public virtual void CreateAction(IMyTerminalBlock block, List<IMyTerminalAction> actions) { }
        public bool IsinEnabledList(IMyTerminalBlock Block)
        {
            if (Block == null)
                return false;
            var logic = Block.GameLogic.GetAs<ControlBase>();
            if (logic == null)
                return false;
            if (Filter == null) return true;
            return Filter(logic.Control);
        }
        protected string ControlID { get; set; }
        protected bool ControlsCreated { get; set; } = false;
        protected bool ActionsCreated { get; set; } = false;
        protected MyStringId CtrlNM { get { return MyStringId.GetOrCompute(Title); } }
        protected StringBuilder CtrlNM_S { get { return new StringBuilder(Title); } }
        public Func<IMyTerminalBlock, ValueType> GetterFunc { get; set; } = (IMyTerminalBlock block)=> { return default(ValueType); };
        public Action<IMyTerminalBlock, ValueType> SetterFunc { get; set; } = (IMyTerminalBlock block, ValueType value) => { };
        protected bool DisabledAddControl(IMyTerminalBlock block) { return (ControlsCreated || (!IsinEnabledList(block))); }
        protected bool DisabledAddAction(IMyTerminalBlock block) { return (ActionsCreated || (!IsinEnabledList(block))); }
        protected string Title { get; private set; }
        private readonly Func<ControllerManageBase, bool> Filter = (ControllerManageBase CtrlLogic)=> { return true; };
    }
}
