using Sandbox.ModAPI;
using SuperBlocks.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using VRageMath;
namespace SuperBlocks
{
    public static partial class Utils
    {
        public class MyRotorThrustRotorCtrl
        {
            public void Running(IMyTerminalBlock Me, Func<IMyTerminalBlock, bool> InThisEntity, bool HoverMode, float? RollIndicate, float MaxiumSpeed, Vector3 MoveInidcate)
            {
                try
                {
                    if (Common.IsNull(Me) || InThisEntity == null) return;
                    HashSet<IMyThrust> thrusts = new HashSet<IMyThrust>(Common.GetTs<IMyThrust>(Me, t => InThisEntity(t) && t.CustomName.Contains(EngineRotorRefTag)));
                    List<IMyMotorStator> motors = Common.GetTs<IMyMotorStator>(Me, t => t.TopGrid != null && InThisEntity(t) && t.CustomName.Contains(EngineRotorRefTag));
                    Dictionary<IMyMotorStator, HashSet<IMyThrust>> MotorThrusts = new Dictionary<IMyMotorStator, HashSet<IMyThrust>>();
                    foreach (var motor in motors)
                    {
                        var thrusts_top = thrusts.Where(t => t.CubeGrid == motor.TopGrid);
                        if (Common.IsNullCollection(thrusts_top)) continue;
                        if (!MotorThrusts.ContainsKey(motor))
                            MotorThrusts.Add(motor, new HashSet<IMyThrust>());
                        MotorThrusts[motor].UnionWith(thrusts_top);
                    }
                    var WorldMoveInidcate = Vector3.TransformNormal(MoveInidcate, Me.WorldMatrix);
                    var roll = RollIndicate ?? 0;
                    var linervelocity = Me.CubeGrid?.Physics?.LinearVelocity ?? Vector3.Zero;
                    var gravity = Me.CubeGrid?.Physics?.Gravity ?? Vector3.Zero;
                    var gl = gravity.Length();
                    Vector3 direction = HoverMode ? (gravity + Vector3.ClampToSphere(linervelocity - WorldMoveInidcate * MaxiumSpeed, gl * diffangle)) : (Vector3.ClampToSphere(PoseProcessFuncs.ProjectLinnerVelocity_CockpitForward(Me, true, true) - WorldMoveInidcate * MaxiumSpeed, diffangle) + (Vector3)Me.WorldMatrix.Backward);
                    if (Vector3.IsZero(direction)) direction = Vector3.Zero;
                    else direction = Vector3.Normalize(direction);
                    foreach (var MotorThrust in MotorThrusts)
                    {
                        if (Common.IsNullCollection(MotorThrust.Value)) continue;
                        var item = MotorThrust.Value.FirstOrDefault();
                        if (Common.NullEntity(item)) continue;
                        MotorThrust.Key.TargetVelocityRad = MyWeaponAndTurretApi.RotorDampener(Vector3.Dot(Vector3.Cross(direction, item.WorldMatrix.Forward) * 8, MotorThrust.Key.Top.WorldMatrix.Up), MotorThrust.Key.TargetVelocityRad, 6);
                    }
                }
                catch (Exception) { }
            }
            private const string EngineRotorRefTag = "UC_RT";
            private readonly float diffangle = (float)Math.Cos(MathHelper.ToRadians(30));
        }
    }
}