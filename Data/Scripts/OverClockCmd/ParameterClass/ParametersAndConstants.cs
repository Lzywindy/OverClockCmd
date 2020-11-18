using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VRageMath;

namespace SuperBlocks
{
    /// <summary>
    /// 陀螺仪参数
    /// </summary>
    [Serializable]
    public class GyroParameters
    {
        public float Roll_Scale { get { return _Roll_Scale; } set { _Roll_Scale = MathHelper.Clamp(value, 0, 1); } }
        public float Pitch_Scale { get { return _Pitch_Scale; } set { _Pitch_Scale = MathHelper.Clamp(value, 0, 1); } }
        public float Yaw_Scale { get { return _Yaw_Scale; } set { _Yaw_Scale = MathHelper.Clamp(value, 0, 1); } }
        protected float _Roll_Scale = 1;
        protected float _Pitch_Scale = 1;
        protected float _Yaw_Scale = 1;
    }
    /// <summary>
    /// 计算参照平面时所得到数据的枚举
    /// </summary>
    [Flags]
    public enum PlaneFocus { None = 0, HasVelocity = 1, HasGravity = 2, HasAll = 3 };
    /// <summary>
    /// 控制器种类
    /// </summary>
    public enum ControllerType { OverClockCtrl, TankTrackCtrl, VTOLCtrl, HeilCtrl, ShipCtrl, SpaceShipCtrl }
}
