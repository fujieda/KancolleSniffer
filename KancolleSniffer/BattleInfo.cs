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

using System.Linq;

namespace KancolleSniffer
{
    public class BattleInfo
    {
        private readonly ShipMaster _shipMaster;
        private readonly ShipInfo _shipInfo;
        public bool InBattle { get; set; }
        public string Formation { get; private set; }
        public int DelayInFormation { get; private set; }

        private struct Delay
        {
            public const int Basic = 7200;
            public const int Tau = 200;
            public const int SearchAirSuccess = 5700;
            public const int SearchAirFailure = 4900;
            public const int SearchSuccess = 5400;
            public const int Submarine = 2500;
            public const int AirFight = 8700;
            public const int AirFightBoth = 2700;
            public const int Support = 9800;
            public const int OpeningAttack = 3500;
            public const int Cutin = 4900;
        }

        public BattleInfo(ShipMaster shipMaster, ShipInfo shipInfo)
        {
            _shipMaster = shipMaster;
            _shipInfo = shipInfo;
        }

        public void InspectBattle(dynamic json)
        {
            InBattle = true;
            var delay = Delay.Basic;
            switch ((int)json.api_formation[2])
            {
                case 1:
                    Formation = "同航戦";
                    break;
                case 2:
                    Formation = "反航戦";
                    break;
                case 3:
                    Formation = "T字有利";
                    delay += Delay.Tau;
                    break;
                case 4:
                    Formation = "T字不利";
                    delay += Delay.Tau;
                    break;
            }
            var subm = (SubmarineFlags)CheckSubmarine(json);
            delay += SearchDelay(json) + SupportDelay(json) + CutinDelay(json) + subm.SubmarineDelay();
            if (!subm.PreventOpeningAttack)
                delay += OpeningAttackDelay(json);
            if (!subm.PreventAirFight)
                delay += AirFightDelay(json);
            DelayInFormation = delay;
        }

        private int SearchDelay(dynamic json)
        {
            switch ((int)json.api_search[0])
            {
                case 1: // 索敵機による索敵成功
                case 2: // 索敵機未帰還あり
                    return Delay.SearchAirSuccess;
                case 3: // 索敵機未帰還
                case 4: // 索敵機による索敵失敗
                    return Delay.SearchAirFailure;
                case 5: // 索敵力による索敵成功
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

            public int SubmarineDelay()
            {
                return Friend.Any(x => x) || Enemy.Any(x => x) ? Delay.Submarine : 0; // どちらかに潜水艦                
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
                Friend = (from status in _shipInfo.GetShipStatuses((int)json.api_dock_id - 1)
                    select _shipMaster[status.ShipId].IsSubmarine).ToArray(),
                Enemy = (from id in (int[])json.api_ship_ke
                    where id != -1
                    select _shipMaster[id].IsSubmarine).ToArray()
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
    }
}