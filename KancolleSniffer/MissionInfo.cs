// Copyright (C) 2013, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Collections.Generic;

namespace KancolleSniffer
{
    public class MissionInfo
    {
        private readonly Dictionary<int, string> _missionNames = new Dictionary<int, string>();
        private readonly NameAndTimer[] _missions = new NameAndTimer[3];

        public MissionInfo()
        {
            for (var i = 0; i < _missions.Length; i++)
                _missions[i] = new NameAndTimer();
        }

        public void InspectMaster(dynamic json)
        {
            foreach (var entry in json)
                _missionNames[(int)entry.api_id] = (string)entry.api_name;
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

        public NameAndTimer[] Missions => _missions;

        public bool InMission(int fleet) => fleet != 0 && _missions[fleet - 1].Name != "";
    }
}