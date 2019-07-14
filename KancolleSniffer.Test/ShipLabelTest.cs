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

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Reflection;
using System.Windows.Forms;
using ExpressionToCodeLib;
using KancolleSniffer.Model;
using KancolleSniffer.View;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class ShipLabelTest
    {
        [TestClass]
        public class TruncateTest
        {
            private class TestData : Dictionary<int, Pair[]>
            {
            }

            private class Pair
            {
                public readonly string Origin;
                public readonly string Result;

                public Pair()
                {
                    Origin = "";
                    Result = "";
                }

                public Pair(string origin, string result)
                {
                    Origin = origin;
                    Result = result;
                }
            }

            /// <summary>
            /// 明石タイマー表示中の艦娘の名前を縮める
            /// </summary>
            [TestMethod]
            public void ForAkashiTimer()
            {
                var data = new TestData
                {
                    {
                        100,
                        new[]
                        {
                            new Pair("夕立改二", "夕立改二"),
                            new Pair("千代田航改", "千代田航"),
                            new Pair("千代田航改二", "千代田航"),
                            new Pair("Bismarck改", "Bismarck"),
                            new Pair("Bismarck zwei", "Bismarck"),
                            new Pair("Bismarck drei", "Bismarck"),
                            new Pair("Prinz Eugen", "Prinz Eug"),
                            new Pair("Prinz Eugen改", "Prinz Eug"),
                            new Pair("Graf Zeppelin", "Graf Zep"),
                            new Pair("Graf Zeppelin改", "Graf Zep"),
                            new Pair("Libeccio改", "Libeccio")
                        }
                    },
                    {
                        125,
                        new[]
                        {
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair(),
                            new Pair("Graf Zeppelin", "Graf Zepp"),
                            new Pair("Graf Zeppelin改", "Graf Zepp"),
                            new Pair("Libeccio改", "Libeccio改")
                        }
                    }
                };
                TestTruncate(data, ShipNameWidth.AkashiTimer);
            }

            /// <summary>
            /// 入渠中の艦娘名の名前を縮める
            /// </summary>
            [TestMethod]
            public void ForNDock()
            {
                var data = new TestData
                {
                    {
                        100, new[]
                        {
                            new Pair("千歳航改二", "千歳航改二"),
                            new Pair("千代田航改二", "千代田航改"),
                            new Pair("Graf Zeppelin", "Graf Zeppeli"),
                            new Pair("Graf Zeppelin改", "Graf Zeppeli")
                        }
                    },
                    {
                        125, new[]
                        {
                            new Pair(),
                            new Pair(),
                            new Pair("Graf Zeppelin", "Graf Zeppelin"),
                            new Pair("Graf Zeppelin改", "Graf Zeppelin")
                        }
                    }
                };
                TestTruncate(data, ShipNameWidth.NDock);
            }

            /// <summary>
            /// 一覧ウィンドウの要修復一覧の艦娘の名前を縮める
            /// </summary>
            [TestMethod]
            public void ForRepairListFull()
            {
                var data = new TestData
                {
                    {
                        100,
                        new[]
                        {
                            new Pair("Graf Zeppelin", "Graf Zeppelin"),
                            new Pair("Graf Zeppelin改", "Graf Zeppelin"),
                            new Pair("千代田航改二", "千代田航改")
                        }
                    },
                    {
                        125,
                        new[]
                        {
                            new Pair(),
                            new Pair(),
                            new Pair("千代田航改二", "千代田航改二")
                        }
                    }
                };
                TestTruncate(data, ShipNameWidth.RepairListFull);
            }

            /// <summary>
            /// メインパネルの艦娘の名前を縮める
            /// </summary>
            [TestMethod]
            public void ForMainPanel()
            {
                var data = new TestData
                {
                    {
                        100,
                        new[]
                        {
                            new Pair("Commandant Teste", "Commandant Tes")
                        }
                    },
                    {
                        125,
                        new[]
                        {
                            new Pair("Commandant Teste", "Commandant Test")
                        }
                    }
                };
                TestTruncate(data, ShipNameWidth.MainPanel);
            }

            /// <summary>
            /// 一覧ウィンドウの艦娘一覧の名前を縮める
            /// </summary>
            [TestMethod]
            public void ForShipList()
            {
                var data = new TestData
                {
                    {
                        100,
                        new[]
                        {
                            new Pair("Commandant Teste", "Commandant T"),
                            new Pair("Graf Zeppelin改", "Graf Zeppelin")
                        }
                    },
                    {
                        125,
                        new[]
                        {
                            new Pair(),
                            new Pair("Graf Zeppelin改", "Graf Zeppelin改")
                        }
                    }
                };
                TestTruncate(data, ShipNameWidth.ShipList);
            }

            private static readonly Font LatinFont = new Font("Tahoma", 8f);

            private static void TestTruncate(TestData data, ShipNameWidth width)
            {
                foreach (var zoom in data.Keys)
                {
                    SetScaleFactor(zoom);
                    var label = CreateLabel(zoom, width);
                    for (var i = 0; i < data[zoom].Length; i++)
                    {
                        var entry = data[zoom][i];
                        if (string.IsNullOrEmpty(entry.Origin))
                            entry = data[100][i];
                        label.SetName(entry.Origin);
                        Assert.AreEqual(entry.Result, label.Text, $"{entry.Origin}: scale {zoom}");
                    }
                }
            }

            private static ShipLabel.Name CreateLabel(int zoom, ShipNameWidth width)
            {
                var label = new ShipLabel.Name(Point.Empty, width) {Parent = new Panel()};
                label.Parent.Font = ZoomFont(label.Parent.Font, zoom);
                ShipLabel.Name.LatinFont = ZoomFont(LatinFont, zoom);
                return label;
            }

            private static void SetScaleFactor(int zoom)
            {
                if (zoom == 100)
                {
                    Scaler.Factor = new SizeF(1, 1);
                    return;
                }
                var form = new Form {AutoScaleMode = AutoScaleMode.Font};
                var prev = form.CurrentAutoScaleDimensions;
                form.Font = ZoomFont(form.Font, zoom);
                var cur = form.CurrentAutoScaleDimensions;
                Scaler.Factor = new SizeF(cur.Width / prev.Width, cur.Height / prev.Height);
            }

            private static Font ZoomFont(Font font, int zoom)
            {
                return zoom == 100 ? font : new Font(font.FontFamily, font.Size * zoom / 100);
            }
        }

        /// <summary>
        /// prefixを加える
        /// </summary>
        [TestMethod]
        public void SetName()
        {
            var label = new ShipLabel.Name(Point.Empty, ShipNameWidth.AkashiTimer) {Parent = new Panel()};
            Scaler.Factor = new SizeF(1, 1);
            label.Set(new ShipStatus
            {
                Spec = new ShipSpec {Name = "綾波改二"},
                Escaped = true
            });
            PAssert.That(() => label.Text == "[避]綾波改二");
            label.Set(new ShipStatus
            {
                Spec = new ShipSpec {Name = "朝潮改二丁"},
                Escaped = true
            });
            PAssert.That(() => label.Text == "[避]朝潮改二");
        }

        /// <summary>
        /// %表示の小数部を切り捨てる
        /// </summary>
        [TestMethod]
        public void RoundOffFractionOfPercent()
        {
            var label = new ShipLabel.Hp {Parent = new Panel()};
            label.SetHp(104, 105);
            label.ToggleHpPercent();
            PAssert.That(() => label.Text == "99%");
        }


        /// <summary>
        /// 装備スロットの状況を調べる
        /// </summary>
        [TestMethod]
        public void SlotStatus()
        {
            var ship = new ShipStatus
            {
                Id = 1,
                Slot = new[] {new ItemStatus(), new ItemStatus(), new ItemStatus()}, SlotEx = new ItemStatus(0),
                Spec = new ShipSpec {SlotNum = 3}
            };
            Assert.AreEqual(2, GetSlotStatus(ship)); // NormalEmpty
            ship.SlotEx.Id = -1;
            Assert.AreEqual(2 | 4, GetSlotStatus(ship)); // | ExtraEmpty
            ship.SlotEx.Id = 1;
            Assert.AreEqual(2, GetSlotStatus(ship)); // NormalEmpty
            ship.Slot[0].Id = ship.Slot[1].Id = ship.Slot[2].Id = 1;
            Assert.AreEqual(0, GetSlotStatus(ship)); // Equipped
            ship.Slot[2].Id = -1;
            Assert.AreEqual(1, GetSlotStatus(ship)); // SemiEquipped
            ship.Spec.SlotNum = 2;
            Assert.AreEqual(0, GetSlotStatus(ship)); // Equipped
            ship.Spec.SlotNum = 0;
            Assert.AreEqual(0, GetSlotStatus(ship)); // Equipped (まるゆ)
        }

        private static int GetSlotStatus(ShipStatus ship)
        {
            var method =
                typeof(ShipLabel.Name).GetMethod("GetSlotStatus", BindingFlags.NonPublic | BindingFlags.Static);
            // ReSharper disable once PossibleNullReferenceException
            return (int)Convert.ChangeType(method.Invoke(null, new object[] {ship}), TypeCode.Int32);
        }
    }
}