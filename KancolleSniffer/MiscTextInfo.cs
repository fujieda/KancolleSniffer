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

using System.Collections.Generic;

namespace KancolleSniffer
{
    public class MiscTextInfo
    {
         public string Text { get; private set; }
        public bool ClearFlag { private get; set; }

        public void ClearIfNeeded()
        {
            if (!ClearFlag)
                return;
            Text = "";
            ClearFlag = false;
        }

        private readonly Dictionary<int, int> _required = new Dictionary<int, int>
        {
            {15, 4},
            {16, 7},
            {25, 4},
            {35, 4},
            {44, 4},
            {45, 5},
            {52, 4},
            {53, 5},
            {54, 5},
            {55, 5},
            {62, 3},
            {63, 4}
        };

        public void InspectMapInfo(dynamic json)
        {
            Text = "[海域ゲージ]\r\n";
            foreach (var entry in json)
            {
                var map = (int)entry.api_id;
                if (entry.api_eventmap())
                {
                    var evmap = entry.api_eventmap;
                    Text += $"{map / 10}-{map % 10} : HP {(int)evmap.api_now_maphp}/{(int)evmap.api_max_maphp}\r\n";
                    continue;
                }
                if (!entry.api_defeat_count())
                    continue;
                int req;
                var reqStr = _required.TryGetValue(map, out req) ? req.ToString() : "?";
                Text += $"{map / 10}-{map % 10} : 撃破 {(int)entry.api_defeat_count}/{reqStr}\r\n";
            }
        }
    }
}