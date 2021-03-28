using System.Collections.Generic;
using System.Linq;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public static class MyRotorAPI
        {
            public static bool DisabledTerminalBlock<T>(T Block) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock => (Block == null || !Block.IsFunctional);
            public static bool DisabledTerminalBlocks<T>(ICollection<T> Blocks) where T : Sandbox.ModAPI.Ingame.IMyTerminalBlock => (Blocks == null || Blocks.Count < 1 || Blocks.All(m => DisabledTerminalBlock(m)));
            public static bool DisabledMotorRotor<T>(T Motor) where T : Sandbox.ModAPI.Ingame.IMyMotorStator => (Motor?.TopGrid == null || !Motor.IsFunctional);
            public static bool DisabledMotorRotors<T>(ICollection<T> Motors) where T : Sandbox.ModAPI.Ingame.IMyMotorStator => (Motors == null || Motors.Count < 1 || Motors.All(m => DisabledMotorRotor(m)));
            public static void RotorSetDefault<T>(T Motor, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                if (Motor == null || Motor.TopGrid == null) return;
                Motor.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -Max_Speed, Max_Speed);
            }
            public static void RotorsSetDefault<T>(ICollection<T> Motors, float Max_Speed = 30) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                if (Common.IsNullCollection(Motors)) return;
                foreach (var Motor in Motors) { RotorSetDefault(Motor, Max_Speed); }
            }
            public static float RotorRunning<T>(T Motor, float value) where T : Sandbox.ModAPI.Ingame.IMyMotorStator
            {
                var upper = Motor.UpperLimitRad;
                var lower = Motor.LowerLimitRad;
                if (value > 0)
                {
                    if (upper >= float.MaxValue) return value;
                    if (Motor.Angle >= upper) return 0;
                    return value;
                }
                else if (value < 0)
                {
                    if (lower <= float.MinValue) return value;
                    if (Motor.Angle <= lower) return 0;
                    return value;
                }
                return 0;
            }
        }
    }
}
