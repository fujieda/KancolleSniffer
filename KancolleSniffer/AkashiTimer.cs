// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

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

            public bool IsRepaired(ShipStatus[] target) => _target.Zip(target, (a, b) => a.NowHp < b.NowHp).Any(x => x);

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

        private enum State
        {
            Continue = 0,
            Reset = 1,
        }

        public void Port()
        {
            CheckFleet();
            var now = DateTime.Now;
            var reset = _repairStatuses.Any(r => r.State == State.Reset);
            if (_start == DateTime.MinValue || now - _start > TimeSpan.FromMinutes(20) || reset)
                _start = now;
        }

        public void InspectChange(string request)
        {
            CheckFleet();
            var values = HttpUtility.ParseQueryString(request);
            if (int.Parse(values["api_ship_idx"]) == -1)
                return;
            if (_repairStatuses.Any(r => r.State == State.Reset))
                _start = DateTime.Now;
        }

        public void CheckFleet()
        {
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
                CheckFleet(fleet);
        }

        private void CheckFleet(int fleet)
        {
            var deck = _shipInfo.GetDeck(fleet).ToArray();
            var repair = _repairStatuses[fleet];
            var fs = _shipInfo.GetStatus(deck[0]);
            repair.State = State.Continue;
            if (!fs.Spec.IsRepairShip)
            {
                repair.UpdateTarget(new ShipStatus[0]);
                repair.Deck = deck;
                return;
            }
            if (repair.DeckChanged(deck))
            {
                repair.State = State.Reset;
                repair.Deck = deck;
            }
            var target = RepairTarget(deck);
            if (repair.IsRepaired(target))
                repair.State = State.Reset;
            repair.UpdateTarget(target);
        }

        private ShipStatus[] RepairTarget(int[] deck)
        {
            var fs = _shipInfo.GetStatus(deck[0]);
            if (!fs.Spec.IsRepairShip || _dockInfo.InNDock(fs.Id) || fs.DamageLevel >= ShipStatus.Damage.Half)
                return new ShipStatus[0];
            var cap = fs.Slot.Count(item => item.Spec.IsRepairFacility) + 2;
            /*
             * 泊地修理の条件を満たさない艦はMaxHp==NowHpのダミーを設定する。
             * - 入渠中の艦娘は終わったときに回復扱いされないようNowHp=MaxHpに
             * - 中破以上でNowHp=MaxHpにすると回復扱いされるのでNowHp=MaxHp=0に
            */
            return (from id in deck.Take(cap)
                let s = _shipInfo.GetStatus(id)
                let full = new ShipStatus {NowHp = s.MaxHp, MaxHp = s.MaxHp}
                let zero = new ShipStatus()
                select _dockInfo.InNDock(id) ? full : s.DamageLevel >= ShipStatus.Damage.Half ? zero : s).ToArray();
        }

        public RepairSpan[] GetTimers(int fleet)
            => _start == DateTime.MinValue ? new RepairSpan[0] : _repairStatuses[fleet].GetTimers(_start, DateTime.Now);

        public TimeSpan PresetDeckTimer
        {
            get
            {
                if (_start == DateTime.MinValue)
                    return TimeSpan.MinValue;
                var r = TimeSpan.FromMinutes(20) - TimeSpan.FromSeconds((int)(DateTime.Now - _start).TotalSeconds);
                return r >= TimeSpan.Zero ? r : TimeSpan.Zero;
            }
        }

        public bool CheckReparing(int fleet) => GetTimers(fleet).Any(r => r.Span != TimeSpan.MinValue);

        public bool CheckReparing() => Enumerable.Range(0, ShipInfo.FleetCount).Any(CheckReparing);

        public bool CheckPresetReparing()
            => _shipInfo.PresetDeck.Where(deck => deck != null)
                .Any(deck => RepairTarget(deck).Any(s => s.NowHp < s.MaxHp));

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