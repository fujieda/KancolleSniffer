// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
    public class BattleInfo
    {
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private dynamic _day;
        private dynamic _night;
        private bool _isSurface;
        private readonly List<int> _escapingShips = new List<int>();
        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int EnemyAirSuperiority { get; private set; }
        public bool HasDamagedShip { get; set; }
        public string[] DamagedShipNames { get; private set; }
        public int AirBattleResult { get; private set; }

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
            if (IsNightBattle(json))
            {
                CauseDamage(false);
                _night = json;
            }
            else
            {
                AirBattleResult = CheckAirBattleResult(json);
                _day = json;
            }
        }

        private int CheckAirBattleResult(dynamic json)
        {
            var stage1 = json.api_kouku.api_stage1;
            if (stage1.api_f_count == 0 && stage1.api_e_count == 0)
                return -1;
            return (int)stage1.api_disp_seiku;
        }

        public void InspectPracticeBattle(dynamic json)
        {
            InspectBattle(json);
            HasDamagedShip = false;
        }

        public void InspectCombinedBattle(dynamic json, bool surfaceFleet)
        {
            InBattle = true;
            Formation = FormationName(json);
            EnemyAirSuperiority = CalcEnemyAirSuperiority(json);
            if (IsNightBattle(json))
            {
                CauseDamageCombined(false);
                _night = json;
            }
            else
            {
                AirBattleResult = CheckAirBattleResult(json);
                _isSurface = surfaceFleet;
                _day = json;
            }
        }

        private bool IsNightBattle(dynamic json)
        {
            return json.api_hougeki();
        }

        public void InspectCombinedBattleResult(dynamic json)
        {
            _escapingShips.Clear();
            CauseDamageCombined();
            if ((int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(damaged / 6)[damaged % 6]);
            var escort = (int)json.api_escape.api_tow_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(escort / 6)[escort % 6]);
        }

        public void CauseCombinedBattleEscape()
        {
            _shipInfo.SetEscapedShips(_escapingShips);
            UpdateDamgedShipNames(_shipInfo.GetShipStatuses(0).Concat(_shipInfo.GetShipStatuses(1)));
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

        private int CalcEnemyAirSuperiority(dynamic json)
        {
            var maxEq = ((int[])json.api_ship_ke).Skip(1).SelectMany(id => _shipMaster[id].MaxEq);
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            return (from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                let spec = _itemInfo.GetSpecByItemId(slot.id)
                where spec.CanAirCombat()
                select (int)Math.Floor(spec.AntiAir * Math.Sqrt(slot.max))).DefaultIfEmpty().Sum();
        }

        public void CauseDamage(bool warnDamagedShip = true)
        {
            if (_day == null && _night == null)
                return;
            var ships = _shipInfo.GetShipStatuses((int)DeckId(_day ?? _night));
            if (_day != null)
            {
                var json = _day;
                if (json.api_kouku.api_stage3 != null)
                    CauseSimpleDamage(ships, json.api_kouku.api_stage3.api_fdam);
                if (json.api_kouku2() && json.api_kouku2.api_stage3 != null) // 航空戦2回目
                    CauseSimpleDamage(ships, json.api_kouku2.api_stage3.api_fdam);
                if (json.api_opening_atack()) // 通常戦
                {
                    if (json.api_opening_atack != null)
                        CauseSimpleDamage(ships, json.api_opening_atack.api_fdam);
                    if (json.api_hougeki1 != null)
                        CauseHougekiDamage(ships, json.api_hougeki1);
                    if (json.api_hougeki2 != null)
                        CauseHougekiDamage(ships, json.api_hougeki2);
                    if (json.api_raigeki != null)
                        CauseSimpleDamage(ships, json.api_raigeki.api_fdam);
                }
                _day = null;
            }
            if (_night != null)
            {
                CauseHougekiDamage(ships, _night.api_hougeki);
                _night = null;
            }
            if (warnDamagedShip)
                UpdateDamgedShipNames(ships);
        }

        public void CausePracticeDamage()
        {
            CauseDamage(false);
        }

        private void UpdateDamgedShipNames(IEnumerable<ShipStatus> ships)
        {
            DamagedShipNames =
                (from ship in ships where ship.DamageLevel == ShipStatus.Damage.Badly select ship.Name).ToArray();
            HasDamagedShip = DamagedShipNames.Any();
        }

        private int DeckId(dynamic json)
        {
            if (json.api_dock_id()) // 昼戦はtypoしている
                return (int)json.api_dock_id - 1;
            if (json.api_deck_id is string) // 通常の夜戦では文字列
                return int.Parse(json.api_deck_id) - 1;
            return (int)json.api_deck_id - 1;
        }

        private void CauseSimpleDamage(ShipStatus[] ships, dynamic rawDamage)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < ships.Length; i++)
                ships[i].NowHp -= damage[i + 1];
        }

        private void CauseHougekiDamage(ShipStatus[] ships, dynamic hougeki)
        {
            var targets = ((dynamic[])hougeki.api_df_list).Skip(1).SelectMany(x => (int[])x);
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).SelectMany(x => (double[])x);
            foreach (var hit in targets.Zip(damages, (t, d) => new {t, d}))
            {
                if (1 <= hit.t && hit.t <= ships.Length)
                    ships[hit.t - 1].NowHp -= (int)hit.d;
            }
        }

        public void CauseDamageCombined(bool warnDamagedShip = true)
        {
            if (_isSurface)
                CauseDamageCombinedFleetSurface(warnDamagedShip);
            else
                CauseDamageCombinedFleetAir(warnDamagedShip);
        }

        private void CauseDamageCombinedFleetAir(bool warnDamagedShip)
        {
            var hontai = _shipInfo.GetShipStatuses(0);
            var goei = _shipInfo.GetShipStatuses(1);
            if (_day != null)
            {
                var json = _day;
                var kouku = json.api_kouku;
                if (kouku.api_stage3 != null)
                    CauseSimpleDamage(hontai, kouku.api_stage3.api_fdam);
                if (kouku.api_stage3_combined != null)
                    CauseSimpleDamage(goei, kouku.api_stage3_combined.api_fdam);
                if (json.api_kouku2()) // 航空戦2回目
                {
                    kouku = json.api_kouku2;
                    if (kouku.api_stage3 != null)
                        CauseSimpleDamage(hontai, kouku.api_stage3.api_fdam);
                    if (kouku.api_stage3_combined != null)
                        CauseSimpleDamage(goei, kouku.api_stage3_combined.api_fdam);
                }
                if (json.api_opening_atack()) // 通常戦
                {
                    if (json.api_opening_atack != null)
                        CauseSimpleDamage(goei, json.api_opening_atack.api_fdam);
                    if (json.api_hougeki1 != null)
                        CauseHougekiDamage(goei, json.api_hougeki1);
                    if (json.api_hougeki2() && json.api_hougeki2 != null)
                        CauseHougekiDamage(hontai, json.api_hougeki2);
                    if (json.api_hougeki3() && json.api_hougeki3 != null)
                        CauseHougekiDamage(hontai, json.api_hougeki3);
                    if (json.api_raigeki() && json.api_raigeki != null)
                        CauseSimpleDamage(goei, json.api_raigeki.api_fdam);
                }
                _day = null;
            }
            if (_night != null)
            {
                CauseHougekiDamage(goei, _night.api_hougeki);
                _night = null;
            }
            if (warnDamagedShip)
                UpdateDamgedShipNames(hontai.Concat(goei));
        }

        private void CauseDamageCombinedFleetSurface(bool warnDamagedShip)
        {
            var hontai = _shipInfo.GetShipStatuses(0);
            var goei = _shipInfo.GetShipStatuses(1);
            if (_day != null)
            {
                var json = _day;
                var kouku = json.api_kouku;
                if (kouku.api_stage3 != null)
                    CauseSimpleDamage(hontai, kouku.api_stage3.api_fdam);
                if (kouku.api_stage3_combined != null)
                    CauseSimpleDamage(goei, kouku.api_stage3_combined.api_fdam);
                if (json.api_opening_atack != null)
                    CauseSimpleDamage(goei, json.api_opening_atack.api_fdam);
                if (json.api_hougeki1 != null)
                    CauseHougekiDamage(hontai, json.api_hougeki1);
                if (json.api_hougeki2() && json.api_hougeki2 != null)
                    CauseHougekiDamage(hontai, json.api_hougeki2);
                if (json.api_hougeki3() && json.api_hougeki3 != null)
                    CauseHougekiDamage(goei, json.api_hougeki3);
                if (json.api_raigeki() && json.api_raigeki != null)
                    CauseSimpleDamage(goei, json.api_raigeki.api_fdam);
                _day = null;
            }
            if (_night != null)
            {
                CauseHougekiDamage(goei, _night.api_hougeki);
                _night = null;
            }
            if (warnDamagedShip)
                UpdateDamgedShipNames(hontai.Concat(goei));
        }
    }
}