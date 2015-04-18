// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
        private int[] _friendHp;
        private int[] _friendStartHp;
        private int[] _guardHp;
        private int[] _guardStartHp;
        private int[] _enemyHp;
        private int[] _enemyStartHp;
        private readonly List<int> _escapingShips = new List<int>();

        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int EnemyAirSuperiority { get; private set; }
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
            EnemyAirSuperiority = CalcEnemyAirSuperiority(json);
            AirControlLevel = CheckAirControlLevel(json);
            _fleet = (int)DeckId(json);
            ShowResult(false); // 昼戦の結果を夜戦のときに表示する
            SetupHp(json);
            if (IsNightBattle(json))
                CalcHougekiDamage(json.api_hougeki, _friendHp, _enemyHp);
            else
                CalcDamage(json);
            ClearOverKill(_friendHp);
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

        private void SetupHp(dynamic json)
        {
            if (_friendHp != null)
                return;
            var nowhps = (int[])json.api_nowhps;
            _friendHp = nowhps.Skip(1).Take(6).TakeWhile(hp => hp != -1).ToArray();
            _friendStartHp = (int[])_friendHp.Clone();
            _enemyHp = nowhps.Skip(7).TakeWhile(hp => hp != -1).ToArray();
            _enemyStartHp = (int[])_enemyHp.Clone();
            if (json.api_nowhps_combined())
            {
                _guardHp = ((int[])json.api_nowhps_combined).Skip(1).ToArray();
                _guardStartHp = (int[])_guardHp.Clone();
            }
            else
            {
                _guardHp = _guardStartHp = new int[0];
            }
        }

        private void CleanupHp()
        {
            _friendHp = null;
        }

        private int CheckAirControlLevel(dynamic json)
        {
            if (!json.api_kouku())
                return -1;
            var stage1 = json.api_kouku.api_stage1;
            if (stage1.api_f_count == 0 && stage1.api_e_count == 0)
                return -1;
            return (int)stage1.api_disp_seiku;
        }

        private int CalcEnemyAirSuperiority(dynamic json)
        {
            var maxEq = ((int[])json.api_ship_ke).Skip(1).SelectMany(id => _shipMaster[id].MaxEq);
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            return (from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                let spec = _itemInfo.GetSpecByItemId(slot.id)
                where spec.CanAirCombat
                select (int)Math.Floor(spec.AntiAir * Math.Sqrt(slot.max))).DefaultIfEmpty().Sum();
        }

        private void CalcDamage(dynamic json)
        {
            if (json.api_kouku.api_stage3 != null)
                CalcSimpleDamage(json.api_kouku.api_stage3, _friendHp, _enemyHp);
            if (json.api_kouku2() && json.api_kouku2.api_stage3 != null) // 航空戦2回目
                CalcSimpleDamage(json.api_kouku2.api_stage3, _friendHp, _enemyHp);
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack, _friendHp, _enemyHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _friendHp, _enemyHp);
            if (json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp, _enemyHp);
            if (json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki, _friendHp, _enemyHp);
        }

        private bool IsNightBattle(dynamic json)
        {
            return json.api_hougeki();
        }

        private void CalcSimpleDamage(dynamic json, int[] friend, int[] enemy)
        {
            CalcSimpleDamage(json.api_fdam, friend);
            CalcSimpleDamage(json.api_edam, enemy);
        }

        private void CalcSimpleDamage(dynamic rawDamage, int[] result)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < result.Length; i++)
                result[i] -= damage[i + 1];
        }

        private void CalcHougekiDamage(dynamic hougeki, int[] friend, int[] enemy)
        {
            var targets = ((dynamic[])hougeki.api_df_list).Skip(1).SelectMany(x => (int[])x);
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).SelectMany(x => (double[])x);
            foreach (var hit in targets.Zip(damages, (t, d) => new {t, d}))
            {
                if (hit.t == -1)
                    continue;
                if (hit.t <= 6)
                    friend[hit.t - 1] -= (int)hit.d;
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
            CleanupHp();
        }

        public void InspectPracticeResult(dynamic json)
        {
            ShowResult(false);
            CleanupHp();
        }

        private void ShowResult(bool warnDamagedShip = true)
        {
            if (_friendHp == null)
                return;
            var ships = _shipInfo.GetShipStatuses(_fleet);
            foreach (var e in ships.Zip(_friendHp, (ship, now) => new {ship, now}))
                e.ship.NowHp = e.now;
            if (warnDamagedShip)
                UpdateDamgedShipNames(ships);
        }

        private void UpdateDamgedShipNames(IEnumerable<ShipStatus> ships)
        {
            DamagedShipNames =
                (from ship in ships where ship.DamageLevel == ShipStatus.Damage.Badly select ship.Name).ToArray();
            HasDamagedShip = DamagedShipNames.Any();
        }

        public void InspectCombinedBattle(dynamic json, bool surfaceFleet)
        {
            InBattle = true;
            Formation = FormationName(json);
            EnemyAirSuperiority = CalcEnemyAirSuperiority(json);
            AirControlLevel = CheckAirControlLevel(json);
            _fleet = 10;
            ShowResultCombined(false);
            SetupHp(json);
            if (IsNightBattle(json))
            {
                CalcHougekiDamage(json.api_hougeki, _guardHp, _enemyHp);
            }
            else
            {
                if (surfaceFleet)
                    CalcDamageCombinedFleetSurface(json);
                else
                    CalcDamageCombinedFleetAir(json);
            }
            ClearOverKill(_friendHp);
            ClearOverKill(_guardHp);
            ClearOverKill(_enemyHp);
            ResultRank = CalcResultRank();
        }

        public void InspectCombinedBattleResult(dynamic json)
        {
            _escapingShips.Clear();
            ShowResultCombined();
            CleanupHp();
            if ((int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(damaged / 6)[damaged % 6]);
            var escort = (int)json.api_escape.api_tow_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(escort / 6)[escort % 6]);
        }

        private void ShowResultCombined(bool warnDamagedShip = true)
        {
            if (_friendHp == null)
                return;
            var ships = _shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)).ToArray();
            foreach (var e in ships.Zip(_friendHp.Concat(_guardHp), (ship, now) => new {ship, now}))
                e.ship.NowHp = e.now;
            if (warnDamagedShip)
                UpdateDamgedShipNames(ships);
        }

        public void CauseCombinedBattleEscape()
        {
            _shipInfo.SetEscapedShips(_escapingShips);
            UpdateDamgedShipNames(_shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)));
        }

        private void CalcDamageCombinedFleetAir(dynamic json)
        {
            var kouku = json.api_kouku;
            if (kouku.api_stage3 != null)
                CalcSimpleDamage(kouku.api_stage3, _friendHp, _enemyHp);
            if (kouku.api_stage3_combined != null)
                CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            if (json.api_kouku2()) // 航空戦2回目
            {
                kouku = json.api_kouku2;
                if (kouku.api_stage3 != null)
                    CalcSimpleDamage(kouku.api_stage3, _friendHp, _enemyHp);
                if (kouku.api_stage3_combined != null)
                    CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            }
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack.api_fdam, _guardHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _guardHp, _enemyHp);
            if (json.api_hougeki2() && json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp, _enemyHp);
            if (json.api_hougeki3() && json.api_hougeki3 != null)
                CalcHougekiDamage(json.api_hougeki3, _friendHp, _enemyHp);
            if (json.api_raigeki() && json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki, _guardHp, _enemyHp);
        }

        private void CalcDamageCombinedFleetSurface(dynamic json)
        {
            var kouku = json.api_kouku;
            if (kouku.api_stage3 != null)
                CalcSimpleDamage(kouku.api_stage3, _friendHp, _enemyHp);
            if (kouku.api_stage3_combined != null)
                CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack, _guardHp, _enemyHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _friendHp, _enemyHp);
            if (json.api_hougeki2() && json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp, _enemyHp);
            if (json.api_hougeki3() && json.api_hougeki3 != null)
                CalcHougekiDamage(json.api_hougeki3, _guardHp, _enemyHp);
            if (json.api_raigeki() && json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki, _guardHp, _enemyHp);
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
            var combinedHp = _friendHp.Concat(_guardHp).ToArray();
            var combinedStartHp = _friendStartHp.Concat(_guardStartHp).ToArray();
            // 戦闘後に残っている艦数
            var friendNowShips = combinedHp.Count(hp => hp > 0);
            var enemyNowShips = _enemyHp.Count(hp => hp > 0);
            // 総ダメージ
            var friendGauge = combinedStartHp.Sum() - combinedHp.Sum();
            var enemyGauge = _enemyStartHp.Sum() - _enemyHp.Sum();
            // 轟沈・撃沈数
            var friendSunk = combinedHp.Count(hp => hp == 0);
            var enemySunk = _enemyHp.Count(hp => hp == 0);

            var friendGaugeRate = Math.Floor((double)friendGauge / combinedStartHp.Sum() * 100);
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