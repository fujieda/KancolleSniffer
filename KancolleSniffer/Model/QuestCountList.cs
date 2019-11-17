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

using System.Collections.Generic;
using System.Linq;

namespace KancolleSniffer.Model
{
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
        // @formatter:off
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
            {220, new QuestEnemyType {Interval = Weekly, Max = 20, EnemyType = new[] {7, 11}, Material = new[] {0, 0, 2, 0}}}, // 220: い号作戦
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
            {264, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {0, 0, 0, 2}}}, // 264: 「空母機動部隊」西へ！
            {265, new QuestSortie {Interval = Monthly, Max = 10, Rank = "A", Maps = new[] {15}, Material = new[] {0, 0, 5, 3}}}, // 265: 海上護衛強化月間
            {266, new QuestSpec {Interval = Monthly, Max = 1, Material = new[] {4, 4, 0, 2}}}, // 266: 「水上反撃部隊」突入せよ！
            {280, new QuestSpec {Interval = Monthly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 4, 4, 2}}}, // 280: 兵站線確保！海上警備を強化実施せよ！
            {284, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 0, 0, 4}}}, // 284: 南西諸島方面「海上警備行動」発令！

            {822, new QuestSortie {Interval = Quarterly, Max = 2, Rank = "S", Maps = new[] {24}, Material = new[] {0, 0, 0, 5}}}, // 822: 沖ノ島海域迎撃戦
            {854, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 0, 0, 4}}}, // 854: 戦果拡張任務！「Z作戦」前段作戦
            {861, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 4, 0, 0}}}, // 861: 強行輸送艦隊、抜錨！
            {862, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 0, 8, 4}}}, // 862: 前線の航空偵察を実施せよ！
            {872, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1}, Material = new[] {0, 0, 0, 4}}}, // 872: 戦果拡張任務！「Z作戦」後段作戦
            {873, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1}, Material = new[] {0, 0, 0, 0}}}, // 873: 北方海域警備を実施せよ！
            {875, new QuestSpec {Interval = Quarterly, Max = 2, Material = new[] {0, 0, 0, 0}}}, // 875: 精鋭「三一駆」、鉄底海域に突入せよ！
            {888, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1}, Material = new[] {0, 0, 0, 0}}}, // 888: 新編成「三川艦隊」、鉄底海峡に突入せよ！
            {893, new QuestSpec {Interval = Quarterly, MaxArray = new[] {3, 3, 3, 3}, Material = new[] {0, 0, 0, 0}}}, // 893: 泊地周辺海域の安全確保を徹底せよ！
            {894, new QuestSpec {Interval = Quarterly, MaxArray = new[] {1, 1, 1, 1, 1}, Material = new[] {0, 0, 0, 0}}}, // 894: 空母戦力の投入による兵站線戦闘哨戒

            {303, new QuestPractice {Interval = Daily, Max = 3, Win = false, Material = new[] {1, 0, 0, 0}}}, // 303: 「演習」で練度向上！
            {304, new QuestPractice {Interval = Daily, Max = 5, Win = true, Material = new[] {0, 0, 1, 0}}}, // 304: 「演習」で他提督を圧倒せよ！
            {302, new QuestPractice {Interval = Weekly, Max = 20, Win = true, Material = new[] {0, 0, 2, 1}}}, // 302: 大規模演習
            {311, new QuestPractice {Interval = Daily, Max = 7, Win = true, Material = new[] {0, 2, 0, 0}}}, // 311: 精鋭艦隊演習
            {315, new QuestPractice {Interval = Daily, Max = 8, Win = true, Material = new[] {0, 0, 0, 0}}}, // 315: 春季大演習
            {318, new QuestSpec {Interval = Daily, Max = 3, Material = new[] {0, 2, 2, 0}, AdjustCount = false}}, // 318: 給糧艦「伊良湖」の支援
            {330, new QuestSpec {Interval = Daily, Max = 4, Material = new[] {0, 0, 3, 0}}}, // 330: 空母機動部隊、演習始め！

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
            {638, new QuestDestroyItem {Interval = Weekly, Max = 6, Types = new[] {21}, Material = new[] {0, 0, 2, 1}}}, // 638: 対空機銃量産
            {643, new QuestDestroyItem {Interval = Quarterly, Max = 2, Ids = new[] {20}, Material = new[] {0, 0, 2, 0}, AdjustCount = false}}, // 643: 主力「陸攻」の調達
            {645, new QuestDestroyItem {Interval = Monthly, Max = 1, Ids = new[] {35}, Material = new[] {0, 0, 0, 0}, AdjustCount = false}}, // 645: 「洋上補給」物資の調達
            {663, new QuestDestroyItem {Interval = Quarterly, Max = 10, Types = new[] {3}, Material = new[] {0, 0, 3, 0}}}, // 663: 新型艤装の継続研究
            {673, new QuestDestroyItem {Interval = Daily, Max = 4, Types = new[] {1}, Shift = 1, Material = new[] {0, 0, 1, 0}}}, // 673: 装備開発力の整備
            {674, new QuestDestroyItem {Interval = Daily, Max = 3, Types = new[] {21}, Shift = 2, Material = new[] {0, 1, 1, 0}}}, // 674: 工廠環境の整備
            {675, new QuestDestroyItem {Interval = Quarterly, MaxArray = new[] {6, 4}, Types = new[] {6, 21}, Material = new[] {0, 0, 0, 0}}}, // 675: 運用装備の統合整備
            {676, new QuestDestroyItem {Interval = Weekly, MaxArray = new[] {3, 3, 1}, Types = new[] {2, 4, 30}, Material = new[] {0, 1, 7, 0}}}, // 676: 装備開発力の集中整備
            {677, new QuestDestroyItem {Interval = Weekly, MaxArray = new[] {4, 2, 3}, Types = new[] {3, 10, 5}, Material = new[] {0, 5, 0, 0}}}, // 677: 継戦支援能力の整備
            {678, new QuestDestroyItem {Interval = Quarterly, MaxArray = new[] {3, 5}, Ids = new[] {19, 20}, Material = new[] {0, 0, 8, 0}}}, // 678: 主力艦上戦闘機の更新
            {680, new QuestSpec {Interval = Quarterly, MaxArray = new[] {4, 4}, Material = new[] {0, 0, 6, 0}}}, // 680: 対空兵装の整備拡充
            {688, new QuestDestroyItem {Interval = Quarterly, MaxArray = new[] {3, 3, 3, 3}, Types = new[] {6, 7, 8, 10}, Material = new[] {0, 0, 0, 0}}}, // 688: 航空戦力の強化

            {702, new QuestPowerUp {Interval = Daily, Max = 2, Material = new[] {0, 1, 0, 0}}}, // 702: 艦の「近代化改修」を実施せよ！
            {703, new QuestPowerUp {Interval = Weekly, Max = 15, Material = new[] {1, 0, 2, 0}}} // 703: 「近代化改修」を進め、戦備を整えよ！
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

        public IEnumerable<QuestCount> NonZeroCountList => _countDict.Values.Where(c => c.Now > 0 || (c.NowArray?.Any(n => n > 0) ?? false));

        public void SetCountList(IEnumerable<QuestCount> questCountList)
        {
            if (questCountList == null)
                return;
            foreach (var count in questCountList)
            {
                count.Spec = QuestSpecs[count.Id];
                _countDict[count.Id] = count;
            }
        }
    }
}