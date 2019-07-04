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

namespace KancolleSniffer.Model
{
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
        public int[] Types { get; set; }
        public int[] Ids { get; set; }

        public bool Count(QuestCount count, ItemSpec[] specs)
        {
            if (count.NowArray == null)
            {
                var num = specs.Count(spec => Types?.Contains(spec.Type) ?? (Ids?.Contains(spec.Id) ?? true));
                count.Now += num;
                return num > 0;
            }
            if (Types == null && Ids == null)
                return false;
            var result = false;
            for (var i = 0; i < count.NowArray.Length; i++)
            {
                var num = specs.Count(spec => Types != null ? Types[i] == spec.Type : Ids[i] == spec.Id);
                count.NowArray[i] += num;
                if (num > 0)
                    result = true;
            }
            return result;
        }
    }

    public class QuestPowerUp : QuestSpec
    {
    }
}