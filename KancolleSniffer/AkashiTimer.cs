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
            private int[] _deck = new int[0];

            public int[] Deck
            {
                set { _deck = value; }
            }

            public void Invalidate()
            {
                _target = new ShipStatus[0];
            }

            public bool DeckChanged(IEnumerable<int> deck)
            {
                return !_deck.SequenceEqual(deck);
            }

            public void UpdateTarget(ShipStatus[] target)
            {
                _target = target;
            }

            public bool IsRepaired(ShipStatus[] target)
            {
                return _target.Zip(target, (a, b) => a.NowHp < b.NowHp).Any(x => x);
            }

            public RepairSpan[] GetTimers(DateTime start, DateTime now)
            {
                var spent = TimeSpan.FromSeconds((int)(now - start).TotalSeconds);
                return _target.Select(s =>
                {
                    var damage = s.MaxHp - s.NowHp;
                    if (damage == 0)
                        return new RepairSpan(0, TimeSpan.MinValue);
                    if (spent < TimeSpan.FromMinutes(20))
                        return new RepairSpan(0, TimeSpan.FromMinutes(20) - spent);
                    if (damage == 1)
                        return new RepairSpan(1, TimeSpan.Zero);
                    for (var d = 2; d <= damage; d++)
                    {
                        var sec = s.CalcRepairSec(d) + 60;
                        if (sec <= 20 * 60)
                            continue;
                        if (TimeSpan.FromSeconds(sec) > spent)
                            return new RepairSpan(d - 1, TimeSpan.FromSeconds(sec) - spent);
                    }
                    return new RepairSpan(damage, TimeSpan.Zero);
                }).ToArray();
            }

            public string GetNotice(DateTime start, DateTime prev, DateTime now)
            {
                var msg = string.Join(" ", _target.Where(s =>
                {
                    var damage = s.MaxHp - s.NowHp;
                    if (damage < 2)
                        return false;
                    for (var d = 2; d <= damage; d++)
                    {
                        var sec = s.CalcRepairSec(d) + 60;
                        if (sec <= 20 * 60)
                            continue;
                        var span = TimeSpan.FromSeconds(sec);
                        if (span > prev - start && span <= now - start)
                            return true;
                    }
                    return false;
                }).Select(s => s.Name));
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
            var target = deck.Take(cap).Select(id =>
            {
                /*
                 * 修理できない艦娘のステータスをNowHp == MaxHpにする。
                 * - 入渠中の艦娘
                 *   入渠が終了して戻ったのを明石による回復と誤認しないためにHPを回復時の値に
                 * - 中破以上の艦娘
                 *   出撃中の損傷で小破以下から中破以上になったのを回復と誤認しないためにHPを0に
                */
                var s = _shipInfo[id];
                var full = new ShipStatus {NowHp = s.MaxHp, MaxHp = s.MaxHp};
                var zero = new ShipStatus();
                return _dockInfo.InNDock(id)
                    ? full
                    : s.DamageLevel >= ShipStatus.Damage.Half ? zero : s;
            }).ToArray();
            var damage = target.Sum(s => s.MaxHp - s.NowHp);
            if (damage == 0)
            {
                repair.Invalidate();
                return false;
            }
            /*
             * 母港に遷移したときに、耐久値が回復しているか修理開始から20分経過している
             * ときにタイマーをリスタートする。
             *
             * 泊地修理中に出撃して母港に戻ったときに、修理開始から20分以上経っていると
             * HPが回復する。最後の戦闘の損傷と回復量が差し引きゼロだと泊地修理の進行が
             * わからないので、20分経っていたらとにかくリスタートする。
            */
            if (port && _start != DateTime.MinValue &&
                (repair.IsRepaired(target) || DateTime.Now - _start > TimeSpan.FromMinutes(20)))
                _start = DateTime.MinValue;
            repair.UpdateTarget(target);
            return true;
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