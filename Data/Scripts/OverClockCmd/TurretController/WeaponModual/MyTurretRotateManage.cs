using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage;
using System.Linq;
namespace SuperBlocks.Controller
{
    public static class MyTurretRotateManage
    {
        public static void RunningDefault(IMyMotorStator TurretAz, List<IMyMotorStator> TurretEvs, float Max_Az_Speed = 30, float Max_Ev_Speed = 30)
        {
            Utils.RotorSetDefault(TurretAz, Max_Az_Speed);
            Utils.RotorsSetDefault(TurretEvs, Max_Ev_Speed);
        }
        public static void RotationMotors(IMyMotorStator TurretAz, Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Groups, Vector3D? Direction, double ForceLength, float Max_Az = 30, float Max_Ev = 30)
        {
            Max_Az = Math.Abs(Max_Az); Max_Ev = Math.Abs(Max_Ev);
            if (TurretAz == null || Gun_Rotor_Groups == null || Gun_Rotor_Groups.Count < 1 || Direction == null)
            {
                Utils.RotorSetDefault(TurretAz, Max_Az);
                Utils.RotorsSetDefault(Gun_Rotor_Groups?.Keys?.ToList(), Max_Ev);
                return;
            }
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in Gun_Rotor_Groups)
            {
                var data = Get_ArmOfForce_Point(Gun_Rotor_Group);
                TurretRotor_Torque += GetTorque(TurretAz, data, Direction, ForceLength);
                Gun_Rotor_Group.Key.TargetVelocityRad = MathHelper.Clamp(GetTorque(Gun_Rotor_Group.Key, data, Direction, ForceLength) - MathHelper.Clamp(Gun_Rotor_Group.Key.TargetVelocityRad, -1, 1) * 0.25f, -Max_Ev, Max_Ev);
            }
            TurretRotor_Torque /= Gun_Rotor_Groups.Count;
            TurretAz.TargetVelocityRad = MathHelper.Clamp(TurretRotor_Torque - MathHelper.Clamp(TurretAz.TargetVelocityRad, -1, 1) * 0.25f, -Max_Az, Max_Az);
        }
        public static Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Get_Gun_Motor_Group(List<IMyMotorStator> GunMotors, List<IMyTerminalBlock> guns)
        {
            if (GunMotors == null || guns == null || GunMotors.Count < 1 || guns.Count < 1) return null;
            Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Group = new Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>>();
            foreach (var GunMotor in GunMotors)
            {
                var grid = GunMotor.TopGrid;
                if (grid == null) continue;
                var weapons = guns.FindAll((IMyTerminalBlock gun) => gun.CubeGrid == grid);
                if (weapons == null || weapons.Count < 1) continue;
                if (!Group.ContainsKey(GunMotor))
                    Group.Add(GunMotor, new HashSet<IMyTerminalBlock>(weapons));
                else
                    Group[GunMotor].UnionWith(weapons);
            }
            return Group;
        }
        public static float GetTorque(IMyMotorStator Motor, MyTuple<Vector3D?, Vector3D?> ArmOfForce_Point, Vector3D? Direction, double ForceLength)
        {
            if (Motor == null || Motor.TopGrid == null) return 0;
            if (ArmOfForce_Point.Item1 == null || ArmOfForce_Point.Item2 == null || Direction == null || Direction.Value == Vector3D.Zero || ForceLength == 0)
                return -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -30, 30);
            var arm = ArmOfForce_Point.Item1.Value;
            var force = Direction.Value * ForceLength;
            return (float)Vector3D.Dot(Vector3D.Cross(force, arm), Motor.Top.WorldMatrix.Up);
        }
        public static MyTuple<Vector3D?, Vector3D?> Get_ArmOfForce_Point(KeyValuePair<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Group)
        {
            if (Gun_Rotor_Group.Key == null || Gun_Rotor_Group.Value == null || Gun_Rotor_Group.Value.Count < 1) return new MyTuple<Vector3D?, Vector3D?>(null, null);
            Vector3D center = Vector3D.Zero;
            Vector3D direction = Vector3D.Zero;
            foreach (var gun in Gun_Rotor_Group.Value)
            {
                center += gun.GetPosition();
                if (direction == Vector3D.Zero)
                    direction = gun.WorldMatrix.Forward;
            }
            center /= Gun_Rotor_Group.Value.Count;
            return new MyTuple<Vector3D?, Vector3D?>(direction * Vector3D.Distance(center, Gun_Rotor_Group.Key.GetPosition()), center);//Get arm of force and it force point
        }
    }
}