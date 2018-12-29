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
using KancolleSniffer.View;
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

    public class BattleInfo
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private Fleet _fleet;
        private Record[] _friend;
        private Record[] _guard;
        private Record[] _enemy;
        private Record[] _enemyGuard;
        private readonly List<int> _escapingShips = new List<int>();
        private bool _lastCell;

        public BattleState BattleState { get; set; }
        public int[] Formation { get; private set; }
        public int[] FighterPower { get; private set; }
        public EnemyFighterPower EnemyFighterPower { get; private set; }
        public int AirControlLevel { get; private set; }
        public BattleResultRank ResultRank { get; private set; }
        public RankPair DisplayedResultRank { get; } = new RankPair();
        public BattleResult Result { get; set; }
        public bool EnemyIsCombined => _enemyGuard.Length > 0;
        public List<AirBattleResult> AirBattleResults { get; } = new List<AirBattleResult>();

        public class RankPair
        {
            public char Assumed { get; set; }
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

        public BattleInfo(ShipInfo shipInfo, ItemInfo itemInfo)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
        }

        public void InspectBattle(string url, string request, dynamic json)
        {
            if (json.api_formation())
                Formation = ((dynamic[])json.api_formation).Select(f => f is string ? (int)int.Parse(f) : (int)f)
                    .ToArray();
            AirControlLevel = CheckAirControlLevel(json);
            ShowResult(false); // 昼戦の結果を夜戦のときに表示する
            SetupResult(request, json, url.Contains("practice"));
            FighterPower = CalcFighterPower();
            EnemyFighterPower = CalcEnemyFighterPower(json);
            BattleState = url.Contains("sp_midnight") ? BattleState.SpNight :
                url.Contains("midnight") ? BattleState.Night : BattleState.Day;
            CalcDamage(json);
            ResultRank = url.Contains("/ld_") ? CalcLdResultRank() : CalcResultRank();
            SetResult();
        }

        public static int DeckId(dynamic json)
        {
            if (json.api_dock_id()) // 昼戦はtypoしている
                return (int)json.api_dock_id - 1;
            if (json.api_deck_id is string) // 通常の夜戦と連合艦隊(味方のみ)では文字列
                return int.Parse(json.api_deck_id) - 1;
            return (int)json.api_deck_id - 1;
        }

        private void SetupResult(string request, dynamic json, bool practice)
        {
            if (_friend != null)
                return;
            _shipInfo.SaveBattleStartStatus();
            var fleets = _shipInfo.Fleets;
            _fleet = fleets[DeckId(json)];
            FlagshipRecovery(request, _fleet.ActualShips[0]);
            _friend = Record.Setup(_fleet.ActualShips, practice);
            _guard = json.api_f_nowhps_combined()
                ? Record.Setup(fleets[1].ActualShips, practice)
                : new Record[0];
            _enemy = Record.Setup((int[])json.api_e_nowhps,
                ((int[])json.api_ship_ke).Select(_shipInfo.GetSpec).ToArray(),
                ((int[][])json.api_eSlot).Select(slot => slot.Select(_itemInfo.GetSpecByItemId).ToArray()).ToArray(),
                practice);
            _enemyGuard = json.api_ship_ke_combined()
                ? Record.Setup((int[])json.api_e_nowhps_combined,
                    ((int[])json.api_ship_ke_combined).Select(_shipInfo.GetSpec).ToArray(),
                    ((int[][])json.api_eSlot).Select(slot => slot.Select(_itemInfo.GetSpecByItemId).ToArray())
                    .ToArray(), practice)
                : new Record[0];
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

        public void CleanupResult()
        {
            _friend = null;
            _lastCell = false;
        }

        private int CheckAirControlLevel(dynamic json)
        {
            if (!json.api_kouku())
                return -1;
            var stage1 = json.api_kouku.api_stage1;
            if (stage1 == null)
                return -1;
            if (stage1.api_f_count == 0 && stage1.api_e_count == 0)
                return -1;
            return (int)stage1.api_disp_seiku;
        }

        private int[] CalcFighterPower()
        {
            var fleets = _shipInfo.Fleets;
            if (_guard.Length > 0 && _enemyGuard.Length > 0)
                return fleets[0].FighterPower.Zip(fleets[1].FighterPower, (a, b) => a + b).ToArray();
            return _fleet.FighterPower;
        }

        private EnemyFighterPower CalcEnemyFighterPower(dynamic json)
        {
            var result = new EnemyFighterPower();
            var ships = (int[])json.api_ship_ke;
            if (json.api_ship_ke_combined() && _guard.Length > 0)
                ships = ships.Concat((int[])json.api_ship_ke_combined).ToArray();
            var maxEq = ships.SelectMany(id =>
            {
                var r = _shipInfo.GetSpec(id).MaxEq;
                if (r != null)
                    return r;
                result.HasUnknown = true;
                return new int[5];
            });
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            if (json.api_eSlot_combined() && _guard.Length > 0)
                equips = equips.Concat(((int[][])json.api_eSlot_combined).SelectMany(x => x));
            foreach (var entry in from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                let spec = _itemInfo.GetSpecByItemId(slot.id)
                let perSlot = (int)Floor(spec.AntiAir * Sqrt(slot.max))
                select new {spec, perSlot})
            {
                if (entry.spec.CanAirCombat)
                    result.AirCombat += entry.perSlot;
                if (entry.spec.IsAircraft)
                    result.Interception += entry.perSlot;
            }
            return result;
        }

        private void CalcDamage(dynamic json)
        {
            AirBattleResults.Clear();
            foreach (KeyValuePair<string, dynamic> kv in json)
            {
                if (kv.Value == null)
                    continue;
                switch (kv.Key)
                {
                    case "api_air_base_injection":
                        AddAirBattleResult(kv.Value, "AB噴式");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_injection_kouku":
                        AddAirBattleResult(kv.Value, "噴式");
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
                        AddAirBattleResult(kv.Value, "航空戦");
                        CalcKoukuDamage(kv.Value);
                        break;
                    case "api_kouku2":
                        AddAirBattleResult(kv.Value, "航空戦2");
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
                AddAirBattleResult(entry, "基地" + i++);
                CalcKoukuDamage(entry);
            }
        }

        private void CalcFriendAttackDamage(dynamic json)
        {
            CalcDamageByTurn(json.api_hougeki, true);
        }

        private void AddAirBattleResult(dynamic json, string phaseName)
        {
            var stage1 = json.api_stage1;
            if (stage1 == null || (stage1.api_f_count == 0 && stage1.api_e_count == 0))
                return;
            var result = new AirBattleResult
            {
                PhaseName = phaseName,
                AirControlLevel = json.api_stage1.api_disp_seiku() ? (int)json.api_stage1.api_disp_seiku : 0,
                Stage1 = new AirBattleResult.StageResult
                {
                    FriendCount = (int)json.api_stage1.api_f_count,
                    FriendLost = (int)json.api_stage1.api_f_lostcount,
                    EnemyCount = (int)json.api_stage1.api_e_count,
                    EnemyLost = (int)json.api_stage1.api_e_lostcount
                },
                Stage2 = json.api_stage2 == null
                    ? new AirBattleResult.StageResult
                    {
                        FriendCount = 0,
                        FriendLost = 0,
                        EnemyCount = 0,
                        EnemyLost = 0
                    }
                    : new AirBattleResult.StageResult
                    {
                        FriendCount = (int)json.api_stage2.api_f_count,
                        FriendLost = (int)json.api_stage2.api_f_lostcount,
                        EnemyCount = (int)json.api_stage2.api_e_count,
                        EnemyLost = (int)json.api_stage2.api_e_lostcount
                    }
            };
            if (json.api_stage2 != null && json.api_stage2.api_air_fire())
            {
                var airFire = json.api_stage2.api_air_fire;
                var idx = (int)airFire.api_idx;
                result.AirFire = new AirBattleResult.AirFireResult
                {
                    ShipName = idx < _friend.Length ? _friend[idx].Name : _guard[idx - 6].Name,
                    Kind = (int)airFire.api_kind,
                    Items = ((int[])airFire.api_use_items).Select(id => _itemInfo.GetSpecByItemId(id).Name).ToArray()
                };
            }
            AirBattleResults.Add(result);
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
                guard[i].ApplyDamage(damage[i + 6]);
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
            var records = new[] {new Record[12], new Record[12]};
            Array.Copy(_friend, records[1], _friend.Length);
            Array.Copy(_guard, 0, records[1], 6, _guard.Length);
            Array.Copy(_enemy, records[0], _enemy.Length);
            Array.Copy(_enemyGuard, 0, records[0], 6, _enemyGuard.Length);
            for (var turn = 0; turn < eFlags.Length; turn++)
            {
                if (ignoreFriendDamage && eFlags[turn] == 1)
                    continue;
                if (types[turn] == 100 || types[turn] == 101) // Nelson Touchと長門一斉射
                    records[eFlags[turn] ^ 1][sources[turn]].TriggerSpecialAttack();
                for (var shot = 0; shot < targets[turn].Length; shot++)
                {
                    var target = targets[turn][shot];
                    var damage = damages[turn][shot];
                    if (target == -1 || damage == -1)
                        continue;
                    records[eFlags[turn]][target].ApplyDamage(damage);
                }
                foreach (var ship in records[1])
                    ship?.CheckDamageControl();
            }
        }

        public void InspectMapStart(dynamic json)
        {
            InspectMapNext(json);
        }

        public void InspectMapNext(dynamic json)
        {
            _lastCell = (int)json.api_next == 0;
        }

        public void InspectBattleResult(dynamic json)
        {
            BattleState = BattleState.Result;
            if (_friend == null)
                return;
            ShowResult(!_lastCell);
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
            ShowResult(false);
            VerifyResultRank(json);
            CleanupResult();
        }

        private void ShowResult(bool warnDamagedShip = true)
        {
            if (_friend == null)
                return;
            var fleets = _shipInfo.Fleets;
            var ships = _guard.Length > 0
                ? fleets[0].ActualShips.Concat(fleets[1].ActualShips)
                : _fleet.ActualShips;
            foreach (var entry in ships.Zip(_friend.Concat(_guard), (ship, now) => new {ship, now}))
                entry.now.UpdateShipStatus(entry.ship);
            if (warnDamagedShip)
                _shipInfo.SetBadlyDamagedShips();
            else
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
                select new Record {_status = (ShipStatus)s.Clone(), _practice = practice, StartHp = s.NowHp}).ToArray();

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
            var combined = _friend.Concat(_guard).Where(r => !r.Escaped).ToArray();
            var friendGauge = combined.Sum(r => r.StartHp - r.NowHp);
            var friendGaugeRate = Floor((double)friendGauge / combined.Sum(r => r.StartHp) * 100);

            if (friendGauge <= 0)
                return BattleResultRank.P;
            if (friendGaugeRate < 10)
                return BattleResultRank.A;
            if (friendGaugeRate < 20)
                return BattleResultRank.B;
            if (friendGaugeRate < 50)
                return BattleResultRank.C;
            if (friendGaugeRate < 80)
                return BattleResultRank.D;
            return BattleResultRank.E;
        }

        private BattleResultRank CalcResultRank()
        {
            var friend = _friend.Concat(_guard).ToArray();
            var enemy = _enemy.Concat(_enemyGuard).ToArray();

            var friendCount = friend.Length;
            var friendStartHpTotal = 0;
            var friendNowHpTotal = 0;
            var friendSunk = 0;
            foreach (var ship in friend)
            {
                if (ship.Escaped)
                    continue;
                friendStartHpTotal += ship.StartHp;
                friendNowHpTotal += ship.NowHp;
                if (ship.NowHp == 0)
                    friendSunk++;
            }
            var friendGaugeRate = (int)((double)(friendStartHpTotal - friendNowHpTotal) / friendStartHpTotal * 100);

            var enemyCount = enemy.Length;
            var enemyStartHpTotal = enemy.Sum(r => r.StartHp);
            var enemyNowHpTotal = enemy.Sum(r => r.NowHp);
            var enemySunk = enemy.Count(r => r.NowHp == 0);
            var enemyGaugeRate = (int)((double)(enemyStartHpTotal - enemyNowHpTotal) / enemyStartHpTotal * 100);

            if (friendSunk == 0 && enemySunk == enemyCount)
            {
                if (friendNowHpTotal >= friendStartHpTotal)
                    return BattleResultRank.P;
                return BattleResultRank.S;
            }
            if (friendSunk == 0 && enemySunk >= (int)(enemyCount * 0.7) && enemyCount > 1)
                return BattleResultRank.A;
            if (friendSunk < enemySunk && enemy[0].NowHp == 0)
                return BattleResultRank.B;
            if (friendCount == 1 && friend[0].DamageLevel == ShipStatus.Damage.Badly)
                return BattleResultRank.D;
            if (enemyGaugeRate > friendGaugeRate * 2.5)
                return BattleResultRank.B;
            if (enemyGaugeRate > friendGaugeRate * 0.9)
                return BattleResultRank.C;
            if (friendCount > 1 && friendCount - 1 == friendSunk)
                return BattleResultRank.E;
            return BattleResultRank.D;
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