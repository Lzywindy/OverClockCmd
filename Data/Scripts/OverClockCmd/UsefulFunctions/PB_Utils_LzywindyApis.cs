using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage.Game.Entity;
namespace SuperBlocks
{
    public static partial class PB_Utils_LzywindyApis
    {
        public delegate Vector3? ProcessRotationPB_Dlgt(Sandbox.ModAPI.Ingame.IMyTerminalBlock Me, Vector4 RotationCtrlLines, ref Vector3 ForwardDirection, Vector3? InitAngularDampener = null, Vector3? AngularDampeners = null, Vector3? ForwardDirectionOverride = null, Vector3? PlaneNormalOverride = null, float MaximumSpeedLimited = 100f, float MaxReactions_AngleV = 1f, float LocationSensetive = 1f, float SafetyStage = 1f, bool _EnabledCuriser = false, bool ForwardOrUp = false, bool PoseMode = false, bool Need2CtrlSignal = true, bool IgnoreForwardVelocity = true, bool Refer2Velocity = true, bool Refer2Gravity = true, bool DisabledRotation = true);
        public delegate void GunDirection_Dlgt(Vector3D? TargetPosition, Vector3D? SelfPosition, Vector3D? TargetVelocity, Vector3D? SelfVelocity, Vector3D? Gravity, double V_project_length, ref Vector3D? V_project, ref double? t_n, Vector3D CannonDirection, int CalcCount = 4, float Delta_Time = 1, float Delta_Precious = 1);
        public static Dictionary<string, Delegate> PackApisForPB()
        {
            //高阶Api，用于更高级的功能
            Dictionary<string, Delegate> apis = new Dictionary<string, Delegate>()
            {
                ["ProcessRotation"] = new ProcessRotationPB_Dlgt(ProcessRotationPB),//用于飞行器的姿态控制函数
                ["GunDirection"] = new GunDirection_Dlgt(Utils.GunDirection),//计算弹道
                ["Solve_Direction"] = new Func<double, Vector3D, Vector3D, Vector3D?, double, bool, Vector3D?>(Utils.Solve_Direction),//计算弹道中的速度方向计算子函数
                ["Solve_Subfunction"] = new Func<double, double, double, double, double, double>(Utils.Solve_Subfunction),//计算弹道中的4次方程迭代求解子函数
                ["ErrorFunction"] = new Func<Vector3D, Vector3D, Vector3D, Vector3D, Vector3D, double, Vector3D?, double>(Utils.ErrorFunction),//计算弹道中的误差函数
            };
            return apis;
        }
        public static Dictionary<string, Delegate> PackAPIsForPB_Basic()
        {
            //常用的基础Api，包括计算和对象获取
            Dictionary<string, Delegate> apis = new Dictionary<string, Delegate>()
            {
                ["GetT_gts"] = new Func<Sandbox.ModAPI.Ingame.IMyGridTerminalSystem, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool>, Sandbox.ModAPI.Ingame.IMyTerminalBlock>(GetT),
                ["GetT_bg"] = new Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool>, Sandbox.ModAPI.Ingame.IMyTerminalBlock>(GetT),
                ["GetTs_gts"] = new Func<Sandbox.ModAPI.Ingame.IMyGridTerminalSystem, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool>, List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>>(GetTs),
                ["GetTs_bg"] = new Func<Sandbox.ModAPI.Ingame.IMyBlockGroup, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool>, List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>>(GetTs),
                ["GetWorldMatrix"] = new Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, Matrix>(GetWorldMatrix),
                ["DampenerV3"] = new Func<Vector3, Vector3>(Utils.Dampener),
                ["Dampener"] = new Func<float, float>(Utils.Dampener),
                ["SolveLine"] = new Func<string, Dictionary<string, string>>(Utils.SolveLine),
                ["GetConfigLines"] = new Func<string, List<Dictionary<string, string>>>(Utils.GetConfigLines),
                ["ProjectOnPlane"] = new Func<Vector3, Vector3, Vector3>(Utils.ProjectOnPlane),
                ["Calc_Direction_Vector"] = new Func<Vector3, Vector3, float>(Utils.Calc_Direction_Vector),
                ["ScaleVectorTimes"] = new Func<Vector3, float, Vector3>(Utils.ScaleVectorTimes),
                ["GetFinalMatrixs"] = new Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, List<MatrixD>>(GetFinalMatrixs),
                ["ProjectLinnerVelocity_CockpitForward"] = new Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool, bool, Vector3>(ProjectLinnerVelocity_CockpitForward)
            };
            return apis;
        }
        //public static void Test()
        //{
        //    var apis = PackApisForPB();
        //    apis["Calc_Direction_Vector"].in
        //}
    }
    public static partial class PB_Utils_LzywindyApis
    {
        private static Vector3 ProjectLinnerVelocity_CockpitForward(Sandbox.ModAPI.Ingame.IMyTerminalBlock TerminalBlock, bool EnableToGet = true, bool IgnoreForwardVelocity = false)
        {
            if (TerminalBlock == null) return Vector3.Zero;
            var tempTerminalBlock = TerminalBlock as IMyTerminalBlock;
            if (tempTerminalBlock == null) return Vector3.Zero;
            var LinearVelocity = Utils.GetLinearVelocity(tempTerminalBlock, EnableToGet) ?? Vector3.Zero;
            if (IgnoreForwardVelocity)
                return Utils.ProjectOnPlane(LinearVelocity, TerminalBlock.WorldMatrix.Forward);
            else
                return LinearVelocity;
        }
        private static Sandbox.ModAPI.Ingame.IMyTerminalBlock GetT(Sandbox.ModAPI.Ingame.IMyGridTerminalSystem gridTerminalSystem, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> requst = null)
        {
            List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> Items = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            gridTerminalSystem.GetBlocksOfType<Sandbox.ModAPI.Ingame.IMyTerminalBlock>(Items, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        private static List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> GetTs(Sandbox.ModAPI.Ingame.IMyGridTerminalSystem gridTerminalSystem, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> requst = null)
        {
            List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> Items = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            gridTerminalSystem.GetBlocksOfType<Sandbox.ModAPI.Ingame.IMyTerminalBlock>(Items, requst);
            return Items;
        }
        private static Sandbox.ModAPI.Ingame.IMyTerminalBlock GetT(Sandbox.ModAPI.Ingame.IMyBlockGroup blockGroup, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> requst = null)
        {
            List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> Items = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            blockGroup.GetBlocksOfType<Sandbox.ModAPI.Ingame.IMyTerminalBlock>(Items, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        private static List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> GetTs(Sandbox.ModAPI.Ingame.IMyBlockGroup blockGroup, Func<Sandbox.ModAPI.Ingame.IMyTerminalBlock, bool> requst = null)
        {
            List<Sandbox.ModAPI.Ingame.IMyTerminalBlock> Items = new List<Sandbox.ModAPI.Ingame.IMyTerminalBlock>();
            blockGroup.GetBlocksOfType<Sandbox.ModAPI.Ingame.IMyTerminalBlock>(Items, requst);
            return Items;
        }
        private static Matrix GetWorldMatrix(Sandbox.ModAPI.Ingame.IMyTerminalBlock Me)
        {
            Matrix me_matrix;
            Me.Orientation.GetMatrix(out me_matrix);
            return me_matrix;
        }
        private static Vector3? ProcessRotationPB(Sandbox.ModAPI.Ingame.IMyTerminalBlock Me, Vector4 RotationCtrlLines, ref Vector3 ForwardDirection, Vector3? InitAngularDampener = null, Vector3? AngularDampeners = null, Vector3? ForwardDirectionOverride = null, Vector3? PlaneNormalOverride = null, float MaximumSpeedLimited = 100f, float MaxReactions_AngleV = 1f, float LocationSensetive = 1f, float SafetyStage = 1f, bool _EnabledCuriser = false, bool ForwardOrUp = false, bool PoseMode = false, bool Need2CtrlSignal = true, bool IgnoreForwardVelocity = true, bool Refer2Velocity = true, bool Refer2Gravity = true, bool DisabledRotation = true)
        {
            if (Me as IMyTerminalBlock == null) return null;
            return Utils.ProcessRotation(_EnabledCuriser, (Me as IMyTerminalBlock), RotationCtrlLines, ref ForwardDirection, InitAngularDampener, AngularDampeners, ForwardOrUp, PoseMode, MaximumSpeedLimited, MaxReactions_AngleV, Need2CtrlSignal, LocationSensetive, SafetyStage, IgnoreForwardVelocity, Refer2Velocity, Refer2Gravity, DisabledRotation, ForwardDirectionOverride, PlaneNormalOverride);
        }
        private static List<MatrixD> GetFinalMatrixs(Sandbox.ModAPI.Ingame.IMyTerminalBlock TerminalBlock)
        {
            if (TerminalBlock == null) return null;
            var ent = TerminalBlock as MyEntity;
            if (ent == null) return null;
            List<MatrixD> matrixs = new List<MatrixD>();
            try
            {
                if (TerminalBlock is IMyLargeTurretBase)
                {
                    foreach (var key in ent.Subparts.Keys)
                        foreach (var keyB in ent.Subparts[key].Subparts.Keys)
                            matrixs.Add(ent.Subparts[key].Subparts[keyB].WorldMatrix);
                }
                else matrixs.Add(TerminalBlock.WorldMatrix);
            }
            catch (Exception) { }
            return matrixs;
        }
    }
}
