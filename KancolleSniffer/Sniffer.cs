﻿// Copyright (C) 2013 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
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

namespace KancolleSniffer
{
    [Flags]
    public enum UpdateInfo
    {
        None = 0,
        Item = 1,
        Ship = 2,
        Timer = 4,
        Quest = 8,
        NDock = 16,
        Mission = 32,
        Charge = 64
    }

    public class Sniffer
    {
        private readonly NameAndTimer[] _ndocInfo = new NameAndTimer[4];
        private readonly RingTimer[] _kdocTimers = new RingTimer[4];
        private readonly ShipMaster _shipMaster = new ShipMaster();
        private readonly ItemInfo _itemInfo = new ItemInfo();
        private readonly QuestInfo _questInfo = new QuestInfo();
        private readonly MissionInfo _missionInfo = new MissionInfo();
        private readonly ShipInfo _shipInfo;

        public Sniffer()
        {
            _shipInfo = new ShipInfo(_shipMaster);
            for (var i = 0; i < _ndocInfo.Length; i++)
                _ndocInfo[i] = new NameAndTimer();
            for (var i = 0; i < _kdocTimers.Length; i++)
                _kdocTimers[i] = new RingTimer(0);
        }

        public UpdateInfo Sniff(string uri, dynamic json)
        {
            if (uri.EndsWith("api_get_master/ship"))
            {
                _shipMaster.InspectShip(json);
                return UpdateInfo.None;
            }
            if (uri.EndsWith("api_get_member/basic"))
            {
                _itemInfo.InspectBasic(json);
                return UpdateInfo.Item;
            }
            if (uri.EndsWith("api_get_member/record"))
            {
                _itemInfo.InspectRecord(json);
                return UpdateInfo.Item;
            }
            if (uri.EndsWith("api_get_member/material"))
            {
                _itemInfo.InspectMaterial(json);
                return UpdateInfo.Item;
            }
            if (uri.EndsWith("api_get_member/slotitem"))
            {
                _itemInfo.InspectSlotItem(json);
                return UpdateInfo.Item;
            }
            if (uri.EndsWith("api_get_member/questlist"))
            {
                _questInfo.Inspect(json);
                return UpdateInfo.Quest;
            }
            if (uri.EndsWith("api_get_member/ndock"))
            {
                InspectNDock(json);
                return UpdateInfo.NDock | UpdateInfo.Timer;
            }
            if (uri.EndsWith("api_get_member/kdock"))
            {
                InspectKDock(json);
                return UpdateInfo.Timer;
            }
            if (uri.EndsWith("api_get_master/mission"))
            {
                _missionInfo.InspectMission(json);
                return UpdateInfo.Mission;
            }
            if (uri.Contains("api_get_member/deck"))
            {
                _missionInfo.InspectDeck(json);
                _shipInfo.InspectDeck(json);
                return UpdateInfo.Mission | UpdateInfo.Ship | UpdateInfo.Charge;
            }
            if (uri.EndsWith("api_get_member/ship2") || uri.EndsWith("api_get_member/ship3"))
            {
                _shipInfo.InspectShipInfo(uri.EndsWith("ship3") ? json.api_ship_data : json);
                _itemInfo.NowShips = _shipInfo.NumShips;
                return UpdateInfo.Ship | UpdateInfo.Item | UpdateInfo.Timer | UpdateInfo.Charge;
            }
            return UpdateInfo.None;
        }

        public void SaveMaster()
        {
            _missionInfo.SaveNames();
            _shipMaster.Save();
        }

        public void LoadMaster()
        {
            _missionInfo.LoadNames();
            _shipMaster.Load();
        }

        private void InspectNDock(dynamic json)
        {
            foreach (var entry in json)
            {
                var id = (int)entry.api_id;
                _ndocInfo[id - 1].Timer.EndTime = (double)entry.api_complete_time;
                var ship = (int)entry.api_ship_id;
                _ndocInfo[id - 1].Name = ship == 0 ? "" : _shipInfo.GetNameById(ship);
            }
        }

        public NameAndTimer[] NDock
        {
            get { return _ndocInfo; }
        }

        private void InspectKDock(dynamic json)
        {
            foreach (var entry in json)
                _kdocTimers[(int)entry.api_id - 1].EndTime = (double)entry.api_complete_time;
        }

        public RingTimer[] KDock
        {
            get { return _kdocTimers; }
        }

        public ItemInfo Item
        {
            get { return _itemInfo; }
        }

        public QuestInfo.NameAndProgress[] Quests
        {
            get { return _questInfo.Quests; }
        }

        public NameAndTimer[] Missions
        {
            get { return _missionInfo.Missions; }
        }

        public string FleetName
        {
            get { return _shipInfo.FleetName; }
        }

        public DateTime[] RecoveryTimes
        {
            get { return _shipInfo.RecoveryTimes; }
        }

        public ShipStatus[] ShipStatuses
        {
            get { return _shipInfo.ShipStatuses; }
        }

        public ChargeStatus[] ChargeStatuses
        {
            get { return _shipInfo.ChargeStatuses; }
        }
    }

    public class NameAndTimer
    {
        public string Name { get; set; }

        public RingTimer Timer { get; set; }

        public NameAndTimer()
        {
            Timer = new RingTimer();
        }
    }

    public class RingTimer
    {
        private bool _ringed;
        private DateTime _endTime;
        private TimeSpan _rest;
        private readonly TimeSpan _spare;

        public RingTimer(int spare = 60)
        {
            _spare = TimeSpan.FromSeconds(spare);
        }

        public double EndTime
        {
            set
            {
// ReSharper disable once CompareOfFloatsByEqualityOperator
                if (value != 0)
                    _endTime = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(value / 1000);
                else
                {
                    _endTime = DateTime.MinValue;
                    _ringed = false;
                }
            }
        }

        public void Update()
        {
            if (_endTime == DateTime.MinValue)
            {
                _rest = TimeSpan.Zero;
                return;
            }
            _rest = _endTime - DateTime.Now;
            if (_rest < TimeSpan.Zero)
                _rest = TimeSpan.Zero;
            if (_rest > _spare || _ringed)
                return;
            _ringed = true;
            NeedRing = true;
        }

        public bool NeedRing { get; set; }

        public bool IsSet
        {
            get { return _endTime != DateTime.MinValue; }
        }

        public override string ToString()
        {
            return _rest.Days == 0 ? _rest.ToString(@"hh\:mm\:ss") : _rest.ToString(@"d\.hh\:mm");
        }
    }
}