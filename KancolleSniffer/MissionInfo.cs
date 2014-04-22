// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class MissionInfo
    {
        private readonly Dictionary<int, string> _missionNames = new Dictionary<int, string>();
        private readonly NameAndTimer[] _missions = new NameAndTimer[3];

        private readonly string _missionNamesFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "missions.json");

        public MissionInfo()
        {
            for (var i = 0; i < _missions.Length; i++)
                _missions[i] = new NameAndTimer();
        }

        public void InspectMission(dynamic json)
        {
            foreach (var entry in json)
                _missionNames[(int)entry.api_id] = (string)entry.api_name;
        }

        public void LoadNames()
        {
            try
            {
                InspectMission(DynamicJson.Parse(File.ReadAllText(_missionNamesFile)));
            }
            catch (FileNotFoundException)
            {
            }
        }

        public void SaveNames()
        {
            var ship = from data in _missionNames select new {api_id = data.Key, api_name = data.Value};
            File.WriteAllText(_missionNamesFile, DynamicJson.Serialize(ship));
        }

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                if (id == 1)
                    continue;
                id -= 2;
                var mission = entry.api_mission;
                if (mission[0] == 0)
                {
                    _missions[id].Name = "";
                    _missions[id].Timer.SetEndTime(0);
                    continue;
                }
                string name;
                _missions[id].Name = _missionNames.TryGetValue((int)mission[1], out name) ? name : "不明";
                _missions[id].Timer.SetEndTime(mission[2]);
            }
        }

        public NameAndTimer[] Missions
        {
            get { return _missions; }
        }
    }
}