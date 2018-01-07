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

        /// <summary>
        /// 連撃を受けて女神が発動する
        /// </summary>
        [TestMethod]
        public void CauseRepairGoddessByDoubleAttack()
        {
            var logs = ReadAllLines("damecon_001");
            var items = JsonParser.Parse("[[[],[],[],[],[43]]]");
            dynamic battle = JsonParser.Parse(logs[2]);
            _shipInfo.InjectShips(battle, items);
            _battleInfo.InspectBattle(logs[0], logs[1], battle);
            dynamic result = JsonParser.Parse(logs[5]);
            _battleInfo.InspectBattleResult(result);
            PAssert.That(() => _shipInfo.GetShipStatuses(2)[4].NowHp == 31);
        }

        /// <summary>
        /// 夜戦で戦艦の攻撃を受ける
        /// </summary>
        [TestMethod]
        public void AttackedByBattleShipInMidnight()
        {
            var logs = ReadAllLines("midnight_002");
            var battle = JsonParser.Parse(logs[3]);
            _shipInfo.InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(JsonParser.Parse(logs[6]));
            PAssert.That(() => _shipInfo.GetShipStatuses(0)[3].NowHp == 12);
        }
    }
}