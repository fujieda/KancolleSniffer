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

using System.Drawing;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class NumberAndHistoryLabels
    {
        public Label NumOfShips { get; set; }
        public Label NumOfEquips { get; set; }
        public Label NumOfBuckets { get; set; }
        public Label BucketHistory { get; set; }
        public Label Achievement { get; set; }
        public Label FuelHistory { get; set; }
        public Label BulletHistory { get; set; }
        public Label SteelHistory { get; set; }
        public Label BauxiteHistory { get; set; }
        public ToolTip ToolTip { get; set; }
    }

    public interface INotifySubmitter
    {
        void Flash();
        void Enqueue(string key, string subject);
    }

    public class NumberAndHistory
    {
        private readonly NumberAndHistoryLabels _labels;
        private readonly Sniffer _sniffer;
        private readonly ToolTip _toolTip;
        private readonly INotifySubmitter _submitter;

        public NumberAndHistory(NumberAndHistoryLabels labels, Sniffer sniffer, INotifySubmitter submitter)
        {
            _labels = labels;
            _sniffer = sniffer;
            _toolTip = labels.ToolTip;
            _submitter = submitter;
        }

        public void Update()
        {
            UpdateNumOfShips();
            UpdateNumOfEquips();
            _submitter.Flash();
            UpdateNumOfBuckets();
            UpdateBucketHistory();
            UpdateAchievement();
            UpdateMaterialHistory();
        }

        private void UpdateNumOfBuckets()
        {
            _labels.NumOfBuckets.Text = _sniffer.Material.MaterialHistory[(int)Material.Bucket].Now.ToString("D");
        }

        private void UpdateAchievement()
        {
            var ac = _sniffer.Achievement.Value;
            if (ac >= 10000)
                ac = 9999;
            _labels.Achievement.Text = ac >= 1000 ? ((int)ac).ToString("D") : ac.ToString("F1");
            _toolTip.SetToolTip(_labels.Achievement,
                "今月 " + _sniffer.Achievement.ValueOfMonth.ToString("F1") + "\n" +
                "EO " + _sniffer.ExMap.Achievement);
        }

        public void UpdateNumOfShips()
        {
            var ship = _sniffer.ShipCounter;
            _labels.NumOfShips.Text = $"{ship.Now:D}/{ship.Max:D}";
            _labels.NumOfShips.ForeColor = ship.TooMany ? CUDColors.Red : Color.Black;
            if (ship.Alarm)
            {
                var message = $"残り{ship.Rest:D}隻";
                _submitter.Enqueue("艦娘数超過", message);
                ship.Alarm = false;
            }
        }

        public void UpdateNumOfEquips()
        {
            var item = _sniffer.ItemCounter;
            _labels.NumOfEquips.Text = $"{item.Now:D}/{item.Max:D}";
            _labels.NumOfEquips.ForeColor = item.TooMany ? CUDColors.Red : Color.Black;
            if (item.Alarm)
            {
                var message = $"残り{item.Rest:D}個";
                _submitter.Enqueue("装備数超過", message);
                item.Alarm = false;
            }
        }

        private void UpdateBucketHistory()
        {
            var count = _sniffer.Material.MaterialHistory[(int)Material.Bucket];
            var day = CutOverflow(count.Now - count.BegOfDay, 999);
            var week = CutOverflow(count.Now - count.BegOfWeek, 999);
            _labels.BucketHistory.Text = $"{day:+#;-#;±0} 今日\n{week:+#;-#;±0} 今週";
        }

        private void UpdateMaterialHistory()
        {
            var labels = new[] {_labels.FuelHistory, _labels.BulletHistory, _labels.SteelHistory, _labels.BauxiteHistory };
            var text = new[] { "燃料", "弾薬", "鋼材", "ボーキ" };
            for (var i = 0; i < labels.Length; i++)
            {
                var count = _sniffer.Material.MaterialHistory[i];
                var port = CutOverflow(count.Now - _sniffer.Material.PrevPort[i], 99999);
                var day = CutOverflow(count.Now - count.BegOfDay, 99999);
                var week = CutOverflow(count.Now - count.BegOfWeek, 99999);
                labels[i].Text = $"{text[i]}\n{port:+#;-#;±0}\n{day:+#;-#;±0}\n{week:+#;-#;±0}";
            }
        }

        private int CutOverflow(int value, int limit)
        {
            if (value > limit)
                return limit;
            if (value < -limit)
                return -limit;
            return value;
        }
    }
}