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
    public class BattleInfo
    {
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private int _fleet;
        private int[] _friendHp;
        private int[] _guardHp;
        private readonly List<int> _escapingShips = new List<int>();

        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int EnemyAirSuperiority { get; private set; }
        public bool HasDamagedShip { get; set; }
        public string[] DamagedShipNames { get; private set; }
        public int AirControlLevel { get; private set; }

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
            if (_friendHp != null)
            {
                ShowResult(false); // 昼戦の結果を夜戦のときに表示する
            }
            else
            {
                _fleet = (int)DeckId(json);
                _friendHp = _shipInfo.GetShipStatuses(_fleet).Select(s => s.NowHp).ToArray();
            }
            if (IsNightBattle(json))
            {
                CalcHougekiDamage(json.api_hougeki, _friendHp);
            }
            else
            {
                AirControlLevel = CheckAirControlLevel(json);
                CalcDamage(json);
            }
        }

        private int DeckId(dynamic json)
        {
            if (json.api_dock_id()) // 昼戦はtypoしている
                return (int)json.api_dock_id - 1;
            if (json.api_deck_id is string) // 通常の夜戦では文字列
                return int.Parse(json.api_deck_id) - 1;
            return (int)json.api_deck_id - 1;
        }

        private bool IsNightBattle(dynamic json)
        {
            return json.api_hougeki();
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

        private int CheckAirControlLevel(dynamic json)
        {
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
                where spec.CanAirCombat()
                select (int)Math.Floor(spec.AntiAir * Math.Sqrt(slot.max))).DefaultIfEmpty().Sum();
        }

        private void CalcDamage(dynamic json)
        {
            if (json.api_kouku.api_stage3 != null)
                CalcSimpleDamage(json.api_kouku.api_stage3.api_fdam, _friendHp);
            if (json.api_kouku2() && json.api_kouku2.api_stage3 != null) // 航空戦2回目
                CalcSimpleDamage(json.api_kouku2.api_stage3.api_fdam, _friendHp);
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack.api_fdam, _friendHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _friendHp);
            if (json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp);
            if (json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki.api_fdam, _friendHp);
        }

        private void CalcSimpleDamage(dynamic rawDamage, int[] result)
        {
            var damage = (int[])rawDamage;
            for (var i = 0; i < _friendHp.Length; i++)
                result[i] -= damage[i + 1];
        }

        private void CalcHougekiDamage(dynamic hougeki, int[] friend)
        {
            var targets = ((dynamic[])hougeki.api_df_list).Skip(1).SelectMany(x => (int[])x);
            var damages = ((dynamic[])hougeki.api_damage).Skip(1).SelectMany(x => (double[])x);
            foreach (var hit in targets.Zip(damages, (t, d) => new {t, d}))
            {
                if (1 <= hit.t && hit.t <= friend.Length)
                    friend[hit.t - 1] -= (int)hit.d;
            }
        }

        public void InspectBattleResult(dynamic json)
        {
            ShowResult();
            _friendHp = null;
        }

        public void InspectPracticeResult(dynamic json)
        {
            ShowResult(false);
            _friendHp = null;
        }

        private void ShowResult(bool warnDamagedShip = true)
        {
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
            if (_friendHp != null)
            {
                ShowResultCombined(false);
            }
            else
            {
                _fleet = 10;
                _friendHp = _shipInfo.GetShipStatuses(0).Select(s => s.NowHp).ToArray();
                _guardHp = _shipInfo.GetShipStatuses(1).Select(s => s.NowHp).ToArray();
            }
            if (IsNightBattle(json))
            {
                CalcHougekiDamage(json.api_hougeki, _guardHp);
            }
            else
            {
                AirControlLevel = CheckAirControlLevel(json);
                if (surfaceFleet)
                    CalcDamageCombinedFleetSurface(json);
                else
                    CalcDamageCombinedFleetAir(json);
            }
        }

        public void InspectCombinedBattleResult(dynamic json)
        {
            _escapingShips.Clear();
            ShowResultCombined();
            _friendHp = null;
            if ((int)json.api_escape_flag == 0)
                return;
            var damaged = (int)json.api_escape.api_escape_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(damaged / 6)[damaged % 6]);
            var escort = (int)json.api_escape.api_tow_idx[0] - 1;
            _escapingShips.Add(_shipInfo.GetDeck(escort / 6)[escort % 6]);
        }

        private void ShowResultCombined(bool warnDamagedShip = true)
        {
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
                CalcSimpleDamage(kouku.api_stage3.api_fdam, _friendHp);
            if (kouku.api_stage3_combined != null)
                CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            if (json.api_kouku2()) // 航空戦2回目
            {
                kouku = json.api_kouku2;
                if (kouku.api_stage3 != null)
                    CalcSimpleDamage(kouku.api_stage3.api_fdam, _friendHp);
                if (kouku.api_stage3_combined != null)
                    CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            }
            if (!json.api_opening_atack()) // 航空戦のみ
                return;
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack.api_fdam, _guardHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _guardHp);
            if (json.api_hougeki2() && json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp);
            if (json.api_hougeki3() && json.api_hougeki3 != null)
                CalcHougekiDamage(json.api_hougeki3, _friendHp);
            if (json.api_raigeki() && json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki.api_fdam, _guardHp);
        }

        private void CalcDamageCombinedFleetSurface(dynamic json)
        {
            var kouku = json.api_kouku;
            if (kouku.api_stage3 != null)
                CalcSimpleDamage(kouku.api_stage3.api_fdam, _friendHp);
            if (kouku.api_stage3_combined != null)
                CalcSimpleDamage(kouku.api_stage3_combined.api_fdam, _guardHp);
            if (json.api_opening_atack != null)
                CalcSimpleDamage(json.api_opening_atack.api_fdam, _guardHp);
            if (json.api_hougeki1 != null)
                CalcHougekiDamage(json.api_hougeki1, _friendHp);
            if (json.api_hougeki2() && json.api_hougeki2 != null)
                CalcHougekiDamage(json.api_hougeki2, _friendHp);
            if (json.api_hougeki3() && json.api_hougeki3 != null)
                CalcHougekiDamage(json.api_hougeki3, _guardHp);
            if (json.api_raigeki() && json.api_raigeki != null)
                CalcSimpleDamage(json.api_raigeki.api_fdam, _guardHp);
        }
    }
}