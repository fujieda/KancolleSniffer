// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//    http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distribukted under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Linq;
using DynaJson;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class BattleBriefTest
    {
        private ItemMaster _itemMaster;
        private ItemInventory _itemInventory;
        private ItemInfo _itemInfo;
        private ShipMaster _shipMaster;
        private ShipInventory _shipInventory;
        private ShipInfo _shipInfo;
        private BattleInfo _battleInfo;

        private string[] ReadAllLines(string log)
        {
            using (var logfile = SnifferTest.OpenLogFile(log))
                return logfile.ReadToEnd().Split(new[] {"\r\n"}, StringSplitOptions.None);
        }

        private void InjectShips(dynamic battle, dynamic item)
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

        private void InjectShips(int deck, int[] nowHps, int[] maxHps, int[][] slots)
        {
            var id = _shipInventory.MaxId + 1;
            var ships = nowHps.Zip(maxHps,
                (now, max) => new ShipStatus {Id = id++, NowHp = now, MaxHp = max}).ToArray();
            _shipInventory.Add(ships);
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
            _itemInventory = new ItemInventory();
            _itemInfo = new ItemInfo(_itemMaster, _itemInventory);
            _shipInventory = new ShipInventory();
            _shipMaster = new ShipMaster();
            _shipInfo = new ShipInfo(_shipMaster, _shipInventory, _itemInventory);
            _battleInfo = new BattleInfo(_shipInfo, _itemInfo, new AirBase(_itemInfo));
        }

        /// <summary>
        /// 連撃を受けて女神が発動する
        /// </summary>
        [TestMethod]
        public void CauseRepairGoddessByDoubleAttack()
        {
            var logs = ReadAllLines("damecon_001");
            var items = JsonObject.Parse("[[[],[],[],[],[43]]]");
            dynamic battle = JsonObject.Parse(logs[2]);
            InjectShips(battle, items);
            _battleInfo.InspectBattle(logs[0], logs[1], battle);
            dynamic result = JsonObject.Parse(logs[5]);
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
            var battle = JsonObject.Parse(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(JsonObject.Parse(logs[6]));
            PAssert.That(() => _shipInfo.Fleets[0].Ships[3].NowHp == 12);
        }

        private dynamic Data(string json) => JsonObject.Parse(json).api_data;

        /// <summary>
        /// NPC友軍の支援攻撃がある
        /// </summary>
        [TestMethod]
        public void NpcFriendFleetAttack()
        {
            var logs = ReadAllLines("friendfleet_001");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattle(logs[4], logs[5], Data(logs[6]));
            _battleInfo.InspectBattleResult(Data(logs[9]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 空襲戦で轟沈する
        /// </summary>
        [TestMethod]
        public void LdAirBattleHaveSunkenShip()
        {
            var logs = ReadAllLines("ld_airbattle_001");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(Data(logs[6]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 空襲戦で女神が発動して復活する
        /// </summary>
        [TestMethod]
        public void LdAirBattleHaveRevivedShip()
        {
            var logs = ReadAllLines("ld_airbattle_002");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(Data(logs[6]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 機動対敵連合の雷撃戦でダメコンが発動する
        /// </summary>
        [TestMethod]
        public void TorpedoTriggerDameConInCombinedBattleAir()
        {
            var logs = ReadAllLines("damecon_002");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattle(logs[4], logs[5], Data(logs[6]));
            _battleInfo.InspectBattleResult(Data(logs[9]));
            PAssert.That(() => !_battleInfo.DisplayedResultRank.IsError);
        }

        /// <summary>
        /// 水上対敵連合の雷撃戦でダメコンが発動する
        /// </summary>
        [TestMethod]
        public void TorpedoTriggerDamageControlInCombinedBattleWater()
        {
            var logs = ReadAllLines("damecon_003");
            var battle = Data(logs[3]);
            InjectShips(battle, JsonObject.Parse(logs[0]));
            _battleInfo.InspectBattle(logs[1], logs[2], battle);
            _battleInfo.InspectBattleResult(Data(logs[6]));
            PAssert.That(() => _shipInfo.Fleets[1].Ships[5].NowHp == 6);
        }
    }
}