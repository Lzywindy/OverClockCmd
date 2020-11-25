using System.Collections.Generic;
using System.Text;

namespace SuperBlocks
{
    using static Utils;

    public class ConfigManager
    {
        private int CurrentIndex { get { return currentindex; } set { if (TotalCount == 0) { currentindex = 0; return; } currentindex = value % TotalCount; } }
        private int TotalCount { get { if (_Config == null || _Config.Count < 1) return 0; return _Config.Count; } }
        private List<Dictionary<string, string>> _Config = null;
        private int currentindex = 0;
        public Dictionary<string, string> CycleConfig()
        {
            if (_Config == null || _Config.Count < 1) return null;
            return _Config[CurrentIndex++];
        }
        public void ResetCycle()
        {
            CurrentIndex = 0;
        }
        public void ModifyConfig(string key, string value)
        {
            if (_Config == null || _Config.Count < 1) return;
            CurrentIndex = CurrentIndex;
            _Config.RemoveAll((Dictionary<string, string> item) => item == null);
            var modify_set_up = _Config[CurrentIndex];
            if (modify_set_up.ContainsKey(key))
                modify_set_up.Remove(key);
            modify_set_up.Add(key, value);
        }
        public void ReadConfigs(string configlines)
        {
            _Config = GetConfigLines(configlines);
            CurrentIndex = CurrentIndex;
        }
        public string SaveConfig()
        {
            if (_Config == null || _Config.Count < 1) return "";
            _Config.RemoveAll((Dictionary<string, string> item) => item == null);
            StringBuilder _configs = new StringBuilder();
            _configs.Clear();
            foreach (var configline in _Config)
            {
                if (configline.Count < 1) continue;
                _configs.Append("{");
                var _enum = configline.GetEnumerator();
                bool hasnext = _enum.MoveNext();
                while (hasnext)
                {
                    var configitem = _enum.Current;
                    _configs.Append($"{configitem.Key}={configitem.Value}");
                    hasnext = _enum.MoveNext();
                    if (hasnext) { _configs.Append(","); continue; }
                    break;
                }
                _configs.Append("}\n\r");
            }
            if (_configs.Length == 0) return "";
            return _configs.ToString();
        }
    }

}
