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
    public class AkashiTimer
    {
        private readonly ShipInfo _shipInfo;
        private readonly ItemInfo _itemInfo;
        private readonly DockInfo _dockInfo;
        private readonly RepairStatus[] _repairStatuses = new RepairStatus[ShipInfo.FleetCount];
        private DateTime _start;
        private DateTime _prev;

        public class RepairSpan
        {
            public int Diff { get; set; }
            public TimeSpan Span { get; set; }

            public RepairSpan(int diff, TimeSpan span)
            {
                Diff = diff;
                Span = span;
            }
        }

        private class RepairStatus
        {
            private ShipStatus[] _target = new ShipStatus[0];
            private RepairSpan[][] _spans = new RepairSpan[0][];
            private int[] _deck = new int[0];

            public int[] Deck
            {
                set { _deck = value; }
            }

            public void Invalidate()
            {
                _target = new ShipStatus[0];
                _spans = new RepairSpan[0][];
            }

            public int TotalHp
            {
                get { return _target.Sum(s => s.NowHp); }
            }

            public bool DeckChanged(IEnumerable<int> deck)
            {
                return !_deck.SequenceEqual(deck);
            }

            public void UpdateTarget(ShipStatus[] target)
            {
                _target = target;
                CalcRepairSpan();
            }

            private void CalcRepairSpan()
            {
                _spans = (from s in _target
                    let damage = s.MaxHp - s.NowHp
                    let first = new RepairSpan(0, TimeSpan.FromMinutes(20))
                    select damage == 0
                        ? null
                        : new[] {first}.Concat(from d in Enumerable.Range(2, damage < 2 ? 0 : damage - 1)
                            let sec = s.CalcRepairSec(d) + 60
                            where sec > 20 * 60
                            select new RepairSpan(d - 1, TimeSpan.FromSeconds(sec))).ToArray()).ToArray();
            }

            public RepairSpan[] GetTimers(DateTime start, DateTime now)
            {
                var span = TimeSpan.FromSeconds((int)(now - start).TotalSeconds);
                return (from spans in _spans
                    select spans == null
                        ? new RepairSpan(0, TimeSpan.MinValue)
                        : (from s in spans select new RepairSpan(s.Diff, s.Span - span))
                            .FirstOrDefault(s => s.Span > TimeSpan.Zero)
                          ?? new RepairSpan(spans.Last().Diff + 1, TimeSpan.Zero)
                    ).ToArray();
            }

            public string GetNotice(DateTime start, DateTime prev, DateTime now)
            {
                var msg = string.Join(" ", from e in _spans.Zip(_target, (spans, ship) => new {spans, ship})
                    where e.spans != null && e.spans.Any(
                        s => s.Span - (prev - start) > TimeSpan.Zero && s.Span - (now - start) <= TimeSpan.Zero)
                    select e.ship.Name);
                return msg == "" ? "" : "修理進行: " + msg;
            }
        }

        public AkashiTimer(ShipInfo ship, ItemInfo item, DockInfo dock)
        {
            _shipInfo = ship;
            _itemInfo = item;
            _dockInfo = dock;
            for (var i = 0; i < _repairStatuses.Length; i++)
                _repairStatuses[i] = new RepairStatus();
        }

        public void SetTimer(bool port = false)
        {
            var cont = false;
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
            {
                if (CheckFleet(fleet, port))
                    cont = true;
            }
            if (!cont)
            {
                _start = DateTime.MinValue;
                return;
            }
            if (_start == DateTime.MinValue)
                _start = DateTime.Now;
        }

        private bool CheckFleet(int fleet, bool port)
        {
            var deck = _shipInfo.GetDeck(fleet);
            var repair = _repairStatuses[fleet];
            var fs = deck[0];
            if (!_shipInfo[fs].Name.StartsWith("明石") || _dockInfo.InNDock(fs) || _shipInfo.InMission(fleet))
            {
                repair.Invalidate();
                return false;
            }
            if (repair.DeckChanged(deck))
            {
                _start = DateTime.MinValue;
                repair.Invalidate();
            }
            repair.Deck = deck.ToArray();
            var cap = _shipInfo[fs].Slot.Count(item => _itemInfo[item].Name == "艦艇修理施設") + 2;
            var target =
                (from id in deck.Take(cap) select IsRepairable(id) ? _shipInfo[id] : new ShipStatus()).ToArray();
            var totalHp = target.Sum(s => s.NowHp);
            if (totalHp == 0)
            {
                repair.Invalidate();
                return false;
            }
            if (totalHp == repair.TotalHp)
                return true;
            if (port && repair.TotalHp > 0 && totalHp != repair.TotalHp)
                _start = DateTime.MinValue;
            repair.UpdateTarget(target);
            return true;
        }

        private bool IsRepairable(int id)
        {
            var s = _shipInfo[id];
            return !_dockInfo.InNDock(id) && s.NowHp < s.MaxHp && s.DamageLevel < ShipStatus.Damage.Half;
        }

        public RepairSpan[] GetTimers(int fleet)
        {
            if (_start == DateTime.MinValue)
                return new RepairSpan[0];
            return _repairStatuses[fleet].GetTimers(_start, DateTime.Now);
        }

        public string[] GetNotice()
        {
            var now = DateTime.Now;
            var prev = _prev;
            _prev = now;
            if (prev == DateTime.MinValue || _start == DateTime.MinValue)
                return new string[0];
            if (prev - _start < TimeSpan.FromMinutes(20) && now - _start >= TimeSpan.FromMinutes(20))
                return new[] {"20分経過しました。"};
            return _repairStatuses.Select(repair => repair.GetNotice(_start, prev, now)).ToArray();
        }
    }
}