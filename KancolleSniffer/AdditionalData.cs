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
    public class AdditionalData
    {
        private static readonly string EnemySlotFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnemySlot.csv");

        private Dictionary<int, int[]> _maxEq;

        public void LoadEnemySlot()
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

        public int[] EnemySlot(int id) =>
            _maxEq != null ? _maxEq.TryGetValue(id, out var slot) ? slot : null : null;

        private const string TpFileName = "TP.csv";

        private static readonly string TpFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, TpFileName);

        private Dictionary<int, double> _tpSpec;

        public void LoadTpSpec()
        {
            try
            {
                _tpSpec = File.ReadAllLines(TpFile)
                    .Select(line => line.Split(','))
                    .ToDictionary(f => int.Parse(f[0]), f => double.Parse(f[2]));
            }
            catch (IOException)
            {
            }
            catch (Exception ex)
            {
                throw new Exception(TpFileName + "が壊れています。", ex);
            }
        }

        public double ItemTp(int id) =>
            _tpSpec != null ? _tpSpec.TryGetValue(id, out var tp) ? tp : -1 : -1;

        private static readonly string NumEquipsFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "NumEquips.csv");

        private Dictionary<int, int> _numEquips = new Dictionary<int, int>();

        public bool UseNumEquipsFile { get; set; } = true;

        public void LoadNumEquips()
        {
            try
            {
                if (!UseNumEquipsFile)
                    return;
                _numEquips = File.ReadLines(NumEquipsFile)
                    .Select(line => line.Split(','))
                    .ToDictionary(f => int.Parse(f[0]), f => int.Parse(f[2]));
            }
            catch (IOException)
            {
            }
        }

        public int NumEquips(int id) => _numEquips.TryGetValue(id, out var num) ? num : -1;

        public void RecordNumEquips(int id, string name, int numEquips)
        {
            _numEquips[id] = numEquips;
            if (UseNumEquipsFile)
                File.AppendAllText(NumEquipsFile, $"{id},{name},{numEquips}\r\n");
        }
    }
}