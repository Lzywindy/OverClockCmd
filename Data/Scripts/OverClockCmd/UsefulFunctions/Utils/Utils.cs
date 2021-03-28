using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
namespace SuperBlocks
{   
    public static partial class Utils
    {
        public static Guid MyGuid { get; } = new Guid("5F1A43D3-02D3-C959-2413-5922F4EEB917");
        public static Dictionary<string, string> SolveLine(string configline)
        {
            var temp = configline.Split(new string[] { "{", "}", "," }, StringSplitOptions.RemoveEmptyEntries);
            if (temp == null || temp.Length < 1) return null;
            Dictionary<string, string> Config_Pairs = new Dictionary<string, string>();
            foreach (var item in temp)
            {
                var config_pair = item.Split(new string[] { " ", "\t", "=" }, StringSplitOptions.RemoveEmptyEntries);
                if (config_pair == null || config_pair.Length < 1) continue;
                if (config_pair.Length < 2) { Config_Pairs.Add(config_pair[0], ""); continue; }
                if (config_pair.Length < 3) { Config_Pairs.Add(config_pair[0], config_pair[1]); continue; }
            }
            if (Config_Pairs.Count < 1) return null;
            return Config_Pairs;
        }
        public static List<Dictionary<string, string>> GetConfigLines(string configlines)
        {
            var linesarray = configlines.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (linesarray == null || linesarray.Length < 1) return null;
            List<Dictionary<string, string>> configpaires = new List<Dictionary<string, string>>();
            foreach (var line in linesarray)
            {
                var temp = SolveLine(line);
                if (temp == null) continue;
                configpaires.Add(temp);
            }
            return configpaires;
        }

    }
    public static partial class Utils
    {
        public enum WeaponType { Energy, Rocket, Projectile }
        public static bool BlockInTurretGroup(IMyBlockGroup group, IMyTerminalBlock Me)
        {
            if (group == null) return false;
            List<IMyTerminalBlock> blocks = new List<IMyTerminalBlock>();
            group.GetBlocks(blocks); if (blocks.Count < 1) return false;
            if (!blocks.Contains(Me)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => block is IMyRemoteControl)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => block is IMyUserControllableGun)) return false;
            if (!blocks.Exists((IMyTerminalBlock block) => (block is IMyMotorStator) && (block.CustomName.Contains("Turret") || block.CustomName.Contains("turret") || block.CustomName.Contains("Gun") || block.CustomName.Contains("gun")))) return false;
            return true;
        }
        public static Sandbox.ModAPI.Ingame.MyDetectedEntityInfo? CreateTarget(IMyEntity Target, IMyTerminalBlock SensorBlock, Vector3D? Position = null)
        {
            if (Target == null || SensorBlock == null) return null;
            return MyDetectedEntityInfoHelper.Create(MyEntities.GetEntityById(Target.EntityId), SensorBlock.OwnerId, Position ?? Target.GetPosition());
        }
        public static List<IMyTerminalBlock> GetTheNearlyBlock(IMyCubeGrid TargetGrid, IMyTerminalBlock TrackDevice, Func<IMyTerminalBlock, bool> AcceptBlockFilter = null)
        {
            if (TargetGrid == null || TrackDevice == null) return null;
            List<IMyTerminalBlock> blocks = Common.GetTs(MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(TargetGrid), (IMyTerminalBlock _block) => _block.IsFunctional && NotFriendlyBlock(_block, TrackDevice) && (AcceptBlockFilter?.Invoke(_block) ?? true));
            if (blocks == null || blocks.Count < 1) return null;
            if (blocks.Count < 2) return blocks;
            blocks.Sort((IMyTerminalBlock a, IMyTerminalBlock b) => Math.Sign(GetBlockP(b) - GetBlockP(a)));
            return blocks;
        }
        public static void UpdateBlocks(ref List<IMyTerminalBlock> blocks, IMyTerminalBlock TrackDevice, Func<IMyTerminalBlock, bool> AcceptBlockFilter = null)
        {
            if (blocks == null || blocks.Count < 1 || TrackDevice == null) { blocks = null; return; }
            blocks.RemoveAll((IMyTerminalBlock _block) => !_block.IsFunctional || !NotFriendlyBlock(_block, TrackDevice) || _block.Closed || _block.MarkedForClose || (!(AcceptBlockFilter?.Invoke(_block) ?? true)));
            if (blocks.Count < 1) blocks = null;
        }
        public static int GetBlockP(IMyTerminalBlock _block)
        {
            if (_block == null) return 8;
            if (_block is IMyUserControllableGun && _block.IsFunctional) return 0;
            if (_block is IMyThrust && _block.IsFunctional) return 1;
            if (_block is IMyGyro && _block.IsFunctional) return 2;
            if (_block is IMyMotorSuspension && _block.IsFunctional) return 3;
            if (_block is IMyReactor && _block.IsFunctional) return 4;
            if (_block is IMyBatteryBlock && _block.IsFunctional) return 5;
            if (_block is IMyCockpit && _block.IsFunctional) return 6;
            return 7;
        }
        public static bool NotFriendlyBlock(IMyTerminalBlock test, IMyTerminalBlock refer)
        {
            if (test == null || refer == null) return true;
            var relation = refer.GetUserRelationToOwner(test.OwnerId);
            return (relation != MyRelationsBetweenPlayerAndBlock.FactionShare) && (relation != MyRelationsBetweenPlayerAndBlock.Friends) && (relation != MyRelationsBetweenPlayerAndBlock.Owner);
        }



        public static int CycleInteger(int value, int lower, int upper)
        {
            return (value >= upper) ? lower : (value < lower) ? (upper - 1) : value;
        }


        public static bool NonTargetBlock(IMyTerminalBlock block)
        {
            return block == null || !block.IsFunctional || block.Closed || block.MarkedForClose;
        }

        public static List<IMyModelDummy> GetDummies(IMyEntity entity, string DummiesNMTag)
        {
            Dictionary<string, IMyModelDummy> muzzle_projectiles = new Dictionary<string, IMyModelDummy>();
            List<IMyModelDummy> muzzle_projectiles_l = new List<IMyModelDummy>();
            entity?.Model?.GetDummies(muzzle_projectiles);
            if (Common.IsNullCollection(muzzle_projectiles)) return muzzle_projectiles_l;
            var keys = muzzle_projectiles.Keys.Where(k => k.Contains(DummiesNMTag))?.ToList();
            if (Common.IsNullCollection(keys)) return muzzle_projectiles_l;
            foreach (var key in keys) { muzzle_projectiles_l.Add(muzzle_projectiles[key]); }
            return muzzle_projectiles_l;
        }
    }
}
