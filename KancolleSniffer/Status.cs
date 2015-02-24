// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.IO;
using System.Windows.Forms;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class Status
    {
        private readonly string _statusFileName = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath),
            "status.json");

        public static bool Restoring { get; set; }
        public int ExperiencePoint { get; set; }
        public DateTime LastResetTime { get; set; }
        public MaterialCount[] MatreialHistory { get; set; }
        public double CondRegenTime { get; set; }

        public Status()
        {
            CondRegenTime = double.MinValue;
        }

        public void Load()
        {
            try
            {
                Restoring = true;
                var obj = (Status)DynamicJson.Parse(File.ReadAllText(_statusFileName));
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(obj, null), null);
            }
            catch (FileNotFoundException)
            {
            }
            finally
            {
                Restoring = false;
            }
        }

        public void Save()
        {
            File.WriteAllText(_statusFileName, DynamicJson.Serialize(this));
        }
    }
}