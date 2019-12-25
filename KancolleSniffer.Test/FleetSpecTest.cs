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

using System.Drawing;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using KancolleSniffer.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    using Sniffer = SnifferTest.TestingSniffer;

    [TestClass]
    public class FleetSpecTest
    {
        /// <summary>
        /// 編成で艦隊をまたがって艦娘を交換する
        /// </summary>
        [TestMethod]
        public void ExchangeFleetMember()
        {
            var sniffer = new Sniffer();
            var expected = new FleetSpec.Record
            {
                AircraftSpec = "",
                Color = SystemColors.Control,
                Equip = "",
                Fleet = "",
                Fleet2 = null,
                Id = 756,
                Ship = "島風改 Lv130",
                Ship2 = "燃17 弾21",
                Spec = "砲64.0 潜82.4",
                Spec2 = "雷104.0 夜158.0"
            };

            SnifferTest.SniffLogFile(sniffer, "deck_002");
            SnifferTest.SniffLogFile(sniffer, "deck_003");
            var spec = FleetSpec.Create(sniffer);
            PAssert.That(() => CompareFleetRecord(spec[13], expected));
        }

        private bool CompareFleetRecord(FleetSpec.Record a, FleetSpec.Record b)
        {
            foreach (var property in typeof(FleetSpec.Record).GetProperties())
            {
                var aVal = property.GetValue(a);
                var bVal = property.GetValue(b);
                if (aVal == null)
                {
                    if (bVal == null)
                        continue;
                    return false;
                }
                if (aVal.Equals(bVal))
                    continue;
                return false;
            }
            return true;
        }

        [TestMethod]
        public void ShowSpeed()
        {
            var sniffer = new Sniffer();
            SnifferTest.SniffLogFile(sniffer, "speed_001");
            var table = FleetSpec.Create(sniffer);
            PAssert.That(() => table[0].Fleet == "第一 高速+   火525 空600 潜39 索724" &&
                               table[37].Fleet == "第二 高速   火185 空215 潜242 索166");
        }

        [TestMethod]
        public void ItemString()
        {
            var item = new ItemStatus
            {
                Spec = new ItemSpec
                {
                    Name = "大発動艇(八九式中戦車&陸戦隊)"
                }
            };
            Assert.AreEqual(item.Spec.Name, FleetSpec.Record.CreateItemRecord(item, 7, 7).Equip);
            item.Level = 10;
            Assert.AreEqual("大発動艇(八九式中戦車&陸戦★10", FleetSpec.Record.CreateItemRecord(item, 7, 7).Equip);

            var aircraft = new ItemStatus
            {
                Spec = new ItemSpec
                {
                    Name = "零式艦戦53型(岩本隊)",
                    Type = 6
                }
            };
            Assert.AreEqual(aircraft.Spec.Name, FleetSpec.Record.CreateItemRecord(aircraft, 7, 7).Equip);
            aircraft.Level = 10;
            Assert.AreEqual("零式艦戦53型(岩本★10", FleetSpec.Record.CreateItemRecord(aircraft, 7, 7).Equip);
        }
    }
}