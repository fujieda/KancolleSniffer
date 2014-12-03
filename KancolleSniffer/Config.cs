// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class ProxyConfig
    {
        public bool Auto { get; set; }
        public int Listen { get; set; }
        public bool UseUpstream { get; set; }
        public int UpstreamPort { get; set; }

        public ProxyConfig()
        {
            Auto = true;
            Listen = 8080;
            UseUpstream = false;
            UpstreamPort = 8888;
        }
    }

    public class ShipListConfig
    {
        public Point Location { get; set; }
        public Size Size { get; set; }
        public bool ShipType { get; set; }

        public ShipListConfig()
        {
            Location = new Point(int.MinValue, int.MinValue);
        }
    }

    public class Config
    {
        private readonly string _configFileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "config.json");

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
        public int SoundVolume { get; set; }
        public string MissionSoundFile { get; set; }
        public string NDockSoundFile { get; set; }
        public string KDockSoundFile { get; set; }
        public string MaxShipsSoundFile { get; set; }
        public string MaxEquipsSoundFile { get; set; }
        public string DamagedShipSoundFile { get; set; }
        public string Akashi20MinSoundFile { get; set; }
        public string AkashiProgressSoundFile { get; set; }
        public string ConditionSoundFile { get; set; }
        public bool Logging { get; set; }
        public string LogFile { get; set; }
        public ProxyConfig Proxy { get; set; }
        public ShipListConfig ShipList { get; set; }

        public Config()
        {
            Location = new Point(int.MinValue, int.MinValue);
            FlashWindow = ShowBaloonTip = PlaySound = true;
            MarginShips = 4;
            MarginEquips = 10;
            NotifyConditions = new List<int>(new[] {40, 49});
            ResetHours = new List<int>();
            SoundVolume = 100;
            var dir = Path.GetDirectoryName(Application.ExecutablePath);
// ReSharper disable AssignNullToNotNullAttribute
            MissionSoundFile = Path.Combine(dir, "ensei.mp3");
            NDockSoundFile = Path.Combine(dir, "nyuukyo.mp3");
            KDockSoundFile = Path.Combine(dir, "kenzou.mp3");
            MaxShipsSoundFile = Path.Combine(dir, "kanmusu.mp3");
            MaxEquipsSoundFile = Path.Combine(dir, "soubi.mp3");
            DamagedShipSoundFile = Path.Combine(dir, "taiha.mp3");
            Akashi20MinSoundFile = Path.Combine(dir, "20min.mp3");
            AkashiProgressSoundFile = Path.Combine(dir, "syuuri.mp3");
            ConditionSoundFile = Path.Combine(dir, "hirou.mp3");
            LogFile = Path.Combine(dir, "log.txt");
// ReSharper restore AssignNullToNotNullAttribute
            Proxy = new ProxyConfig();
            ShipList = new ShipListConfig();
        }

        public void Load()
        {
            try
            {
                var config = (Config)DynamicJson.Parse(File.ReadAllText(_configFileName));
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(config, null), null);
            }
            catch (FileNotFoundException)
            {
            }
        }

        public void Save()
        {
            File.WriteAllText(_configFileName, DynamicJson.Serialize(this));
        }
    }
}