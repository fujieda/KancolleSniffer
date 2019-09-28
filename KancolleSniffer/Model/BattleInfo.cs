// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Util;
using static System.Math;

namespace KancolleSniffer.Model
{
    public enum BattleResultRank
    {
        P,
        S,
        A,
        B,
        C,
        D,
        E
    }

    public enum BattleState
    {
        None,
        Day,
        Night,
        SpNight,
        AirRaid,
        Result,
        Unknown
    }

    public class EnemyFighterPower
    {
        public bool HasUnknown { get; set; }
        public string UnknownMark => HasUnknown ? "+" : "";
        public int AirCombat { get; set; }
        public int Interception { get; set; }
    }

    public class BattleInfo : Sniffer.IPort
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private readonly AirBase _airBase;
        private Fleet _fleet;
        private Record[] _friend;
        private Record[] _guard;
        private Record[] _enemy;
        private Record[] _enemyGuard;
        private readonly List<int> _escapingShips = new List<int>();
        private bool _lastCell;

        public BattleState BattleState { get; set; }
        public int[] Formation { get; private set; }
        public Range FighterPower { get; private set; }
        public EnemyFighterPower EnemyFighterPower { get; private set; }
        public int AirControlLevel { get; private set; }
        public BattleResultRank ResultRank { get; private set; }
        public RankPair DisplayedResultRank { get; } = new RankPair();
        public BattleResult Result { get; set; }
        public bool EnemyIsCombined => _enemyGuard.Length > 0;
        public AirBattleResult AirBattleResult;
        public int SupportType { get; private set; }

        public class RankPair
        {
            public char Assumed { get; set; } = 'X';
            public char Actual { get; set; }
            public bool IsError => Assumed != Actual;
        }

        public class BattleResult
        {
            public class Combined
            {
                public ShipStatus[] Main { get; set; }
                public ShipStatus[] Guard { get; set; }
            }

            public Combined Friend { get; set; }
            public Combined Enemy { get; set; }
        }

        public BattleInfo(ShipInfo shipInfo, ItemInfo itemInfo, AirBase airBase)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
            _airBase = airBase;
            AirBattleResult = new AirBattleResult(GetAirFireShipName, GetItemNames);
        }

        private string GetAirFireShipName(int idx)
        {
            return idx < _friend.Length ? _friend[idx].Name : _guard[idx - 6].Name;
        }

        private string[] GetItemNames(int[] ids)
        {
            return ids.Select(id => _itemInfo.GetSpecByItemId(id).Name).ToArray();
        }

        public void Port()
        {
            CleanupResult();
            _lastCell = false;
            BattleState = BattleState.None;
        }

        public void InspectBattle(string url, string request, dynamic json)
        {
            SetFormation(json);
            SetSupportType(json);
            ClearDamagedShipWarning();
            ShowResult(); // 昼戦の結果を夜戦のときに表示する
            SetupDamageRecord(request, json, url.Contains("practice"));
            SetFighterPower();
            SetEnemyFighterPower();
            BattleState = url.Contains("sp_midnight") ? BattleState.SpNight :
                url.Contains("midnight") ? BattleState.Night : BattleState.Day;
            if (BattleState != BattleState.Night)
            {
                AirBattleResult.Clear();
                SetAirControlLevel(json);
            }
            CalcDamage(json);
            ResultRank = url.Contains("/ld_") ? CalcLdResultRank() : CalcResultRank();
            SetResult();
        }

        private void SetFormation(dynamic json)
        {
            if (json.api_formation())
                Formation = (int[])json.api_formation;
        }

        private void SetAirControlLevel(dynamic json)
        {
            AirControlLevel = -1;
            if (!json.api_kouku())
                return;
            var stage1 = json.api_kouku.api_stage1;
            if (stage1 == null || stage1.api_f_count == 0 && stage1.api_e_count == 0)
                return;
            AirControlLevel = (int)stage1.api_disp_seiku;
        }

        private void SetSupportType(dynamic json)
        {
            SupportType = json.api_support_flag() ? (int)json.api_support_flag :
                json.api_n_support_flag() ? (int)json.api_n_support_flag : 0;
        }

        private void SetupDamageRecord(string request, dynamic json, bool practice)
        {
            if (_friend != null)
                return;
            _shipInfo.SaveBattleStartStatus();
            SetupFriendDamageRecord(request, json, practice);
            SetupEnemyDamageRecord(json, practice);
        }

        private void SetupFriendDamageRecord(string request, dynamic json, bool practice)
        {
            _fleet = _shipInfo.Fleets[(int)json.api_deck_id - 1];
            FlagshipRecovery(request, _fleet.ActualShips[0]);
            _friend = Record.Setup(_fleet.ActualShips, practice);
            _guard = json.api_f_nowhps_combined()
                ? Record.Setup(_shipInfo.Fleets[1].ActualShips, practice)
                : new Record[0];
        }

        private void SetupEnemyDamageRecord(dynamic json, bool practice)
        {
            _enemy = Record.Setup((int[])json.api_e_nowhps,
                EnemyShipSpecs(json.api_ship_ke),
                EnemySlots(json.api_eSlot), practice);
            _enemyGuard = json.api_ship_ke_combined()
                ? Record.Setup((int[])json.api_e_nowhps_combined,
                    EnemyShipSpecs(json.api_ship_ke_combined),
                    EnemySlots(json.api_eSlot_combined), practice)
                : new Record[0];
        }

        private ShipSpec[] EnemyShipSpecs(dynamic ships)
        {
            return ((int[])ships).Select(_shipInfo.GetSpec).ToArray();
        }

        private ItemSpec[][] EnemySlots(dynamic slots)
        {
            return ((int[][])slots).Select(slot => slot.Select(_itemInfo.GetSpecByItemId).ToArray()).ToArray();
        }

        private void SetResult()
        {
            Result = new BattleResult
            {
                Friend = new BattleResult.Combined
                {
                    Main = _friend.Select(r => r.SnapShot).ToArray(),
                    Guard = _guard.Select(r => r.SnapShot).ToArray()
                },
                Enemy = new BattleResult.Combined
                {
                    Main = _enemy.Select(r => r.SnapShot).ToArray(),
                    Guard = _enemyGuard.Select(r => r.SnapShot).ToArray()
                }
            };
        }

        private void FlagshipRecovery(string request, ShipStatus flagship)
        {
            var type = int.Parse(HttpUtility.ParseQueryString(request)["api_recovery_type"] ?? "0");
            switch (type)
            {
                case 0:
                    return;
                case 1:
                    flagship.NowHp = flagship.MaxHp / 2;
                    ConsumeSlotItem(flagship, 42); // ダメコン
                    break;
                case 2:
                    flagship.NowHp = flagship.MaxHp;
                    ConsumeSlotItem(flagship, 43); // 女神
                    break;
            }
            if (type != 0)
                _shipInfo.SetBadlyDamagedShips();
        }

        private static void ConsumeSlotItem(ShipStatus ship, int id)
        {
            if (ship.SlotEx.Spec.Id == id)
            {
                ship.SlotEx = new ItemStatus();
                return;
            }
            for (var i = 0; i < ship.Slot.Count; i++)
            {
                if (ship.Slot[i].Spec.Id == id)
                {
                    ship.FreeSlot(i);
                    break;
                }
            }
        }

        private void CleanupResult()
        {
            _friend = null;
        }

        private void SetFighterPower()
        {
            var fleets = _shipInfo.Fleets;
            FighterPower = _guard.Length > 0 && _enemyGuard.Length > 0
                ? fleets[0].FighterPower + fleets[1].FighterPower
                : _fleet.FighterPower;
        }

        private void SetEnemyFighterPower()
        {
            EnemyFighterPower = new EnemyFighterPower();
            foreach (var record in _guard.Length == 0 ? _enemy : _enemy.Concat(_enemyGuard))
            {
                var ship = record.SnapShot;
                if (ship.Spec.MaxEq == null)
                {
                    EnemyFighterPower.HasUnknown = true;
                    continue;
                }
                foreach (var entry in ship.Slot.Zip(ship.Spec.MaxEq, (item, maxEq) => new {item.Spec, maxEq}))
                {
                    var perSlot = (int)Floor(entry.Spec.AntiAir * Sqrt(entry.maxEq));
                    if (entry.Spec.CanAirCombat)
                        EnemyFighterPower.AirCombat += perSlot;
                    if (entry.Spec.IsAircraft)
                        EnemyFighterPower.Interception += perSlot;
                }
            }
        }

        public void InspectMapStart(dynamic json)
        {
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _lastCell = (int)json.api_next == 0;

            if (!json.api_destruction_battle())
                return;
            InspectAirRaidBattle((int)json.api_maparea_id, json.api_destruction_battle);
        }

        public void InspectAirRaidBattle(int areaId, dynamic json)
        {
            SetFormation(json);
            var attack = json.api_air_base_attack;
            var stage1 = attack.api_stage1;
            AirControlLevel = (int)stage1.api_disp_seiku;
            var ships = (ShipStatus[])CreateShipsForAirBase(json);
            _friend = Record.Setup(ships, false);
            _guard = new Record[0];
            FighterPower = _airBase.GetAirBase(areaId).CalcInterceptionFighterPower();
            SetupEnemyDamageRecord(json, false);
            SetEnemyFighterPower();
            BattleState = BattleState.AirRaid;
            AirBattleResult.Clear();
            AirBattleResult.Add(json.api_air_base_attack, "空襲");
            CalcKoukuDamage(json.api_air_base_attack);
            SetAirRaidResultRank(json);
            SetResult();
            CleanupResult();
        }

        private ShipStatus[] CreateShipsForAirBase(dynamic json)
        {
            var nowHps = (int[])json.api_f_nowhps;
            var maxHps = (int[])json.api_f_maxhps;
            var maxEq = new[] {18, 18, 18, 18};
            var ships = nowHps.Select((hp, n) => new ShipStatus
            {
                Id = 1,
                Spec = new ShipSpec {Name = "基地航空隊" + (n + 1), GetMaxEq = () => maxEq},
                NowHp = nowHps[n],
                MaxHp = maxHps[n]
            }).ToArray();
            var planes = json.api_air_base_attack.api_map_squadron_plane;
            if (planes == null)
                return ships;
            foreach (KeyValuePair<string, dynamic> entry in planes)
            {
                var num = int.Parse(entry.Key) - 1;
                var slot = new List<ItemStatus>();
                var onSlot = new List<int>();
                foreach (var plane in entry.Value)
                {
                    slot.Add(new ItemStatus {Id = 1, Spec = _itemInfo.GetSpecByItemId((int)plane.api_mst_id)});
                    onSlot.Add((int)plane.api_count);
                }
                ships[num].Slot = slot;
                ships[num].OnSlot = onSlot.ToArray();
            }
            return ships;
        }

        private void SetAirRaidResultRank(dynamic json)
        {
            switch ((int)json.api_lost_kind)
            {
                case 1:
                    ResultRank = BattleResultRank.A;
                    break;
                case 2:
                    ResultRank = BattleResultRank.B;
                    break;
                case 3:
                    ResultRank = BattleResultRank.C;
                    break;
                case 4:
                    ResultRank = BattleResultRank.S;
                    break;
            }
        }

        private void CalcDamage(dynamic json)
        {
            foreach (KeyValuePair<string, dynamic> kv in json)
            {
                if (kv.Value == null)
                    continue;
                switch (kv.Key)
                {
                    case "api_air_base_injection":
                        AirBattleResult.Add(kv.Value, "AB噴式");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_injection_kouku":
                        AirBattleResult.Add(kv.Value, "噴式");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_air_base_attack":
                        CalcAirBaseAttackDamage(kv.Value);
                        break;
                    case "api_n_support_info":
                        CalcSupportDamage(kv.Value);
                        break;
                    case "api_n_hougeki1":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_n_hougeki2":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_kouku":
                        AirBattleResult.Add(kv.Value, "航空戦");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_kouku2":
                        AirBattleResult.Add(kv.Value, "航空戦2");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_support_info":
                        CalcSupportDamage(kv.Value);
                        break;
                    case "api_opening_taisen":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_opening_atack":
                        CalcDamageAtOnce(kv.Value);
                        break;
                    case "api_friendly_battle":
                        CalcFriendAttackDamage(kv.Value);
                        break;
                    case "api_hougeki":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_hougeki1":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_hougeki2":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_hougeki3":
                        CalcDamageByTurn(kv.Value);
                        break;
                    case "api_raigeki":
                        CalcDamageAtOnce(kv.Value);
                        break;
                }
            }
        }

        private void CalcSupportDamage(dynamic json)
        {
            if (json.api_support_hourai != null)
            {
                CalcRawDamageAtOnce(json.api_support_hourai.api_damage, _enemy, _enemyGuard);
            }
            else if (json.api_support_airatack != null)
            {
                CalcRawDamageAtOnce(json.api_support_airatack.api_stage3.api_edam, _enemy, _enemyGuard);
            }
        }

        private void CalcAirBaseAttackDamage(dynamic json)
        {
            var i = 1;
            foreach (var entry in json)
            {
                AirBattleResult.Add(entry, "基地" + i++);
                CalcKoukuDamage(entry);
            }
        }

        private void CalcFriendAttackDamage(dynamic json)
        {
            CalcDamageByTurn(json.api_hougeki, true);
        }

        private void CalcKoukuDamage(dynamic json)
        {
            if (json.api_stage3() && json.api_stage3 != null)
                CalcDamageAtOnce(json.api_stage3, _friend, _enemy);
            if (json.api_stage3_combined() && json.api_stage3_combined != null)
                CalcDamageAtOnce(json.api_stage3_combined, _guard, _enemyGuard);
        }

        private void CalcDamageAtOnce(dynamic json)
        {
            CalcDamageAtOnce(json, _friend, _guard, _enemy, _enemyGuard);
        }

        private void CalcDamageAtOnce(dynamic json, Record[] friend, Record[] enemy)
        {
            CalcDamageAtOnce(json, friend, null, enemy, null);
        }

        private void CalcDamageAtOnce(dynamic json,
            Record[] friend, Record[] guard, Record[] enemy, Record[] enemyGuard)
        {
            if (json.api_fdam() && json.api_fdam != null)
                CalcRawDamageAtOnce(json.api_fdam, friend, guard);
            if (json.api_edam() && json.api_edam != null)
                CalcRawDamageAtOnce(json.api_edam, enemy, enemyGuard);
        }

        private void CalcRawDamageAtOnce(dynamic rawDamage, Record[] friend, Record[] guard = null)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < friend.Length; i++)
            {
                friend[i].ApplyDamage(damage[i]);
                friend[i].CheckDamageControl();
            }
            if (guard == null)
                return;
            for (var i = 0; i < guard.Length; i++)
            {
                guard[i].ApplyDamage(damage[i + 6]);
                guard[i].CheckDamageControl();
            }
        }

        private void CalcDamageByTurn(dynamic json, bool ignoreFriendDamage = false)
        {
            if (!(json.api_df_list() && json.api_df_list != null &&
                  json.api_damage() && json.api_damage != null &&
                  json.api_at_eflag() && json.api_at_eflag != null))
                return;

            var eFlags = (int[])json.api_at_eflag;
            var sources = (int[])json.api_at_list;
            var types = json.api_at_type() ? (int[])json.api_at_type : (int[])json.api_sp_list;
            var targets = (int[][])json.api_df_list;
            var damages = (int[][])json.api_damage;
            var records = new BothRecord(_friend, _guard, _enemy, _enemyGuard);
            for (var turn = 0; turn < eFlags.Length; turn++)
            {
                if (ignoreFriendDamage && eFlags[turn] == 1)
                    continue;
                if (IsSpecialAttack(types[turn]))
                    records.TriggerSpecialAttack(eFlags[turn] ^ 1, sources[turn]);
                for (var shot = 0; shot < targets[turn].Length; shot++)
                {
                    var target = targets[turn][shot];
                    var damage = damages[turn][shot];
                    if (target == -1 || damage == -1)
                        continue;
                    records.ApplyDamage(eFlags[turn], target, damage);
                }
                records.CheckDamageControl();
            }
        }

        private bool IsSpecialAttack(int type)
        {
            // 100: Nelson Touch
            // 101: 長門一斉射
            // 102: 陸奥一斉射
            // 200: 瑞雲一体攻撃
            // 201: 海陸立体攻撃
            return type >= 100 && type < 200;
        }

        private class BothRecord
        {
            private readonly Record[][] _records;

            public BothRecord(Record[] friend, Record[] guard, Record[] enemy, Record[] enemyGuard)
            {
                _records = new[] {new Record[12], new Record[12]};
                Array.Copy(friend, _records[1], friend.Length);
                Array.Copy(guard, 0, _records[1], 6, guard.Length);
                Array.Copy(enemy, _records[0], enemy.Length);
                Array.Copy(enemyGuard, 0, _records[0], 6, enemyGuard.Length);
            }

            public void TriggerSpecialAttack(int side, int index)
            {
                _records[side][index].TriggerSpecialAttack();
            }

            public void ApplyDamage(int side, int index, int damage)
            {
                _records[side][index].ApplyDamage(damage);
            }

            public void CheckDamageControl()
            {
                foreach (var ship in _records[1])
                    ship?.CheckDamageControl();
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            BattleState = BattleState.Result;
            if (_friend == null)
                return;
            ShowResult();
            if (!_lastCell)
                SetDamagedShipWarning();
            _shipInfo.SaveBattleResult();
            _shipInfo.DropShipId = json.api_get_ship() ? (int)json.api_get_ship.api_ship_id : -1;
            VerifyResultRank(json);
            CleanupResult();
            SetEscapeShips(json);
        }

        public void InspectPracticeResult(dynamic json)
        {
            BattleState = BattleState.Result;
            if (_friend == null)
                return;
            ShowResult();
            VerifyResultRank(json);
            CleanupResult();
        }

        private void ShowResult()
        {
            if (_friend == null)
                return;
            var fleets = _shipInfo.Fleets;
            var ships = _guard.Length > 0
                ? fleets[0].ActualShips.Concat(fleets[1].ActualShips)
                : _fleet.ActualShips;
            foreach (var entry in ships.Zip(_friend.Concat(_guard), (ship, now) => new {ship, now}))
                entry.now.UpdateShipStatus(entry.ship);
        }

        private void SetDamagedShipWarning()
        {
            _shipInfo.SetBadlyDamagedShips();
        }

        private void ClearDamagedShipWarning()
        {
            _shipInfo.ClearBadlyDamagedShips();
        }

        private void VerifyResultRank(dynamic json)
        {
            if (!json.api_win_rank())
                return;
            var assumed = "PSABCDE"[(int)ResultRank];
            if (assumed == 'P')
                assumed = 'S';
            var actual = ((string)json.api_win_rank)[0];
            DisplayedResultRank.Assumed = assumed;
            DisplayedResultRank.Actual = actual;
        }

        public void SetEscapeShips(dynamic json)
        {
            _escapingShips.Clear();
            if (!json.api_escape_flag() || (int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            if (json.api_escape.api_tow_idx())
            {
                _escapingShips.Add(_shipInfo.Fleets[damaged / 6].Deck[damaged % 6]);
                var escort = (int)json.api_escape.api_tow_idx[0] - 1;
                _escapingShips.Add(_shipInfo.Fleets[escort / 6].Deck[escort % 6]);
            }
            else
            {
                _escapingShips.Add(_shipInfo.Fleets[2].Deck[damaged]);
            }
        }

        public void CauseEscape()
        {
            _shipInfo.SetEscapedShips(_escapingShips);
            _shipInfo.SetBadlyDamagedShips();
        }

        private class Record
        {
            private ShipStatus _status;
            private bool _practice;
            public ShipStatus SnapShot => (ShipStatus)_status.Clone();
            public int NowHp => _status.NowHp;
            public bool Escaped => _status.Escaped;
            public ShipStatus.Damage DamageLevel => _status.DamageLevel;
            public string Name => _status.Name;
            public int StartHp { get; private set; }

            public static Record[] Setup(IEnumerable<ShipStatus> ships, bool practice) =>
                (from s in ships
                    select new Record {_status = (ShipStatus)s.Clone(), _practice = practice, StartHp = s.NowHp})
                .ToArray();

            public static Record[] Setup(int[] nowHps, ShipSpec[] specs, ItemSpec[][] slots, bool practice)
            {
                return Enumerable.Range(0, nowHps.Length).Select(i =>
                    new Record
                    {
                        StartHp = nowHps[i],
                        _status = new ShipStatus
                        {
                            Id = specs[i].Id,
                            NowHp = nowHps[i],
                            MaxHp = nowHps[i],
                            Spec = specs[i],
                            Slot = slots[i].Select(spec => new ItemStatus {Id = spec.Id, Spec = spec}).ToArray(),
                            SlotEx = new ItemStatus(0)
                        },
                        _practice = practice
                    }).ToArray();
            }

            public void TriggerSpecialAttack()
            {
                _status.SpecialAttack = ShipStatus.Attack.Fire;
            }

            public void ApplyDamage(int damage)
            {
                _status.NowHp = Max(0, _status.NowHp - damage);
            }

            public void CheckDamageControl()
            {
                if (_status.NowHp > 0 || _practice)
                    return;
                foreach (var item in new[] {_status.SlotEx}.Concat(_status.Slot))
                {
                    if (item.Spec.Id == 42)
                    {
                        _status.NowHp = (int)(_status.MaxHp * 0.2);
                        ConsumeSlotItem(_status, 42);
                        break;
                    }
                    if (item.Spec.Id == 43)
                    {
                        _status.NowHp = _status.MaxHp;
                        ConsumeSlotItem(_status, 43);
                        break;
                    }
                }
            }

            public void UpdateShipStatus(ShipStatus ship)
            {
                ship.NowHp = NowHp;
                ship.Slot = _status.Slot;
                ship.SlotEx = _status.SlotEx;
                ship.SpecialAttack = _status.SpecialAttack == ShipStatus.Attack.Fire
                    ? ShipStatus.Attack.Fired
                    : ShipStatus.Attack.None;
            }
        }

        private BattleResultRank CalcLdResultRank()
        {
            var friend = new ResultRankParams(_friend.Concat(_guard).ToArray());

            if (friend.Gauge <= 0)
                return BattleResultRank.P;
            if (friend.GaugeRate < 10)
                return BattleResultRank.A;
            if (friend.GaugeRate < 20)
                return BattleResultRank.B;
            if (friend.GaugeRate < 50)
                return BattleResultRank.C;
            if (friend.GaugeRate < 80)
                return BattleResultRank.D;
            return BattleResultRank.E;
        }

        private BattleResultRank CalcResultRank()
        {
            var friend = new ResultRankParams(_friend.Concat(_guard).ToArray());
            var enemy = new ResultRankParams(_enemy.Concat(_enemyGuard).ToArray());
            if (friend.Sunk == 0 && enemy.Sunk == enemy.Count)
            {
                if (friend.Gauge <= 0)
                    return BattleResultRank.P;
                return BattleResultRank.S;
            }
            if (friend.Sunk == 0 && enemy.Sunk >= (int)(enemy.Count * 0.7) && enemy.Count > 1)
                return BattleResultRank.A;
            if (friend.Sunk < enemy.Sunk && _enemy[0].NowHp == 0)
                return BattleResultRank.B;
            if (friend.Count == 1 && _friend[0].DamageLevel == ShipStatus.Damage.Badly)
                return BattleResultRank.D;
            if (enemy.GaugeRate > friend.GaugeRate * 2.5)
                return BattleResultRank.B;
            if (enemy.GaugeRate > friend.GaugeRate * 0.9)
                return BattleResultRank.C;
            if (friend.Count > 1 && friend.Count - 1 == friend.Sunk)
                return BattleResultRank.E;
            return BattleResultRank.D;
        }

        private class ResultRankParams
        {
            public readonly int Count;
            public readonly int Sunk;
            public readonly int Gauge;
            public readonly int GaugeRate;

            public ResultRankParams(Record[] records)
            {
                var staying = records.Where(r => !r.Escaped).ToArray();
                Count = records.Length;
                Sunk = staying.Count(r => r.NowHp == 0);
                Gauge = staying.Sum(r => r.StartHp - r.NowHp);
                GaugeRate = (int)((double)Gauge / records.Sum(r => r.StartHp) * 100);
            }
        }

        /// <summary>
        /// テスト専用
        /// </summary>
        public void InjectResultStatus(ShipStatus[] main, ShipStatus[] guard, ShipStatus[] enemy,
            ShipStatus[] enemyGuard)
        {
            Result = new BattleResult
            {
                Friend = new BattleResult.Combined {Main = main, Guard = guard},
                Enemy = new BattleResult.Combined {Main = enemy, Guard = enemyGuard}
            };
        }
    }
}