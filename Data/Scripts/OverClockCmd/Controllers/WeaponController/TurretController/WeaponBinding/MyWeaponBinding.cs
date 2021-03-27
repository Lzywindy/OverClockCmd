using Sandbox.ModAPI;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    public interface MyWeaponBinding
    {
        void Init(IMyTerminalBlock motorAz);
        void SetConfig(MyWeaponParametersConfig Config);
        void SetWeaponAmmoConfigInfo(MyWeaponParametersConfig Config, string WeaponName, string AmmoName);
        void Running();
        MyTargetDetected AimTarget { get; set; }
        bool AutoFire { get; set; }
        bool Enabled { get; set; }
        Vector3D? PredictDirection { get; }
    }
}