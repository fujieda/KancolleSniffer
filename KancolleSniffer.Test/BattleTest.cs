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
using System.Linq;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class BattleTest
    {
        private ItemMaster _itemMaster;
        private ItemInventry _itemInventry;
        private ItemInfo _itemInfo;
        private ShipMaster _shipMaster;
        private ShipInventry _shipInventry;
        private ShipInfo _shipInfo;
        private BattleInfo _battleInfo;

        private string[] ReadAllLines(string log)
        {
            using (var logfile = SnifferTest.OpenLogFile(log))
                return logfile.ReadToEnd().Split(new [] {"\r\n"}, StringSplitOptions.None);
        }

        public void InjectShips(dynamic battle, dynamic item)
        {
            var deck = (int)battle.api_deck_id - 1;
            InjectShips(deck, (int[])battle.api_f_nowhps, (int[])battle.api_f_maxhps, (int[][])item[0]);
            if (battle.api_f_nowhps_combined())
                InjectShips(1, (int[])battle.api_f_nowhps_combined, (int[])battle.api_f_maxhps_combined,
                    (int[][])item[1]);
            foreach (var enemy in (int[])battle.api_ship_ke)
                _shipMaster.InjectSpec(enemy);
            if (battle.api_ship_ke_combined())
            {
                foreach (var enemy in (int[])battle.api_ship_ke_combined)
                    _shipMaster.InjectSpec(enemy);
            }
            _itemInfo.InjectItems(((int[][])battle.api_eSlot).SelectMany(x => x));
            if (battle.api_eSlot_combined())
                _itemInfo.InjectItems(((int[][])battle.api_eSlot_combined).SelectMany(x => x));
        }

        private void InjectShips(int deck, int[] nowhps, int[] maxhps, int[][] slots)
        {
            var id = _shipInventry.MaxId + 1;
            var ships = nowhps.Zip(maxhps,
                (now, max) => new ShipStatus {Id = id++, NowHp = now, MaxHp = max}).ToArray();
            _shipInventry.Add(ships);
            _shipInfo.Fleets[deck].Deck = (from ship in ships select ship.Id).ToArray();
            foreach (var entry in ships.Zip(slots, (ship, slot) => new {ship, slot}))
            {
                entry.ship.Slot = _itemInfo.InjectItems(entry.slot.Take(5));
                if (entry.slot.Length >= 6)
                    entry.ship.SlotEx = _itemInfo.InjectItems(entry.slot.Skip(5)).First();
            }
        }

        [TestInitialize]
        public void Initialize()
        {
            _itemMaster = new ItemMaster();
            _itemInventry = new ItemInventry();
            _itemInfo = new ItemInfo(_itemMaster, _itemInventry);
            _shipInventry = new ShipInventry();
            _shipMaster = new ShipMaster();
            _shipInfo = new ShipInfo(_shipMaster, _shipInventry, _itemInventry);
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
            InjectShips(battle, items);
            _battleInfo.InspectBattle(logs[0], logs[1], battle);
            dynamic result = JsonParser.Parse(logs[5]);
            _battleInfo.InspectBattleResult(result);
            PAssert.That(() => _shipInfo.Fleets[2].Ships[4].NowHp == 31);
        }

        /// <summary>
        /// 夜戦で戦艦の攻撃を受ける
        /// </summary>
        [TestMethod]
        public void AttackedByBattleShipInMidnight()
        {
            var logs = ReadAllLines("midnight_002");
            var battle = JsonParser.Parse(logs[3]);
            InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(JsonParser.Parse(logs[6]));
            PAssert.That(() => _shipInfo.Fleets[0].Ships[3].NowHp == 12);
        }

        private dynamic Data(string json) => ((dynamic)JsonParser.Parse(json)).api_data;

        /// <summary>
        /// NPC友軍の支援攻撃がある
        /// </summary>
        [TestMethod]
        public void NpcFriendFleetAttack()
        {
            var logs = ReadAllLines("friendfleet_001");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattle(logs[4], logs[5], Data(logs[6]));
            _battleInfo.InspectBattleResult(Data(logs[9]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 空襲戦で轟沈する
        /// </summary>
        [TestMethod]
        public void LdAirbattleHaveSunkenShip()
        {
            var logs = ReadAllLines("ld_airbattle_001");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(Data(logs[6]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 空襲戦で女神が発動して復活する
        /// </summary>
        [TestMethod]
        public void LdAirbattleHaveRevivedShip()
        {
            var logs = ReadAllLines("ld_airbattle_002");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(Data(logs[6]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 機動対敵連合の雷撃戦でダメコンが発動する
        /// </summary>
        [TestMethod]
        public void TreiggerDameconInCombinedBattle()
        {
            var logs = ReadAllLines("damecon_002");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonParser.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattle(logs[4], logs[5], Data(logs[6]));
            _battleInfo.InspectBattleResult(Data(logs[9]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }
    }
}