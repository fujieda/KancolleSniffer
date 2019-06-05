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

            if (_quests.TryGetValue(861, out var q861))
            {
                if (_map == 16 && (int)json.api_event_id == 8)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet.Count(s => s == 10 || s == 22) == 2)
                        Increment(q861.Count);
                }
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            var rank = json.api_win_rank;
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                switch (count.Spec)
                {
                    case QuestSortie sortie:
                        if (count.Id == 216 && !_boss || sortie.Check(rank, _map, _boss))
                            Increment(count);
                        break;
                    case QuestEnemyType enemyType:
                        var num = enemyType.CountResult(
                            _battleInfo.Result.Enemy.Main.Concat(_battleInfo.Result.Enemy.Guard));
                        if (num > 0)
                            Add(count, num);
                        break;
                }
            }
            if (_quests.TryGetValue(214, out var ago))
            {
                var count = ago.Count;
                if (_boss)
                {
                    IncrementNth(count, 2);
                    if (QuestSortie.CompareRank(rank, "B") <= 0)
                        IncrementNth(count, 3);
                }
                if (rank == "S")
                    IncrementNth(count, 1);
            }
            if (_quests.TryGetValue(249, out var q249))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id)
                        .ToArray();
                    if (fleet.Intersect(new[] {62, 63, 64, 265, 266, 268, 319, 192, 194}).Count() == 3)
                        Increment(q249.Count);
                }
            }
            if (_quests.TryGetValue(257, out var q257))
            {
                if (_map == 14 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 3 && fleet.Count(s => s == 3) <= 3 && fleet.All(s => s == 2 || s == 3))
                        Increment(q257.Count);
                }
            }
            if (_quests.TryGetValue(259, out var q259))
            {
                if (_map == 51 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec).ToArray();
                    // ReSharper disable once IdentifierTypo
                    var ctype = new[]
                    {
                        2, // 伊勢型
                        19, // 長門型
                        26, // 扶桑型
                        37 // 大和型
                    };
                    if (fleet.Select(s => s.ShipClass).Count(c => ctype.Contains(c)) == 3 &&
                        fleet.Count(s => s.ShipType == 3) > 0)
                    {
                        Increment(q259.Count);
                    }
                }
            }
            if (_quests.TryGetValue(264, out var q264))
            {
                if (_map == 42 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec)
                        .ToArray();
                    if (fleet.Count(spec => spec.ShipType == 2) >= 2 &&
                        fleet.Count(spec => spec.IsAircraftCarrier) >= 2)
                        Increment(q264.Count);
                }
            }
            if (_quests.TryGetValue(266, out var q266))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 2 && fleet.OrderBy(x => x).SequenceEqual(new[] {2, 2, 2, 2, 3, 5}))
                        Increment(q266.Count);
                }
            }
            if (_quests.TryGetValue(280, out var q280))
            {
                if (!(_boss && QuestSortie.CompareRank(rank, "S") == 0))
                    return;
                var shipTypes = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                    .ToArray();
                if (!(shipTypes.Count(type => type == 1 || type == 2) >= 3 &&
                      shipTypes.Any(type => new[] {3, 4, 7, 21}.Contains(type))))
                    return;
                var count = q280.Count;
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
            }
            if (_quests.TryGetValue(854, out var opz) && _boss)
            {
                var count = opz.Count;
                switch (_map)
                {
                    case 24 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNth(count, 0);
                        break;
                    case 61 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNth(count, 1);
                        break;
                    case 63 when QuestSortie.CompareRank(rank, "A") <= 0:
                        IncrementNth(count, 2);
                        break;
                    case 64 when QuestSortie.CompareRank(rank, "S") <= 0:
                        IncrementNth(count, 3);
                        break;
                }
            }
            if (_quests.TryGetValue(862, out var q862))
            {
                if (_map == 63 && _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet.Count(s => s == 3) >= 2 && fleet.Count(s => s == 16) >= 1)
                        Increment(q862.Count);
                }
            }
            if (_quests.TryGetValue(873, out var q873))
            {
                if (_battleInfo.Result.Friend.Main.Count(s => s.NowHp > 0 && s.Spec.ShipType == 3) >= 1 &&
                    _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var count = q873.Count;
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
            }
            if (_quests.TryGetValue(875, out var q875))
            {
                if (_map == 54 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id).ToArray();
                    if (fleet.Contains(543) && fleet.Intersect(new[] {344, 345, 359}).Any())
                        Increment(q875.Count);
                }
            }
            if (_quests.TryGetValue(888, out var q888))
            {
                if (!_boss || QuestSortie.CompareRank(rank, "S") != 0)
                    return;
                var fleet = from ship in _battleInfo.Result.Friend.Main where ship.NowHp > 0 select ship.Spec.Id;
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
                if (fleet.Intersect(member).Count() < 4)
                    return;
                var count = q888.Count;
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
            }
            if (_quests.TryGetValue(893, out var q893))
            {
                if (!_boss || QuestSortie.CompareRank(rank, "S") != 0)
                    return;
                var count = q893.Count;
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
            }
            if (_quests.TryGetValue(894, out var q894))
            {
                if (!_boss ||
                    QuestSortie.CompareRank(rank, "S") != 0 ||
                    !_battleInfo.Result.Friend.Main.Any(s => s.Spec.IsAircraftCarrier && s.NowHp > 0))
                    return;
                var count = q894.Count;
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
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPractice practice))
                    continue;
                if (practice.Check(json.api_win_rank))
                    Increment(count);
            }
            if (_quests.TryGetValue(318, out var q318))
            {
                if (_questFleet == 0 && QuestSortie.CompareRank(json.api_win_rank, "B") <= 0 &&
                    _battleInfo.Result.Friend.Main.Count(s => s.Spec.ShipType == 3) >= 2)
                {
                    Increment(q318.Count);
                }
            }
            if (_quests.TryGetValue(330, out var q330))
            {
                var fleet = _battleInfo.Result.Friend.Main;
                if (QuestSortie.CompareRank(json.api_win_rank, "B") <= 0 &&
                    fleet.Count(s => s.Spec.IsAircraftCarrier) >= 2 &&
                    fleet.Count(s => s.Spec.ShipType == 2) >= 2 &&
                    fleet[0].Spec.IsAircraftCarrier)
                {
                    Increment(q330.Count);
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
                if (!(count.Spec is QuestMission mission))
                    continue;
                if (mission.Check(mid))
                    Increment(count);
            }
            if (_quests.TryGetValue(426, out var q426))
            {
                var count = q426.Count;
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
            }
            if (_quests.TryGetValue(428, out var q428))
            {
                var count = q428.Count;
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
                if (!(count.Spec is QuestDestroyItem destroy))
                    continue;
                if (destroy.Count(count, items))
                    NeedSave = true;
            }
            if (_quests.TryGetValue(680, out var q680))
            {
                q680.Count.NowArray[0] += items.Count(spec => spec.Type == 21);
                q680.Count.NowArray[1] += items.Count(spec => spec.Type == 12 || spec.Type == 13);
                NeedSave = true;
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