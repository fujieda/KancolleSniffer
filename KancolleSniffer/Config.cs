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
using KancolleSniffer.Forms;

// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global
// ReSharper disable FieldCanBeMadeReadOnly.Global

namespace KancolleSniffer
{
    public class ProxyConfig
    {
        private const int DefaultListenPort = 8080;
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

    [Flags]
    public enum ShipCategory
    {
        // ReSharper disable UnusedMember.Global
        None = 0,
        BattleShip = 1,
        AircraftCarrier = 1 << 1,
        HeavyCruiser = 1 << 2,
        LightCruiser = 1 << 3,
        Destroyer = 1 << 4,
        Escort = 1 << 5,
        Submarine = 1 << 6,
        Assistant = 1 << 7,
        // ReSharper restore UnusedMember.Global
        All = (1 << 8) - 1
    }

    public class ShipListConfig
    {
        public bool Visible { get; set; }
        public Point Location { get; set; }
        public Size Size { get; set; }
        public string Mode { get; set; }
        public ShipCategory ShipCategories { get; set; } = ShipCategory.All;
        public bool ShipType { get; set; }
        public bool ShowHpInPercent { get; set; }
        public ListForm.SortOrder SortOrder { get; set; } = ListForm.SortOrder.ExpToNext;
        public List<List<int>> ShipGroup { get; set; }
        public bool AutoBattleResult { get; set; } = true;

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

    public class PushbulletConfig
    {
        public bool On { get; set; }
        public string Token { get; set; } = "";
    }

    public class PushoverConfig
    {
        public bool On { get; set; }
        public string ApiKey { get; set; } = "";
        public string UserKey { get; set; } = "";
    }

    public class SoundConfig
    {
        public int Volume { get; set; } = 100;

        public string[] Files =
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
            "hirou.mp3",
            "ninmu.mp3"
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
        // ReSharper disable once IdentifierTypo
        ShowBaloonTip = 1 << 1,
        PlaySound = 1 << 2,
        All = (1 << 3) - 1,
        Push = 1 << 4,
        Repeat = 1 << 5,
        Cont = 1 << 6,
        Preliminary = 1 << 7
    }

    public class NotificationSpec
    {
        public string Name { get; set; }
        public NotificationType Flags { get; set; }
        public int RepeatInterval { get; set; }
        public int PreliminaryPeriod { get; set; }
    }

    public class NotificationConfig
    {
        public NotificationType[] Settings =
            Config.NotificationNames.Select(x => NotificationType.All).ToArray();

        public int[] RepeatIntervals =
            Config.NotificationNames.Select(x => 0).ToArray();

        public int[] PreliminaryPeriods =
            Config.NotificationNames.Select(x => 0).ToArray();

        public NotificationSpec this[string name]
        {
            get => new NotificationSpec
            {
                Name = name,
                Flags = Settings[Config.NotificationIndex[name]],
                RepeatInterval = RepeatIntervals[Config.NotificationIndex[name]],
                PreliminaryPeriod = PreliminaryPeriods[Config.NotificationIndex[name]]
            };
            set
            {
                Settings[Config.NotificationIndex[name]] = value.Flags;
                RepeatIntervals[Config.NotificationIndex[name]] = value.RepeatInterval;
                PreliminaryPeriods[Config.NotificationIndex[name]] = value.PreliminaryPeriod;
            }
        }
    }

    public class LocationPerMachine
    {
        public string MachineName { get; set; }
        public Point Location { get; set; }
        public int Zoom { get; set; } = 100;
        public Point ListLocation { get; set; }
        public Size ListSize { get; set; }
        public List<ShipListConfig> ListFromGroup { get; set; }
    }

    [Flags]
    public enum Spoiler
    {
        ResultRank = 1,
        AirBattleResult = 1 << 1,
        BattleResult = 1 << 2,
        NextCell = 1 << 3,
        // ReSharper disable once UnusedMember.Global
        All = (1 << 4) - 1
    }

    [Flags]
    public enum TimerKind
    {
        Mission = 1,
        NDock = 1 << 1
    }


    public class Config
    {
        public Point Location { get; set; } = new Point(int.MinValue, int.MinValue);
        public bool TopMost { get; set; }
        public bool HideOnMinimized { get; set; }
        public bool ExitSilently { get; set; }
        public int Zoom { get; set; } = 100;
        public string Shape { get; set; } = "縦長";
        public int QuestLines { get; set; } = 6;
        public bool SaveLocationPerMachine { get; set; }
        public List<LocationPerMachine> LocationList { get; set; } = new List<LocationPerMachine>();
        public bool ShowHpInPercent { get; set; }
        public TimerKind ShowEndTime { get; set; }
        public NotificationType NotificationFlags { get; set; } = NotificationType.All;
        public NotificationConfig Notifications { get; set; } = new NotificationConfig();
        public int MarginShips { get; set; } = 5;
        public int MarginEquips { get; set; } = 5;
        public List<int> NotifyConditions { get; set; }
        public List<int> ResetHours { get; set; }
        public Spoiler Spoilers { get; set; }
        public bool UsePresetAkashi { get; set; }
        public bool WarnBadDamageWithDameCon { get; set; }
        public SoundConfig Sounds { get; set; } = new SoundConfig();
        public bool DebugLogging { get; set; }
        public string DebugLogFile { get; set; } = "log.txt";
        public ProxyConfig Proxy { get; set; } = new ProxyConfig();
        public ShipListConfig ShipList { get; set; } = new ShipListConfig();
        public List<ShipListConfig> ListFormGroup { get; set; } = new List<ShipListConfig>();
        public LogConfig Log { get; set; } = new LogConfig();
        public PushbulletConfig Pushbullet { get; set; } = new PushbulletConfig();
        public PushoverConfig Pushover { get; set; } = new PushoverConfig();

        public static readonly string[] NotificationNames =
        {
            "遠征終了", "入渠終了", "建造完了", "艦娘数超過", "装備数超過",
            "大破警告", "泊地修理20分経過", "泊地修理進行", "泊地修理完了", "疲労回復", "任務達成"
        };

        public static readonly Dictionary<string, int> NotificationIndex =
            NotificationNames.Select((name, i) => new {name, i}).ToDictionary(entry => entry.name, entry => entry.i);


        private const string FileName = "config.xml";
        public static readonly string BaseDir = AppDomain.CurrentDomain.BaseDirectory;
        private static readonly string ConfigFile = Path.Combine(BaseDir, FileName);

        public Config()
        {
            ConvertPath(PrependBaseDir);
        }

        private void InitializeValues()
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
                using (var file = File.OpenText(ConfigFile))
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
                        ListFormGroup = l.ListFromGroup;
                    }
                }
            }
            catch (FileNotFoundException)
            {
                InitializeValues();
                Save();
            }
            catch (InvalidOperationException ex)
            {
                File.Delete(ConfigFile);
                throw new Exception(FileName + "が壊れています。", ex);
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
                    ListSize = ShipList.Size,
                    ListFromGroup = ListFormGroup
                });
            }
            else
            {
                LocationList = new List<LocationPerMachine>();
            }
            ConvertPath(StripBaseDir);
            var serializer = new XmlSerializer(typeof(Config));
            using (var file = File.CreateText(ConfigFile + ".tmp"))
                serializer.Serialize(file, this);
            File.Copy(ConfigFile + ".tmp", ConfigFile, true);
            File.Delete(ConfigFile + ".tmp");
            ConvertPath(PrependBaseDir);
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
            if (!path.StartsWith(BaseDir))
                return path;
            path = path.Substring(BaseDir.Length);
            return path.TrimStart(Path.DirectorySeparatorChar);
        }

        private string PrependBaseDir(string path) => Path.IsPathRooted(path) ? path : Path.Combine(BaseDir, path);
    }
}