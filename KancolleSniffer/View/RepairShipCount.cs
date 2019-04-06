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

using System.Collections.Generic;
using System.Linq;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class RepairShipCount
    {
        private const int Minor = (int)ShipStatus.Damage.Minor;
        private const int Small = (int)ShipStatus.Damage.Small;
        private const int Half = (int)ShipStatus.Damage.Half;
        private const int Badly = (int)ShipStatus.Damage.Badly;

        private readonly int[] _counts = new int[typeof(ShipStatus.Damage).GetEnumValues().Length];
        private readonly List<string> _result = new List<string>();

        public RepairShipCount(ShipStatus[] repairList)
        {
            foreach (var ship in repairList)
                _counts[(int)ship.DamageLevel]++;
        }

        public override string ToString()
        {
            if (_counts.All(n => n == 0))
                return "なし";
            CountTwoLevels(Minor);
            CountTwoLevels(Half);
            return string.Join("\r\n", _result);
        }

        private void CountTwoLevels(int level)
        {
            FormatCount(level);
            FormatCount(level + 1);
            if (EitherNonZero(_counts[level], _counts[level + 1]) && ShowSum)
                _result.Add($"　計 {_counts[level] + _counts[level + 1]}");
        }

        private readonly string[] _damageString = {"軽微", "小破", "中破", "大破"};

        private void FormatCount(int level)
        {
            if (_counts[level] > 0)
                _result.Add($"{_damageString[level]} {_counts[level]}");
        }

        private bool ShowSum =>
            BothNonZero(_counts[Minor], _counts[Small]) || BothNonZero(_counts[Half], _counts[Badly]);

        private static bool BothNonZero(int a, int b) => a * b > 0;

        private static bool EitherNonZero(int a, int b) => a + b > 0;
    }
}