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
            return Spec.MaxArray != null && Spec.MaxArray.All(x => x == 1)
                ? string.Join("\u200a", NowArray.Select(n => (n % 10).ToString()))
                : Spec.MaxArray != null
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

        private static string MapString(int map)
        {
            return map switch
            {
                721 => "7-2G",
                722 => "7-2M",
                _ => $"{map / 10}-{map % 10}"
            };
        }

        public string ToToolTip()
        {
            if (Spec is QuestSortie spec && spec.Maps != null)
            {
                var flags = spec.MaxArray.All(x => x == 1);
                return string.Join(" ",
                    spec.Maps.Zip(NowArray, (map, n) => n >= 1 ? $"{MapString(map)}{(flags ? "" : $":{n}")}" : "")
                    .Where(s => !string.IsNullOrEmpty(s)));
            }
            return Id switch
            {
                426 => string.Join(" ",
                    new[] {"警備任務", "対潜警戒任務", "海上護衛任務", "強硬偵察任務"}.Zip(NowArray, (mission, n) => n >= 1 ? mission : "")
                        .Where(s => !string.IsNullOrEmpty(s))),
                428 => string.Join(" ",
                    new[] {"対潜警戒任務", "海峡警備行動", "長時間対潜警戒"}.Zip(NowArray, (mission, n) => n >= 1 ? mission + n : "")
                        .Where(s => !string.IsNullOrEmpty(s))),
                688 => string.Join(" ",
                    new[] {"艦戦", "艦爆", "艦攻", "水偵"}.Zip(NowArray, (type, n) => n >= 1 ? type + n : "")
                        .Where(s => !string.IsNullOrEmpty(s))),
                _ => ""
            };
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
        private bool _boss;

        private class ResultShipSpecs
        {
            public ResultShipSpecs(BattleInfo battleInfo)
            {
                Specs = battleInfo.Result?.Friend.Main.Where(s => s.NowHp > 0).Select(ship => ship.Spec).ToArray() ??
                        new ShipSpec[0];
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
            if (_map == 72)
            {
                var cell = json.api_no() ? (int)json.api_no : 0;
                _map *= 10;
                if (cell == 7)
                    _map++;
                if (cell == 15)
                    _map += 2;
            }
            _boss = (int)json.api_event_id == 5;

            if (_quests.TryGetValue(861, out var q861) && _map == 16 && (int)json.api_event_id == 8)
            {
                if (new ResultShipSpecs(_battleInfo).Types.Count(s => s == 10 || s == 22) == 2)
                    Increment(q861.Count);
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            var rank = json.api_win_rank;
            foreach (var count in _quests.Values.Select(q => q.Count))
            {
                switch (count.Spec)
                {
                    case QuestSortie sortie:
                        if (!FleetCheck(count.Id))
                            continue;
                        if (!_boss && count.Id == 216)
                        {
                            Increment(count);
                            continue;
                        }
                        if (sortie.Count(count, rank, _map, _boss))
                            NeedSave = true;
                        continue;
                    case QuestEnemyType enemyType:
                        var num = enemyType.CountResult(
                            _battleInfo.Result.Enemy.Main.Concat(_battleInfo.Result.Enemy.Guard));
                        if (num > 0)
                            Add(count, num);
                        continue;
                }
                if (count.Id == 214)
                    CountAgo(count, rank);
            }
        }

        private void CountAgo(QuestCount count, string rank)
        {
            if (QuestSortie.CompareRank(rank, "S") == 0)
                IncrementNth(count, 1);
            if (!_boss)
                return;
            IncrementNth(count, 2);
            if (QuestSortie.CompareRank(rank, "B") <= 0)
                IncrementNth(count, 3);
        }

        private bool FleetCheck(int id)
        {
            var specs = new ResultShipSpecs(_battleInfo);
            switch (id)
            {
                case 249:
                    return specs.Ids.Intersect(new[] {62, 63, 64, 265, 266, 268, 319, 192, 194}).Count() == 3;
                case 257:
                    return specs.FlagshipType == 3 && specs.Types.Count(s => s == 3) <= 3 &&
                           specs.Types.All(s => s == 2 || s == 3);
                case 259:
                    return specs.Types.Count(type => type == 3) > 0 && specs.Classes.Count(c => new[]
                    {
                        2, // 伊勢型
                        19, // 長門型
                        26, // 扶桑型
                        37 // 大和型
                    }.Contains(c)) == 3;
                case 264:
                    return specs.Types.Count(type => type == 2) >= 2 &&
                           specs.Specs.Count(spec => spec.IsAircraftCarrier) >= 2;
                case 266:
                    return specs.FlagshipType == 2 &&
                           specs.Types.OrderBy(x => x).SequenceEqual(new[] {2, 2, 2, 2, 3, 5});
                case 280:
                case 284:
                    return specs.Types.Count(type => type == 1 || type == 2) >= 3 &&
                           specs.Types.Intersect(new[] {3, 4, 7, 21}).Any();
                case 862:
                    return specs.Types.Count(s => s == 3) >= 2 && specs.Types.Count(s => s == 16) >= 1;
                case 873:
                    return specs.Types.Count(type => type == 3) >= 1;
                case 875:
                    return specs.Ids.Contains(543) &&
                           specs.Ids.Intersect(new[]
                           {
                               344, // 朝霜改
                               345, // 高波改
                               359, // 沖波改
                               578 // 朝霜改二
                           }).Any();
                case 888:
                    return specs.Ids.Intersect(new[]
                    {
                        69, 272, 427, // 鳥海
                        61, 264, // 青葉
                        123, 295, 142, // 衣笠
                        59, 262, 416, // 古鷹
                        60, 263, 417, // 加古
                        51, 213, 477, // 天龍
                        115, 293 // 夕張
                    }).Count() >= 4;
                case 894:
                    return specs.Specs.Any(spec => spec.IsAircraftCarrier);
                case 318:
                    return specs.Types.Count(type => type == 3) >= 2;
                case 330:
                    return specs.Flagship.IsAircraftCarrier &&
                           specs.Specs.Count(spec => spec.IsAircraftCarrier) >= 2 &&
                           specs.Types.Count(type => type == 2) >= 2;
                case 337:
                    return specs.Ids.Intersect(new[]
                    {
                        17, 225, 566, // 陽炎
                        18, 226, 567, // 不知火
                        48, 198, 252, // 霰
                        49, 253, 464 // 霞
                    }).Count() == 4;
                default:
                    return true;
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
            foreach (var count in _quests.Values.Select(q => q.Count))
            {
                if (!FleetCheck(count.Id))
                    continue;
                if (count.Id == 318 && _questFleet != 0)
                    continue;
                if (!(count.Spec is QuestPractice practice))
                    continue;
                if (practice.Check(json.api_win_rank))
                    Increment(count);
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
            foreach (var count in _quests.Values.Select(q => q.Count))
            {
                if (!(count.Spec is QuestMission mission))
                    continue;
                if (mission.Count(count, mid))
                    NeedSave = true;
            }
        }

        public void CountNyukyo() => Increment(503);

        public void CountCharge() => Increment(504);

        public void InspectCreateItem(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var count = values["api_multiple_flag"] == "1" ? 3 : 1;
            Add(605, count);
            Add(607, count);
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