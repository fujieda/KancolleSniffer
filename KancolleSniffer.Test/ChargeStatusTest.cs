// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using ExpressionToCodeLib;
using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable CompareOfFloatsByEqualityOperator

namespace KancolleSniffer.Test
{
    [TestClass]
    public class ChargeStatusTest
    {
        private readonly Fleet _fleet = new Fleet(null, 6, null);

        [TestMethod]
        public void NoShips()
        {
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 0 && stat.Bull == 0);
            PAssert.That(() => stat.Empty);
        }

        [TestMethod]
        public void FullFlagshipOnly()
        {
            var fs =_fleet.Ships[0];
            fs.Fuel = 9;
            fs.Spec.FuelMax = 9;
            fs.Bull = 9;
            fs.Spec.BullMax = 9;
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 0 && stat.FuelRatio == 1);
            PAssert.That(() => stat.Bull == 0 && stat.BullRatio == 1);
        }

        [TestMethod]
        public void FlagshipOnly()
        {
            var fs = _fleet.Ships[0];
            fs.Fuel = 3;
            fs.Spec.FuelMax = 9;
            fs.Bull = 0;
            fs.Spec.BullMax = 9;
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 2 && stat.FuelRatio == 3 / 9.0);
            PAssert.That(() => stat.Bull == 4 && stat.BullRatio == 0.0);
        }

        [TestMethod]
        public void FullFlagshipAndFullOther()
        {
            var fs = _fleet.Ships[0];
            fs.Fuel = 9;
            fs.Spec.FuelMax = 9;
            fs.Bull = 9;
            fs.Spec.BullMax = 9;
            var other = _fleet.Ships[1];
            other.Fuel = 9;
            other.Spec.FuelMax = 9;
            other.Bull = 9;
            other.Spec.BullMax = 9;
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 5 && stat.FuelRatio == 1);
            PAssert.That(() => stat.Bull == 5 && stat.BullRatio == 1);
        }

        [TestMethod]
        public void FullFlagshipAndOther()
        {
            var fs = _fleet.Ships[0];
            fs.Fuel = 9;
            fs.Spec.FuelMax = 9;
            fs.Bull = 9;
            fs.Spec.BullMax = 9;
            var other = _fleet.Ships[1];
            other.Fuel = 7;
            other.Spec.FuelMax = 9;
            other.Bull = 3;
            other.Spec.BullMax = 9;
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 6 && stat.FuelRatio == 7 / 9.0);
            PAssert.That(() => stat.Bull == 7 && stat.BullRatio == 3 / 9.0);
        }

        [TestMethod]
        public void FullFlagshipAndOthers()
        {
            var fs = _fleet.Ships[0];
            fs.Fuel = 9;
            fs.Spec.FuelMax = 9;
            fs.Bull = 9;
            fs.Spec.BullMax = 9;
            var second = _fleet.Ships[1];
            second.Fuel = 3;
            second.Spec.FuelMax = 9;
            second.Bull = 9;
            second.Spec.BullMax = 9;
            var third = _fleet.Ships[2];
            third.Fuel = 7;
            third.Spec.FuelMax = 9;
            third.Bull = 0;
            third.Spec.BullMax = 9;
            var stat = _fleet.ChargeStatus;
            PAssert.That(() => stat.Fuel == 7 && stat.FuelRatio == 3 / 9.0);
            PAssert.That(() => stat.Bull == 9 && stat.BullRatio == 0.0);
        }
    }
}