// Copyright (C) 2013, 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Codeplex.Data;

namespace KancolleSniffer
{
    public class ShipInfo
    {
        private readonly Dictionary<int, string> _shipNames = new Dictionary<int, string>();
        private readonly int[] _deck = {-1, -1, -1, -1, -1, -1};
        private readonly Dictionary<int, ShipStatus> _shipInfo = new Dictionary<int, ShipStatus>();
        private readonly DateTime[] _recoveryTimes = new DateTime[3];

        private readonly string _shipNamesFile =
            Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), "shipnames.json");

        public string FleetName { get; set; }

        public DateTime[] RecoveryTimes
        {
            get { return _recoveryTimes; }
        }

        public void InspectShip(dynamic json)
        {
            foreach (var entry in json)
                _shipNames[(int)entry.api_ship_id] = (string)entry.api_name;
        }

        public void LoadNames()
        {
            try
            {
                InspectShip(DynamicJson.Parse(File.ReadAllText(_shipNamesFile)));
            }
            catch (FileNotFoundException)
            {
            }
        }

        public void SaveNames()
        {
            var ship = from data in _shipNames select new {api_ship_id = data.Key, api_name = data.Value};
            File.WriteAllText(_shipNamesFile, DynamicJson.Serialize(ship));
        }

        public string GetNameByShipId(int shipId)
        {
            string name;
            return _shipNames.TryGetValue(shipId, out name) ? name : "不明";
        }

        public string GetNameById(int id)
        {
            ShipStatus ship;
            return _shipInfo.TryGetValue(id, out ship) ? GetNameByShipId(ship.ShipId) : "不明";
        }

        public void InspectBattleResult(dynamic json)
        {
            if (!json.IsDefined("api_get_ship")) // 艦娘がドロップしていない。
                return;
            var entry = json.api_get_ship;
            _shipNames[(int)entry.api_ship_id] = (string)entry.api_ship_name;
        }

        public void InspectDeck(dynamic json)
        {
            foreach (var entry in json)
            {
                var fleet = (int)entry.api_id;
                if (fleet != 1)
                    continue;
                FleetName = (string)entry.api_name;
                for (var i = 0; i < _deck.Length; i++)
                    _deck[i] = (int)entry.api_ship[i];
            }
        }

        public int NumShips
        {
            get { return _shipInfo.Count; }
        }

        public void InspectShipInfo(dynamic json)
        {
            _shipInfo.Clear();
            foreach (var entry in json)
            {
                _shipInfo[(int)entry.api_id] = new ShipStatus
                {
                    ShipId = (int)entry.api_ship_id,
                    Level = (int)entry.api_lv,
                    ExpToNext = (int)entry.api_exp[1],
                    MaxHp = (int)entry.api_maxhp,
                    NowHp = (int)entry.api_nowhp,
                    Cond = (int)entry.api_cond
                };
            }
            SetRecoveryTime();
        }

        private void SetRecoveryTime()
        {
            var cond =
                (from id in _deck where _shipInfo.ContainsKey(id) select _shipInfo[id].Cond).DefaultIfEmpty(49).Min();
            if (cond < 49 && _recoveryTimes[2] != DateTime.MinValue) // 計時中
            {
                // コンディション値から推定される残り時刻と経過時間の差
                var diff = TimeSpan.FromMinutes((49 - cond + 2) / 3 * 3) - (_recoveryTimes[2] - DateTime.Now);
                if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(3)) // 差が0以上3分以内ならタイマーを更新しない。
                    return;
            }
            var thresh = new[] {30, 40, 49};
            for (var i = 0; i < thresh.Length; i++)
                _recoveryTimes[i] = cond < thresh[i]
                    ? DateTime.Now.AddMinutes((thresh[i] - cond + 2) / 3 * 3)
                    : DateTime.MinValue;
        }

        public ShipStatus[] ShipStatuses
        {
            get
            {
                var result = new ShipStatus[_deck.Length];
                for (var i = 0; i < _deck.Length; i++)
                {
                    var id = _deck[i];
                    ShipStatus status;
                    if (id == -1 || !_shipInfo.TryGetValue(id, out status))
                        continue;
                    status.Name = GetNameByShipId(status.ShipId);
                    result[i] = status;
                }
                return result;
            }
        }
    }

    public struct ShipStatus
    {
        public int ShipId { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public int ExpToNext { get; set; }
        public int MaxHp { get; set; }
        public int NowHp { get; set; }
        public int Cond { get; set; }
    }
}