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
    public class Config
    {
        private readonly string _configFileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "config.json");

        public Point Location { get; set; }
        public bool TopMost { get; set; }
        public bool FlashWindow { get; set; }
        public bool ShowBaloonTip { get; set; }
        public bool PlaySound { get; set; }
        public int MarginShips { get; set; }
        public List<int> ResetHours { get; set; }
        public int SoundVolume { get; set; }
        public string MissionSoundFile { get; set; }
        public string NDockSoundFile { get; set; }
        public string KDockSoundFile { get; set; }
        public string MaxShipsSoundFile { get; set; }
        public string DamagedShipSoundFile { get; set; }

        public Config()
        {
            Location = new Point(int.MinValue, int.MinValue);
            FlashWindow = ShowBaloonTip = PlaySound = true;
            MarginShips = 4;
            ResetHours = new List<int>();
            SoundVolume = 100;
            var dir = Path.GetDirectoryName(Application.ExecutablePath);
// ReSharper disable AssignNullToNotNullAttribute
            MissionSoundFile = Path.Combine(dir, "ensei.mp3");
            NDockSoundFile = Path.Combine(dir, "nyuukyo.mp3");
            KDockSoundFile = Path.Combine(dir, "kenzou.mp3");
            MaxShipsSoundFile = Path.Combine(dir, "kanmusu.mp3");
            DamagedShipSoundFile = Path.Combine(dir, "taiha.mp3");
// ReSharper restore AssignNullToNotNullAttribute
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