using System;
using Sandbox.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public class PlanetVehicle : VehicleControllerBase, ICtrlDevCtrl, IPlanetVehicle
    {
        public PlanetVehicle(IMyTerminalBlock refered_block) : base(refered_block) { }
        protected virtual Action Init4GetAction() { return () => { }; }
        protected virtual Action Init4GetAction10() { return () => { }; }
        protected virtual Action Init4GetAction100() { return () => { }; }
        protected override void Init(IMyTerminalBlock refered_block)
        {
            base.Init(refered_block);
            AppRunning1 += Init4GetAction();
            AppRunning10 += Init4GetAction10();
            AppRunning100 += Init4GetAction100();
        }
        protected override void PoseCtrl()
        {
            GyroControllerSystem?.SetEnabled(EnabledGyros && ExtraEnabledGyros);
            GyroControllerSystem?.GyrosOverride(姿态调整参数);
        }
        protected override void ThrustControl()
        {
            ThrustControllerSystem?.SetupMode(false, true, (!EnabledThrusters), MaximumSpeed);
            ThrustControllerSystem?.Running(推进器控制参数, 0, true);
        }
        protected virtual bool ExtraEnabledGyros => true;
        public float MaximumCruiseSpeed { get { return _MaxiumSpeed * 3.6f; } set { _MaxiumSpeed = MathHelper.Clamp(Math.Abs(value / 3.6f), -360f, 360f); } }
        protected override float MaximumSpeed { get { if (HandBrake) return 0; return _MaxiumSpeed; } }
        private float _MaxiumSpeed;
    }
}
