using System;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using System.Text;
using VRage.Game.Components;
using VRage.Game.ModAPI;

namespace SuperBlocks
{
    public static class Utils
    {        
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
        public static Matrix GetWorldMatrix(IMyTerminalBlock Me)
        {
            Matrix me_matrix;
            Me.Orientation.GetMatrix(out me_matrix);
            return me_matrix;
        }
        public static Vector3 ProjectOnPlane(Vector3 direction, Vector3 planeNormal)
        {
            return Vector3.ProjectOnPlane(ref direction, ref planeNormal);
        }
        public static float Dampener(float value) { return value * Math.Abs(value); }
        public static Vector3 Dampener(Vector3 value) { return value * Math.Abs(value.Length()); }
        public static float CalculateDirection(Vector3D direction_t, Vector3D direction_c, Vector3D rotor_normal, Vector3D rotor_right, float rotor_max_rad_scare = MathHelper.FourPi, float rotor_max_rad = MathHelper.PiOver2)
        {
            var diff = direction_t - direction_c;
            return MathHelper.Clamp(Calc_Direction_Vector(Vector3D.ProjectOnPlane(ref diff, ref rotor_normal), rotor_right) * rotor_max_rad_scare, -rotor_max_rad, rotor_max_rad);
        }
        public static T GetT<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class
        {
            List<T> Items = new List<T>();
            gridTerminalSystem.GetBlocksOfType<T>(Items, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        public static List<T> GetTs<T>(IMyGridTerminalSystem gridTerminalSystem, Func<T, bool> requst = null) where T : class
        {
            List<T> Items = new List<T>();
            gridTerminalSystem.GetBlocksOfType<T>(Items, requst);
            return Items;
        }
        public static T GetT<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class
        {
            List<T> Items = new List<T>();
            blockGroup.GetBlocksOfType<T>(Items, requst);
            if (Items.Count > 0)
                return Items[0];
            else
                return null;
        }
        public static List<T> GetTs<T>(IMyBlockGroup blockGroup, Func<T, bool> requst = null) where T : class
        {
            List<T> Items = new List<T>();
            blockGroup.GetBlocksOfType<T>(Items, requst);
            return Items;
        }
        public static void Restrict<T>(T obj)
        {
            if (obj == null)
            {
                new Exception("Null Object Found!");
            }
        }
        public static void Restrict<T>(List<T> obj)
        {
            if (obj == null || obj.Count == 0)
            {
                new Exception("Null Object Found!");
            }
        }
        public static float Calc_Direction_Vector(Vector3 vector, Vector3 direction)
        {
            return Vector3.Normalize(direction).Dot(vector);
        }
        public static float Calc_Direction_Vector(Vector3 vector, Vector3 direction, float Maxium)
        {
            return MathHelper.Clamp(Calc_Direction_Vector(vector, direction), -Maxium, Maxium);
        }
        public static Vector3 ScaleVectorTimes(Vector3 vector)
        {
            return vector * 10f;
        }
    }
}
