using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using System.Linq;
using VRage.Game.Entity;
using Sandbox.ModAPI.Interfaces;

namespace SuperBlocks.Controller
{
    public struct MyWeaponInfo : IEqualityComparer<MyWeaponInfo>, IEquatable<MyWeaponInfo>
    {
        public IMyTerminalBlock WeaponBlock { get; private set; }
        public List<Matrix> TipMatrix { get; private set; }
        public float WeaponLength { get; private set; }
        public bool IsAvaliable => !Utils.Common.NullEntity(WeaponBlock);
        public bool HasTips => !Utils.Common.IsNullCollection(TipMatrix);
        public MyWeaponInfo(IMyTerminalBlock _WeaponBlock)
        {
            WeaponBlock = _WeaponBlock; var ent = WeaponBlock as MyEntity; TipMatrix = null; WeaponLength = 0;
            if (Utils.Common.NullEntity(ent)) return;
            if (Utils.Common.IsNullCollection(ent.Subparts))
                GetDummies(WeaponBlock);
            else
                foreach (var subpart in ent.Subparts)
                    GetDummies(subpart.Value);
            if (!HasTips) return; Vector3 point = Vector3.Zero; foreach (var m in TipMatrix) point += m.Translation;
            point /= TipMatrix.Count; WeaponLength = Math.Max(Math.Max(point.X, point.Y), point.Z);
        }
        public void FireWeapon(bool Enabled) => FireWeapon(WeaponBlock, Enabled);
        public void FireWeaponOnce() => FireWeaponOnce(WeaponBlock);
        public IMyInventory GetInventory() => WeaponBlock?.GetInventory();
        public List<VRage.Game.ModAPI.Ingame.MyItemType> GetMyItemTypes()
        {
            List<VRage.Game.ModAPI.Ingame.MyItemType> myItems = new List<VRage.Game.ModAPI.Ingame.MyItemType>();
            WeaponBlock?.GetInventory()?.GetAcceptedItems(myItems);
            return myItems;
        }
        public Vector3D? GetAvgFirePoint()
        {
            if (!IsAvaliable || !HasTips) return null;
            return WeaponBlock.GetPosition() + WeaponBlock.WorldMatrix.Forward * WeaponLength;
        }
        public List<Vector3D> GetFirePoints()
        {
            if (!IsAvaliable || !HasTips) return null;
            var Points = new List<Vector3D>();
            foreach (var m in TipMatrix)
                Points.Add(Vector3D.Transform(m.Translation, WeaponBlock.WorldMatrix));
            return Points;
        }
        private void GetDummies(IMyEntity entity)
        {
            if (Utils.Common.NullEntity(entity)) return;
            Dictionary<string, IMyModelDummy> muzzle_projectiles = new Dictionary<string, IMyModelDummy>();
            entity?.Model?.GetDummies(muzzle_projectiles);
            var keys = muzzle_projectiles.Keys?.Where(k => k.Contains(GunPointNM_tag))?.ToList();
            if (Utils.Common.IsNullCollection(keys)) return;
            if (Utils.Common.IsNullCollection(TipMatrix)) TipMatrix = new List<Matrix>();
            foreach (var key in keys)
                TipMatrix.Add(muzzle_projectiles[key].Matrix);
        }
        #region EqualityOverride
        public bool Equals(MyWeaponInfo x, MyWeaponInfo y) => (x.WeaponBlock == y.WeaponBlock);
        public int GetHashCode(MyWeaponInfo obj) => WeaponBlock?.GetHashCode() ?? -1;
        public bool Equals(MyWeaponInfo other) => Equals(this, other);
        #endregion
        #region PublicFunctions
        public static List<MyWeaponInfo> WeaponsInfoCreator(ICollection<IMyTerminalBlock> WeaponBlocks) { List<MyWeaponInfo> WeaponInfos = new List<MyWeaponInfo>(); if (Utils.Common.IsNullCollection(WeaponBlocks)) return WeaponInfos; foreach (var WeaponBlock in WeaponBlocks) { var data = WeaponInfoCreator(WeaponBlock); if (data == null) continue; WeaponInfos.Add(data.Value); } return WeaponInfos; }
        public static MyWeaponInfo? WeaponInfoCreator(IMyTerminalBlock WeaponBlock) { var data = new MyWeaponInfo(WeaponBlock); if (!data.IsAvaliable || !data.HasTips) return null; return data; }
        public static Vector3D? CalculateTotalAvgFirePoint(ICollection<MyWeaponInfo> weapons)
        {
            var non_null = weapons?.Where((wp) => wp.IsAvaliable && wp.HasTips)?.ToList();
            if (Utils.Common.IsNullCollection(non_null)) return null; Vector3D Point = Vector3D.Zero;
            foreach (var non_null_item in non_null) Point += non_null_item.GetAvgFirePoint() ?? Vector3D.Zero;
            Point /= non_null.Count(); return Point;
        }
        private static void FireWeapon(IMyTerminalBlock Weapon, bool Enabled)
        {
            if (BasicInfoService.WcApi.HasCoreWeapon(Weapon))
                BasicInfoService.WcApi.ToggleWeaponFire(Weapon, Enabled, true, GetWeaponID(Weapon));
            else
                (Weapon as IMyUserControllableGun)?.SetValue("Shoot", Enabled);
        }
        private static void FireWeaponOnce(IMyTerminalBlock Weapon)
        {
            if (BasicInfoService.WcApi.HasCoreWeapon(Weapon))
            {
                BasicInfoService.WcApi.FireWeaponOnce(Weapon, true, GetWeaponID(Weapon));
            }
            else
            {
                (Weapon as IMyUserControllableGun)?.SetValue("Shoot", true);
                (Weapon as IMyUserControllableGun)?.SetValue("Shoot", false);
            }
        }
        private static int GetWeaponID(IMyTerminalBlock weapon)
        {
            Dictionary<string, int> weaponmap = new Dictionary<string, int>();
            BasicInfoService.WcApi.GetBlockWeaponMap(weapon, weaponmap);
            if (weaponmap.Count < 1) return -1;
            var list = weaponmap.Keys.ToList();
            return weaponmap[list[list.Count - 1]];
        }
        private const string GunPointNM_tag = "muzzle_projectile";
        #endregion
    }
}