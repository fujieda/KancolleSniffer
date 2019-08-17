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

namespace KancolleSniffer.Model
{
    public class AirBattleResult
    {
        public List<AirBattleRecord> Result = new List<AirBattleRecord>();

        private bool _updated;
        private readonly Func<int, string> _getShipName;
        private readonly Func<int[], string[]> _getItemNames;

        public AirBattleResult(Func<int, string> getShipName, Func<int[], string[]> getItemNames)
        {
            _getShipName = getShipName;
            _getItemNames = getItemNames;
        }

        public class AirBattleRecord
        {
            public string PhaseName { get; set; }
            public int AirControlLevel { get; set; }
            public StageResult Stage1 { get; set; }
            public StageResult Stage2 { get; set; }
            public AirFireResult AirFire { get; set; }
        }

        public bool CheckUpdate()
        {
            if (!_updated)
                return false;
            _updated = false;
            return true;
        }

        public void Clear()
        {
            Result.Clear();
            _updated = true;
        }

        public class StageResult
        {
            public int FriendCount { get; set; }
            public int FriendLost { get; set; }
            public int EnemyCount { get; set; }
            public int EnemyLost { get; set; }
        }

        public class AirFireResult
        {
            public string ShipName { get; set; }
            public int Kind { get; set; }
            public string[] Items { get; set; }
        }

        public void Add(dynamic json, string phaseName)
        {
            var stage1 = json.api_stage1;
            if (stage1 == null || (stage1.api_f_count == 0 && stage1.api_e_count == 0))
                return;
            var result = new AirBattleRecord
            {
                PhaseName = phaseName,
                AirControlLevel = json.api_stage1.api_disp_seiku() ? (int)json.api_stage1.api_disp_seiku : 0,
                Stage1 = CreateStageResult(json.api_stage1),
                Stage2 = json.api_stage2 == null
                    ? new StageResult()
                    : CreateStageResult(json.api_stage2),
                AirFire = CreateAirFireResult(json)
            };
            Result.Add(result);
        }

        private StageResult CreateStageResult(dynamic stage)
        {
            return new StageResult
            {
                FriendCount = (int)stage.api_f_count,
                FriendLost = (int)stage.api_f_lostcount,
                EnemyCount = (int)stage.api_e_count,
                EnemyLost = (int)stage.api_e_lostcount
            };
        }

        private AirFireResult CreateAirFireResult(dynamic json)
        {
            if (json.api_stage2 == null || !json.api_stage2.api_air_fire())
                return null;
            var airFire = json.api_stage2.api_air_fire;
            var idx = (int)airFire.api_idx;
            return new AirFireResult
            {
                ShipName = _getShipName(idx),
                Kind = (int)airFire.api_kind,
                Items = _getItemNames((int[])airFire.api_use_items)
            };
        }
    }
}