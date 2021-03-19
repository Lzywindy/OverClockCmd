using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks.Controller
{
    using static Utils;
    public partial class UniversalController
    {
        #region Variables
        private float MultAttitude = 25f;
        private float _MaxiumHoverSpeed = DefaultSpeed;
        private float _MaxiumFlightSpeed = DefaultSpeed;
        private float _MaxiumSpeed = DefaultSpeed;
        private bool _EnabledCuriser = false;
        private bool _ForwardOrUp = false;
        private float _SafetyStage = 1f;
        private float _LocationSensetive = 1f;
        private float _MaxReactions_AngleV = 1f;
        private Vector3 AngularDampeners = Vector3.One;
        private Vector3 ForwardDirection;
        private IMyShipController _Controller;
        private bool StartReady = false;
        private ControllerRole _Role = ControllerRole.None;
        private const float DefaultSpeed = 100;
        public const float SafetyStageMin = 0f;
        public const float SafetyStageMax = 9f;
        private double sealevel, _Target_Sealevel;
        private float target_speed = 0, diffsealevel = 0;
        private MyThrusterController ThrustControllerSystem { get; } = new MyThrusterController();
        private MyGyrosController GyroControllerSystem { get; } = new MyGyrosController();
        private MyWheelsController WheelsController { get; } = new MyWheelsController();
        private Dictionary<string, Dictionary<string, string>> Configs { get; } = new Dictionary<string, Dictionary<string, string>>();
        private MyAutoCloseDoorController AutoCloseDoorController { get; } = new MyAutoCloseDoorController();
        public ControllerRole Role { get { return _Role; } set { _Role = value; } }
        #endregion
    }
}
