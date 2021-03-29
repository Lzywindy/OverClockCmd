using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyGyrosController
        {
            public Vector3 PowerScale3Axis { get; set; } = Vector3.One;
            public void ForceUpdate(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity)
            {
                if (Common.IsNull(Me) || InThisEntity == null) return;
                gyros = Common.GetTs(Me, (IMyGyro gyro) => Common.ExceptKeywords(gyro) && InThisEntity(gyro));
            }
            public void SetOverclocked(float mult = 1)
            {
                if (Common.IsNullCollection(gyros)) return;
                foreach (var gyro in gyros)
                {
                    gyro.PowerConsumptionMultiplier = mult;
                    gyro.GyroStrengthMultiplier = mult;
                }
            }
            public void SetEnabled(bool Enabled) { if (gyros == null || gyros.Count < 1) return; foreach (var gyro in gyros) gyro.Enabled = Enabled; }
            public void GyrosOverride(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, Vector3? RotationIndicate)
            {
                if (Common.IsNull(Me) || InThisEntity == null) return;
                if (Common.IsNullCollection(gyros)) return;
                foreach (var gyro in gyros)
                {
                    gyro.GyroOverride = RotationIndicate.HasValue && (Me != null);
                    if (Me == null) gyro.Roll = gyro.Yaw = gyro.Pitch = 0;
                }
                if (!RotationIndicate.HasValue) return;
                Matrix matrix_Main = Common.GetWorldMatrix(Me);
                foreach (var gyro in gyros)
                {
                    var result = Vector3.TransformNormal(RotationIndicate.Value * PowerScale3Axis, matrix_Main * Matrix.Transpose(Common.GetWorldMatrix(gyro)));
                    gyro.Roll = result.Z; gyro.Yaw = result.Y; gyro.Pitch = result.X;
                }
            }
            public MyGyrosController() { }
            private List<IMyGyro> gyros;
        }
    }
}