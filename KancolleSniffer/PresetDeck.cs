// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer
{
    public class PresetDeck
    {
        private readonly Dictionary<int, int[]> _presetDeck = new Dictionary<int, int[]>();

        public void Inspect(dynamic json)
        {
            foreach (KeyValuePair<string, dynamic> entry in json.api_deck)
                InspectRegister(entry.Value);
        }

        public void InspectRegister(dynamic json)
        {
            var no = (int)json.api_preset_no - 1;
            _presetDeck[no] = json.api_ship;
        }

        public void InspectDelete(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _presetDeck[int.Parse(values["api_preset_no"]) - 1] = null;
        }

        public int[][] Decks => _presetDeck.Values.ToArray();
    }
}