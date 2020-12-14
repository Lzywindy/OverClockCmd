using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.ModAPI;
using VRageMath;
namespace SuperBlocks
{
    public class SignalController
    {
        public IMyShipController MainCtrl { get; set; }
        public bool ControlByOtherSignal { get; set; }
        public Vector3 MoveIndicator
        {
            get
            {
                if (ControlByOtherSignal)
                    return _MoveIndicator;
                _MoveIndicator = Vector3.Zero;
                if (MainCtrl == null)
                    return Vector3.Zero;
                return MainCtrl.MoveIndicator;
            }
            set { _MoveIndicator = Vector3.ClampToSphere(value, 1f); }
        }
        public Vector3 RotateIndicator
        {
            get
            {
                if (ControlByOtherSignal)
                    return _RotateIndicator;
                _RotateIndicator = Vector3.Zero;
                if (MainCtrl == null)
                    return Vector3.Zero;
                return new Vector3(MainCtrl.RotationIndicator, MainCtrl.RollIndicator);
            }
            set { _RotateIndicator = Vector3.ClampToSphere(value, 1f); }
        }
        public float SeaLevel
        {
            get
            {
                double height = 0;
                MainCtrl?.TryGetPlanetElevation(Sandbox.ModAPI.Ingame.MyPlanetElevation.Sealevel, out height);
                return (float)height;
            }
        }
        public float SurfaceDistance
        {
            get
            {
                double height = 0;
                MainCtrl?.TryGetPlanetElevation(Sandbox.ModAPI.Ingame.MyPlanetElevation.Surface, out height);
                return (float)height;
            }
        }
        public bool HandBrake
        {
            get
            {
                if (ControlByOtherSignal)
                    return _Brake;
                _Brake = false;
                if (MainCtrl == null)
                    return true;
                return MainCtrl.HandBrake;
            }
            set { _Brake = value; }
        }
        public bool NullMainCtrl { get { return MainCtrl == null; } }
        private Vector3 _MoveIndicator;
        private Vector3 _RotateIndicator;
        private bool _Brake;
    }
}
