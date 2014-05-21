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
    public class AkashiTimer
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private readonly DockInfo _dockInfo;
        private readonly MissionInfo _missionInfo;
        private readonly RepairStatus[] _repairStatuses = new RepairStatus[ShipInfo.FleetCount];

        private struct RepairStatus
        {
            public DateTime Timer { get; set; }
            public int TotalHp { get; set; }
            public int[] Deck { private get; set; }

            public void Invalidate()
            {
                Timer = DateTime.MinValue;
                TotalHp = 0;
                Deck = null;
            }

            public bool DeckChanged(int[] deck)
            {
                return Deck != null && Deck.Where((t, i) => deck[i] != t).Any();
            }
        }

        public AkashiTimer(ShipInfo ship, ItemInfo item, DockInfo dock, MissionInfo mission)
        {
            _shipInfo = ship;
            _itemInfo = item;
            _dockInfo = dock;
            _missionInfo = mission;
        }

        public void SetTimer(bool port = false)
        {
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
                SetTimer(fleet, port);
        }

        private void SetTimer(int fleet, bool port)
        {
            var deck = _shipInfo.GetDeck(fleet);
            var fs = deck[0];
            if (fs == -1 || !_shipInfo[fs].Name.StartsWith("明石") || _dockInfo.InNDock(fs) ||
                _missionInfo.InMission(fleet))
            {
                InvalidateTimer(fleet);
                return;
            }
            if (_repairStatuses[fleet].DeckChanged(deck))
                InvalidateTimer(fleet);
            var cap = _shipInfo[fs].Slot.Count(item => _itemInfo[item].Name == "艦艇修理施設") + 2;
            var targets = deck.Take(cap).Where(id => id != -1 && !_dockInfo.InNDock(id)).ToArray();
            var totalHp = (from id in targets
                let status = _shipInfo[id]
                where status.NowHp < status.MaxHp && status.DamageLevel < 2
                select status.NowHp).Sum();
            if (totalHp == 0)
            {
                InvalidateTimer(fleet);
                return;
            }
            var r = _repairStatuses[fleet];
            if (r.TotalHp == totalHp)
                return;
            var timer = r.Timer;
            /*
             * 母港に遷移したときに、耐久値が回復しているか修理開始から20分経過しているときに
             * タイマーをリスタートする。
             * 
             * Q. なぜ20分でリスタートするのか？
             * 
             * A. 明石の修理は戦闘中も続くので、戦闘中に修理開始から20分経つと母港に戻った
             *    ときに耐久値が回復する。最後の戦闘で損傷すると、損傷と回復が同時に耐久値に
             *    反映されて回復が起こったことがわからない。耐久値の回復だけを基準にすると
             *    リスタートできないので、20分経過していたらリスタートする。
            */
            if (timer == DateTime.MinValue ||
                (port && (totalHp > r.TotalHp || (DateTime.Now - timer).TotalMinutes > 20)))
                timer = DateTime.Now;
            _repairStatuses[fleet] = new RepairStatus
            {
                Timer = timer,
                TotalHp = totalHp,
                Deck = deck.ToArray()
            };
        }

        private void InvalidateTimer(int fleet)
        {
            _repairStatuses[fleet].Invalidate();
        }

        public DateTime this[int fleet]
        {
            get { return _repairStatuses[fleet].Timer; }
        }
    }
}