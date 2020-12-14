using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using VRage;
namespace SuperBlocks.Controller
{
    public struct MyTurretRotateManage
    {
        public MyTurretRotateManage(float Max_Az_Speed, float Max_Ev_Speed, float RotationMult)
        {
            this.Max_Az_Speed = MathHelper.Clamp(Max_Az_Speed, 0.01f, 90f);
            this.Max_Ev_Speed = MathHelper.Clamp(Max_Ev_Speed, 0.01f, 90f);
            this.RotationMult = Math.Max(RotationMult, 0.01f);
            Gun_Rotor_Groups = null;
            turret_guns = null;
            turret_base = null;
        }
        public void Set_Max_Az_Speed(float Max_Az_Speed = 30)
        {
            this.Max_Az_Speed = MathHelper.Clamp(Max_Az_Speed, 0.01f, 90f);
        }
        public void Set_Max_Ev_Speed(float Max_Ev_Speed = 30)
        {
            this.Max_Ev_Speed = MathHelper.Clamp(Max_Ev_Speed, 0.01f, 90f);
        }
        public void Set_RotationMult(float RotationMult = 1)
        {
            this.RotationMult = MathHelper.Clamp(RotationMult, 0.01f, 90f);
        }
        public void Running(Vector3D? Direction = null)
        {
            if (Direction.HasValue)
                RotationMotors(turret_base, Gun_Rotor_Groups, Direction, RotationMult, Max_Az_Speed, Max_Ev_Speed);
            else
                RunningDefault();
        }
        public void SetupTurrets(IMyMotorStator turret_base, List<IMyMotorStator> turret_guns, List<IMyTerminalBlock> topguns)
        {
            SetupRotors(turret_base, turret_guns);
            SetupGuns(topguns);
        }
        public void SetupRotors(IMyMotorStator turret_base, List<IMyMotorStator> turret_guns)
        {
            this.turret_base = turret_base;
            this.turret_guns = turret_guns;
            RunningDefault();
        }
        public void SetupGuns(List<IMyTerminalBlock> topguns)
        {
            Gun_Rotor_Groups = Get_Gun_Motor_Group(turret_guns, topguns);
        }
        private float Max_Az_Speed;
        private float Max_Ev_Speed;
        private float RotationMult;
        public bool CanRunning => (!IncompleteStructOfTurret) && (Gun_Rotor_Groups != null && Gun_Rotor_Groups.Count > 0);
        public bool IncompleteStructOfTurret => MissingAzRotor || MissingEvRotor;
        public bool MissingAzRotor => turret_base == null;
        public bool MissingEvRotor => turret_guns == null || turret_guns.Count < 1;
        public void RunningDefault()
        {
            if (!MissingAzRotor)
                turret_base.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(turret_base.Angle), -Max_Az_Speed, Max_Az_Speed);
            if (!MissingEvRotor)
                foreach (var turret_gun in turret_guns)
                    turret_gun.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(turret_gun.Angle), -Max_Ev_Speed, Max_Ev_Speed);
        }
        public IMyMotorStator turret_base { get; private set; }
        public List<IMyMotorStator> turret_guns { get; private set; }
        private Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Groups;
        public static void RotationMotors(IMyMotorStator Turret, Dictionary<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Groups, Vector3D? Direction, double ForceLength, float Max_Az = 30, float Max_Ev = 30)
        {
            if (Turret == null || Gun_Rotor_Groups == null || Gun_Rotor_Groups.Count < 1) return;
            Max_Az = Math.Abs(Max_Az); Max_Ev = Math.Abs(Max_Ev);
            float TurretRotor_Torque = 0;
            foreach (var Gun_Rotor_Group in Gun_Rotor_Groups)
            {
                if (Direction == null)
                {
                    Gun_Rotor_Group.Key.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Gun_Rotor_Group.Key.Angle), -Max_Ev, Max_Ev);
                }
                else
                {
                    var data = Get_ArmOfForce_Point(Gun_Rotor_Group);
                    TurretRotor_Torque += GetTorque(Turret, data, Direction, ForceLength);
                    Gun_Rotor_Group.Key.TargetVelocityRad = MathHelper.Clamp(GetTorque(Gun_Rotor_Group.Key, data, Direction, ForceLength) - MathHelper.Clamp(Gun_Rotor_Group.Key.TargetVelocityRad, -1, 1) * 0.25f, -Max_Ev, Max_Ev);
                }
            }
            TurretRotor_Torque /= Gun_Rotor_Groups.Count;
            if (Direction == null)
                Turret.TargetVelocityRad = -MathHelper.Clamp(MathHelper.WrapAngle(Turret.Angle), -Max_Az, Max_Az);
            else
                Turret.TargetVelocityRad = MathHelper.Clamp(TurretRotor_Torque - MathHelper.Clamp(Turret.TargetVelocityRad, -1, 1) * 0.25f, -Max_Az, Max_Az);
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