// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;
using System.Xml.Serialization;
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class QuestCount
    {
        public int Id { get; set; }
        public int Now { get; set; }
        public int[] NowArray { get; set; }

        [XmlIgnore]
        public QuestSpec Spec { get; set; }

        public bool AdjustCount(int progress)
        {
            if (!Spec.AdjustCount)
                return false;
            if (NowArray != null)
            {
                if (progress != 100)
                    return false;
                NowArray = NowArray.Zip(Spec.MaxArray, Math.Max).ToArray();
                return true;
            }
            var next = 0;
            switch (progress)
            {
                case 0:
                    next = 50;
                    break;
                case 50:
                    next = 80;
                    break;
                case 80:
                    next = 100;
                    break;
                case 100:
                    next = 100000;
                    break;
            }
            var now = Now + Spec.Shift;
            var max = Spec.Max + Spec.Shift;
            var low = (int)Math.Ceiling(max * progress / 100.0);
            if (low >= max && progress != 100)
                low = max - 1;
            var high = (int)Math.Ceiling(max * next / 100.0);
            if (now < low)
            {
                Now = low - Spec.Shift;
                return true;
            }
            if (now >= high)
            {
                Now = high - 1 - Spec.Shift;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (Id == 280 || Id == 426 || Id == 854 || Id == 873 || Id == 888 || Id == 894)
                return $"{NowArray.Count(n => n >= 1)}/{Spec.MaxArray.Length}";
            return NowArray != null
                ? string.Join(" ", NowArray.Zip(Spec.MaxArray, (n, m) => $"{n}/{m}"))
                : $"{Now}/{Spec.Max}";
        }

        public QuestCount Clone()
        {
            var clone = (QuestCount)MemberwiseClone();
            if (NowArray != null)
                clone.NowArray = (int[])NowArray.Clone();
            return clone;
        }

        public bool Equals(QuestCount other)
        {
            if (Id != other.Id)
                return false;
            if (NowArray == null)
                return Now == other.Now;
            return NowArray.SequenceEqual(other.NowArray);
        }

        public string ToToolTip()
        {
            switch (Id)
            {
                case 280:
                    return string.Join(" ",
                        new[] {"1-2", "1-3", "1-4", "2-1"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 426:
                    return string.Join(" ",
                        new[] {"警備任務", "対潜警戒任務", "海上護衛任務", "強硬偵察任務"}
                            .Zip(NowArray, (mission, n) => n >= 1 ? mission : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 428:
                    return string.Join(" ",
                        new[] {"対潜警戒任務", "海峡警備行動", "長時間対潜警戒"}.Zip(NowArray, (mission, n) => n >= 1 ? mission + n : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 854:
                    return string.Join(" ",
                        new[] {"2-4", "6-1", "6-3", "6-4"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 873:
                    return string.Join(" ",
                        new[] {"3-1", "3-2", "3-3"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 888:
                    return string.Join(" ",
                        new[] {"5-1", "5-3", "5-4"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 688:
                    return string.Join(" ",
                        new[] {"艦戦", "艦爆", "艦攻", "水偵"}.Zip(NowArray, (type, n) => n >= 1 ? type + n : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 893:
                    return string.Join(" ",
                        new[] {"1-5", "7-1", "7-2G", "7-2M"}.Zip(NowArray, (map, n) => n >= 1 ? $"{map}:{n}" : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
                case 894:
                    return string.Join(" ",
                        new[] {"1-3", "1-4", "2-1", "2-2", "2-3"}.Zip(NowArray, (map, n) => n >= 1 ? map : "")
                            .Where(s => !string.IsNullOrEmpty(s)));
            }
            return "";
        }

        public bool Cleared => NowArray?.Zip(Spec.MaxArray, (n, m) => n >= m).All(x => x) ??
                               Spec.Max != 0 && Now >= Spec.Max;
    }

    public class QuestCounter
    {
        private readonly QuestInfo _questInfo;
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private readonly SortedDictionary<int, QuestStatus> _quests;
        private int _map;
        private int _cell;
        private bool _boss;

        private class ResultShipSpecs
        {
            public ResultShipSpecs(BattleInfo battleInfo)
            {
                Specs = battleInfo.Result?.Friend.Main.Where(s => s.NowHp > 0).Select(ship => ship.Spec).ToArray() ?? new ShipSpec[0];
                Ids = Specs.Select(spec => spec.Id).ToArray();
                Types = Specs.Select(spec => spec.ShipType).ToArray();
                Classes = Specs.Select(spec => spec.ShipClass).ToArray();
                Flagship = Specs.FirstOrDefault();
                FlagshipType = Types.FirstOrDefault();
            }

            public ShipSpec[] Specs { get; }
            public int[] Ids { get; }
            public int[] Types { get; }
            public int[] Classes { get; }
            public ShipSpec Flagship { get; }
            public int FlagshipType { get; }
        }

        public QuestCounter(QuestInfo questInfo, ItemInfo itemInfo, BattleInfo battleInfo)
        {
            _questInfo = questInfo;
            _quests = questInfo.QuestDictionary;
            _itemInfo = itemInfo;
            _battleInfo = battleInfo;
        }

        private bool NeedSave
        {
            set => _questInfo.NeedSave = value;
        }

        public void InspectMapStart(dynamic json)
        {
            if (_quests.TryGetValue(214, out var ago)) // あ号
                ago.Count.NowArray[0]++;
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _map = (int)json.api_maparea_id * 10 + (int)json.api_mapinfo_no;
            _cell = json.api_no() ? (int)json.api_no : 0;
            _boss = (int)json.api_event_id == 5;

            if (_quests.TryGetValue(861, out var q861) && _map == 16 && (int)json.api_event_id == 8)
            {
                if (new ResultShipSpecs(_battleInfo).Types.Count(s => s == 10 || s == 22) == 2)
                    Increment(q861.Count);
            }
        }

        private class Rank
        {
            private readonly string _rank;

            public Rank(string rank)
            {
                _rank = rank;
            }

            public bool S => QuestSortie.CompareRank(_rank, "S") == 0;
            public bool A => QuestSortie.CompareRank(_rank, "A") <= 0;
            public bool B => QuestSortie.CompareRank(_rank, "B") <= 0;
        }

        public void InspectBattleResult(dynamic json)
        {
            var rawRak = json.api_win_rank;
            var rank = new Rank(rawRak);
            var specs = new ResultShipSpecs(_battleInfo);
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                switch (count.Spec)
                {
                    case QuestSortie sortie:
                        if (count.Id == 216 && !_boss || sortie.Check(rawRak, _map, _boss))
                            Increment(count);
                        continue;
                    case QuestEnemyType enemyType:
                        var num = enemyType.CountResult(
                            _battleInfo.Result.Enemy.Main.Concat(_battleInfo.Result.Enemy.Guard));
                        if (num > 0)
                            Add(count, num);
                        continue;
                }
                switch (quest.Id)
                {
                    case 214:
                        if (rank.S)
                            IncrementNth(count, 1);
                        if (_boss)
                        {
                            IncrementNth(count, 2);
                            if (rank.B)
                                IncrementNth(count, 3);
                        }
                        break;
                    case 249:
                        if (_map == 25 && _boss && rank.S &&
                            specs.Ids.Intersect(new[] {62, 63, 64, 265, 266, 268, 319, 192, 194}).Count() == 3)
                        {
                            Increment(count);
                        }
                        break;
                    case 257:
                        if (_map == 14 && _boss && rank.S &&
                            specs.FlagshipType == 3 &&
                            specs.Types.Count(s => s == 3) <= 3 &&
                            specs.Types.All(s => s == 2 || s == 3))
                        {
                            Increment(count);
                        }
                        break;
                    case 259:
                        if (_map == 51 && _boss && rank.S &&
                            specs.Types.Count(type => type == 3) > 0 &&
                            specs.Classes.Count(c => new[]
                            {
                                2, // 伊勢型
                                19, // 長門型
                                26, // 扶桑型
                                37 // 大和型
                            }.Contains(c)) == 3)
                        {
                            Increment(count);
                        }
                        break;
                    case 264:
                        if (_map == 42 && _boss && rank.S &&
                            specs.Types.Count(type => type == 2) >= 2 &&
                            specs.Specs.Count(spec => spec.IsAircraftCarrier) >= 2)
                        {
                            Increment(count);
                        }
                        break;
                    case 266:
                        if (_map == 25 && _boss && rank.S &&
                            specs.FlagshipType == 2 &&
                            specs.Types.OrderBy(x => x).SequenceEqual(new[] {2, 2, 2, 2, 3, 5}))
                        {
                            Increment(count);
                        }
                        break;
                    case 280:
                        if (!(_boss && rank.S))
                            return;
                        if (!(specs.Types.Count(type => type == 1 || type == 2) >= 3 &&
                              specs.Types.Any(type => new[] {3, 4, 7, 21}.Contains(type))))
                            return;
                        switch (_map)
                        {
                            case 12:
                                IncrementNth(count, 0);
                                break;
                            case 13:
                                IncrementNth(count, 1);
                                break;
                            case 14:
                                IncrementNth(count, 2);
                                break;
                            case 21:
                                IncrementNth(count, 3);
                                break;
                        }
                        break;
                    case 854:
                        if (_boss)
                        {
                            switch (_map)
                            {
                                case 24 when rank.A:
                                    IncrementNth(count, 0);
                                    break;
                                case 61 when rank.A:
                                    IncrementNth(count, 1);
                                    break;
                                case 63 when rank.A:
                                    IncrementNth(count, 2);
                                    break;
                                case 64 when rank.S:
                                    IncrementNth(count, 3);
                                    break;
                            }
                        }
                        break;
                    case 862:
                        if (_map == 63 && _boss && rank.A &&
                            specs.Types.Count(s => s == 3) >= 2 &&
                            specs.Types.Count(s => s == 16) >= 1)
                        {
                            Increment(count);
                        }
                        break;
                    case 873:
                        if (_boss && rank.A &&
                            specs.Types.Count(type => type == 3) >= 1)
                        {
                            switch (_map)
                            {
                                case 31:
                                    IncrementNth(count, 0);
                                    break;
                                case 32:
                                    IncrementNth(count, 1);
                                    break;
                                case 33:
                                    IncrementNth(count, 2);
                                    break;
                            }
                        }
                        break;
                    case 875:
                        if (_map == 54 && _boss && rank.S &&
                            specs.Ids.Contains(543) &&
                            specs.Ids.Intersect(new[] {344, 345, 359}).Any())
                        {
                            Increment(count);
                        }
                        break;
                    case 888:
                        if (!_boss || !rank.S)
                            return;
                        var member = new[]
                        {
                            69, 272, 427, // 鳥海
                            61, 264, // 青葉
                            123, 295, 142, // 衣笠
                            59, 262, 416, // 古鷹
                            60, 263, 417, // 加古
                            51, 213, 477, // 天龍
                            115, 293 // 夕張
                        };
                        if (specs.Ids.Intersect(member).Count() < 4)
                            return;
                        switch (_map)
                        {
                            case 51:
                                IncrementNth(count, 0);
                                break;
                            case 53:
                                IncrementNth(count, 1);
                                break;
                            case 54:
                                IncrementNth(count, 2);
                                break;
                        }
                        break;
                    case 893:
                        if (!_boss || !rank.S)
                            return;
                        switch (_map)
                        {
                            case 15:
                                IncrementNth(count, 0);
                                break;
                            case 71:
                                IncrementNth(count, 1);
                                break;
                            case 72:
                                if (_cell == 7)
                                {
                                    IncrementNth(count, 2);
                                    break;
                                }
                                IncrementNth(count, 3);
                                break;
                        }
                        break;
                    case 894:
                        if (!_boss || !rank.S ||
                            !specs.Specs.Any(spec => spec.IsAircraftCarrier))
                            return;
                        switch (_map)
                        {
                            case 13:
                                IncrementNth(count, 0);
                                break;
                            case 14:
                                IncrementNth(count, 1);
                                break;
                            case 21:
                                IncrementNth(count, 2);
                                break;
                            case 22:
                                IncrementNth(count, 3);
                                break;
                            case 23:
                                IncrementNth(count, 4);
                                break;
                        }
                        break;
                }
            }
        }

        private int _questFleet;

        public void StartPractice(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _questFleet = int.Parse(values["api_deck_id"]) - 1;
        }

        public void InspectPracticeResult(dynamic json)
        {
            var rank = new Rank(json.api_win_rank);
            var specs = new ResultShipSpecs(_battleInfo);
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (count.Spec is QuestPractice practice)
                {
                    if (practice.Check(json.api_win_rank))
                        Increment(count);
                    continue;
                }
                switch (quest.Id)
                {
                    case 318:
                        if (_questFleet == 0 && rank.B &&
                            specs.Types.Count(type => type == 3) >= 2)
                        {
                            Increment(count);
                        }
                        break;
                    case 330:
                        if (rank.B &&
                            specs.Flagship.IsAircraftCarrier &&
                            specs.Specs.Count(spec => spec.IsAircraftCarrier) >= 2 &&
                            specs.Types.Count(type => type == 2) >= 2)
                        {
                            Increment(count);
                        }
                        break;
                }
            }
        }

        private readonly int[] _missionId = new int[ShipInfo.FleetCount];

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
                _missionId[(int)entry.api_id - 1] = (int)entry.api_mission[1];
        }

        public void InspectMissionResult(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var deck = int.Parse(values["api_deck_id"]);
            if ((int)json.api_clear_result == 0)
                return;
            var mid = _missionId[deck - 1];
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (count.Spec is QuestMission mission)
                {
                    if (mission.Check(mid))
                        Increment(count);
                    continue;
                }
                switch (quest.Id)
                {
                    case 426:
                        switch (mid)
                        {
                            case 3:
                                IncrementNth(count, 0);
                                break;
                            case 4:
                                IncrementNth(count, 1);
                                break;
                            case 5:
                                IncrementNth(count, 2);
                                break;
                            case 10:
                                IncrementNth(count, 3);
                                break;
                        }
                        break;
                    case 428:
                        switch (mid)
                        {
                            case 4:
                                IncrementNth(count, 0);
                                break;
                            case 101:
                                IncrementNth(count, 1);
                                break;
                            case 102:
                                IncrementNth(count, 2);
                                break;
                        }
                        break;
                }
            }
        }

        public void CountNyukyo() => Increment(503);

        public void CountCharge() => Increment(504);

        public void CountCreateItem()
        {
            Increment(605);
            Increment(607);
        }

        public void CountCreateShip()
        {
            Increment(606);
            Increment(608);
        }

        public void InspectDestroyShip(string request)
        {
            Add(609, HttpUtility.ParseQueryString(request)["api_ship_id"].Split(',').Length);
        }

        public void CountRemodelSlot() => Increment(619);

        public void InspectDestroyItem(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var items = values["api_slotitem_ids"].Split(',')
                .Select(id => _itemInfo.GetStatus(int.Parse(id)).Spec).ToArray();
            Increment(613); // 613: 資源の再利用
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (count.Spec is QuestDestroyItem destroy)
                {
                    if (destroy.Count(count, items))
                        NeedSave = true;
                    continue;
                }
                if (quest.Id == 680)
                {
                    count.NowArray[0] += items.Count(spec => spec.Type == 21);
                    count.NowArray[1] += items.Count(spec => spec.Type == 12 || spec.Type == 13);
                    NeedSave = true;
                }
            }
        }

        public void InspectPowerUp(dynamic json)
        {
            if ((int)json.api_powerup_flag == 0)
                return;
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPowerUp))
                    continue;
                Increment(count);
            }
        }

        public void Increment(QuestCount count)
        {
            Add(count, 1);
        }

        public void Add(QuestCount count, int value)
        {
            count.Now += value;
            NeedSave = true;
        }

        public void Increment(int id)
        {
            Add(id, 1);
        }

        public void Add(int id, int value)
        {
            if (!_quests.TryGetValue(id, out var quest))
                return;
            quest.Count.Now += value;
            NeedSave = true;
        }

        public void IncrementNth(QuestCount count, int n)
        {
            count.NowArray[n]++;
            NeedSave = true;
        }
    }
}