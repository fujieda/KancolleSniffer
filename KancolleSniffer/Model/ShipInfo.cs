// Copyright (C) 2013, 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Util;

namespace KancolleSniffer.Model
{
    public class ShipInfo
    {
        public const int FleetCount = 4;
        public const int MemberCount = 6;

        private readonly IReadOnlyList<Fleet> _fleets;
        private readonly ShipMaster _shipMaster;
        private readonly ShipInventory _shipInventory;
        private readonly ItemInventory _itemInventory;
        private ShipStatus[] _battleResult = new ShipStatus[0];
        private readonly NumEquipsChecker _numEquipsChecker = new NumEquipsChecker();
        private int _hqLevel;
        public AlarmCounter Counter { get; }
        public ShipStatusPair[] BattleResultDiff { get; private set; } = new ShipStatusPair[0];
        public bool IsBattleResultError => BattleResultDiff.Length > 0;
        public ShipStatus[] BattleStartStatus { get; private set; } = new ShipStatus[0];
        public int DropShipId { private get; set; } = -1;

        private class NumEquipsChecker
        {
            public int MaxId { private get; set; } = int.MaxValue;

            public void Check(ShipStatus ship)
            {
                var spec = ship.Spec;
                if (spec.NumEquips != -1 || ship.Id <= MaxId)
                    return;
                spec.NumEquips = ship.Slot.Count(item => !item.Empty);
            }
        }

        public class ShipStatusPair
        {
            public ShipStatus Assumed { get; set; }
            public ShipStatus Actual { get; set; }

            public ShipStatusPair(ShipStatus assumed, ShipStatus actual)
            {
                Assumed = assumed;
                Actual = actual;
            }
        }

        public ShipInfo(ShipMaster shipMaster, ShipInventory shipInventory, ItemInventory itemInventory)
        {
            _shipMaster = shipMaster;
            _shipInventory = shipInventory;
            _fleets = Enumerable.Range(0, FleetCount).Select((x, i) => new Fleet(_shipInventory, i, () => _hqLevel)).ToArray();
            _itemInventory = itemInventory;
            Counter = new AlarmCounter(() => _shipInventory.Count) {Margin = 5};
        }

        public void InspectMaster(dynamic json)
        {
            _shipMaster.Inspect(json);
            _shipInventory.Clear();
        }

        public void InspectShip(string url, dynamic json)
        {
            if (url.Contains("port"))
            {
                HandlePort(json);
            }
            else if (url.Contains("ship2"))
            {
                FillShipData(json.api_data, json.api_data_deck);
            }
            else if (url.Contains("ship3"))
            {
                FillShipData(json.api_ship_data, json.api_deck_data);
            }
            else if (url.Contains("ship_deck"))
            {
                HandleShipDeck(json);
            }
            else if (url.Contains("getship")) // getship
            {
                HandleGetShip(json);
            }
            DropShipId = -1;
        }

        private void HandlePort(dynamic json)
        {
            _shipInventory.Clear();
            for (var i = 0; i < FleetCount; i++)
                _fleets[i].State = FleetState.Port;
            FillPortShipData(json);
            InspectBasic(json.api_basic);
            if (json.api_combined_flag())
                _fleets[0].CombinedType = _fleets[1].CombinedType = (CombinedType)(int)json.api_combined_flag;
            VerifyBattleResult();
        }

        private void FillPortShipData(dynamic json)
        {
            foreach (var entry in json.api_ship)
            {
                var ship = (ShipStatus)CreateShipStatus(entry);
                _shipInventory.Add(ship);
                _numEquipsChecker.Check(ship);
            }
            _numEquipsChecker.MaxId = _shipInventory.MaxId;
            InspectDeck(json.api_deck_port);
        }

        private void HandleShipDeck(dynamic json)
        {
            FillShipDeckShipData(json);
            VerifyBattleResult();
            // ドロップ艦を反映する
            if (DropShipId != -1)
            {
                _shipInventory.InflateCount(1);
                var num = _shipMaster.GetSpec(DropShipId).NumEquips;
                if (num > 0)
                    _itemInventory.InflateCount(num);
            }
        }

        private void FillShipDeckShipData(dynamic json)
        {
            foreach (var entry in json.api_ship_data)
            {
                var ship = (ShipStatus)CreateShipStatus(entry);
                var org = _shipInventory[ship.Id];
                ship.Escaped = org.Escaped; // 出撃中は継続する
                ship.SpecialAttack = org.SpecialAttack;
                _shipInventory.Add(ship);
            }
            InspectDeck(json.api_deck_data);
        }

        private void FillShipData(dynamic ship, dynamic deck)
        {
            FillShips(ship);
            InspectDeck(deck); // FleetのDeckを設定した時点でShipStatusを取得するので必ずdeckが後
        }

        private void HandleGetShip(dynamic json)
        {
            var ship = CreateShipStatus(json.api_ship);
            _shipInventory.Add(ship);
            _numEquipsChecker.Check(ship);
            _numEquipsChecker.MaxId = _shipInventory.MaxId;
        }

        private void FillShips(dynamic json)
        {
            foreach (var entry in json)
                _shipInventory.Add(CreateShipStatus(entry));
        }

        public void SaveBattleResult()
        {
            _battleResult = _fleets.Where(fleet =>
                    fleet.State >= FleetState.Sortie && !fleet.Ships[0].Spec.IsRepairShip)
                .SelectMany(fleet => fleet.Ships).ToArray();
        }

        public void ClearBattleResult()
        {
            _battleResult = new ShipStatus[0];
        }

        private void VerifyBattleResult()
        {
            BattleResultDiff = (from assumed in _battleResult
                let actual = GetShip(assumed.Id)
                where !assumed.Escaped && assumed.NowHp != actual.NowHp
                select new ShipStatusPair(assumed, actual)).ToArray();
            ClearBattleResult();
        }

        public void SaveBattleStartStatus()
        {
            BattleStartStatus = _fleets.Where(fleet => fleet.State >= FleetState.Sortie)
                .SelectMany(fleet => fleet.Ships.Select(ship => (ShipStatus)ship.Clone())).ToArray();
        }

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var fleet = (int)entry.api_id - 1;
                _fleets[fleet].Deck = (int[])entry.api_ship;
                if ((int)entry.api_mission[0] != 0)
                    _fleets[fleet].State = FleetState.Mission;
            }
        }

        private ShipStatus CreateShipStatus(dynamic entry)
        {
            return new ShipStatus
            {
                Id = (int)entry.api_id,
                Spec = _shipMaster.GetSpec((int)entry.api_ship_id),
                Level = (int)entry.api_lv,
                ExpToNext = (int)entry.api_exp[1],
                MaxHp = (int)entry.api_maxhp,
                NowHp = (int)entry.api_nowhp,
                Speed = entry.api_soku() ? (int)entry.api_soku : 0,
                Cond = (int)entry.api_cond,
                Fuel = (int)entry.api_fuel,
                Bull = (int)entry.api_bull,
                OnSlot = (int[])entry.api_onslot,
                GetItem = item => _itemInventory[item.Id],
                Slot = ((int[])entry.api_slot).Select(item => new ItemStatus(item)).ToArray(),
                SlotEx = entry.api_slot_ex() ? new ItemStatus((int)entry.api_slot_ex) : new ItemStatus(0),
                NdockTime = (int)entry.api_ndock_time,
                NdockItem = (int[])entry.api_ndock_item,
                LoS = (int)entry.api_sakuteki[0],
                Firepower = (int)entry.api_karyoku[0],
                Torpedo = (int)entry.api_raisou[0],
                AntiSubmarine = (int)entry.api_taisen[0],
                AntiAir = (int)entry.api_taiku[0],
                Lucky = (int)entry.api_lucky[0],
                Locked = entry.api_locked() && entry.api_locked == 1,
            };
        }

        private void InspectBasic(dynamic json)
        {
            _hqLevel = (int)json.api_level;
            Counter.Max = (int)json.api_max_chara;
        }

        public void InspectCharge(dynamic json)
        {
            foreach (var entry in json.api_ship)
            {
                var status = _shipInventory[(int)entry.api_id];
                status.Bull = (int)entry.api_bull;
                status.Fuel = (int)entry.api_fuel;
                status.OnSlot = (from num in (dynamic[])entry.api_onslot select (int)num).ToArray();
            }
        }

        public void InspectChange(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var dstFleet = _fleets[int.Parse(values["api_id"]) - 1];
            var dstIdx = int.Parse(values["api_ship_idx"]);
            var shipId = int.Parse(values["api_ship_id"]);

            if (shipId == -2)
            {
                dstFleet.WithdrawAccompanyingShips();
                return;
            }
            if (shipId == -1)
            {
                dstFleet.WithdrawShip(dstIdx);
                return;
            }
            var srcFleet = FindFleet(shipId, out var srcIdx);
            var prevShipId = dstFleet.SetShip(dstIdx, shipId);
            if (srcFleet == null)
                return;
            // 入れ替えの場合
            srcFleet.SetShip(srcIdx, prevShipId);
            if (prevShipId == -1)
                srcFleet.WithdrawShip(srcIdx);
        }

        private Fleet FindFleet(int ship, out int idx)
        {
            foreach (var fleet in _fleets)
            {
                idx = fleet.Deck.ToList().IndexOf(ship);
                if (idx < 0)
                    continue;
                return fleet;
            }
            idx = -1;
            return null;
        }

        public void InspectPowerUp(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ships = values["api_id_items"].Split(',').Select(int.Parse).ToArray();
            if (!_shipInventory.Contains(ships[0])) // 二重に実行された場合
                return;
            _itemInventory.Remove(ships.SelectMany(id => _shipInventory[id].Slot));
            _shipInventory.Remove(ships);
            FillShipData(new[]{json.api_ship}, json.api_deck);
        }

        public void InspectSlotExchange(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var ship = int.Parse(values["api_id"]);
            _shipInventory[ship].Slot = ((int[])json.api_slot).Select(id => new ItemStatus(id)).ToArray();
        }

        public void InspectSlotDeprive(dynamic json)
        {
            FillShips(new[] {json.api_ship_data.api_set_ship, json.api_ship_data.api_unset_ship});
            foreach (var fleet in _fleets)
                fleet.SetDeck(); // ShipStatusの差し替え
        }

        public void InspectMarriage(dynamic json)
        {
            FillShips(new[]{json});
            foreach (var fleet in _fleets)
                fleet.SetDeck(); // ShipStatusの差し替え
        }

        public void InspectDestroyShip(string request, dynamic json)
        {
            var values = HttpUtility.ParseQueryString(request);
            var delItem = int.Parse(values["api_slot_dest_flag"] ?? "0") == 1;
            foreach (var shipId in values["api_ship_id"].Split(',').Select(int.Parse))
            {
                if (delItem)
                    _itemInventory.Remove(_shipInventory[shipId].AllSlot);
                var srcFleet = FindFleet(shipId, out var srcIdx);
                srcFleet?.WithdrawShip(srcIdx);
                _shipInventory.Remove(shipId);
            }
        }

        public void InspectCombined(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            _fleets[0].CombinedType = _fleets[1].CombinedType = (CombinedType)int.Parse(values["api_combined_type"]);
        }

        public void InspectMapStart(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            if (_fleets[0].CombinedType == 0 || fleet > 1)
            {
                _fleets[fleet].State = FleetState.Sortie;
            }
            else
            {
                _fleets[0].State = _fleets[1].State = FleetState.Sortie;
            }
            SetBadlyDamagedShips();
        }

        public void StartPractice(string request)
        {
            var values = HttpUtility.ParseQueryString(request);
            var fleet = int.Parse(values["api_deck_id"]) - 1;
            _fleets[fleet].State = FleetState.Practice;
        }

        public IReadOnlyList<Fleet> Fleets => _fleets;

        public ShipStatus GetShip(int id) => _shipInventory[id];

        public void SetItemHolder()
        {
            foreach (var ship in _shipInventory.AllShips)
            {
                foreach (var item in ship.Slot)
                    _itemInventory[item.Id].Holder = ship;
                _itemInventory[ship.SlotEx.Id].Holder = ship;
            }
        }

        public ShipSpec GetSpec(int id) => _shipMaster.GetSpec(id);

        public ShipStatus[] ShipList => _shipInventory.AllShips.ToArray();

        public ShipStatus[] GetRepairList(DockInfo dockInfo)
            => (from s in ShipList
                where s.NowHp < s.MaxHp && !dockInfo.InNDock(s.Id) &&
                      (s.Fleet == null || s.Fleet.State != FleetState.Practice)
                select s).OrderByDescending(s => s.RepairTime).ToArray();

        public string[] BadlyDamagedShips { get; private set; } = new string[0];

        public void SetBadlyDamagedShips()
        {
            BadlyDamagedShips =
            (from s in _fleets.Where(fleet => fleet.State == FleetState.Sortie)
                    .SelectMany(fleet => fleet.CombinedType != 0 && fleet.Number == 1
                        ? fleet.ActualShips.Skip(1) // 第二艦隊の旗艦を除く
                        : fleet.ActualShips)
                where !s.Escaped && s.DamageLevel == ShipStatus.Damage.Badly
                select s.Name).ToArray();
        }

        public void ClearBadlyDamagedShips()
        {
            BadlyDamagedShips = new string[0];
        }

        public void SetEscapedShips(List<int> ids)
        {
            foreach (var id in ids)
                _shipInventory[id].Escaped = true;
        }
    }
}