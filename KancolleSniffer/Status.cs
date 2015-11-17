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
using System.IO;
using Codeplex.Data;

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
        private readonly string _statusFileName = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "status.json");
        public static bool Restoring { get; set; }
        public int ExperiencePoint { get; set; }
        public DateTime LastResetTime { get; set; }
        public Achievement Achievement { get; set; }
        public MaterialCount[] MatreialHistory { get; set; }
        public double CondRegenTime { get; set; }
        public ExMapInfo.ExMapState ExMapState { get; set; }

        public Status()
        {
            CondRegenTime = double.MinValue;
        }

        public void Load()
        {
            try
            {
                Restoring = true;
                var obj = (Status)DynamicJson.Parse(File.ReadAllText(_statusFileName));
                foreach (var property in GetType().GetProperties())
                    property.SetValue(this, property.GetValue(obj, null), null);
            }
            catch (FileNotFoundException)
            {
            }
            finally
            {
                Restoring = false;
            }
        }

        public void Save()
        {
            File.WriteAllText(_statusFileName, DynamicJson.Serialize(this));
        }
    }
}