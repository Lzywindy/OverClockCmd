using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Text;
namespace SuperBlocks
{
    public static class MyConfigs
    {
        public static Dictionary<string, Dictionary<string, string>> CustomDataConfigRead_INI(string CustomData)
        {
            var lines = CustomData.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines == null || lines.Length < 1) return null;
            Dictionary<string, Dictionary<string, string>> Configs = new Dictionary<string, Dictionary<string, string>>();
            string current_block_name = "";
            for (int index = 0; index < lines.Length; index++)
            {
                var line = RemoveStartEndEmpty(RemoveIniComment(lines[index]));
                if (IsNewBlockStart(line))
                {
                    current_block_name = NewBlockName(line);
                    Configs.Add(current_block_name, new Dictionary<string, string>());
                    continue;
                }
                if (!Configs.ContainsKey(current_block_name))
                    continue;
                GetProperty(Configs[current_block_name], line);
            }
            return Configs;
        }
        public static void CustomDataConfigRead_INI(IMyTerminalBlock ShipController, Dictionary<string, Dictionary<string, string>> Configs)
        {
            if (ShipController == null || Configs == null) return;
            var lines = ShipController.CustomData.Split(new string[] { "\n", "\r" }, StringSplitOptions.RemoveEmptyEntries);
            if (lines == null || lines.Length < 1) return;
            string current_block_name = "";
            for (int index = 0; index < lines.Length; index++)
            {
                var line = RemoveStartEndEmpty(RemoveIniComment(lines[index]));
                if (IsNewBlockStart(line))
                {
                    current_block_name = NewBlockName(line);
                    if (!Configs.ContainsKey(current_block_name))
                        Configs.Add(current_block_name, new Dictionary<string, string>());
                    continue;
                }
                if (!Configs.ContainsKey(current_block_name))
                    continue;
                GetProperty(Configs[current_block_name], line);
            }
        }
        public static void ModifyProperty(Dictionary<string, string> Properties, string key, string value)
        {
            if (Properties == null) return;
            if (Properties.ContainsKey(key))
                Properties.Remove(key);
            Properties.Add(key, value);
        }
        public static string CustomDataConfigSave_INI(Dictionary<string, Dictionary<string, string>> ConfigTree)
        {
            if (ConfigTree == null || ConfigTree.Count < 1) return "";
            StringBuilder _str = new StringBuilder();
            _str.Clear();
            foreach (var ConfigBlock in ConfigTree)
            {
                _str.AppendLine($"[{ConfigBlock.Key}]");
                foreach (var ConfigItem in ConfigBlock.Value)
                    _str.AppendLine($"{ConfigItem.Key}={ConfigItem.Value}");
                _str.AppendLine();
            }
            return _str.ToString();
        }
        #region BasicFunctions
        public static int ParseInt(string str) { int value; if (!int.TryParse(str, out value)) value = 0; return value; }
        public static float ParseFloat(string str) { float value; if (!float.TryParse(str, out value)) value = 0; return value; }
        public static double ParseDouble(string str) { double value; if (!double.TryParse(str, out value)) value = 0; return value; }
        public static bool ParseBool(string str) { bool value; if (!bool.TryParse(str, out value)) value = false; return value; }
        public static string RemoveIniComment(string line) { var com_start_index = line.IndexOf(';'); if (com_start_index < 0) return line; return line.Remove(com_start_index); }
        public static string RemoveStartEndEmpty(string str) => str.TrimStart(' ', '\t').TrimEnd(' ', '\t');
        public static bool IsNewBlockStart(string str) => str.StartsWith("[") && str.EndsWith("]");
        public static string NewBlockName(string str) => str.TrimStart('[').TrimEnd(']');
        public static void GetProperty(Dictionary<string, string> Properties, string line)
        {
            if (Properties == null) return;
            var key_value = line.Split(new string[] { "=" }, StringSplitOptions.RemoveEmptyEntries);
            if (key_value == null || key_value.Length < 1) return;
            for (int index = 0; index < key_value.Length; index++)
                key_value[index] = RemoveStartEndEmpty(key_value[index]);
            if (key_value.Length == 1)
            {
                ModifyProperty(Properties, key_value[0], "");
                return;
            }
            ModifyProperty(Properties, key_value[0], key_value[1]);
        }
        #endregion
    }
}
