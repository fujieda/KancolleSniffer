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
        public Label FuelHistory { get; set; }
        public Label BulletHistory { get; set; }
        public Label SteelHistory { get; set; }
        public Label BauxiteHistory { get; set; }
        public ToolTip ToolTip { get; set; }
    }

    public class NumberAndHistory
    {
        private readonly NumberAndHistoryLabels _labels;
        private readonly Sniffer _sniffer;
        private readonly ToolTip _toolTip;
        private readonly MainForm.INotifySubmitter _submitter;

        public NumberAndHistory(NumberAndHistoryLabels labels, Sniffer sniffer, MainForm.INotifySubmitter submitter)
        {
            _labels = labels;
            _sniffer = sniffer;
            _toolTip = labels.ToolTip;
            _submitter = submitter;
        }

        public void Update()
        {
            UpdateMaterialHistory();
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