using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
namespace SuperBlocks
{
    public class 姿态控制器
    {
        public Vector3 PowerScale3Axis { get; set; }
        public 姿态控制器(List<IMyGyro> gyros, IMyTerminalBlock Me)
        {
            UpdateBlocks(gyros, Me);
        }
        public void UpdateBlocks(List<IMyGyro> gyros, IMyTerminalBlock Me)
        {
            this.gyros = gyros;
            this.Me = Me;
            PowerScale3Axis = Vector3.One;
        }
        public void GyrosOverride(Vector3? RotationIndicate)
        {
            if (gyros == null || gyros.Count < 1) return;
            foreach (var gyro in gyros)
            {
                gyro.GyroOverride = RotationIndicate.HasValue && (Me != null);
                if (Me == null) gyro.Roll = gyro.Yaw = gyro.Pitch = 0;
            }
            if (!RotationIndicate.HasValue) return;
            Matrix matrix_Main = Utils.GetWorldMatrix(Me);
            //Me.Orientation.GetMatrix(out matrix_Main);
            foreach (var gyro in gyros)
            {
                //Matrix matrix_Gyro = Utils.GetWorldMatrix(gyro);
                //gyro.Orientation.GetMatrix(out matrix_Gyro);
                var result = Vector3.TransformNormal(RotationIndicate.Value * PowerScale3Axis, matrix_Main * Matrix.Transpose(Utils.GetWorldMatrix(gyro)));
                gyro.Roll = result.Z; gyro.Yaw = result.Y; gyro.Pitch = result.X;
            }
        }
        public void SetPowerPercentage(float power)
        {
            if (gyros == null || gyros.Count < 1) return;
            foreach (var gyro in gyros)
                gyro.GyroPower = power;
        }
        public void SetEnabled(bool Enabled)
        {
            if (gyros == null || gyros.Count < 1) return;
            foreach (var gyro in gyros)
                gyro.Enabled = Enabled;
        }
        public void SetOverride(bool Enabled)
        {
            if (gyros == null || gyros.Count < 1) return;
            foreach (var gyro in gyros)
                gyro.GyroOverride = Enabled;
        }
        private List<IMyGyro> gyros;
        private IMyTerminalBlock Me;
    }
}
