﻿using Sandbox.ModAPI;
using Sandbox.ModAPI.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage;
using VRageMath;
using static SuperBlocks.Definitions.Structures;

namespace SuperBlocks.Controller
{
    internal static class MyWeaponAndTurretApi
    {
        internal static bool IsIMyTerminalBlockWeapon(IMyTerminalBlock block) => BasicInfoService.WeaponInfos.ContainsKey(block.BlockDefinition.SubtypeId) || (block is IMyUserControllableGun);
        internal static void SetIMyTerminalBlock(IMyTerminalBlock block, bool Enabled) { if (block is IMyFunctionalBlock) (block as IMyFunctionalBlock).Enabled = Enabled; }
        internal static bool GetIMyTerminalBlock(IMyTerminalBlock block) => (block as IMyFunctionalBlock)?.Enabled ?? true;
        internal static float GetMaxVector3D(Vector3D value) => (float)Math.Max(Math.Max(value.X, value.Y), value.Z);

        internal static bool CanFire(IMyTerminalBlock Weapon)
        {
            if (Weapon == null) return false;
            return (BasicInfoService.WeaponInfos.ContainsKey(Weapon.BlockDefinition.SubtypeId) && BasicInfoService.WcApi.IsWeaponReadyToFire(Weapon, GetWeaponID(Weapon))) || (!BasicInfoService.WeaponInfos.ContainsKey(Weapon.BlockDefinition.SubtypeId));
        }
        internal static void FireWeapon(IMyTerminalBlock Weapon, bool Enabled)
        {
            try
            {
                if (Utils.Common.NullEntity(Weapon)) return;
                if (Weapon is IMyFunctionalBlock) (Weapon as IMyFunctionalBlock).Enabled = true;
                if (BasicInfoService.WcApi.HasCoreWeapon(Weapon))
                    BasicInfoService.WcApi.ToggleWeaponFire(Weapon, Enabled, true, GetWeaponID(Weapon));
            }
            catch (Exception) { }
        }
        internal static void FireWeaponOnce(IMyTerminalBlock Weapon)
        {
            try
            {
                if (Utils.Common.NullEntity(Weapon)) return;
                if (Weapon is IMyFunctionalBlock) (Weapon as IMyFunctionalBlock).Enabled = true;
                if (BasicInfoService.WcApi.HasCoreWeapon(Weapon))
                    BasicInfoService.WcApi.FireWeaponOnce(Weapon, true, GetWeaponID(Weapon));
            }
            catch (Exception) { }
        }
        internal static int GetWeaponID(IMyTerminalBlock weapon)
        {
            Dictionary<string, int> weaponmap = new Dictionary<string, int>();
            BasicInfoService.WcApi.GetBlockWeaponMap(weapon, weaponmap);
            if (weaponmap.Count < 1) return -1;
            var list = weaponmap.Keys.ToList();
            return weaponmap[list[list.Count - 1]];
        }
        internal static string GetWeaponNM(IMyTerminalBlock weapon)
        {
            Dictionary<string, int> weaponmap = new Dictionary<string, int>();
            BasicInfoService.WcApi.GetBlockWeaponMap(weapon, weaponmap);
            if (weaponmap.Count < 1) return "DefaultWeapon";
            var list = weaponmap.Keys.ToList();
            return list[list.Count - 1];
        }
        internal static Dictionary<IMyTerminalBlock, string> GetCurrentAmmo(ICollection<IMyTerminalBlock> WBs)
        {
            if (WBs == null || WBs.Count < 1) return null;
            Dictionary<IMyTerminalBlock, string> WeaponAmmo = new Dictionary<IMyTerminalBlock, string>();
            foreach (var item in WBs) { if (item.IsFunctional) WeaponAmmo.Add(item, BasicInfoService.WcApi.GetActiveAmmo(item, GetWeaponID(item))); }
            return WeaponAmmo;
        }
        internal static float RotorDampener(float ControlValue, float CurretValue, float MaxValue)
        {
            return MathHelper.Clamp(ControlValue - MathHelper.Clamp(Math.Max(ControlValue, CurretValue), -1, 1) * 0.5f, -MaxValue, MaxValue);
        }
        internal static float GetTorque(IMyMotorStator Motor, MyTuple<Vector3D?, Vector3D?> ArmOfForce_Point, Vector3D? Direction, double ForceLength)
        {
            if (Motor == null || Motor.TopGrid == null) return 0;
            if (ArmOfForce_Point.Item1 == null || ArmOfForce_Point.Item2 == null || Direction == null || Direction.Value == Vector3D.Zero || ForceLength == 0)
                return -MathHelper.Clamp(MathHelper.WrapAngle(Motor.Angle), -30, 30);
            var arm = ArmOfForce_Point.Item1.Value;
            var force = Direction.Value * ForceLength;
            return (float)Vector3D.Dot(Vector3D.Cross(force, arm), Motor.Top.WorldMatrix.Up);
        }
        internal static MyTuple<Vector3D?, Vector3D?> Get_ArmOfForce_Point(KeyValuePair<IMyMotorStator, HashSet<IMyTerminalBlock>> Gun_Rotor_Group)
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
        internal static bool ActiveRotorBasicFilter(IMyMotorStator block) => block?.TopGrid != null;
        internal static MyWeaponParametersConfig? GetWeaponCoreDefinition(IMyTerminalBlock block, string AmmoName)
        {
            if (!BasicInfoService.WeaponInfos.ContainsKey(block.BlockDefinition.SubtypeId)) return null;
            if (!BasicInfoService.WeaponInfos[block.BlockDefinition.SubtypeId].ContainsKey(AmmoName)) return null;
            var ammo = BasicInfoService.WeaponInfos[block.BlockDefinition.SubtypeId][AmmoName];
            return new MyWeaponParametersConfig
            {
                Delta_t = 1.2f,
                Delta_precious = 0.005f,
                Calc_t = 12,
                TimeFixed = 5,
                Range = ammo.MaxTrajectory,
                Trajectory = ammo
            };
        }
    }
}