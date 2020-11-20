using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;

namespace SuperBlocks
{
    using static Utils;

    public class ShipController : PlanetVehicle, ICtrlDevCtrl, IPlanetVehicle, ISeaVehicle
    {
        public ShipController(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            MaximumCruiseSpeed = 80f;
        }
        protected override Action Init4GetAction100()
        {
            Action UtilsCtrl = () => { };
            var Doors = GridTerminalSystem.GetBlockGroupWithName("ACDoors");
            if (Doors != null)
            {
                List<IMyDoor> doors = GetTs<IMyDoor>(Doors);
                foreach (var door in doors) { UtilsCtrl += () => { if (door.OpenRatio == 1) door.CloseDoor(); }; }
            }
            return UtilsCtrl;
        }
        protected override void PoseCtrl()
        {
            GyroControllerSystem?.SetEnabled(EnabledGyros && ExtraEnabledGyros);
            GyroControllerSystem?.GyrosOverride(CtrlSignal_Gyros);
        }
        
        protected override Vector3 CtrlSignal_Thrusts { get { if (HandBrake) return Vector3.Zero; Vector3 Ctrl = Vector3.Backward; if (IsSubmarine) Ctrl += Vector3.Up; Ctrl *= MainCtrl.MoveIndicator; return (Ctrl != Vector3.Zero) ? Ctrl : Vector3.Forward; } }
        protected override Vector3? CtrlSignal_Gyros
        {
            get
            {
                if (MainCtrl == null || NoGravity) return null;
                AngularDampeners = Vector3.Clamp(AngularDampeners, Vector3.One, Vector3.One * 50);
                return (ProcessPlaneFunctions.ProcessPose_Roll_Pitch(Me, Gravity, AngularDampeners) + Me.WorldMatrix.Up * MainCtrl.MoveIndicator.X)* MaxReactions_AngleV;
            }
        }
        protected override bool ExtraEnabledGyros => true;
        public bool IsSubmarine { get; set; } = false;
        protected override bool Refer2Gravity => true;
        protected override bool Refer2Velocity => false;
        protected override bool Need2CtrlSignal => false;
        protected override bool IngroForwardVelocity => true;
        protected override bool ForwardOrUp => false;
        protected override bool EnabledAllDirection => true;
        protected override bool PoseMode => false;
    }
}
