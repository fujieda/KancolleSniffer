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
    public class AkashiTimer
    {
        private readonly ShipInfo _shipInfo;
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

            public State State { get; set; }

            public void Invalidate()
            {
                _target = new ShipStatus[0];
            }

            public bool DeckChanged(IEnumerable<int> deck) => !_deck.SequenceEqual(deck);

            public void UpdateTarget(ShipStatus[] target)
            {
                _target = target;
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

            public Notice GetNotice(DateTime start, DateTime prev, DateTime now)
            {
                var m20 = TimeSpan.FromMinutes(20);
                var proc = new List<string>();
                var comp = new List<string>();
                foreach (var s in _target)
                {
                    var damage = s.MaxHp - s.NowHp;
                    if (damage == 0)
                        continue;
                    if (damage == 1)
                    {
                        if (prev - start < m20 && now - start >= m20)
                            comp.Add(s.Name);
                        continue;
                    }
                    // スリープで時間が飛んだときに修理完了だけを表示するために、
                    // 完全回復から減らしながら所要時間と経過時間と比較する。
                    for (var d = damage; d >= 2; d--)
                    {
                        var sec = s.CalcRepairSec(d) + 60;
                        if (sec <= 20 * 60)
                        {
                            if (d == damage && (prev - start < m20 && now - start >= m20))
                                comp.Add(s.Name);
                            continue;
                        }
                        var span = TimeSpan.FromSeconds(sec);
                        if (span <= prev - start || now - start < span)
                            continue;
                        if (d == damage)
                            comp.Add(s.Name);
                        else
                            proc.Add(s.Name);
                        break;
                    }
                }
                return new Notice
                {
                    Proceeded = proc.Count == 0 ? "" : string.Join(" ", proc),
                    Completed = comp.Count == 0 ? "" : string.Join(" ", comp)
                };
            }
        }

        public class Notice
        {
            public string Proceeded { get; set; }
            public string Completed { get; set; }
        }

        public AkashiTimer(ShipInfo ship, DockInfo dock)
        {
            _shipInfo = ship;
            _dockInfo = dock;
            for (var i = 0; i < _repairStatuses.Length; i++)
                _repairStatuses[i] = new RepairStatus();
        }

        [Flags]
        private enum State
        {
            Stop = 0,
            Continue = 1,
            Reset = 2,
            Reparing = 4
        }

        public void SetTimer(bool port = false)
        {
            var now = DateTime.Now;
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
                CheckFleet(fleet);
            if (_repairStatuses.Any(r => (r.State & State.Reset) == State.Reset))
            {
                _start = now;
                return;
            }
            if (port && now - _start > TimeSpan.FromMinutes(20))
            {
                if (_repairStatuses.Any(r => (r.State & State.Reparing) == State.Reparing))
                    _start = now;
                else if (_repairStatuses.All(r => (r.State & State.Continue) == 0))
                    _start = DateTime.MinValue;
            }
            else if (_repairStatuses.Any(r => (r.State & State.Continue) != 0))
            {
                if (_start == DateTime.MinValue)
                    _start = now;
            }
        }

        private void CheckFleet(int fleet)
        {
            var deck = _shipInfo.GetDeck(fleet).ToArray();
            var repair = _repairStatuses[fleet];
            var fs = _shipInfo.GetStatus(deck[0]);
            /*
             * 旗艦が明石でないか明石がドックに入っている場合は泊地修理を止める。
            */
            if (!fs.Name.StartsWith("明石") || _shipInfo.InMission(fleet))
            {
                repair.Invalidate();
                repair.State = State.Stop;
                repair.Deck = deck;
                return;
            }
            var cap = fs.Slot.Count(item => item.Spec.Name == "艦艇修理施設") + 2;
            /*
             * 泊地修理の条件を満たさない艦はMaxHp==NowHpのダミーを設定する。
             * 入渠中の艦娘は終わったときに回復扱いされないようNowHp=MaxHpにする。
             * 中破以上でNowHp=MaxHpにすると回復扱いされるのでNowHp=MaxHp=0にする。
            */
            var target = (from id in deck.Take(cap)
                let s = _shipInfo.GetStatus(id)
                let full = new ShipStatus {NowHp = s.MaxHp, MaxHp = s.MaxHp}
                let zero = new ShipStatus()
                select _dockInfo.InNDock(id) ? full : s.DamageLevel >= ShipStatus.Damage.Half ? zero : s).ToArray();
            repair.State = State.Continue;
            if (repair.DeckChanged(deck))
                repair.State |= State.Reset;
            if (target[0].DamageLevel < ShipStatus.Damage.Half &&
                target.Any(s => s.NowHp < s.MaxHp))
                repair.State |= State.Reparing;
            repair.UpdateTarget(target);
            repair.Deck = deck;
        }

        public RepairSpan[] GetTimers(int fleet)
            => _start == DateTime.MinValue ? new RepairSpan[0] : _repairStatuses[fleet].GetTimers(_start, DateTime.Now);


        public Notice[] GetNotice()
        {
            var now = DateTime.Now;
            var prev = _prev;
            _prev = now;
            if (prev == DateTime.MinValue || _start == DateTime.MinValue)
                return new Notice[0];
            var r = _repairStatuses.Select(repair => repair.GetNotice(_start, prev, now)).ToArray();
            var m20 = TimeSpan.FromMinutes(20);
            if (prev - _start < m20 && now - _start >= m20)
                r[0].Proceeded = "20分経過しました。";
            return r;
        }
    }
}