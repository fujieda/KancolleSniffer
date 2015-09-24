// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
//
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace KancolleSniffer
{
    internal class MissingData
    {
        private static readonly string EnemySlotFile =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "EnemySlot.csv");

        private static Dictionary<int, int[]> _maxEq;

        public static Dictionary<int, int[]> MaxEq
        {
            get
            {
                if (_maxEq != null)
                    return _maxEq;
                _maxEq = new Dictionary<int, int[]>();
                try
                {
                    foreach (var line in File.ReadLines(EnemySlotFile))
                    {
                        int num;
                        var entry = line.Split(',').Select(e => int.TryParse(e, out num) ? num : 0).ToArray();
                        _maxEq[entry[0]] = entry.Skip(1).ToArray();
                    }
                }
                catch (FileNotFoundException)
                {
                }
                return _maxEq;
            }
        }
    }
}