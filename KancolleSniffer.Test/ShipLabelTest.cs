// Copyright (C) 2016 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Collections.Generic;
using System.Windows.Forms;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class ShipLabelTest
    {
        /// <summary>
        /// 明石タイマー表示中の艦娘の名前を縮める
        /// </summary>
        [TestMethod]
        public void TruncateNameForAkashiTimer()
        {
            var dict = new Dictionary<string, string>
            {
                {"夕立改二", "夕立改二"},
                {"千代田航改", "千代田航"},
                {"千代田航改二", "千代田航"},
                {"Bismarck改", "Bismarck"},
                {"Bismarck twei", "Bismarck"},
                {"Bismarck drei", "Bismarck"},
                {"Prinz Eugen", "Prinz Eug"},
                {"Prinz Eugen改", "Prinz Eug"},
                {"Graf Zeppelin", "Graf Zep"},
                {"Graf Zeppelin改", "Graf Zep"},
                {"Libeccio改", "Libeccio"},
            };
            TruncateNameSub(dict, ShipNameWidth.AkashiTimer);
        }

        /// <summary>
        /// 入渠中の艦娘名の名前を縮める
        /// </summary>
        [TestMethod]
        public void TruncateNameForNDock()
        {
            var dict = new Dictionary<string, string>
            {
                {"千歳航改二", "千歳航改二"},
                {"Graf Zeppelin", "Graf Zeppeli"},
                {"Graf Zeppelin改", "Graf Zeppeli"},
                {"千代田航改二", "千代田航改"}
            };
            TruncateNameSub(dict, ShipNameWidth.NDock);
        }

        /// <summary>
        /// 一覧ウィンドウの要修復一覧の艦娘の名前を縮める
        /// </summary>
        [TestMethod]
        public void TruncateNameForRepairListFull()
        {
            var dict = new Dictionary<string, string>
            {
                {"Graf Zeppelin", "Graf Zeppelin"},
                {"Graf Zeppelin改", "Graf Zeppelin"},
                {"千代田航改二", "千代田航改"}
            };
            TruncateNameSub(dict, ShipNameWidth.RepairListFull);
        }

        /// <summary>
        /// メインパネルの艦娘の名前を縮める
        /// </summary>
        [TestMethod]
        public void TruncateNameForMainPanel()
        {
            var dict = new Dictionary<string, string>
            {
                {"Commandant Teste", "Commandant Tes"}
            };
            TruncateNameSub(dict, ShipNameWidth.MainPanel);
        }

        [TestMethod]
        public void TruncateNameForShipList()
        {
            var dict = new Dictionary<string, string>
            {
                {"Commandant Test", "Commandant T"},
                {"Graf Zeppelin改", "Graf Zeppelin"}
            };
            TruncateNameSub(dict, ShipNameWidth.ShipList);
        }

        private void TruncateNameSub(Dictionary<string, string> dict, ShipNameWidth width)
        {
            var label = new ShipLabel {Parent = new Panel()};
            foreach (var entry in dict)
            {
                label.SetName("", entry.Key, "", width);
                PAssert.That(() => label.Text == entry.Value, entry.Key);
            }
        }


        /// <summary>
        /// prefixとsuffixを加える
        /// </summary>
        [TestMethod]
        public void SetName()
        {
            var label = new ShipLabel {Parent = new Panel()};
            label.SetName("[避]", "綾波改二", "▫");
            PAssert.That(() => label.Text == "[避]綾波改二▫");
            label.SetName("[避]", "朝潮改二丁", "▫", ShipNameWidth.AkashiTimer);
            PAssert.That(() => label.Text == "[避]朝潮改二▫");
        }
    }
}