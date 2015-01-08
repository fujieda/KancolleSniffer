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
        private readonly RepairStatus[] _repairStatuses = new RepairStatus[ShipInfo.FleetCount];

        public class RepairTime
        {
            public int Diff { get; set; }
            public DateTime Time { get; set; }

            public RepairTime(int diff, DateTime time)
            {
                Diff = diff;
                Time = time;
            }
        }

        public class RepairSpan
        {
            public int Diff { get; set; }
            public TimeSpan Span { get; set; }

            public RepairSpan(int diff, TimeSpan span)
            {
                Diff = diff;
                Span = span;
            }

            public RepairSpan(RepairTime time)
            {
                Diff = time.Diff;
                Span = TimeSpan.FromSeconds(Math.Ceiling((time.Time - DateTime.Now).TotalSeconds));
            }
        }

        private class RepairStatus
        {
            private ShipStatus[] _target;
            private RepairTime[][] _times = new RepairTime[0][];
            private DateTime _prev;
            public DateTime Start { get; set; }
            public int[] Deck { private get; set; }

            public int TotalHp
            {
                get { return _target == null ? 0 : _target.Sum(s => s.NowHp); }
            }

            public void Invalidate()
            {
                Start = DateTime.MinValue;
                Deck = null;
                _target = null;
                _times = new RepairTime[0][];
            }

            public bool DeckChanged(int[] deck)
            {
                return Deck != null && Deck.Where((t, i) => deck[i] != t).Any();
            }

            public void UpdateTarget(ShipStatus[] target)
            {
                _target = target;
                UpdateTimes();
            }

            private void UpdateTimes()
            {
                _times = (from s in _target
                    let damage = s.MaxHp - s.NowHp
                    let first = new RepairTime(0, Start.AddMinutes(20))
                    select damage == 0
                        ? null
                        : new[] {first}.Concat(from d in Enumerable.Range(2, damage < 2 ? 0 : damage - 1)
                            let span = s.CalcRepairTime(d) + TimeSpan.FromSeconds(30)
                            where span.TotalSeconds > 20 * 60
                            select new RepairTime(d - 1, Start + span)).ToArray()).ToArray();
            }

            public RepairSpan[] GetTimers()
            {
                return (from times in _times
                    select times == null
                        ? new RepairSpan(0, TimeSpan.MinValue)
                        : (from t in times select new RepairSpan(t)).FirstOrDefault(s => s.Span > TimeSpan.Zero)
                          ?? new RepairSpan(times.Last().Diff + 1, TimeSpan.Zero)
                    ).ToArray();
            }

            public string GetNotice()
            {
                var now = DateTime.Now;
                if (Start == DateTime.MinValue)
                    return "";
                var pr = _prev;
                _prev = now;
                if (pr == DateTime.MinValue)
                    return "";
                var m20 = TimeSpan.FromMinutes(20);
                if (pr - Start < m20 && now - Start >= m20)
                    return "20分経過しました。";
                var margin = TimeSpan.Zero;
                var msg = string.Join(" ", from e in _times.Zip(_target, (times, ship) => new {times, ship})
                    where e.times != null && e.times.Any(rt => rt.Time - pr > margin && rt.Time - now <= margin)
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
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
                SetTimer(fleet, port);
        }

        private void SetTimer(int fleet, bool port)
        {
            var deck = _shipInfo.GetDeck(fleet);
            var repair = _repairStatuses[fleet];
            var fs = deck[0];
            if (!_shipInfo[fs].Name.StartsWith("明石") || _dockInfo.InNDock(fs) || _shipInfo.InMission(fleet))
            {
                repair.Invalidate();
                return;
            }
            if (repair.DeckChanged(deck))
                repair.Invalidate();
            repair.Deck = deck.ToArray();
            var cap = _shipInfo[fs].Slot.Count(item => _itemInfo[item].Name == "艦艇修理施設") + 2;
            var target = (from id in deck.Take(cap) select IsRepairable(id) ? _shipInfo[id] : new ShipStatus()).ToArray();
            var totalHp = target.Sum(s => s.NowHp);
            if (totalHp == 0)
            {
                repair.Invalidate();
                return;
            }
            if (totalHp == repair.TotalHp)
                return;
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
            if (repair.Start == DateTime.MinValue ||
                (port && (totalHp > repair.TotalHp || (DateTime.Now - repair.Start).TotalMinutes > 20)))
                repair.Start = DateTime.Now;
            repair.UpdateTarget(target);
        }

        private bool IsRepairable(int id)
        {
            var s = _shipInfo[id];
            return !_dockInfo.InNDock(id) && s.NowHp < s.MaxHp && s.DamageLevel < ShipStatus.Damage.Half;
        }

        public RepairSpan[] GetTimers(int fleet)
        {
            return _repairStatuses[fleet].GetTimers();
        }

        public string[] GetNotice()
        {
            return _repairStatuses.Select(repair => repair.GetNotice()).ToArray();
        }
    }
}