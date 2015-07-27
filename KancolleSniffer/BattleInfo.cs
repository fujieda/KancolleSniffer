// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;

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
        E
    }

    public class BattleInfo
    {
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private int _fleet;
        private Record[] _friend;
        private Record[] _guard;
        private int[] _enemyHp;
        private int[] _enemyStartHp;
        private readonly List<int> _escapingShips = new List<int>();

        public const int IncollectFighterPowerFlag = 0x10000;
        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int EnemyFighterPower { get; private set; }
        public bool HasDamagedShip { get; set; }
        public string[] DamagedShipNames { get; private set; }
        public int AirControlLevel { get; private set; }
        public BattleResultRank ResultRank { get; private set; }

        public BattleInfo(ShipMaster shipMaster, ShipInfo shipInfo, ItemInfo itemInfo)
        {
            _shipMaster = shipMaster;
            _shipInfo = shipInfo;
            _itemInfo = itemInfo;
        }

        public void InspectBattle(dynamic json)
        {
            InBattle = true;
            Formation = FormationName(json);
            EnemyFighterPower = CalcEnemyFighterPower(json);
            AirControlLevel = CheckAirControlLevel(json);
            ShowResult(false); // 昼戦の結果を夜戦のときに表示する
            SetupResult(json);
            if (IsNightBattle(json))
                CalcHougekiDamage(json.api_hougeki, _friend, _enemyHp);
            else
                CalcDamage(json);
            ClearOverKill(_enemyHp);
            ResultRank = CalcResultRank();
        }

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
            var combined = json.api_nowhps_combined();
            var nowhps = (int[])json.api_nowhps;
            _fleet = combined ? 0 : DeckId(json);
            _friend = Record.Setup(
                nowhps, (int[])json.api_maxhps,
                _shipInfo.GetShipStatuses(_fleet).Select(s => s.Slot).ToArray(),
                _itemInfo);
            _enemyHp = nowhps.Skip(7).TakeWhile(hp => hp != -1).ToArray();
            _enemyStartHp = (int[])_enemyHp.Clone();
            if (combined)
            {
                _guard = Record.Setup(
                    (int[])json.api_nowhps_combined,
                    (int[])json.api_maxhps_combined,
                    _shipInfo.GetShipStatuses(1).Select(s => s.Slot).ToArray(),
                    _itemInfo);
            }
            else
            {
                _guard = new Record[0];
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

        private int CalcEnemyFighterPower(dynamic json)
        {
            var missing = 0;
            var maxEq = ((int[])json.api_ship_ke).Skip(1).SelectMany(id =>
            {
                var r = _shipMaster[id].MaxEq;
                if (r != null)
                    return r;
                missing = IncollectFighterPowerFlag;
                return new int[5];
            });
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            return (from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                let spec = _itemInfo.GetSpecByItemId(slot.id)
                where spec.CanAirCombat
                select (int)Math.Floor(spec.AntiAir * Math.Sqrt(slot.max))).DefaultIfEmpty().Sum() | missing;
        }

        private void CalcDamage(dynamic json, bool surfaceFleet = false)
        {
            var combined = json.api_nowhps_combined();
            if (json.api_kouku.api_stage3 != null)
                CalcSimpleDamage(json.api_kouku.api_stage3, _friend, _enemyHp);
            if (json.api_kouku.api_stage3_combined() && json.api_kouku.api_stage3_combined != null)
                CalcSimpleDamage(json.api_kouku.api_stage3_combined.api_fdam, _guard);
            if (json.api_kouku2()) // 航空戦2回目
            {
                if (json.api_kouku2.api_stage3 != null)
                    CalcSimpleDamage(json.api_kouku2.api_stage3, _friend, _enemyHp);
                if (json.api_kouku2.api_stage3_combined() && json.api_kouku2.api_stage3_combined != null)
                    CalcSimpleDamage(json.api_kouku2.api_stage3_combined.api_fdam, _guard);
            }
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_support_info() && json.api_support_info != null)
                CalcSupportDamage(json.api_support_info);
            if (json.api_opening_atack != null)
            {
                var friend = combined ? _guard : _friend; // 雷撃の対象は護衛
                CalcSimpleDamage(json.api_opening_atack, friend, _enemyHp);
            }
            if (json.api_hougeki1 != null)
            {
                var friend = combined && !surfaceFleet ? _guard : _friend; // 空母機動部隊は一巡目が護衛
                CalcHougekiDamage(json.api_hougeki1, friend, _enemyHp);
            }
            if (json.api_hougeki2() && json.api_hougeki2 != null)
            {
                CalcHougekiDamage(json.api_hougeki2, _friend, _enemyHp);
            }
            if (json.api_hougeki3() && json.api_hougeki3 != null)
            {
                var friend = combined && surfaceFleet ? _guard : _friend; // 水上打撃部隊は三順目が護衛
                CalcHougekiDamage(json.api_hougeki3, friend, _enemyHp);
            }
            if (json.api_raigeki() && json.api_raigeki != null)
            {
                var friend = combined ? _guard : _friend;
                CalcSimpleDamage(json.api_raigeki, friend, _enemyHp);
            }
        }

        private void CalcSupportDamage(dynamic json)
        {
            if (json.api_support_hourai != null)
                CalcSimpleDamage(json.api_support_hourai.api_damage, _enemyHp);
            else if (json.api_support_airatack != null)
            {
                var stage3 = json.api_support_airatack.api_stage3;
                if (stage3 != null)
                    CalcSimpleDamage(stage3.api_edam, _enemyHp);
            }
        }

        private bool IsNightBattle(dynamic json)
        {
            return json.api_hougeki();
        }

        private void CalcSimpleDamage(dynamic json, Record[] friend, int[] enemy)
        {
            CalcSimpleDamage(json.api_fdam, friend);
            CalcSimpleDamage(json.api_edam, enemy);
        }

        private void CalcSimpleDamage(dynamic rawDamage, Record[] result)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < result.Length; i++)
                result[i].ApplyDamage(damage[i + 1]);
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
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).SelectMany(x => (double[])x);
            foreach (var hit in targets.Zip(damages, (t, d) => new {t, d}))
            {
                if (hit.t == -1)
                    continue;
                if (hit.t <= 6)
                    friend[hit.t - 1].ApplyDamage((int)hit.d);
                else
                    enemy[(hit.t - 1) % 6] -= (int)hit.d;
            }
        }

        private void ClearOverKill(int[] result)
        {
            for (var i = 0; i < result.Length; i++)
                if (result[i] < 0)
                    result[i] = 0;
        }

        public void InspectBattleResult(dynamic json)
        {
            ShowResult();
            CleanupResult();
        }

        public void InspectPracticeResult(dynamic json)
        {
            ShowResult(false);
            CleanupResult();
        }

        private void ShowResult(bool warnDamagedShip = true)
        {
            if (_friend == null)
                return;
            var ships = _shipInfo.GetShipStatuses(_fleet);
            foreach (var e in ships.Zip(_friend, (ship, now) => new {ship, now}))
                e.now.UpdateShipStatus(e.ship);
            if (warnDamagedShip)
                UpdateDamgedShipNames(ships);
        }

        private void UpdateDamgedShipNames(IEnumerable<ShipStatus> ships)
        {
            DamagedShipNames =
                (from s in ships where s.DamageLevel == ShipStatus.Damage.Badly && !s.Escaped select s.Name).ToArray();
            HasDamagedShip = DamagedShipNames.Any();
        }

        public void InspectCombinedBattle(dynamic json, bool surfaceFleet)
        {
            InBattle = true;
            Formation = FormationName(json);
            EnemyFighterPower = CalcEnemyFighterPower(json);
            AirControlLevel = CheckAirControlLevel(json);
            _fleet = 10;
            ShowResultCombined(false);
            SetupResult(json);
            if (IsNightBattle(json))
                CalcHougekiDamage(json.api_hougeki, _guard, _enemyHp);
            else
                CalcDamage(json, surfaceFleet);
            ClearOverKill(_enemyHp);
            ResultRank = CalcResultRank();
        }

        public void InspectCombinedBattleResult(dynamic json)
        {
            _escapingShips.Clear();
            ShowResultCombined();
            CleanupResult();
            if ((int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(damaged / 6)[damaged % 6]);
            var escort = (int)json.api_escape.api_tow_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(escort / 6)[escort % 6]);
        }

        private void ShowResultCombined(bool warnDamagedShip = true)
        {
            if (_friend == null)
                return;
            var ships = _shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)).ToArray();
            foreach (var e in ships.Zip(_friend.Concat(_guard), (ship, now) => new {ship, now}))
                e.now.UpdateShipStatus(e.ship);
            if (warnDamagedShip)
                UpdateDamgedShipNames(ships);
        }

        public void CauseCombinedBattleEscape()
        {
            _shipInfo.SetEscapedShips(_escapingShips);
            UpdateDamgedShipNames(_shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)));
        }

        private class Record
        {
            private ItemInfo _itemInfo;
            private int _maxHp;
            private int[] _slot;
            public int NowHp;
            public int StartHp;
            public int Damage;

            public static Record[] Setup(int[] rawHp, int[] rawMax, int[][] slots, ItemInfo itemInfo)
            {
                var hp = rawHp.Skip(1).Take(6).TakeWhile(h => h != -1).ToArray();
                var max = rawMax.Skip(1).Take(6).TakeWhile(h => h != -1).ToArray();
                var r = new Record[hp.Length];
                for (var i = 0; i < hp.Length; i++)
                {
                    r[i] = new Record
                    {
                        NowHp = hp[i],
                        StartHp = hp[i],
                        _maxHp = max[i],
                        _slot = slots[i].ToArray(),
                        _itemInfo = itemInfo
                    };
                }
                return r;
            }

            public void ApplyDamage(int damage)
            {
                if (NowHp > damage)
                {
                    NowHp -= damage;
                    Damage += damage;
                    return;
                }
                Damage += NowHp;
                NowHp = 0;
                for (var j = 0; j < _slot.Length; j++)
                {
                    var id = _itemInfo[_slot[j]].Id;
                    if (id == 42) // ダメコン
                    {
                        _slot[j] = -1;
                        NowHp = (int)(_maxHp * 0.2);
                        break;
                    }
                    if (id == 43) // 女神
                    {
                        _slot[j] = -1;
                        NowHp = _maxHp;
                        break;
                    }
                }
            }

            public void UpdateShipStatus(ShipStatus ship)
            {
                ship.NowHp = NowHp;
                ship.Slot = _slot;
            }
        }

        // 以下のコードは航海日誌拡張版の以下のファイルのcalcResultRankを移植したもの
        // https://github.com/nekopanda/logbook/blob/94ceca4be6d4ce79a8759d1ee747fb9827c08edc/main/logbook/dto/BattleExDto.java
        //
        // The MIT License (MIT)
        //
        // Copyright (c) 2013-2014 sanae_hirotaka
        //
        // Permission is hereby granted, free of charge, to any person obtaining a copy
        // of this software and associated documentation files (the "Software"), to deal
        // in the Software without restriction, including without limitation the rights
        // to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
        // copies of the Software, and to permit persons to whom the Software is
        // furnished to do so, subject to the following conditions:
        //
        // The above copyright notice and this permission notice shall be included in
        // all copies or substantial portions of the Software.
        //
        // THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
        // IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
        // FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
        // AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
        // LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
        // OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
        // THE SOFTWARE.
        //
        private BattleResultRank CalcResultRank()
        {
            var combined = _friend.Concat(_guard).ToArray();
            // 戦闘後に残っている艦数
            var friendNowShips = combined.Count(r => r.NowHp > 0);
            var enemyNowShips = _enemyHp.Count(hp => hp > 0);
            // 総ダメージ
            var friendGauge = combined.Sum(r => r.Damage);
            var enemyGauge = _enemyStartHp.Sum() - _enemyHp.Sum();
            // 轟沈・撃沈数
            var friendSunk = combined.Count(r => r.NowHp == 0);
            var enemySunk = _enemyHp.Count(hp => hp == 0);

            var friendGaugeRate = Math.Floor((double)friendGauge / combined.Sum(r => r.StartHp) * 100);
            var enemyGaugeRate = Math.Floor((double)enemyGauge / _enemyStartHp.Sum() * 100);
            var equalOrMore = enemyGaugeRate > (0.9 * friendGaugeRate);
            var superior = enemyGaugeRate > 0 && enemyGaugeRate > (2.5 * friendGaugeRate);

            if (friendSunk == 0)
            {
                if (enemyNowShips == 0)
                {
                    if (friendGauge == 0)
                        return BattleResultRank.P;
                    return BattleResultRank.S;
                }
                if (_enemyHp.Length == 6)
                {
                    if (enemySunk >= 4)
                        return BattleResultRank.A;
                }
                else if (enemySunk * 2 >= _enemyHp.Length)
                {
                    return BattleResultRank.A;
                }
                if (_enemyHp[0] == 0)
                    return BattleResultRank.B;
                if (superior)
                    return BattleResultRank.B;
            }
            else
            {
                if (enemyNowShips == 0)
                    return BattleResultRank.B;
                if (_enemyHp[0] == 0 && friendSunk < enemySunk)
                    return BattleResultRank.B;
                if (superior)
                    return BattleResultRank.B;
                if (_enemyHp[0] == 0)
                    return BattleResultRank.C;
            }
            if (enemyGauge > 0 && equalOrMore)
                return BattleResultRank.C;
            if (friendSunk > 0 && friendNowShips == 1)
                return BattleResultRank.E;
            return BattleResultRank.D;
        }
    }
}