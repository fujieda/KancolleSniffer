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
using System.Linq;

namespace KancolleSniffer
{
    public class BattleInfo
    {
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int DelayInFormation { get; private set; }
        public int EnemyAirSuperiority { get; private set; }
        public int DelayInAirSuperiority { get; private set; }
        public bool HasDamagedShip { get; set; }
        public string[] DamagedShipNames { get; private set; }

        private struct Delay
        {
            public const int Basic = 4100;
            public const int Formation = 1100;
            public const int Tau = 200;
            public const int SearchAirSuccess = 5700;
            public const int SearchAirFailure = 4900;
            public const int SearchSuccess = 5400;
            public const int Submarine = 2500;
            public const int Emergence = 2000;
            public const int AirFight = 8700;
            public const int AirFightBoth = 2700;
            public const int Support = 9800;
            public const int OpeningAttack = 3500;
            public const int Cutin = 4500;
        }

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
            SetDelay(json);
            CauseDamage(json);
        }

        public void InspectCombinedBattle(dynamic json)
        {
            InBattle = true;
            Formation = FormationName(json);
            EnemyAirSuperiority = CalcEnemyAirSuperiority(json);
            DelayInFormation = DelayInAirSuperiority = Delay.Basic;
            CauseDamageCombined(json);
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

        private void SetDelay(dynamic json)
        {
            // 敵出現まで
            var delay = Delay.Basic;
            if (json.api_hougeki()) // 夜戦
            {
                DelayInAirSuperiority = DelayInFormation = delay;
                return;
            }
            var subm = (SubmarineFlags)CheckSubmarine(json);
            bool success;
            delay += SearchDelay(json, out success) + subm.AddtionalDelay;
            DelayInAirSuperiority = delay + (success ? 0 : Delay.Emergence); // 失敗すると出現が遅れる
            // 敵艦隊発見以降
            delay += Delay.Emergence + Delay.Formation + SupportDelay(json) + CutinDelay(json);
            if ((int)json.api_formation[2] >= 3)
                delay += Delay.Tau;
            if (!subm.PreventAirFight)
                delay += AirFightDelay(json);
            if (!subm.PreventOpeningAttack)
                delay += OpeningAttackDelay(json);
            DelayInFormation = delay;
        }

        private int SearchDelay(dynamic json, out bool success)
        {
            success = false;
            switch ((int)json.api_search[0])
            {
                case 1: // 索敵機による索敵成功
                case 2: // 索敵機未帰還あり
                    success = true;
                    return Delay.SearchAirSuccess;
                case 3: // 索敵機未帰還
                case 4: // 索敵機による索敵失敗
                    return Delay.SearchAirFailure;
                case 5: // 索敵力による索敵成功
                    success = true;
                    return Delay.SearchSuccess;
            }
            return 0;
        }

        private int OpeningAttackDelay(dynamic json)
        {
            return json.api_opening_flag == 1 ? Delay.OpeningAttack : 0;
        }

        private class SubmarineFlags
        {
            public bool[] Friend;
            public bool[] Enemy;

            public int AddtionalDelay
            {
                get { return Friend.Any(x => x) || Enemy.Any(x => x) ? Delay.Submarine : 0; }
                // どちらかに潜水艦                
            }

            public bool PreventAirFight
            {
                get { return Friend.All(x => x) || Enemy.All(x => x); } // 一方がすべて潜水艦
            }

            public bool PreventOpeningAttack
            {
                get { return Friend.All(x => x) && Enemy.All(x => x); } // 双方すべて潜水艦
            }
        }

        private SubmarineFlags CheckSubmarine(dynamic json)
        {
            return new SubmarineFlags
            {
                Friend = (from status in _shipInfo.GetShipStatuses((int)DeckId(json))
                    select _shipMaster[status.ShipId].IsSubmarine).ToArray(),
                Enemy = (from id in (int[])json.api_ship_ke where id != -1 select _shipMaster[id].IsSubmarine).ToArray()
            };
        }

        private int AirFightDelay(dynamic json)
        {
            // 僚艦が偵察機だけだと航空戦に参加しないので、
            // 艦載機の配備ではなく遭遇戦の状況を調べる。
            var f = json.api_kouku.api_stage1.api_f_count > 0;
            var e = json.api_kouku.api_stage1.api_e_count > 0;
            var result = 0;
            if (f || e) // 艦載機発艦
                result += Delay.AirFight;
            if (f && e) // 双方とも
                result += Delay.AirFightBoth;
            return result;
        }

        private int SupportDelay(dynamic json)
        {
            return json.api_support_flag() && json.api_support_flag == 1 ? Delay.Support : 0;
        }

        private int CutinDelay(dynamic json)
        {
            var cutin = 0;
            var maxHps = (int[])json.api_nowhps;
            var nowHps = (int[])json.api_nowhps;
            var planeFrom = (int[][])json.api_kouku.api_plane_from;
            if ((planeFrom[0][0] != -1 || planeFrom[1][0] != -1) && // どちらかに艦載機あり
                json.api_kouku.api_stage3 != null) // 敵艦載機が全滅しているとnull
            {
                // 航空戦による中破大破の判定
                var damages = (int[])json.api_kouku.api_stage3.api_fdam;
                var newHps = nowHps.Zip(damages, (hp, dmg) => hp - dmg).ToArray();
                if (IsCutinShown(nowHps, newHps, maxHps))
                    cutin++;
                nowHps = newHps;
            }
            if ((int)json.api_opening_flag != 0)
            {
                // 開幕雷撃による中破大破の判定
                var damages = (int[])json.api_opening_atack.api_fdam;
                var newHps = nowHps.Zip(damages, (hp, dmg) => hp - dmg).ToArray();
                if (IsCutinShown(nowHps, newHps, maxHps))
                    cutin++;
            }
            return Delay.Cutin * cutin;
        }

        private bool IsCutinShown(int[] nowHps, int[] newHps, int[] maxHps)
        {
            for (var i = 1; i < ShipInfo.MemberCount + 1; i++)
            {
                if (ShipStatus.CalcDamage(nowHps[i], maxHps[i]) <= ShipStatus.Damage.Small &&
                    ShipStatus.CalcDamage(newHps[i], maxHps[i]) >= ShipStatus.Damage.Half)
                    return true;
            }
            return false;
        }

        private int CalcEnemyAirSuperiority(dynamic json)
        {
            var maxEq = ((int[])json.api_ship_ke).Skip(1).SelectMany(id => _shipMaster[id].MaxEq);
            var equips = ((int[][])json.api_eSlot).SelectMany(x => x);
            return (from slot in equips.Zip(maxEq, (id, max) => new {id, max})
                select (int)Math.Floor(_itemInfo.GetSpecByItemId(slot.id).TyKu * Math.Sqrt(slot.max))).Sum();
        }

        private void CauseDamage(dynamic json)
        {
            var ships = _shipInfo.GetShipStatuses((int)DeckId(json));
            if (json.api_hougeki()) // 夜戦
            {
                CauseHougekiDamage(ships, json.api_hougeki);
            }
            else // 昼戦
            {
                if (json.api_kouku.api_stage3 != null)
                    CauseSimpleDamage(ships, json.api_kouku.api_stage3.api_fdam);
                if (json.api_opening_atack != null)
                    CauseSimpleDamage(ships, json.api_opening_atack.api_fdam);
                if (json.api_hougeki1 != null)
                    CauseHougekiDamage(ships, json.api_hougeki1);
                if (json.api_hougeki2 != null)
                    CauseHougekiDamage(ships, json.api_hougeki2);
                if (json.api_raigeki != null)
                    CauseSimpleDamage(ships, json.api_raigeki.api_fdam);
            }
            DamagedShipNames =
                (from ship in ships where ship.DamageLevel == ShipStatus.Damage.Badly select ship.Name).ToArray();
            HasDamagedShip = DamagedShipNames.Any();
        }

        private int DeckId(dynamic json)
        {
            return (int)(json.api_dock_id() ? json.api_dock_id : json.api_deck_id) - 1; // 昼戦はtypoをしている
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
                if (hit.t - 1 < ships.Length)
                    ships[hit.t - 1].NowHp -= (int)hit.d;
            }
        }

        private void CauseDamageCombined(dynamic json)
        {
            bool midnight = json.api_hougeki();
            var hontai = _shipInfo.GetShipStatuses(0);
            var goei = _shipInfo.GetShipStatuses(1);
            if (midnight)
            {
                CauseHougekiDamage(goei, json.api_hougeki);
            }
            else // 昼戦
            {
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
                if (!json.api_opening_atack()) // 航空戦のみ
                    return;
                if (json.api_opening_atack != null)
                    CauseSimpleDamage(goei, json.api_opening_atack.api_fdam);
                if (json.api_hougeki1 != null)
                    CauseHougekiDamage(goei, json.api_hougeki1);
                if (json.api_hougeki2() && json.api_hougeki2 != null)
                    CauseHougekiDamage(hontai, json.api_hougeki2);
                if (json.api_hougeki3() && json.api_hougeki3 != null)
                    CauseHougekiDamage(hontai, json.api_hougeki3);
                if (json.api_raigeki != null)
                    CauseSimpleDamage(goei, json.api_raigeki.api_fdam);
            }
            DamagedShipNames =
                (from ship in hontai.Concat(goei) where ship.DamageLevel == ShipStatus.Damage.Badly select ship.Name).ToArray();
            HasDamagedShip = DamagedShipNames.Any();
        }
    }
}