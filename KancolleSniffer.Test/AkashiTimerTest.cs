// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Linq;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class AkashiTimerTest
    {
        private TimeProvider _timeProvider;
        private ShipInventory _shipInventory;
        private ShipInfo _shipInfo;
        private AkashiTimer _akashiTimer;
        private ShipStatus[] _ships;

        private class TimeProvider
        {
            public DateTime DateTime { set; private get; }

            public DateTime GetTime() => DateTime;
        }

        [TestInitialize]
        public void Initialize()
        {
            _timeProvider = new TimeProvider();
            _shipInventory = new ShipInventory();
            _shipInfo = new ShipInfo(null, _shipInventory, new ItemInventory());
            _akashiTimer = new AkashiTimer(_shipInfo, new DockInfo(null, null), null, _timeProvider.GetTime);
            SetupFleet();
        }

        public void SetupFleet()
        {
            _ships = new[]
            {
                new ShipStatus
                {
                    Id = 17160,
                    Spec = new ShipSpec {Id = 187, Name = "明石改", ShipType = 19},
                    NowHp = 45,
                    MaxHp = 45,
                    Slot = new[] {26181, 26501, 37732, 28338}
                        .Select(id => new ItemStatus(id) {Spec = new ItemSpec {Id = 86, Type = 31}})
                        .Concat(new[] {new ItemStatus()}).ToArray()
                },
                new ShipStatus
                {
                    Id = 1,
                    Spec = new ShipSpec {Id = 237, Name = "電改", ShipType = 2},
                    MaxHp = 30,
                    NowHp = 30,
                    Slot = Enumerable.Repeat(new ItemStatus(), 5).ToArray()
                }
            };
            foreach (var ship in _ships)
                _shipInventory[ship.Id] = ship;
            _shipInfo.Fleets[0].Deck = new[] {17160, 1, -1, -1, -1, -1};
        }

        /// <summary>
        /// 母港
        /// </summary>
        public void Port()
        {
            _timeProvider.DateTime = new DateTime(2018, 1, 1, 0, 0, 0);
            _akashiTimer.Port();
            PAssert.That(
                () => _akashiTimer.GetPresetDeckTimer(new DateTime(2018, 1, 1, 0, 1, 0)) == TimeSpan.FromMinutes(19),
                "母港で開始");
        }

        /// <summary>
        /// 二番艦を外してリセット
        /// </summary>
        [TestMethod]
        public void WithdrawShip()
        {
            Port();
            _timeProvider.DateTime = new DateTime(2018, 1, 1, 0, 2, 0);
            _shipInfo.Fleets[0].Deck = new[] {17160, -1, -1, -1, -1, -1};
            _akashiTimer.InspectChange("api_id=1&api_ship_idx=0&api_ship_id=-1");
            PAssert.That(
                () => _akashiTimer.GetPresetDeckTimer(new DateTime(2018, 1, 1, 0, 3, 0)) == TimeSpan.FromMinutes(19));
        }

        /// <summary>
        /// 随伴艦一括解除でリセットしない
        /// </summary>
        [TestMethod]
        public void WithdrawAccompanyingShips()
        {
            Port();
            _timeProvider.DateTime = new DateTime(2018, 1, 1, 0, 2, 0);
            _shipInfo.Fleets[0].Deck = new[] {17160, -1, -1, -1, -1, -1};
            _akashiTimer.InspectChange("api_id=1&api_ship_idx=0&api_ship_id=-2");
            PAssert.That(
                () => _akashiTimer.GetPresetDeckTimer(new DateTime(2018, 1, 1, 0, 3, 0)) == TimeSpan.FromMinutes(17));
        }
    }
}