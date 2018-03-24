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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Xml.Serialization;
using static System.Math;

namespace KancolleSniffer
{
    public class QuestStatus
    {
        public int Id { get; set; }
        public int Category { get; set; }
        public string Name { get; set; }
        public string Detail { get; set; }
        public int[] Material { get; set; }
        public int Progress { get; set; }

        [XmlIgnore]
        public QuestCount Count { get; set; }

        [XmlIgnore]
        public Color Color { get; set; }

        public string ToToolTip() =>
            Detail +
            (Material == null || Material.All(x => x == 0)
                ? ""
                : "\r\n" + string.Join(" ",
                      new[] {"燃", "弾", "鋼", "ボ", "建造", "修復", "開発", "改修"}
                          .Zip(Material, (m, num) => num == 0 ? "" : m + num)
                          .Where(s => !string.IsNullOrEmpty(s))));
    }

    public enum QuestInterval
    {
        Other,
        Daily,
        Weekly,
        Monthly,
        Quarterly
    }

    public class QuestSpec
    {
        public QuestInterval Interval { get; set; }
        public int Max { get; set; }
        public int[] MaxArray { get; set; }
        public bool AdjustCount { get; set; } = true;
        public int Shift { get; set; }
        public int[] Material { get; set; }
    }

    public class QuestSortie : QuestSpec
    {
        public string Rank { get; set; }
        public int[] Maps { get; set; }
        public int[] ShipTypes { get; set; }

        public static int CompareRank(string a, string b)
        {
            const string ranks = "SABCDE";
            return ranks.IndexOf(a, StringComparison.Ordinal) -
                   ranks.IndexOf(b, StringComparison.Ordinal);
        }

        public bool Check(string rank, int map, bool boss)
        {
            return (Rank == null || CompareRank(rank, Rank) <= 0) &&
                   (Maps == null || Maps.Contains(map) && boss);
        }
    }

    public class QuestEnemyType : QuestSpec
    {
        public int[] EnemyType { get; set; } = new int[0];

        public int CountResult(IEnumerable<ShipStatus> enemyResult) =>
            enemyResult.Count(ship => ship.NowHp == 0 && EnemyType.Contains(ship.Spec.ShipType));
    }

    public class QuestPractice : QuestSpec
    {
        public bool Win { get; set; }
        public bool Check(string rank) => !Win || QuestSortie.CompareRank(rank, "B") <= 0;
    }

    public class QuestMission : QuestSpec
    {
        public int[] Ids { get; set; }
        public bool Check(int id) => Ids == null || Ids.Contains(id);
    }

    public class QuestDestroyItem : QuestSpec
    {
        public int[] Items { get; set; }
        public bool Check(int id) => Items == null || Items.Contains(id);
    }

    public class QuestPowerup : QuestSpec
    {
    }

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
                NowArray = NowArray.Zip(Spec.MaxArray, Max).ToArray();
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
            var low = (int)Ceiling(max * progress / 100.0);
            if (low >= max && progress != 100)
                low = max - 1;
            var high = (int)Ceiling(max * next / 100.0);
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
            if (Id == 426 || Id == 854 || Id == 873)
                return $"{NowArray.Count(n => n >= 1)}/{Spec.MaxArray.Length}";
            return NowArray != null
                ? string.Join(" ", NowArray.Zip(Spec.MaxArray, (n, m) => $"{n}/{m}"))
                : $"{Now}/{Spec.Max}";
        }

        public string ToToolTip()
        {
            switch (Id)
            {
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
            }
            return "";
        }

        public bool Cleared => NowArray?.Zip(Spec.MaxArray, (n, m) => n >= m).All(x => x) ?? Now >= Spec.Max;
    }

    // @formatter:off
    public class QuestCountList
    {
        private const QuestInterval Daily = QuestInterval.Daily;
        private const QuestInterval Weekly = QuestInterval.Weekly;
        private const QuestInterval Monthly = QuestInterval.Monthly;
        private const QuestInterval Quarterly = QuestInterval.Quarterly;

        /// <summary>
        /// このテーブルは七四式電子観測儀を参考に作成した。
        /// https://github.com/andanteyk/ElectronicObserver/blob/develop/ElectronicObserver/Data/Quest/QuestProgressManager.cs
        /// </summary>
        private static readonly Dictionary<int, QuestSpec> QuestSpecs = new Dictionary<int, QuestSpec>
        {
            {201, new QuestSortie {Interval = Daily, Max = 1, Rank = "B", Material = new[] {0, 0, 1, 0}}}, // 201: 敵艦隊を撃滅せよ！
            {216, new QuestSortie {Interval = Daily, Max = 1, Rank = "B", Material = new[] {0, 1, 1, 0}}}, // 216: 敵艦隊主力を撃滅せよ！
            {210, new QuestSortie {Interval = Daily, Max = 10, Material = new[] {0, 0, 1, 0}}}, // 210: 敵艦隊を10回邀撃せよ！
            {211, new QuestEnemyType {Interval = Daily, Max = 3, EnemyType = new[] {7, 11}, Material = new[] {0, 2, 0, 0}}}, // 211: 敵空母を3隻撃沈せよ！
            {212, new QuestEnemyType {Interval = Daily, Max = 5, EnemyType = new[] {15}, Material = new[] {0, 0, 2, 0}}}, // 212: 敵輸送船団を叩け！
            {218, new QuestEnemyType {Interval = Daily, Max = 3, EnemyType = new[] {15}, Material = new[] {0, 1, 1, 0}}}, // 218: 敵補給艦を3隻撃沈せよ！
            {226, new QuestSortie {Interval = Daily, Max = 5, Rank = "B", Maps = new[] {21, 22, 23, 24, 25}, Material = new[] {1, 1, 0, 0}}}, // 226: 南西諸島海域の制海権を握れ！
            {230, new QuestEnemyType {Interval = Daily, Max = 6, EnemyType = new[] {13}, Material = new[] {0, 1, 0, 0}}}, // 230: 敵潜水艦を制圧せよ！

            {213, new QuestEnemyType {Interval = Weekly, Max = 20, EnemyType = new[] {15}, Material = new[] {0, 0, 3, 0}}}, // 213: 海上通商破壊作戦
            {214, new QuestSpec {Interval = Weekly, MaxArray = new[] {36, 6, 24, 12}, Material = new[] {2, 0, 2, 0}}}, // 214: あ号作戦
            {220, new QuestEnemyType {Interval = Weekly, Max = 20, EnemyType = new[] {7, 11}, Material = new[] {0, 2, 0, 0}}}, // 220: い号作戦
            {221, new QuestEnemyType {Interval = Weekly, Max = 50, EnemyType = new[] {15}, Material = new[] {0, 3, 0, 0}}}, // 221: ろ号作戦
            {228, new QuestEnemyType {Interval = Weekly, Max = 15, EnemyType = new[] {13}, Material = new[] {0, 2, 0, 1}}}, // 228: 海上護衛戦
            {229, new QuestSortie {Interval = Weekly, Max = 12, Rank = "B", Maps = new[] {41, 42, 43, 44, 45}, Material = new[] {0, 0, 2, 0}}}, // 229: 敵東方艦隊を撃滅せよ！
            {241, new QuestSortie {Interval = Weekly, Max = 5, Rank = "B", Maps = new[] {33, 34, 35}, Material = new[] {0, 0, 3, 3}}}, // 241: 敵北方艦隊主力を撃滅せよ！
            {242, new QuestSortie {Interval = Weekly, Max = 1, Rank = "B", Maps = new[] {44}, Material = new[] {0, 1, 1, 0}}}, // 242: 敵東方中枢艦隊を撃破せよ！
            {243, new QuestSortie {Interval = Weekly, Max = 2, Rank = "S", Maps = new[] {52}, Material = new[] {0, 0, 2, 2}}}, // 243: 南方海域珊瑚諸島沖の制空権を握れ！
            {249, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {0, 0, 5, 0}}}, // 249: 「第五戦隊」出撃せよ！
            {256, new QuestSortie {Interval = Monthly, Max = 3, Rank = "S", Maps = new[] {61}, Material = new[] {0, 0, 0, 0}}}, // 256: 「潜水艦隊」出撃せよ！
            {257, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {0, 0, 0, 3}}}, // 257: 「水雷戦隊」南西へ！
            {259, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {0, 3, 0, 4}}}, // 259: 「水上打撃部隊」南方へ！
            {261, new QuestSortie {Interval = Weekly, Max = 3, Rank = "A", Maps = new[] {15}, Material = new[] {0, 0, 0, 3}}}, // 261: 海上輸送路の安全確保に努めよ！
            {265, new QuestSortie {Interval = Monthly, Max = 10, Rank = "A", Maps = new[] {15}, Material = new[] {0, 0, 5, 3}}}, // 265: 海上護衛強化月間
            {266, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {0, 0, 4, 2}}}, // 266: 「水上反撃部隊」突入せよ！

            {822, new QuestSortie {Interval = Quarterly, Max = 2, Rank = "S", Maps = new[] {24}, Material = new[] {0, 0, 0, 5}}}, // 822: 沖ノ島海域迎撃戦
            {854, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 0, 0, 4}}}, // 854: 戦果拡張任務！「Z作戦」前段作戦
            {861, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 4, 0, 0}}}, // 861: 強行輸送艦隊、抜錨！
            {862, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 0, 8, 4}}}, // 862: 前線の航空偵察を実施せよ！
            {873, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1}, Material = new[] {0, 0, 0, 0}}}, // 873: 北方海域警備を実施せよ！
            {875, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 0, 0, 0}}}, // 875: 精鋭「三一駆」、鉄底海域に突入せよ！

            {303, new QuestPractice {Interval = Daily, Max = 3, Win = false, Material = new[] {1, 0, 0, 0}}}, // 303: 「演習」で練度向上！
            {304, new QuestPractice {Interval = Daily, Max = 5, Win = true, Material = new[] {0, 0, 1, 0}}}, // 304: 「演習」で他提督を圧倒せよ！
            {302, new QuestPractice {Interval = Weekly, Max = 20, Win = true, Material = new[] {0, 0, 2, 1}}}, // 302: 大規模演習
            {311, new QuestPractice {Interval = Daily, Max = 7, Win = true, Material = new[] {0, 2, 0, 0}}}, // 311: 精鋭艦隊演習
            {318, new QuestSpec {Interval = Daily, Max = 3, Material = new[] {0, 2, 2, 0}, AdjustCount = false}}, // 318: 給糧艦「伊良湖」の支援

            {402, new QuestMission {Interval = Daily, Max = 3, Material = new[] {0, 0, 1, 0}}}, // 402: 「遠征」を3回成功させよう！
            {403, new QuestMission {Interval = Daily, Max = 10, Material = new[] {0, 0, 0, 0}}}, // 403: 「遠征」を10回成功させよう！
            {404, new QuestMission {Interval = Weekly, Max = 30, Material = new[] {0, 0, 3, 0}}}, // 404: 大規模遠征作戦、発令！
            {410, new QuestMission {Interval = Weekly, Max = 1, Ids = new[] {37, 38}, Material = new[] {0, 0, 0, 0}}}, // 410: 南方への輸送作戦を成功させよ！
            {411, new QuestMission {Interval = Weekly, Max = 6, Shift = 1, Ids = new[] {37, 38}, Material = new[] {0, 0, 2, 1}}}, // 411: 南方への鼠輸送を継続実施せよ！
            {424, new QuestMission {Interval = Monthly, Max = 4, Shift = 1, Ids = new[] {5}, Material = new[] {0, 0, 0, 0}}}, // 424: 輸送船団護衛を強化せよ！
            {426, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 0, 4, 0}}}, // 426: 海上通商航路の警戒を厳とせよ！
            {428, new QuestSpec {Interval = Quarterly, MaxArray = new[] {2, 2, 2}, Material = new[] {0, 0, 0, 3}}}, // 428: 近海に侵入する敵潜を制圧せよ！

            {503, new QuestSpec {Interval = Daily, Max = 5, Material = new[] {0, 2, 0, 0}}}, // 503: 艦隊大整備！
            {504, new QuestSpec {Interval = Daily, Max = 15, Material = new[] {1, 0, 1, 0}}}, // 504: 艦隊酒保祭り！

            {605, new QuestSpec {Interval = Daily, Max = 1, Material = new[] {1, 0, 1, 0}}}, // 605: 新装備「開発」指令
            {606, new QuestSpec {Interval = Daily, Max = 1, Material = new[] {0, 1, 1, 0}}}, // 606: 新造艦「建造」指令
            {607, new QuestSpec {Interval = Daily, Max = 3, Shift = 1, Material = new[] {0, 0, 2, 0}}}, // 607: 装備「開発」集中強化！
            {608, new QuestSpec {Interval = Daily, Max = 3, Shift = 1, Material = new[] {1, 0, 2, 0}}}, // 608: 艦娘「建造」艦隊強化！
            {609, new QuestSpec {Interval = Daily, Max = 2, Material = new[] {0, 1, 0, 0}}}, // 609: 軍縮条約対応！
            {619, new QuestSpec {Interval = Daily, Max = 1, Material = new[] {0, 0, 0, 1}}}, // 619: 装備の改修強化

            {613, new QuestSpec {Interval = Weekly, Max = 24, Material = new[] {0, 0, 0, 0}}}, // 613: 資源の再利用
            {638, new QuestDestroyItem {Interval = Weekly, Max = 6, Items = new[] {21}, Material = new[] {0, 0, 2, 1}}}, // 638: 対空機銃量産
            {663, new QuestDestroyItem {Interval = Quarterly, Max = 10, Items = new[] {3}, Material = new[] {0, 0, 3, 0}}}, // 663: 新型艤装の継続研究
            {673, new QuestDestroyItem {Interval = Daily, Max = 4, Items = new[] {1}, Shift = 1, Material = new[] {0, 0, 1, 0}}}, // 673: 装備開発力の整備
            {674, new QuestDestroyItem {Interval = Daily, Max = 3, Items = new[] {21}, Shift = 2, Material = new[] {0, 1, 1, 0}}}, // 674: 工廠環境の整備
            {675, new QuestSpec {Interval = Quarterly, MaxArray = new[] {6, 4}, Material = new[] {0, 0, 0, 0}}}, // 675: 運用装備の統合整備
            {676, new QuestSpec {Interval = Weekly, MaxArray = new[] {3, 3, 1}, Material = new[] {0, 1, 7, 0}}}, // 676: 装備開発力の集中整備
            {677, new QuestSpec {Interval = Weekly, MaxArray = new[] {4, 2, 3}, Material = new[] {0, 5, 0, 0}}}, // 677: 継戦支援能力の整備
            {678, new QuestSpec {Interval = Quarterly, MaxArray = new[] {3, 5}, Material = new[] {0, 0, 8, 0}}}, // 678: 主力艦上戦闘機の更新
            {680, new QuestSpec {Interval = Quarterly, MaxArray = new[] {4, 4}, Material = new[] {0, 0, 6, 0}}}, // 680: 対空兵装の整備拡充

            {702, new QuestPowerup {Interval = Daily, Max = 2, Material = new[] {0, 1, 0, 0}}}, // 702: 艦の「近代化改修」を実施せよ！
            {703, new QuestPowerup {Interval = Weekly, Max = 15, Material = new[] {1, 0, 2, 0}}} // 703: 「近代化改修」を進め、戦備を整えよ！
        };
        // @formatter:on

        private readonly Dictionary<int, QuestCount> _countDict = new Dictionary<int, QuestCount>();

        public QuestCount GetCount(int id)
        {
            if (_countDict.TryGetValue(id, out var value))
                return value;
            if (QuestSpecs.TryGetValue(id, out var spec))
            {
                var nowArray = spec.MaxArray?.Select(x => 0).ToArray();
                return _countDict[id] = new QuestCount
                {
                    Id = id,
                    Now = 0,
                    NowArray = nowArray,
                    Spec = spec
                };
            }
            return new QuestCount {Spec = new QuestSpec {Material = new int[0], AdjustCount = false}};
        }

        public void Remove(int id)
        {
            _countDict.Remove(id);
        }

        public void Remove(QuestInterval interval)
        {
            foreach (var id in
                _countDict.Where(pair => pair.Value.Spec.Interval == interval).Select(pair => pair.Key).ToArray())
            {
                _countDict.Remove(id);
            }
        }

        public IEnumerable<QuestCount> CountList
        {
            get => _countDict.Values.Where(c => c.Now > 0 || (c.NowArray?.Any(n => n > 0) ?? false));
            set
            {
                if (value == null)
                    return;
                foreach (var count in value)
                {
                    count.Spec = QuestSpecs[count.Id];
                    _countDict[count.Id] = count;
                }
            }
        }
    }

    public class QuestInfo : IHaveState
    {
        private readonly SortedDictionary<int, QuestStatus> _quests = new SortedDictionary<int, QuestStatus>();
        private readonly QuestCountList _countList = new QuestCountList();
        private readonly ItemInfo _itemInfo;
        private readonly BattleInfo _battleInfo;
        private readonly Func<DateTime> _nowFunc = () => DateTime.Now;
        private DateTime _lastReset;

        private readonly Color[] _color =
        {
            Color.FromArgb(60, 141, 76), Color.FromArgb(232, 57, 41), Color.FromArgb(136, 204, 120),
            Color.FromArgb(52, 147, 185), Color.FromArgb(220, 198, 126), Color.FromArgb(168, 111, 76),
            Color.FromArgb(200, 148, 231), Color.FromArgb(232, 57, 41)
        };

        public int AcceptMax { get; set; } = 5;

        public QuestStatus[] Quests => _quests.Values.ToArray();

        public QuestInfo(ItemInfo itemInfo, BattleInfo battleInfo, Func<DateTime> nowFunc = null)
        {
            _itemInfo = itemInfo;
            _battleInfo = battleInfo;
            if (nowFunc != null)
                _nowFunc = nowFunc;
        }

        public void InspectQuestList(dynamic json)
        {
            ResetQuests();
            if (json.api_list == null)
                return;
            for (var i = 0; i < 2; i++)
            {
                foreach (var entry in json.api_list)
                {
                    if (entry is double) // -1の場合がある。
                        continue;

                    var id = (int)entry.api_no;
                    var state = (int)entry.api_state;
                    var progress = (int)entry.api_progress_flag;
                    var cat = (int)entry.api_category;
                    var name = (string)entry.api_title;
                    var detail = ((string)entry.api_detail).Replace("<br>", "\r\n");
                    var material = (int[])entry.api_get_material;

                    switch (progress)
                    {
                        case 0:
                            break;
                        case 1:
                            progress = 50;
                            break;
                        case 2:
                            progress = 80;
                            break;
                    }
                    switch (state)
                    {
                        case 1:
                            if (_quests.Remove(id))
                                NeedSave = true;
                            break;
                        case 3:
                            progress = 100;
                            goto case 2;
                        case 2:
                            AddQuest(id, cat, name, detail, material, progress, true);
                            break;
                    }
                }
                if (_quests.Count <= AcceptMax)
                    break;
                /*
                 * ほかのPCで任務を達成した場合、任務が消えずに受領した任務の数が_questCountを超えることがある。
                 * その場合はいったん任務をクリアして、現在のページの任務だけを登録し直す。
                 */
                _quests.Clear();
            }
        }

        private void AddQuest(int id, int category, string name, string detail, int[] material, int progress,
            bool adjustCount)
        {
            var count = _countList.GetCount(id);
            if (adjustCount)
            {
                if (count.AdjustCount(progress))
                    NeedSave = true;
            }
            _quests[id] = new QuestStatus
            {
                Id = id,
                Category = category,
                Name = name,
                Detail = detail,
                Material = adjustCount ? material?.Concat(count.Spec.Material).ToArray() : material,
                Count = count,
                Progress = progress,
                Color = category <= _color.Length ? _color[category - 1] : Control.DefaultBackColor
            };
        }

        public void ClearQuests()
        {
            _quests.Clear();
        }

        private void ResetQuests()
        {
            var now = _nowFunc();
            var daily = now.Date.AddHours(5);
            if (!(_lastReset < daily && daily <= now))
                return;
            _quests.Clear(); // 前日に未消化のデイリーを消す。
            _countList.Remove(QuestInterval.Daily);
            var weekly = now.Date.AddDays(-((6 + (int)now.DayOfWeek) % 7)).AddHours(5);
            if (_lastReset < weekly && weekly <= now)
                _countList.Remove(QuestInterval.Weekly);
            var monthly = new DateTime(now.Year, now.Month, 1, 5, 0, 0);
            if (_lastReset < monthly && monthly <= now)
                _countList.Remove(QuestInterval.Monthly);
            var season = now.Month / 3;
            var quarterly = new DateTime(now.Year - (season == 0 ? 1 : 0), (season == 0 ? 12 : season * 3), 1, 5, 0, 0);
            if (_lastReset < quarterly && quarterly <= now)
                _countList.Remove(QuestInterval.Quarterly);
            _lastReset = now;
            NeedSave = true;
        }


        private int _map;
        private bool _boss;

        public void InspectMapStart(dynamic json)
        {
            if (_quests.TryGetValue(214, out var ago)) // あ号
                ago.Count.NowArray[0]++;
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _map = (int)json.api_maparea_id * 10 + (int)json.api_mapinfo_no;
            _boss = (int)json.api_event_id == 5;

            if (_quests.TryGetValue(861, out var q861))
            {
                if (_map == 16 && (int)json.api_event_id == 8)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet.Count(s => s == 10 || s == 22) == 2)
                        IncrementCount(q861.Count);
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
                            IncrementCount(count);
                        break;
                    case QuestEnemyType enemyType:
                        var num = enemyType.CountResult(
                            _battleInfo.Result.Enemy.Main.Concat(_battleInfo.Result.Enemy.Guard));
                        if (num > 0)
                            AddCount(count, num);
                        break;
                }
            }
            if (_quests.TryGetValue(214, out var ago))
            {
                var array = ago.Count.NowArray;
                if (_boss)
                {
                    array[2]++;
                    if (QuestSortie.CompareRank(rank, "B") <= 0)
                        array[3]++;
                    NeedSave = true;
                }
                if (rank == "S")
                {
                    array[1]++;
                    NeedSave = true;
                }
            }
            if (_quests.TryGetValue(249, out var q249))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id)
                        .ToArray();
                    if (fleet.Intersect(new[] {62, 63, 64, 265, 266, 268, 319, 192, 194}).Count() == 3)
                        IncrementCount(q249.Count);
                }
            }
            if (_quests.TryGetValue(257, out var q257))
            {
                if (_map == 14 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 3 && fleet.Count(s => s == 3) <= 3 && fleet.All(s => s == 2 || s == 3))
                        IncrementCount(q257.Count);
                }
            }
            if (_quests.TryGetValue(259, out var q259))
            {
                if (_map == 51 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec).ToArray();
                    var senkan = new[]
                    {
                        131, 136, // 大和
                        143, 148, 546, // 武蔵
                        80, 275, 541, // 長門
                        81, 276, // 陸奥
                        26, 286, 411, // 扶桑
                        27, 287, 412, // 山城
                        77, 82, // 伊勢
                        87, 88 // 日向
                    };
                    if (fleet.Select(s => s.Id).Intersect(senkan).Count() == 3 &&
                        fleet.Count(s => s.ShipType == 3) > 0)
                    {
                        IncrementCount(q259.Count);
                    }
                }
            }
            if (_quests.TryGetValue(266, out var q266))
            {
                if (_map == 25 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType)
                        .ToArray();
                    if (fleet[0] == 2 && fleet.OrderBy(x => x).SequenceEqual(new[] {2, 2, 2, 2, 3, 5}))
                        IncrementCount(q266.Count);
                }
            }
            if (_quests.TryGetValue(854, out var opz) && _boss)
            {
                var array = opz.Count.NowArray;
                switch (_map)
                {
                    case 24 when QuestSortie.CompareRank(rank, "A") <= 0:
                        array[0]++;
                        NeedSave = true;
                        break;
                    case 61 when QuestSortie.CompareRank(rank, "A") <= 0:
                        array[1]++;
                        NeedSave = true;
                        break;
                    case 63 when QuestSortie.CompareRank(rank, "A") <= 0:
                        array[2]++;
                        NeedSave = true;
                        break;
                    case 64 when QuestSortie.CompareRank(rank, "S") <= 0:
                        array[3]++;
                        NeedSave = true;
                        break;
                }
            }
            if (_quests.TryGetValue(862, out var q862))
            {
                if (_map == 63 && _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.ShipType).ToArray();
                    if (fleet.Count(s => s == 3) == 2 && fleet.Count(s => s == 16) == 1)
                        IncrementCount(q862.Count);
                }
            }
            if (_quests.TryGetValue(873, out var q873))
            {
                if (_battleInfo.Result.Friend.Main.Count(s => s.NowHp > 0 && s.Spec.ShipType == 3) >= 1 &&
                    _boss && QuestSortie.CompareRank(rank, "A") <= 0)
                {
                    var array = q873.Count.NowArray;
                    switch (_map)
                    {
                        case 31:
                            array[0]++;
                            NeedSave = true;
                            break;
                        case 32:
                            array[1]++;
                            NeedSave = true;
                            break;
                        case 33:
                            array[2]++;
                            NeedSave = true;
                            break;
                    }
                }
            }
            if (_quests.TryGetValue(875, out var q875))
            {
                if (_map == 54 && _boss && QuestSortie.CompareRank(rank, "S") == 0)
                {
                    var fleet = _battleInfo.Result.Friend.Main.Where(s => s.NowHp > 0).Select(s => s.Spec.Id).ToArray();
                    if (fleet.Contains(543) && fleet.Intersect(new []{344, 345, 359}).Any())
                        IncrementCount(q875.Count);
                }
            }
        }

        public void InspectPracticeResult(dynamic json)
        {
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPractice practice))
                    continue;
                if (practice.Check(json.api_win_rank))
                    IncrementCount(count);
            }
            if (_quests.TryGetValue(318, out var q318))
            {
                if (QuestSortie.CompareRank(json.api_win_rank, "B") <= 0 &&
                    _battleInfo.Result.Friend.Main.Count(s => s.Spec.ShipType == 3) >= 2)
                {
                    IncrementCount(q318.Count);
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
                    IncrementCount(count);
            }
            if (_quests.TryGetValue(426, out var q426))
            {
                var count = q426.Count;
                switch (mid)
                {
                    case 3:
                        count.NowArray[0]++;
                        break;
                    case 4:
                        count.NowArray[1]++;
                        break;
                    case 5:
                        count.NowArray[2]++;
                        break;
                    case 10:
                        count.NowArray[3]++;
                        break;
                }
            }
            if (_quests.TryGetValue(428, out var q428))
            {
                var count = q428.Count;
                switch (mid)
                {
                    case 4:
                        count.NowArray[0]++;
                        break;
                    case 101:
                        count.NowArray[1]++;
                        break;
                    case 102:
                        count.NowArray[2]++;
                        break;
                }
            }
        }

        private void IncrementCount(QuestCount count)
        {
            count.Now++;
            NeedSave = true;
        }

        private void AddCount(QuestCount count, int value)
        {
            count.Now += value;
            NeedSave = true;
        }

        private void IncrementCount(int id)
        {
            AddCount(id, 1);
        }

        private void AddCount(int id, int value)
        {
            if (_quests.TryGetValue(id, out var quest))
            {
                quest.Count.Now += value;
                NeedSave = true;
            }
        }

        public void CountNyukyo() => IncrementCount(503);

        public void CountCharge() => IncrementCount(504);

        public void CountCreateItem()
        {
            IncrementCount(605);
            IncrementCount(607);
        }

        public void CountCreateShip()
        {
            IncrementCount(606);
            IncrementCount(608);
        }

        public void InspectDestroyShip(string request)
        {
            AddCount(609, HttpUtility.ParseQueryString(request)["api_ship_id"].Split(',').Length);
        }

        public void CountRemodelSlot() => IncrementCount(619);

        public void InspectDestroyItem(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var items = values["api_slotitem_ids"].Split(',')
                .Select(id => _itemInfo.GetStatus(int.Parse(id)).Spec).ToArray();
            IncrementCount(613); // 613: 資源の再利用
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestDestroyItem destroy))
                    continue;
                AddCount(count, items.Count(spec => destroy.Check(spec.Type)));
            }
            if (_quests.TryGetValue(675, out var q675))
            {
                q675.Count.NowArray[0] += items.Count(spec => spec.Type == 6);
                q675.Count.NowArray[1] += items.Count(spec => spec.Type == 21);
                NeedSave = true;
            }
            if (_quests.TryGetValue(676, out var q676))
            {
                q676.Count.NowArray[0] += items.Count(spec => spec.Type == 2);
                q676.Count.NowArray[1] += items.Count(spec => spec.Type == 4);
                q676.Count.NowArray[2] += items.Count(spec => spec.Type == 30);
                NeedSave = true;
            }
            if (_quests.TryGetValue(677, out var q677))
            {
                q677.Count.NowArray[0] += items.Count(spec => spec.Type == 3);
                q677.Count.NowArray[1] += items.Count(spec => spec.Type == 10);
                q677.Count.NowArray[2] += items.Count(spec => spec.Type == 5);
                NeedSave = true;
            }
            if (_quests.TryGetValue(678, out var q678))
            {
                q678.Count.NowArray[0] += items.Count(spec => spec.Id == 19);
                q678.Count.NowArray[1] += items.Count(spec => spec.Id == 20);
                NeedSave = true;
            }
            if (_quests.TryGetValue(680, out var q680))
            {
                q680.Count.NowArray[0] += items.Count(spec => spec.Type == 21);
                q680.Count.NowArray[1] += items.Count(spec => spec.Type == 12 || spec.Type == 13);
                NeedSave = true;
            }
        }

        public void InspectPowerup(dynamic json)
        {
            if ((int)json.api_powerup_flag == 0)
                return;
            foreach (var quest in _quests.Values)
            {
                var count = quest.Count;
                if (!(count.Spec is QuestPowerup))
                    continue;
                IncrementCount(count);
            }
        }

        public void InspectStop(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _quests.Remove(int.Parse(values["api_quest_id"]));
            NeedSave = true;
        }

        public void InspectClearItemGet(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var id = int.Parse(values["api_quest_id"]);
            _countList.Remove(id);
            _quests.Remove(id);
            NeedSave = true;
        }

        public bool NeedSave { get; private set; }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.QuestLastReset = _lastReset;
            if (_quests != null)
                status.QuestList = _quests.Values.ToArray();
            if (_countList != null)
                status.QuestCountList = _countList.CountList.ToArray();
        }

        public void LoadState(Status status)
        {
            _lastReset = status.QuestLastReset;
            if (status.QuestCountList != null)
                _countList.CountList = status.QuestCountList;
            if (status.QuestList != null)
            {
                _quests.Clear();
                foreach (var q in status.QuestList)
                    AddQuest(q.Id, q.Category, q.Name, q.Detail, q.Material, q.Progress, false);
            }
        }
    }
}