// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.IO;
using System.Linq;

namespace KancolleSniffer
{
    public static  class DataLoader
    {
        private static readonly string EnemySlotFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnemySlot.csv");

        private static Dictionary<int, int[]> _maxEq;

        public static void LoadEnemySlot()
        {
            try
            {
                _maxEq = File.ReadLines(EnemySlotFile)
                    .Select(line => line.Split(',').Select(s => int.TryParse(s, out var num) ? num : 0))
                    .ToDictionary(nums => nums.First(), nums => nums.Skip(1).ToArray());
            }
            catch (IOException)
            {
            }
        }

        public static int[] EnemySlot(int id) =>
            _maxEq != null ? _maxEq.TryGetValue(id, out var slot) ? slot : null : null;
    }
}