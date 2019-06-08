// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
{
    public class CellInfo : Sniffer.IPort
    {
        private int _battleCount;

        public string Next { get; set; }

        public string Current { get; set; }

        private string _info;

        public void Port()
        {
            Current = Next = _info;
            _info = "";
        }

        public void StartBattle()
        {
            Current = Next = _info;
        }

        public void StartPractice()
        {
            Current = Next = _info;
        }

        public void InspectMapStart(dynamic json)
        {
            _battleCount = 0;
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            Current = _info;
            SetInfo(json);
            Next = "次" + _info;
        }

        private void SetInfo(dynamic json)
        {
            switch ((int)json.api_color_no)
            {
                case 2:
                    _info = "資源";
                    break;
                case 3:
                    _info = "渦潮";
                    break;
                case 4:
                    switch ((int)json.api_event_id)
                    {
                        case 4:
                            _battleCount++;
                            _info = $"{BattleCount}戦目";
                            break;
                        case 6:
                            _info = "気のせい";
                            break;
                    }
                    break;
                case 5:
                    _info = "ボス戦";
                    break;
                case 6:
                    _info = "揚陸地点";
                    break;
                case 7:
                    _battleCount++;
                    _info = $"{BattleCount}戦目(航空)";
                    break;
                case 8:
                    _info = "護衛成功";
                    break;
                case 9:
                    _info = "航空偵察";
                    break;
                case 10:
                    _battleCount++;
                    _info = $"{BattleCount}戦目(空襲)";
                    break;
                case 11:
                    _battleCount++;
                    _info = $"{BattleCount}戦目(夜戦)";
                    break;
            }
        }

        private string BattleCount => ((char)('０' + _battleCount)).ToString();
    }
}