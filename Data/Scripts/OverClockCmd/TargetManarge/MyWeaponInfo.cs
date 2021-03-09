using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.ModAPI;
using VRage.Game.ModAPI;
using VRageMath;
using System.Linq;
using VRage.Game.Entity;

namespace SuperBlocks.Controller
{
    public struct MyWeaponInfo
    {
        private const string GunPointNM_tag = "muzzle_projectile";
        public IMyTerminalBlock WeaponBlock { get; private set; }
        public List<Matrix> TipMatrix { get; private set; }
        public float WeaponLength { get; private set; }
        public bool IsAvaliable => !Utils.NullEntity(WeaponBlock);
        public bool HasTips => !Utils.IsNullCollection(TipMatrix);
        public MyWeaponInfo(IMyTerminalBlock _WeaponBlock)
        {
            WeaponBlock = _WeaponBlock;
            var ent = WeaponBlock as MyEntity;
            TipMatrix = null;
            WeaponLength = 0;
            if (Utils.NullEntity(ent)) return;
            if (Utils.IsNullCollection(ent.Subparts))
                GetDummies(WeaponBlock);
            else
                foreach (var subpart in ent.Subparts)
                    GetDummies(subpart.Value);
            if (!HasTips) return;
            Vector3 point = Vector3.Zero;
            foreach (var m in TipMatrix)
                point += m.Translation;
            point /= TipMatrix.Count;
            WeaponLength = Math.Max(Math.Max(point.X, point.Y), point.Z);
        }
        public static MyWeaponInfo? WeaponInfoCreator(IMyTerminalBlock WeaponBlock)
        {
            var data = new MyWeaponInfo(WeaponBlock);
            if (!data.IsAvaliable || !data.HasTips) return null;
            return data;
        }
        public static List<MyWeaponInfo> WeaponsInfoCreator(List<IMyTerminalBlock> WeaponBlocks)
        {
            if (Utils.IsNullCollection(WeaponBlocks)) return null;
            List<MyWeaponInfo> WeaponInfos = new List<MyWeaponInfo>();
            foreach (var WeaponBlock in WeaponBlocks)
            {
                var data = new MyWeaponInfo(WeaponBlock);
                if (!data.IsAvaliable || !data.HasTips) continue;
                WeaponInfos.Add(data);
            }
            if (Utils.IsNullCollection(WeaponInfos)) return null;
            return WeaponInfos;
        }
        public static Vector3D? CalculateTotalAvgFirePoint(List<MyWeaponInfo> weapons)
        {
            if (Utils.IsNullCollection(weapons)) return null;
            var non_null = weapons.Where((wp) => wp.IsAvaliable && wp.HasTips);
            if (Utils.IsNullCollection(non_null)) return null;
            Vector3D Point = Vector3D.Zero;
            foreach (var non_null_item in non_null)
                Point += non_null_item.GetAvgFirePoint() ?? Vector3D.Zero;
            Point /= non_null.Count();
            return Point;
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
            if (Utils.NullEntity(entity)) return;
            Dictionary<string, IMyModelDummy> muzzle_projectiles = new Dictionary<string, IMyModelDummy>();
            entity?.Model?.GetDummies(muzzle_projectiles);
            var keys = muzzle_projectiles.Keys?.Where(k => k.Contains(GunPointNM_tag))?.ToList();
            if (Utils.IsNullCollection(keys)) return;
            if (Utils.IsNullCollection(TipMatrix)) TipMatrix = new List<Matrix>();
            foreach (var key in keys)
                TipMatrix.Add(muzzle_projectiles[key].Matrix);
        }
    }
}