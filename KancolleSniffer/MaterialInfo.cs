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
using System.Linq;
using System.Xml.Serialization;

namespace KancolleSniffer
{
    public class MaterialInfo : IHaveState
    {
        private bool _inPort;
        private DateTime _lastMission;

        public MaterialCount[] MaterialHistory { get; }
        public int[] PrevPort { get; }
        public int[] Current => MaterialHistory.Select(h => h.Now).ToArray();

        public MaterialInfo()
        {
            var n = Enum.GetValues(typeof(Material)).Length;
            MaterialHistory = new MaterialCount[n];
            for (var i = 0; i < n; i++)
                MaterialHistory[i] = new MaterialCount();
            PrevPort = new int[n];
            PrevPort[0] = -1;
        }

        public bool NeedSave
        {
            get { return MaterialHistory.Any(m => m.NeedSave); }
            private set
            {
                foreach (var m in MaterialHistory)
                    m.NeedSave = value;
            }
        }

        public void InspectMaterialPort(dynamic json)
        {
            InspectMaterial(json);
            _inPort = true;
            if (PrevPort[0] == -1)
            {
                // 初期化する
                for (var i = 0; i < MaterialHistory.Length; i++)
                    PrevPort[i] = MaterialHistory[i].Now;
            }
        }

        public void InspectMaterial(dynamic json)
        {
            UpdatePrevPort();
            foreach (var entry in json)
            {
                var i = (int)entry.api_id - 1;
                var v = (int)entry.api_value;
                MaterialHistory[i].Now = v;
            }
        }

        // 母港に戻ってから最初に資材に変化があったときに、母港の時点の資材を記録する
        private void UpdatePrevPort()
        {
            if (!_inPort)
                return;
            for (var i = 0; i < MaterialHistory.Length; i++)
                PrevPort[i] = MaterialHistory[i].Now;
            _inPort = false;
        }

        public void InspectCharge(dynamic json)
        {
            SetMaterials((int[])json.api_material);
        }

        public void InspectMissionResult(dynamic json)
        {
            if ((int)json.api_clear_result == 0) // 失敗
                return;
            // 連続して遠征が帰投したときに資材の増加を積算する
            if (DateTime.Now - _lastMission < TimeSpan.FromMinutes(1))
                _inPort = false;
            _lastMission = DateTime.Now;
            AddMaterials((int[])json.api_get_material);
        }

        public void InspectDestroyShip(dynamic json)
        {
            SetMaterials((int[])json.api_material);
        }

        public void InspectCreateIem(dynamic json)
        {
            SetMaterials((int[])json.api_material);
        }

        public void InspectDestroyItem(dynamic json)
        {
            AddMaterials((int[])json.api_get_material);
        }

        public void InspectRemodelSlot(dynamic json)
        {
            SetMaterials((int[])json.api_after_material);
        }

        public void InspectAirCorpsSupply(dynamic json)
        {
            SetMaterial(Material.Fuel, (int)json.api_after_fuel);
            SetMaterial(Material.Bouxite, (int)json.api_after_bauxite);
        }

        public void InspectAirCorpsSetPlane(dynamic json)
        {
            if (json.api_after_bauxite())
                SetMaterial(Material.Bouxite, (int)json.api_after_bauxite);
        }

        public void SetMaterials(int[] material)
        {
            UpdatePrevPort();
            for (var i = 0; i < material.Length; i++)
                MaterialHistory[i].Now = material[i];
        }

        public void SetMaterial(Material m, int v)
        {
            UpdatePrevPort();
            MaterialHistory[(int)m].Now = v;
        }

        public void AddMaterials(int[] v)
        {
            UpdatePrevPort();
            for (var i = 0; i < v.Length; i++)
                MaterialHistory[i].Now += v[i];
        }

        public void SubMaterial(Material m, int v)
        {
            UpdatePrevPort();
            MaterialHistory[(int)m].Now -= v;
        }

        public void SaveState(Status status)
        {
            NeedSave = false;
            status.MaterialHistory = MaterialHistory.ToList();
        }

        public void LoadState(Status status)
        {
            status.MaterialHistory?.CopyTo(MaterialHistory, 0);
        }
    }


    public enum Material
    {
        Fuel,
        Bullet,
        Steal,
        Bouxite,
        Burner,
        Bucket,
        Development,
        Screw,
    }

    public class MaterialCount
    {
        private int _now;

        public int BegOfDay { get; set; }
        public int BegOfWeek { get; set; }
        public DateTime LastSet { get; set; }
        [XmlIgnore]
        public bool NeedSave { get; set; }

        public int Now
        {
            get => _now;
            set
            {
                var prev = _now;
                _now = value;
                if (Status.Restoring) // JSONから値を復旧するときは履歴に触らない
                    return;
                if (_now != prev)
                    NeedSave = true;
                if (LastSet == DateTime.MinValue)
                {
                    BegOfDay = BegOfWeek = value;
                    LastSet = DateTime.Now;
                    return;
                }
                UpdateHistory(prev);
                LastSet = DateTime.Now;
            }
        }

        private void UpdateHistory(int prev)
        {
            var morning = DateTime.Today.AddHours(5);
            var dow = (int)morning.DayOfWeek;
            var monday = morning.AddDays(dow == 0 ? -6 : -dow + 1);
            if (DateTime.Now >= morning && LastSet < morning)
                BegOfDay = prev;
            if (DateTime.Now >= monday && LastSet < monday)
                BegOfWeek = prev;
        }
    }
}