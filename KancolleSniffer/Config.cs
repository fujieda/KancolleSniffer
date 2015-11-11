// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class ProxyConfig
    {
        public const int DefaultListenPort = 8080;
        public const string AutoConfigUrl = "https://kancollesniffer.osdn.jp/proxy.pac";
        public const string AutoConfigUrlWithPort = "https://kancollesniffer.osdn.jp/proxy.php?port=";
        public bool Auto { get; set; }
        public int Listen { get; set; }
        public bool UseUpstream { get; set; }
        public int UpstreamPort { get; set; }

        public ProxyConfig()
        {
            Auto = true;
            Listen = DefaultListenPort;
            UseUpstream = false;
            UpstreamPort = 8888;
        }
    }

    public class ShipListConfig
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool ShipType { get; set; }
        public List<int>[] ShipGroup { get; set; }

        public ShipListConfig()
        {
            Location = new Point(int.MinValue, int.MinValue);
            ShipGroup = new List<int>[ShipListForm.GroupCount];
            for (var i = 0; i < ShipGroup.Length; i++)
                ShipGroup[i] = new List<int>();
        }
    }

    public class LogConfig
    {
        public bool On { get; set; }
        public string OutputDir { get; set; }
        public int MaterialLogInterval { get; set; }
        public bool ServerOn { get; set; }
        public int Listen { get; set; }

        public LogConfig()
        {
            On = true;
            OutputDir = "";
            MaterialLogInterval = 10;
            ServerOn = true;
            Listen = 8008;
        }
    }

    public class Config
    {
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _configFileName;

        public Point Location { get; set; }
        public bool TopMost { get; set; }
        public bool HideOnMinimized { get; set; }
        public bool FlashWindow { get; set; }
        public bool ShowBaloonTip { get; set; }
        public bool PlaySound { get; set; }
        public int MarginShips { get; set; }
        public int MarginEquips { get; set; }
        public List<int> NotifyConditions { get; set; }
        public List<int> ResetHours { get; set; }
        public bool AlwaysShowResultRank { get; set; }
        public bool UsePresetAkashi { get; set; }
        public int SoundVolume { get; set; }
        public string MissionSoundFile { get; set; }
        public string NDockSoundFile { get; set; }
        public string KDockSoundFile { get; set; }
        public string MaxShipsSoundFile { get; set; }
        public string MaxEquipsSoundFile { get; set; }
        public string DamagedShipSoundFile { get; set; }
        public string Akashi20MinSoundFile { get; set; }
        public string AkashiProgressSoundFile { get; set; }
        public string AkashiCompleteSoundFile { get; set; }
        public string ConditionSoundFile { get; set; }
        public bool DebugLogging { get; set; }
        public string DebugLogFile { get; set; }
        public ProxyConfig Proxy { get; set; }
        public ShipListConfig ShipList { get; set; }
        public LogConfig Log { get; set; }

        public Config()
        {
            _configFileName = Path.Combine(_baseDir, "config.json");
            Location = new Point(int.MinValue, int.MinValue);
            FlashWindow = ShowBaloonTip = PlaySound = true;
            MarginShips = 4;
            MarginEquips = 10;
            NotifyConditions = new List<int>(new[] {40, 49});
            ResetHours = new List<int>(new[] {2});
            AlwaysShowResultRank = false;
            SoundVolume = 100;
            MissionSoundFile = "ensei.mp3";
            NDockSoundFile = "nyuukyo.mp3";
            KDockSoundFile = "kenzou.mp3";
            MaxShipsSoundFile = "kanmusu.mp3";
            MaxEquipsSoundFile = "soubi.mp3";
            DamagedShipSoundFile = "taiha.mp3";
            Akashi20MinSoundFile = "20min.mp3";
            AkashiProgressSoundFile = "syuuri.mp3";
            AkashiCompleteSoundFile = "syuuri2.mp3";
            ConditionSoundFile = "hirou.mp3";
            DebugLogFile = "log.txt";
            Proxy = new ProxyConfig();
            ShipList = new ShipListConfig();
            Log = new LogConfig();
            ConvertPath(PrependBaseDir);
        }

        public void Load()
        {
            try
            {
                var config = (Config)DynamicJson.Parse(File.ReadAllText(_configFileName));
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(config, null), null);
                ConvertPath(PrependBaseDir);
            }
            catch (FileNotFoundException)
            {
            }
        }

        public void Save()
        {
            ConvertPath(StripBaseDir);
            File.WriteAllText(_configFileName, DynamicJson.Serialize(this));
        }

        private void ConvertPath(Func<string, string> func)
        {
            foreach (var property in GetType().GetProperties())
            {
                if (!property.Name.EndsWith("File"))
                    continue;
                property.SetValue(this, func((string)property.GetValue(this)));
            }
            Log.OutputDir = func(Log.OutputDir);
        }

        private string StripBaseDir(string path)
        {
            if (!path.StartsWith(_baseDir))
                return path;
            path = path.Substring(_baseDir.Length);
            return path.StartsWith(Path.DirectorySeparatorChar.ToString()) ? path.Substring(1) : path;
        }

        private string PrependBaseDir(string path) => Path.IsPathRooted(path) ? path : Path.Combine(_baseDir, path);
    }
}