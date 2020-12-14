using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using Sandbox.Game.Gui;
namespace SuperBlocks.Controller
{
    public struct MyWeaponManager
    {
        public List<IMyUserControllableGun> Weapons { get; private set; }
        public bool SalvoEnabled { get; private set; }
        private int count;
        private int index;
        private bool Firing;
        public List<long> GetWeaponIds()
        {
            if (!WeaponManagersOnline) return null;
            return Weapons.ConvertAll(block => block.EntityId);
        }
        public bool WeaponManagersOnline => (Weapons != null && Weapons.Count > 0);
        public MyWeaponManager(List<IMyUserControllableGun> weapons = null)
        {
            SalvoEnabled = true;
            Firing = false;
            index = 0;
            Weapons = weapons;
            count = 0;
        }
        public void Salvo(bool? CanSalvo = null)
        {
            if (!CanSalvo.HasValue) { SalvoEnabled = !SalvoEnabled; }
            SalvoEnabled = CanSalvo.Value;
        }
        public void SetWeaponsHook(List<IMyUserControllableGun> weapons)
        {
            Weapons = weapons;
        }
        public void FiringWeapons()
        {
            Firing = true;
        }
        public void Salvo()
        {
            if ((!WeaponManagersOnline) || !Firing) return;
            if (SalvoEnabled)
            {
                Firing = false;
                foreach (var weapon in Weapons) weapon.ApplyAction("Shoot");
            }
            else
            {
                if (count > 0) { count--; return; }
                count = 60;
                Weapons[index].ApplyAction("Shoot");
                index = (index + 1) % Weapons.Count;
                if (index == 0) Firing = false;
            }
        }
        public void CalcParameters(out Vector3? FirePosition, out Vector3? Current_CannonDirection, float Cannon_Offset)
        {
            Vector3 vector = Vector3.Zero;
            Vector3 position = Vector3.Zero;
            foreach (var weapon in Weapons)
            {
                vector += weapon.WorldMatrix.Forward;
                position += weapon.GetPosition() + Cannon_Offset * weapon.WorldMatrix.Forward;
            }
            if (vector == Vector3.Zero) Current_CannonDirection = null;
            else { vector.Normalize(); Current_CannonDirection = vector; }
            position /= Weapons.Count;
            FirePosition = position;
        }
    }
}