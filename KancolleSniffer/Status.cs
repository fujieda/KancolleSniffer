// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;

namespace KancolleSniffer
{
    public interface IHaveState
    {
        bool NeedSave { get; }
        void SaveState(Status status);
        void LoadState(Status status);
    }

    public class Status
    {
        private readonly string _baseDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly string _statusFileName;
        public static bool Restoring { get; set; }
        public Achievement Achievement { get; set; }
        public List<MaterialCount> MaterialHistory { get; set; }
        public double CondRegenTime { get; set; }
        public ExMapInfo.ExMapState ExMapState { get; set; }

        public Status()
        {
            _statusFileName = Path.Combine(_baseDir, "status.xml");
            CondRegenTime = double.MinValue;
        }

        public void Load()
        {
            try
            {
                Restoring = true;
                var serializer = new XmlSerializer(typeof(Status));
                Status status;
                using (var file = File.OpenText(_statusFileName))
                    status = (Status)serializer.Deserialize(file);
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(status, null), null);
            }
            catch (FileNotFoundException)
            {
                ReadOldStatus();
            }
            finally
            {
                Restoring = false;
            }
        }

        public void Save()
        {
            var serializer = new XmlSerializer(typeof(Status));
            using (var file = File.CreateText(_statusFileName))
                serializer.Serialize(file, this);
        }

        public void ReadOldStatus()
        {
            var old = Path.Combine(_baseDir, "status.json");
            dynamic json;
            try
            {
                json = JsonParser.Parse(File.ReadAllText(old));
            }
            catch (FileNotFoundException)
            {
                return;
            }
            var ac = json.Achievement;
            Achievement = new Achievement
            {
                Start = (int)ac.Start,
                StartOfMonth = (int)ac.StartOfMonth,
                LastReset = DateTime.Parse(ac.LastReset),
                LastResetOfMonth = DateTime.Parse(ac.LastResetOfMonth),
                ResetHours = new List<int>((int[])ac.ResetHours),
            };
            var history = new List<MaterialCount>();
            foreach (var h in json.MatreialHistory)
            {
                history.Add(new MaterialCount
                {
                    BegOfDay = (int)h.BegOfDay,
                    BegOfWeek = (int)h.BegOfWeek,
                    Now = (int)h.Now,
                    LastSet = DateTime.Parse(h.LastSet)
                });
            }
            MaterialHistory = history;
            CondRegenTime = json.CondRegenTime;
            ExMapState = new ExMapInfo.ExMapState();
            var clear = new List<ExMapInfo.ClearStatus>();
            foreach (var cs in json.ExMapState.ClearStatusList)
            {
                clear.Add(new ExMapInfo.ClearStatus
                {
                    Map = (int)cs.Map,
                    Cleared = cs.Cleared,
                    Rate = (int)cs.Rate,
                });
            }
            ExMapState.ClearStatusList = clear;
            Save();
        }
    }
}