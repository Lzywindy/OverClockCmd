using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using static SuperBlocks.Utils;

namespace SuperBlocks.Controller
{
    public class WeaponSelector
    {
        public bool UnabledFire => Common.IsNullCollection(_CurretWeapons);
        public List<IMyTerminalBlock> CurrentWeapons => _CurretWeapons;
        public List<IMyTerminalBlock> AllWeapons
        {
            get { return _AllWeapons; }
            set
            {
                try
                {
                    if (Common.IsNullCollection(value)) { _AllWeapons.Clear(); WeaponsKinds.Clear(); _CurretWeapons.Clear(); Weapons2Fire.Clear(); return; }
                    _AllWeapons.Clear();
                    _AllWeapons.AddRange(value);
                    ReferWeapon();
                }
                catch (Exception) { }
            }
        }
        public void ReferWeapon()
        {
            try
            {
                count = 0;
                if (Common.IsNullCollection(_AllWeapons)) { WeaponsKinds.Clear(); _CurretWeapons.Clear(); return; }
                WeaponsKinds.Clear();
                foreach (var item in _AllWeapons) WeaponsKinds.Add(item.BlockDefinition.SubtypeId);
                Pos = WeaponsKinds.GetEnumerator();
                _CurretWeapons.Clear();
                var value = _AllWeapons.Where(b => b.BlockDefinition.SubtypeId == Pos.Current);
                if (Common.IsNullCollection(value)) Pos.MoveNext();
                value = _AllWeapons.Where(b => b.BlockDefinition.SubtypeId == Pos.Current);
                if (Common.IsNullCollection(value)) return;
                _CurretWeapons.AddRange(value);
            }
            catch (Exception) { Pos = WeaponsKinds.GetEnumerator(); }
        }
        public void CycleWeapon()
        {
            try
            {
                if (Common.IsNullCollection(_AllWeapons)) { WeaponsKinds.Clear(); _CurretWeapons.Clear(); return; }
                Pos.MoveNext();
                _CurretWeapons.Clear();
                _CurretWeapons.AddRange(_AllWeapons.Where(b => b.BlockDefinition.SubtypeId == Pos.Current));
            }
            catch (Exception) { Pos = WeaponsKinds.GetEnumerator(); }
        }
        public string GetCurrentWeaponNM() { try { return Pos.Current; } catch (Exception) { Pos = WeaponsKinds.GetEnumerator(); return Definitions.ConfigName.WeaponDef.DefaultWeapon; } }
        public string GetCurrentAmmoNM() { try { return BasicInfoService.WcApi.GetActiveAmmo(_CurretWeapons.FirstOrDefault(), MyWeaponAndTurretApi.GetWeaponID(_CurretWeapons.FirstOrDefault())); } catch (Exception) { Pos = WeaponsKinds.GetEnumerator(); return null; } }
        public void Clear() { _AllWeapons.Clear(); WeaponsKinds.Clear(); _CurretWeapons.Clear(); Weapons2Fire.Clear(); }
        private Queue<IMyTerminalBlock> Weapons2Fire { get; } = new Queue<IMyTerminalBlock>();
        public void ResetFireRPM(float RPM) => firegap = (int)(3600 / RPM);
        public void SetFire(bool Fire = false)
        {
            if (Common.IsNullCollection(_CurretWeapons)) return;
            if (BasicInfoService.WcApi.HasCoreWeapon(_CurretWeapons.FirstOrDefault()))
            {
                if (firegap <= 0)
                {
                    foreach (var weapon in _CurretWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, Fire);
                    Weapons2Fire.Clear(); count = 0;
                    return;
                }
                if (!Common.IsNullCollection(Weapons2Fire)) return;
                if (Common.IsNullCollection(Weapons2Fire) && _CurretWeapons.All(MyWeaponAndTurretApi.CanFire))
                {
                    foreach (var weapon in _CurretWeapons) MyWeaponAndTurretApi.FireWeapon(weapon, false);
                    foreach (var CurrentWeapon in _CurretWeapons)
                        Weapons2Fire.Enqueue(CurrentWeapon);
                    count = 0;
                    return;
                }
            }
        }
        public void RunningAutoFire(bool CanFire = false)
        {
            if (Common.IsNullCollection(_CurretWeapons)) return;
            if (BasicInfoService.WcApi.HasCoreWeapon(_CurretWeapons.FirstOrDefault()))
            {
                if (!CanFire) return;
                if (Weapons2Fire.Count < 1) return;
                if (!MyWeaponAndTurretApi.CanFire(Weapons2Fire.Peek())) return;
                if (count > 0) { count--; return; }
                MyWeaponAndTurretApi.FireWeaponOnce(Weapons2Fire.Dequeue());
                count = firegap;
            }
        }
        private List<IMyTerminalBlock> _CurretWeapons { get; } = new List<IMyTerminalBlock>();
        private List<IMyTerminalBlock> _AllWeapons { get; } = new List<IMyTerminalBlock>();
        private HashSet<string> WeaponsKinds { get; } = new HashSet<string>();
        private IEnumerator<string> Pos;
        private int firegap;
        private int count = 0;
    }
}