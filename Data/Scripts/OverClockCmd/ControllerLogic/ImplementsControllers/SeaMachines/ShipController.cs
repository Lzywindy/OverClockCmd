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
        protected override Vector3 推进器控制参数 { get { if (HandBrake) return Vector3.Zero; return new Vector3(0, IsSubmarine ? MainCtrl.MoveIndicator.Y : 0, MainCtrl.MoveIndicator.Z); } }
        protected override Vector3? 姿态调整参数 => 姿态处理(true);
        protected override Vector4 RotationCtrlLines => new Vector4(0, 0, 0, MainCtrl.MoveIndicator.X);
        protected override bool DisabledRotation => (MainCtrl.NullMainCtrl || NoGravity);
        protected override bool ExtraEnabledGyros => true;
        public bool IsSubmarine { get; set; } = false;
        protected override bool Refer2Gravity => true;
        protected override bool Refer2Velocity => false;
        protected override bool Need2CtrlSignal => false;
        protected override bool IngroForwardVelocity => true;
        protected override bool ForwardOrUp => true;
        protected override bool EnabledAllDirection => true;
        protected override bool PoseMode => false;
        protected override float SafetyStageCurrent { get { return 1; } set { } }
        protected override float _LocationSensetive { get { return 1; } set { } }
        protected override float _MaxReactions_AngleV { get { return 1; } set { } }
    }
}
