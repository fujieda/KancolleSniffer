// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class BattleTest
    {
        private ItemInfo _itemInfo;
        private ShipInfo _shipInfo;
        private BattleInfo _battleInfo;

        private string[] ReadAllLines(string log)
        {
            using (var logfile = SnifferTest.OpenLogFile(log))
                return logfile.ReadToEnd().Split(new [] {"\r\n"}, StringSplitOptions.None);
        }

        [TestInitialize]
        public void Initialize()
        {
            _itemInfo = new ItemInfo();
            _shipInfo = new ShipInfo(_itemInfo);
            _battleInfo = new BattleInfo(_shipInfo, _itemInfo);
        }

        [TestMethod]
        public void Damecon()
        {
            var logs = ReadAllLines("damecon_001");
            var items = _itemInfo.InjectItems(new[]{43});
            dynamic battle = JsonParser.Parse(logs[2]);
            _shipInfo.InjectShips(battle);
            _shipInfo.GetStatus(5).Slot = new []{items[0]};
            _battleInfo.InspectBattle(battle, logs[0]);
            dynamic result = JsonParser.Parse(logs[5]);
            _battleInfo.InspectBattleResult(result);
            PAssert.That(() => _shipInfo.GetStatus(5).NowHp == 31);
        }
    }
}