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

using KancolleSniffer.Model;
using KancolleSniffer.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class RepairShipCountTest
    {
        [TestMethod]
        public void TestCreateRepairShipCountString()
        {
            Assert.AreEqual("なし", new RepairShipCount(new ShipStatus[0]).ToString());

            const int max = 31;
            const int minor = 30;
            const int small = (int)(max * 0.75);
            const int half = (int)(max * 0.5);
            const int badly = (int)(max * 0.25);

            var repairList = new[]
            {
                new ShipStatus {NowHp = minor, MaxHp = max},
                new ShipStatus {NowHp = minor, MaxHp = max},
                new ShipStatus {NowHp = minor, MaxHp = max},
                new ShipStatus {NowHp = minor, MaxHp = max}
            };
            Assert.AreEqual("軽微 4", new RepairShipCount(repairList).ToString());

            repairList[0].NowHp = small;
            Assert.AreEqual("軽微 3\r\n小破 1\r\n　計 4",
                new RepairShipCount(repairList).ToString());

            repairList[1].NowHp = half;
            Assert.AreEqual("軽微 2\r\n小破 1\r\n　計 3\r\n中破 1\r\n　計 1",
                new RepairShipCount(repairList).ToString());

            repairList[2].NowHp = badly;
            Assert.AreEqual("軽微 1\r\n小破 1\r\n　計 2\r\n中破 1\r\n大破 1\r\n　計 2",
                new RepairShipCount(repairList).ToString());

            repairList[3].NowHp = small;
            Assert.AreEqual("小破 2\r\n　計 2\r\n中破 1\r\n大破 1\r\n　計 2",
                new RepairShipCount(repairList).ToString());

            repairList[1].NowHp = badly;
            Assert.AreEqual("小破 2\r\n大破 2",
                new RepairShipCount(repairList).ToString());

            repairList[0].NowHp = half;
            repairList[3].NowHp = half;
            repairList[1].NowHp = badly;
            Assert.AreEqual("中破 2\r\n大破 2\r\n　計 4",
                new RepairShipCount(repairList).ToString());

            repairList[0].NowHp = badly;
            repairList[3].NowHp = badly;
            Assert.AreEqual("大破 4",
                new RepairShipCount(repairList).ToString());
        }
    }
}