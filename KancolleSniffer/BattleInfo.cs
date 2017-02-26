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

using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace KancolleSniffer
{
    public enum BattleResultRank
    {
        P,
        S,
        A,
        B,
        C,
        D,
        E,
    }

    public enum BattleState
    {
        None,
        Day,
        Night,
        Result
    }

    public class BattleInfo
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private int _fleet;
        private Record[] _friend;
        private Record[] _guard;
        private int[] _enemyHp;
        private int[] _enemyGuardHp;
        private int[] _enemyStartHp;
        private int[] _enemyGuardStartHp;
        private readonly List<int> _escapingShips = new List<int>();
        private int _flagshipRecoveryType;

        public BattleState BattleState { get; set; }
        public string Formation { get; private set; }
        public string EnemyFighterPower { get; private set; }
        public int AirControlLevel { get; private set; }
        public BattleResultRank ResultRank { get; private set; }
        public ShipStatus[] EnemyResultStatus { get; private set; }
        public List<AirBattleResult> AirBattleResults { get; } = new List<AirBattleResult>();


        public BattleInfo(ShipInfo shipInfo, ItemInfo itemInfo)
        {
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
        }

        public void InspectBattle(dynamic json, string url)
        {
            Formation = FormationName(json);
            EnemyFighterPower = CalcEnemyFighterPower(json);
            AirControlLevel = CheckAirControlLevel(json);
            ShowResult(false); // 昼戦の結果を夜戦のときに表示する
            SetupResult(json);
            if (IsNightBattle(json))
            {
                BattleState = BattleState.Night;
                CalcHougekiDamage(json.api_hougeki,
                    _guard.Length > 0 ? _guard : _friend,
                    json.api_active_deck() && json.api_active_deck[1] != 1 ? _enemyGuardHp : _enemyHp);
            }
            else
            {
                BattleState = BattleState.Day;
                CalcDamage(json, url.EndsWith("battle_water"));
            }
            ClearEnemyOverKill();
            ResultRank = url.EndsWith("ld_airbattle") ? CalcLdAirBattleRank() : CalcResultRank();
        }

        private void ClearEnemyOverKill()
        {
            _enemyHp = _enemyHp.Select(hp => hp < 0 ? 0 : hp).ToArray();
            _enemyGuardHp = _enemyGuardHp.Select(hp => hp < 0 ? 0 : hp).ToArray();
        }

        public void InspectMapNext(string request)
        {
            var type = HttpUtility.ParseQueryString(request)["api_recovery_type"];
            if (type == null)
                return;
            _flagshipRecoveryType = int.Parse(type);
        }

        private bool IsNightBattle(dynamic json) => json.api_hougeki();

        private int DeckId(dynamic json)
        {
            if (json.api_dock_id()) // 昼戦はtypoしている
                return (int)json.api_dock_id - 1;
            if (json.api_deck_id is string) // 通常の夜戦では文字列
                return int.Parse(json.api_deck_id) - 1;
            return (int)json.api_deck_id - 1;
        }

        private string FormationName(dynamic json)
        {
            if (!json.api_formation()) // 演習の夜戦
                return "";
            switch ((int)json.api_formation[2])
            {
                case 1:
                    return "同航戦";
                case 2:
                    return "反航戦";
                case 3:
                    return "T字有利";
                case 4:
                    return "T字不利";
            }
            return "";
        }

        private void SetupResult(dynamic json)
        {
            if (_friend != null)
                return;
            var nowhps = (int[])json.api_nowhps;
            _fleet = DeckId(json);
            var fstats = _shipInfo.GetShipStatuses(_fleet);
            FlagshipRecovery(fstats[0]);
            _friend = Record.Setup(fstats);
            _enemyHp = nowhps.Skip(7).TakeWhile(hp => hp != -1).ToArray();
            _enemyStartHp = (int[])_enemyHp.Clone();
            EnemyResultStatus =
            (from id in (int[])json.api_ship_ke
                where id != -1
                select new ShipStatus {Id = id, Spec = _shipInfo.GetSpec(id)}).ToArray();
            _guard = new Record[0];
            _enemyGuardHp = new int[0];
            _enemyGuardStartHp = new int[0];
            if (!json.api_nowhps_combined())
                return;
            var combined = (int[])json.api_nowhps_combined;
            if (combined[1] != -1) // 味方が連合艦隊
                _guard = Record.Setup(_shipInfo.GetShipStatuses(1));
            if (combined.Length > 7) // 敵が連合艦隊
            {
                _enemyGuardHp =
                    ((int[])json.api_nowhps_combined).
                        Skip(7).TakeWhile(hp => hp != -1).ToArray();
                _enemyGuardStartHp = (int[])_enemyGuardHp.Clone();
            }
        }

        private void FlagshipRecovery(ShipStatus flagship)
        {
            switch (_flagshipRecoveryType)
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
            if (_flagshipRecoveryType != 0)
                _shipInfo.SetBadlyDamagedShips();
            _flagshipRecoveryType = 0;
        }

        private static void ConsumeSlotItem(ShipStatus ship, int id)
        {
            if (ship.SlotEx.Spec.Id == id)
            {
                ship.SlotEx = new ItemStatus();
                return;
            }
            for (var i = 0; i < ship.Slot.Length; i++)
            {
                if (ship.Slot[i].Spec.Id == id)
                {
                    ship.Slot[i] = new ItemStatus();
                    break;
                }
            }
        }

        public void CleanupResult()
        {
            _friend = null;
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

        private string CalcEnemyFighterPower(dynamic json)
        {
            var missing = "";
            var maxEq = ((int[])json.api_ship_ke).Skip(1).SelectMany(id =>
            {
                var r = _shipInfo.GetSpec(id).MaxEq;
                if (r != null)
                    return r;
                missing = "+";
                return new int[5];
            }).ToArray();
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            return (from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                       let spec = _itemInfo.GetSpecByItemId(slot.id)
                       where spec.CanAirCombat
                       select (int)Floor(spec.AntiAir * Sqrt(slot.max))).DefaultIfEmpty().Sum() + missing;
        }

        private void CalcDamage(dynamic json, bool surfaceFleet = false)
        {
            AirBattleResults.Clear();
            var fc = _guard.Length > 0;
            var ec = _enemyGuardHp.Length > 0;
            var both = fc && ec;
            if (json.api_air_base_injection())
            {
                AddAirBattleResult(json.api_air_base_injection, "AB噴式");
                CalcKoukuDamage(json.api_air_base_injection);
            }
            if (json.api_injection_kouku())
            {
                AddAirBattleResult(json.api_injection_kouku, "噴式");
                CalcKoukuDamage(json.api_injection_kouku);
            }
            if (json.api_air_base_attack())
                CalcAirBaseAttackDamage(json.api_air_base_attack);
            if (json.api_kouku())
            {
                AddAirBattleResult(json.api_kouku, "航空戦");
                CalcKoukuDamage(json.api_kouku);
            }
            if (json.api_kouku2()) // 航空戦2回目
            {
                AddAirBattleResult(json.api_kouku2, "航空戦2");
                CalcKoukuDamage(json.api_kouku2);
            }
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_support_info() && json.api_support_info != null)
                CalcSupportDamage(json.api_support_info);
            if (json.api_opening_taisen() && json.api_opening_taisen != null)
            {
                if (json.api_opening_taisen.api_at_eflag())
                {
                    CalcCombinedHougekiDamage(json.api_opening_taisen, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcHougekiDamage(json.api_opening_taisen,
                        fc ? _guard : _friend, // 先制対潜攻撃の対象は護衛
                        _enemyHp);
                }
            }
            if (json.api_opening_atack != null)
            {
                if (both)
                {
                    CalcSimpleDamage(json.api_opening_atack, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcSimpleDamage(json.api_opening_atack,
                        fc ? _guard : _friend, // 雷撃の対象は護衛
                        _enemyHp, _enemyGuardHp);
                }
            }
            if (json.api_hougeki1() && json.api_hougeki1 != null)
            {
                if (json.api_hougeki1.api_at_eflag())
                {
                    CalcCombinedHougekiDamage(json.api_hougeki1, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcHougekiDamage(json.api_hougeki1,
                        fc && !surfaceFleet ? _guard : _friend, // 空母機動部隊は一巡目が護衛
                        ec ? _enemyGuardHp : _enemyHp); // 敵連合艦隊は一巡目が護衛
                }
            }
            if (json.api_hougeki2() && json.api_hougeki2 != null)
            {
                if (json.api_hougeki2.api_at_eflag())
                {
                    CalcCombinedHougekiDamage(json.api_hougeki2, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcHougekiDamage(json.api_hougeki2, _friend, _enemyHp);
                }
            }
            if (json.api_hougeki3() && json.api_hougeki3 != null)
            {
                if (json.api_hougeki3.api_at_eflag())
                {
                    CalcCombinedHougekiDamage(json.api_hougeki3, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcHougekiDamage(json.api_hougeki3,
                        fc && surfaceFleet ? _guard : _friend, // 水上打撃部隊は三順目が護衛
                        _enemyHp);
                }
            }
            if (json.api_raigeki() && json.api_raigeki != null)
            {
                if (both)
                {
                    CalcSimpleDamage(json.api_raigeki, _friend, _guard, _enemyHp, _enemyGuardHp);
                }
                else
                {
                    CalcSimpleDamage(json.api_raigeki,
                        fc ? _guard : _friend, // 雷撃の対象は護衛
                        _enemyHp, _enemyGuardHp);
                }
            }
        }

        private void CalcSupportDamage(dynamic json)
        {
            if (json.api_support_hourai != null)
            {
                CalcSimpleDamage(json.api_support_hourai.api_damage, _enemyHp, _enemyGuardHp);
            }
            else if (json.api_support_airatack != null)
            {
                CalcSimpleDamage(json.api_support_airatack.api_stage3.api_edam, _enemyHp, _enemyGuardHp);
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

        private void AddAirBattleResult(dynamic json, string phaseName)
        {
            var stage1 = json.api_stage1;
            if (stage1 == null || (stage1.api_f_count == 0 && stage1.api_e_count == 0))
                return;
            AirBattleResults.Add(new AirBattleResult
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
            });
        }

        private void CalcKoukuDamage(dynamic json)
        {
            if (!json.api_stage3() || json.api_stage3 == null)
                return;
            CalcSimpleDamage(json.api_stage3, _friend, _enemyHp);
            if (json.api_stage3_combined())
                CalcSimpleDamage(json.api_stage3_combined, _guard, _enemyGuardHp);
        }

        private void CalcSimpleDamage(dynamic json, Record[] friend, int[] enemy)
        {
            if (json.api_fdam())
                CalcSimpleDamage(json.api_fdam, friend);
            if (json.api_edam())
                CalcSimpleDamage(json.api_edam, enemy);
        }

        private void CalcSimpleDamage(dynamic json, Record[] friend, int[] enemy, int[] enemyGuard)
        {
            CalcSimpleDamage(json.api_fdam, friend);
            CalcSimpleDamage(json.api_edam, enemy, enemyGuard);
        }

        private void CalcSimpleDamage(dynamic json, Record[] friend, Record[] guard, int[] enemy, int[] enemyGuard)
        {
            CalcSimpleDamage(json.api_fdam, friend, guard);
            CalcSimpleDamage(json.api_edam, enemy, enemyGuard);
        }

        private void CalcSimpleDamage(dynamic rawDamage, Record[] friend, Record[] guard)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < friend.Length; i++)
                friend[i].ApplyDamage(damage[i + 1]);
            for (var i = 0; i < guard.Length; i++)
                guard[i].ApplyDamage(damage[i + 6 + 1]);
        }

        private void CalcSimpleDamage(dynamic rawDamage, Record[] friend)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < friend.Length; i++)
                friend[i].ApplyDamage(damage[i + 1]);
        }

        private void CalcSimpleDamage(dynamic rawDamage, int[] enemy, int[] enemyGuard)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < enemy.Length; i++)
                enemy[i] -= damage[i + 1];
            for (var i = 0; i < enemyGuard.Length; i++)
                enemyGuard[i] -= damage[i + 6 + 1];
        }

        private void CalcSimpleDamage(dynamic rawDamage, int[] result)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < result.Length; i++)
                result[i] -= damage[i + 1];
        }

        private void CalcHougekiDamage(dynamic hougeki, Record[] friend, int[] enemy)
        {
            var targets = ((dynamic[])hougeki.api_df_list).Skip(1).SelectMany(x => (int[])x);
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).SelectMany(x => (int[])x);
            foreach (var hit in targets.Zip(damages, (t, d) => new {t, d}))
            {
                if (hit.t == -1)
                    continue;
                if (hit.t <= 6)
                    friend[hit.t - 1].ApplyDamage(hit.d);
                else
                    enemy[(hit.t - 1) % 6] -= hit.d;
            }
        }

        private void CalcCombinedHougekiDamage(dynamic hougeki, Record[] friend, Record[] guard,
            int[] enemy, int[] enemyGuard)
        {
            var targets = ((dynamic[])hougeki.api_df_list).Skip(1).Select(x => (int[])x);
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).Select(x => (int[])x);
            var eflags = ((int[])hougeki.api_at_eflag).Skip(1);
            foreach (var turn in
                targets.Zip(damages, (t, d) => new {t, d}).
                    Zip(eflags, (td, e) => new {e, td.t, td.d}))
            {
                foreach (var hit in turn.t.Zip(turn.d, (t, d) => new {t, d}))
                {
                    if (turn.e == 1)
                    {
                        if (hit.t <= 6)
                            friend[hit.t - 1].ApplyDamage(hit.d);
                        else
                            guard[(hit.t - 1) % 6].ApplyDamage(hit.d);
                    }
                    else
                    {
                        if (hit.t <= 6)
                            enemy[hit.t - 1] -= hit.d;
                        else
                            enemyGuard[(hit.t - 1) % 6] -= hit.d;
                    }
                }
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            BattleState = BattleState.Result;
            ShowResult();
            CleanupResult();
            SetEscapeShips(json);
        }

        public void InspectPracticeResult(dynamic json)
        {
            BattleState = BattleState.Result;
            ShowResult(false);
            CleanupResult();
        }

        private void ShowResult(bool warnDamagedShip = true)
        {
            if (_friend == null)
                return;
            var ships = _guard.Length > 0
                ? _shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)).ToArray()
                : _shipInfo.GetShipStatuses(_fleet);
            foreach (var entry in ships.Zip(_friend.Concat(_guard), (ship, now) => new {ship, now}))
                entry.now.UpdateShipStatus(entry.ship);
            if (warnDamagedShip)
                _shipInfo.SetBadlyDamagedShips();
            else
                _shipInfo.ClearBadlyDamagedShips();
            SetEnemyResultStatus();
        }

        private void SetEnemyResultStatus()
        {
            for (var i = 0; i < EnemyResultStatus.Length; i++)
            {
                EnemyResultStatus[i].MaxHp = _enemyStartHp[i];
                EnemyResultStatus[i].NowHp = _enemyHp[i];
            }
        }

        public void SetEscapeShips(dynamic json)
        {
            _escapingShips.Clear();
            if (!json.api_escape_flag() || (int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(damaged / 6)[damaged % 6]);
            var escort = (int)json.api_escape.api_tow_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(escort / 6)[escort % 6]);
        }

        public void CauseCombinedBattleEscape()
        {
            _shipInfo.SetEscapedShips(_escapingShips);
            _shipInfo.SetBadlyDamagedShips();
        }

        private class Record
        {
            private ShipStatus _status;
            public int NowHp => _status.NowHp;
            public bool Escaped => _status.Escaped;
            public ShipStatus.Damage DamageLevel => _status.DamageLevel;
            public int StartHp;

            public static Record[] Setup(ShipStatus[] ships) =>
                (from s in ships select new Record {_status = (ShipStatus)s.Clone(), StartHp = s.NowHp}).ToArray();

            public void ApplyDamage(int damage)
            {
                if (_status.NowHp > damage)
                {
                    _status.NowHp -= damage;
                    return;
                }
                _status.NowHp = 0;
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
            }
        }

        private BattleResultRank CalcLdAirBattleRank()
        {
            var combined = _friend.Concat(_guard).ToArray();
            var friendNowShips = combined.Count(r => r.NowHp > 0);
            var friendGauge = combined.Sum(r => r.StartHp - r.NowHp);
            var friendSunk = combined.Count(r => r.NowHp == 0);
            var friendGaugeRate = Floor((double)friendGauge / combined.Sum(r => r.StartHp) * 100);

            if (friendSunk == 0)
            {
                if (friendGauge == 0)
                    return BattleResultRank.P;
                if (friendGaugeRate < 10)
                    return BattleResultRank.A;
                if (friendGaugeRate < 20)
                    return BattleResultRank.B;
                if (friendGaugeRate < 50)
                    return BattleResultRank.C;
                return BattleResultRank.D;
            }
            if (friendSunk < friendNowShips)
                return BattleResultRank.D;
            return BattleResultRank.E;
        }

        private BattleResultRank CalcResultRank()
        {
            var friend = _friend.Concat(_guard).ToArray();
            var enemyHp = _enemyHp.Concat(_enemyGuardHp).ToArray();
            var enemyStartHp = _enemyStartHp.Concat(_enemyGuardStartHp).ToArray();

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

            var enemyCount = enemyHp.Length;
            var enemyStartHpTotal = enemyStartHp.Sum();
            var enemyNowHpTotal = enemyHp.Sum();
            var enemySunk = enemyHp.Count(hp => hp == 0);
            var enemyGaugeRate = (int)((double)(enemyStartHpTotal - enemyNowHpTotal) / enemyStartHpTotal * 100);

            if (friendSunk == 0 && enemySunk == enemyCount)
            {
                if (friendNowHpTotal >= friendStartHpTotal)
                    return BattleResultRank.P;
                return BattleResultRank.S;
            }
            if (friendSunk == 0 && enemySunk >= (int)(enemyCount * 0.7) && enemyCount > 1)
                return BattleResultRank.A;
            if (friendSunk < enemySunk && enemyHp[0] == 0)
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
    }
}