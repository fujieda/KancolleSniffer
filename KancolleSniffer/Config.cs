// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Serialization;

namespace KancolleSniffer
{
    public class ProxyConfig
    {
        public const int DefaultListenPort = 8080;
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
        public bool Visible { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Mode { get; set; }
        public bool ShipType { get; set; }
        public bool ShowHpInPercent { get; set; }
        public ListForm.SortOrder SortOrder { get; set; } = ListForm.SortOrder.ExpToNext;
        public List<List<int>> ShipGroup { get; set; }

        public ShipListConfig()
        {
            Location = new Point(int.MinValue, int.MinValue);
            ShipGroup = new List<List<int>>();
        }
    }

    public class LogConfig
    {
        public bool On { get; set; }
        public string OutputDir { get; set; }
        public int MaterialLogInterval { get; set; }

        public LogConfig()
        {
            On = true;
            OutputDir = "";
            MaterialLogInterval = 10;
        }
    }

    public class KancolleDbConfig
    {
        public bool On { get; set; }
        public string Token { get; set; } = "";
    }

    public class PushbulletConfig
    {
        public bool On { get; set; }
        public string Token { get; set; } = "";
    }

    public class SoundConfig
    {
        public int Volume { get; set; } = 100;

        public string[] Files { get; set; } =
        {
            "ensei.mp3",
            "nyuukyo.mp3",
            "kenzou.mp3",
            "kanmusu.mp3",
            "soubi.mp3",
            "taiha.mp3",
            "20min.mp3",
            "syuuri.mp3",
            "syuuri2.mp3",
            "hirou.mp3"
        };

        public string this[string name]
        {
            get => Files[Config.NotificationIndex[name]];
            set => Files[Config.NotificationIndex[name]] = value;
        }
    }

    [Flags]
    public enum NotificationType
    {
        FlashWindow = 1,
        ShowBaloonTip = 1 << 1,
        PlaySound = 1 << 2,
        Pushbullet = 1 << 3,
        All = (1 << 3) - 1 // Pushbullet以外
    }

    public class NotificationConfig
    {
        public NotificationType[] Settings =
            Config.NotificationNames.Select(x => NotificationType.All).ToArray();

        public NotificationType this[string name]
        {
            get => Settings[Config.NotificationIndex[name]];
            set => Settings[Config.NotificationIndex[name]] = value;
        }
    }

    public class LocationPerMachine
    {
        public string MachineName { get; set; }
        public Point Location { get; set; }
        public int Zoom { get; set; } = 100;
        public Point ListLocation { get; set; }
        public Size ListSize { get; set; }
    }

    public class Config
    {
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _configFileName;

        public Point Location { get; set; } = new Point(int.MinValue, int.MinValue);
        public bool TopMost { get; set; }
        public bool HideOnMinimized { get; set; }
        public bool ExitSilently { get; set; }
        public int Zoom { get; set; } = 100;
        public bool SaveLocationPerMachine { get; set; }
        public List<LocationPerMachine> LocationList { get; set; } = new List<LocationPerMachine>();
        public bool ShowHpInPercent { get; set; }
        public bool FlashWindow { get; set; } = true;
        public bool ShowBaloonTip { get; set; }
        public bool PlaySound { get; set; } = true;
        public NotificationConfig Notifications { get; set; } = new NotificationConfig();
        public int MarginShips { get; set; } = 4;
        public int MarginEquips { get; set; } = 10;
        public List<int> NotifyConditions { get; set; }
        public List<int> ResetHours { get; set; }
        public bool AlwaysShowResultRank { get; set; }
        public bool UsePresetAkashi { get; set; }
        public SoundConfig Sounds { get; set; } = new SoundConfig();
        public bool DebugLogging { get; set; }
        public string DebugLogFile { get; set; } = "log.txt";
        public ProxyConfig Proxy { get; set; } = new ProxyConfig();
        public ShipListConfig ShipList { get; set; } = new ShipListConfig();
        public LogConfig Log { get; set; } = new LogConfig();
        public KancolleDbConfig KancolleDb { get; set; } = new KancolleDbConfig();
        public PushbulletConfig Pushbullet { get; set; } = new PushbulletConfig();

        public static readonly string[] NotificationNames =
        {
            "遠征終了", "入渠終了", "建造完了", "艦娘数超過", "装備数超過",
            "大破警告", "泊地修理20分経過", "泊地修理進行", "泊地修理完了", "疲労回復"
        };

        public static readonly Dictionary<string, int> NotificationIndex =
            NotificationNames.Select((name, i) => new {name, i}).ToDictionary(entry => entry.name, entry => entry.i);

        public Config()
        {
            _configFileName = Path.Combine(_baseDir, "config.xml");
            ConvertPath(PrependBaseDir);
        }

        public void InitializeValues()
        {
            NotifyConditions = new List<int>(new[] {40, 49});
            ResetHours = new List<int>(new[] {2});
        }

        public void Load()
        {
            try
            {
                var serializer = new XmlSerializer(typeof(Config));
                Config config;
                using (var file = File.OpenText(_configFileName))
                    config = (Config)serializer.Deserialize(file);
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(config, null), null);
                if (SaveLocationPerMachine)
                {
                    foreach (var l in LocationList)
                    {
                        if (l.MachineName != Environment.MachineName)
                            continue;
                        Location = l.Location;
                        Zoom = l.Zoom;
                        ShipList.Location = l.ListLocation;
                        ShipList.Size = l.ListSize;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                InitializeValues();
                ReadOldConfig();
                Save();
            }
            ConvertPath(PrependBaseDir);
        }

        public void Save()
        {
            if (SaveLocationPerMachine)
            {
                LocationList = LocationList.Where(l => l.MachineName != Environment.MachineName).ToList();
                LocationList.Add(new LocationPerMachine
                {
                    MachineName = Environment.MachineName,
                    Location = Location,
                    Zoom = Zoom,
                    ListLocation = ShipList.Location,
                    ListSize = ShipList.Size
                });
            }
            else
            {
                LocationList = new List<LocationPerMachine>();
            }
            ConvertPath(StripBaseDir);
            var serializer = new XmlSerializer(typeof(Config));
            using (var file = File.CreateText(_configFileName))
                serializer.Serialize(file, this);
        }

        private void ConvertPath(Func<string, string> func)
        {
            DebugLogFile = func(DebugLogFile);
            Log.OutputDir = func(Log.OutputDir);
            for (var i = 0; i < Sounds.Files.Length; i++)
                Sounds.Files[i] = func(Sounds.Files[i]);
        }

        private string StripBaseDir(string path)
        {
            if (!path.StartsWith(_baseDir))
                return path;
            path = path.Substring(_baseDir.Length);
            return path.StartsWith(Path.DirectorySeparatorChar.ToString()) ? path.Substring(1) : path;
        }

        private string PrependBaseDir(string path) => Path.IsPathRooted(path) ? path : Path.Combine(_baseDir, path);

        private void ReadOldConfig()
        {
            var old = Path.Combine(_baseDir, "config.json");
            dynamic json;
            try
            {
                json = JsonParser.Parse(File.ReadAllText(old));
            }
            catch (FileNotFoundException)
            {
                return;
            }
            Location = new Point((int)json.Location.X, (int)json.Location.Y);
            foreach (var property in (from prop in GetType().GetProperties()
                let type = prop.PropertyType
                where type == typeof(bool) || type == typeof(int) || type == typeof(string)
                select prop))
            {
                if (!json.IsDefined(property.Name))
                    continue;
                var v = json[property.Name];
                property.SetValue(this, property.PropertyType == typeof(int) ? (int)v : v);
            }
            NotifyConditions = new List<int>((int[])json.NotifyConditions);
            ResetHours = new List<int>((int[])json.ResetHours);
            Sounds.Volume = (int)json.SoundVolume;
            var idx = 0;
            foreach (var name in new[]
            {
                "Mission", "NDock", "KDock", "MaxShips", "MaxEquips",
                "DamagedShip", "Akashi20Min", "AkashiProgress", "AkashiComplete", "Condition"
            })
            {
                if (json.IsDefined(name + "SoundFile"))
                    Sounds.Files[idx] = json[name + "SoundFile"];
                idx++;
            }
            Proxy.Auto = json.Proxy.Auto;
            Proxy.Listen = (int)json.Proxy.Listen;
            Proxy.UseUpstream = json.Proxy.UseUpstream;
            Proxy.UpstreamPort = (int)json.Proxy.UpstreamPort;
            var sl = json.ShipList;
            ShipList.Location = new Point((int)sl.Location.X, (int)sl.Location.Y);
            ShipList.Size = new Size((int)sl.Size.Width, (int)sl.Size.Height);
            ShipList.ShipType = sl.ShipType;
            var sg = (int[][])sl.ShipGroup;
            ShipList.ShipGroup = new List<List<int>>();
            foreach (var g in sg)
                ShipList.ShipGroup.Add(new List<int>(g));
            Log.On = json.Log.On;
            Log.OutputDir = json.Log.OutputDir;
            Log.MaterialLogInterval = (int)json.Log.MaterialLogInterval;
        }
    }
}