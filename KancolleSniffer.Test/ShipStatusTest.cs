using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class ShipStatusTest
    {
        private static readonly ItemStatus 三式水中探信儀 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 47,
                Name = "三式水中探信儀",
                Type = 14,
                AntiSubmarine = 10
            }
        };

        private static readonly ItemStatus 三式爆雷投射機 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 45,
                Name = "三式爆雷投射機",
                Type = 15,
                AntiSubmarine = 8
            }
        };

        private static readonly ItemStatus 九五式爆雷 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 226,
                Name = "九五式爆雷",
                Type = 15,
                AntiSubmarine = 4
            }
        };

        // ReSharper disable once InconsistentNaming
        private static readonly ItemStatus SGレーダー初期型 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 315,
                Name = "SG レーダー(初期型)",
                Type = 12,
                AntiSubmarine = 3
            }
        };

        // ReSharper disable once InconsistentNaming
        private static readonly ItemStatus 試製15cm9連装対潜噴進砲 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 288,
                Name = "試製15cm9連装対潜噴進砲",
                Type = 15,
                AntiSubmarine = 15
            }
        };

        private static readonly ItemStatus 流星改 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 52,
                Name = "流星改",
                Type = 8,
                AntiSubmarine = 3
            }
        };

        private static readonly ItemStatus カ号観測機 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 69,
                Name = "カ号観測機",
                Type = 25,
                AntiSubmarine = 9
            }
        };

        private static readonly ItemStatus 三式指揮連絡機対潜 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 70,
                Name = "三式指揮連絡機(対潜)",
                Type = 26,
                AntiSubmarine = 7
            }
        };

        private static readonly ItemStatus 九九式艦爆 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 23,
                Name = "九九式艦爆",
                Type = 7,
                AntiSubmarine = 3
            }
        };

        private static readonly ItemStatus 水中聴音機零式 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 132,
                Name = "水中聴音機零式",
                Type = 40,
                AntiSubmarine = 11
            }
        };

        private static readonly ItemStatus 九七式艦攻九三一空 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 82,
                Name = "九七式艦攻(九三一空)",
                Type = 8,
                AntiSubmarine = 7,
                Torpedo = 6
            }
        };

        [TestClass]
        public class OpeningSubmarineAttack
        {
            /// <summary>
            /// 通常の先制対潜
            /// </summary>
            [TestMethod]
            public void CheckStandardCase()
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec {ShipType = 3},
                    Slot = new[] {三式水中探信儀},
                    AntiSubmarine = 99
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack, "対潜不足");
                ship.AntiSubmarine = 100;
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
                ship.Slot = new[] {new ItemStatus()};
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack, "ソナー未搭載");
            }


            /// <summary>
            /// 海防艦の先制対潜
            /// </summary>
            [TestMethod]
            public void CheckCoastGuard()
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec {ShipType = 1},
                    Slot = new[] {九五式爆雷},
                    AntiSubmarine = 74
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack, "対潜不足");
                ship.AntiSubmarine = 75;
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
                ship.Slot = new[]
                {
                    SGレーダー初期型
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack, "装備対潜不足");
            }

            /// <summary>
            /// 無条件で先制対潜が可能
            /// </summary>
            [DataTestMethod]
            [DataRow("五十鈴改二")]
            [DataRow("龍田改二")]
            [DataRow("Jervis改")]
            [DataRow("Samuel B.Roberts改")]
            [DataRow("Johnston")]
            [DataRow("Johnston改")]
            public void CheckNonConditional(string name)
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = name
                    }
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
            }

            /// <summary>
            /// 大鷹改・改二、神鷹改・改二
            /// </summary>
            [DataTestMethod]
            [DataRow("大鷹改")]
            [DataRow("大鷹改二")]
            [DataRow("神鷹改")]
            [DataRow("神鷹改二")]
            public void CheckSpecialEscortCarrier(string name)
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = name
                    },
                    Slot = new ItemStatus[0]
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[] {流星改};
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[] {カ号観測機};
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[] {三式指揮連絡機対潜};
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[] {九九式艦爆};
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
            }

            [DataTestMethod]
            [DataRow("大鷹")]
            [DataRow("Gambier Bay")]
            [DataRow("Gambier Bay改")]
            [DataRow("瑞鳳改二乙")]
            [DataRow("神鷹")]
            public void CheckEscortCarrierLevel50(string name)
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = name
                    },
                    Slot = new ItemStatus[0]
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 49;
                ship.Slot = new[]
                {
                    水中聴音機零式,
                    九七式艦攻九三一空
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 50;
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[]
                {
                    水中聴音機零式,
                    三式指揮連絡機対潜
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[]
                {
                    水中聴音機零式,
                    カ号観測機
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
            }

            [DataTestMethod]
            [DataRow("大鷹")]
            [DataRow("Gambier Bay")]
            [DataRow("Gambier Bay改")]
            [DataRow("瑞鳳改二乙")]
            [DataRow("神鷹")]
            public void CheckEscortCarrierLevel65(string name)
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = name
                    },
                    Slot = new ItemStatus[0]
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 64;
                ship.Slot = new[]
                {
                    九七式艦攻九三一空
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 65;
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[]
                {
                    カ号観測機
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[]
                {
                    三式指揮連絡機対潜
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
            }

            [DataTestMethod]
            [DataRow("大鷹")]
            [DataRow("Gambier Bay")]
            [DataRow("Gambier Bay改")]
            [DataRow("瑞鳳改二乙")]
            [DataRow("神鷹")]
            public void CheckEscortCarrierLevel100(string name)
            {
                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = name
                    },
                    Slot = new ItemStatus[0]
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 100;
                ship.Slot = new[]
                {
                    水中聴音機零式,
                    カ号観測機
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.Slot = new[]
                {
                    水中聴音機零式,
                    流星改
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 99;
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 100;
                ship.Slot = new[]
                {
                    水中聴音機零式,
                    九九式艦爆
                };
                Assert.IsTrue(ship.CanOpeningAntiSubmarineAttack);
            }

            [TestMethod]
            public void 瑞鳳改二()
            {
                CheckEscortCarrierLevel50("瑞鳳改二");

                var ship = new ShipStatus
                {
                    Spec = new ShipSpec
                    {
                        Name = "瑞鳳改二"
                    },
                    Slot = new ItemStatus[0]
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 65;
                ship.Slot = new[]
                {
                    カ号観測機
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);

                ship.AntiSubmarine = 100;
                ship.Slot = new[]
                {
                    水中聴音機零式,
                    流星改
                };
                Assert.IsFalse(ship.CanOpeningAntiSubmarineAttack);
            }
        }

        [TestClass]
        public class NightBattlePower
        {
            [TestMethod]
            public void 甲標的の改修効果()
            {
                var ship = new ShipStatus
                {
                    Torpedo = 102,
                    Slot = new[]
                    {
                        new ItemStatus
                        {
                            Id = 1,
                            Spec = new ItemSpec
                            {
                                Id = 309,
                                Name = "甲標的 丙型",
                                AntiSubmarine = 14,
                                Type = 22
                            },
                            Level = 4
                        }
                    }
                };
                Assert.AreEqual(104, ship.NightBattlePower);
            }
        }

        [TestClass]
        public class AntiSubmarine
        {
            [TestMethod]
            public void 軽空母()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 49,
                    Spec = new ShipSpec
                    {
                        ShipType = 7
                    },
                    AntiSubmarine = 47 + 11,
                    Slot = new[]
                    {
                        水中聴音機零式
                    }
                };
                Assert.AreEqual(0, ship.EffectiveAntiSubmarine, "艦載機なし");

                ship.AntiSubmarine = 47 + 18;
                ship.Slot = new[]
                {
                    九七式艦攻九三一空,
                    水中聴音機零式
                };
                Assert.AreEqual("48.7", ship.EffectiveAntiSubmarine.ToString("f1"), "艦載機あり");
            }

            [TestMethod]
            public void 水上機母艦()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 58,
                    Spec = new ShipSpec
                    {
                        ShipType = 16
                    },
                    AntiSubmarine = 10,
                    Slot = new[]
                    {
                        三式水中探信儀
                    }
                };
                Assert.AreEqual(0, ship.EffectiveAntiSubmarine);

                ship.AntiSubmarine = 19;
                ship.Slot = new[]
                {
                    三式水中探信儀,
                    カ号観測機
                };
                Assert.AreEqual("36.5", ship.EffectiveAntiSubmarine.ToString("f1"));
            }

            [TestMethod]
            public void 対潜装備一つ()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 50,
                    Spec = new ShipSpec
                    {
                        ShipType = 2
                    },
                    AntiSubmarine = 63 + 10,
                    Slot = new[]
                    {
                        三式水中探信儀
                    }
                };
                Assert.AreEqual("43.9", ship.EffectiveAntiSubmarine.ToString("f1"));

                ship.AntiSubmarine = 63 + 8;
                ship.Slot = new[]
                {
                    三式爆雷投射機
                };
                Assert.AreEqual("40.9", ship.EffectiveAntiSubmarine.ToString("f1"));

                ship.AntiSubmarine = 63 + 4;
                ship.Slot = new[]
                {
                    九五式爆雷
                };
                Assert.AreEqual("34.9", ship.EffectiveAntiSubmarine.ToString("f1"));
            }

            [TestMethod]
            public void 爆雷投射機と爆雷()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 50,
                    Spec = new ShipSpec
                    {
                        ShipType = 2
                    },
                    AntiSubmarine = 63 + 12,
                    Slot = new[]
                    {
                        三式爆雷投射機,
                        九五式爆雷
                    }
                };
                Assert.AreEqual("51.6", ship.EffectiveAntiSubmarine.ToString("f1"));
            }

            [TestMethod]
            public void ソナーとそれ以外()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 50,
                    Spec = new ShipSpec
                    {
                        ShipType = 2
                    },
                    AntiSubmarine = 63 + 18,
                    Slot = new[]
                    {
                        三式水中探信儀,
                        三式爆雷投射機
                    }
                };
                Assert.AreEqual("64.3", ship.EffectiveAntiSubmarine.ToString("f1"));

                ship.AntiSubmarine = 63 + 14;
                ship.Slot = new[]
                {
                    三式水中探信儀,
                    九五式爆雷
                };
                Assert.AreEqual("57.4", ship.EffectiveAntiSubmarine.ToString("f1"));

                ship.AntiSubmarine = 63 + 25;
                ship.Slot = new[]
                {
                    三式水中探信儀,
                    試製15cm9連装対潜噴進砲
                };
                Assert.AreEqual("76.3", ship.EffectiveAntiSubmarine.ToString("f1"));
            }

            [TestMethod]
            public void 三種コンビネーション()
            {
                var ship = new ShipStatus
                {
                    Fleet = new Fleet(null, 0, null),
                    Firepower = 50,
                    Spec = new ShipSpec
                    {
                        ShipType = 2
                    },
                    AntiSubmarine = 63 + 22,
                    Slot = new[]
                    {
                        三式水中探信儀,
                        三式爆雷投射機,
                        九五式爆雷
                    }
                };
                Assert.AreEqual("88.9", ship.EffectiveAntiSubmarine.ToString("f1"));

                ship.AntiSubmarine = 63 + 33;
                ship.Slot = new[]
                {
                    三式水中探信儀,
                    試製15cm9連装対潜噴進砲,
                    九五式爆雷
                };
                Assert.AreEqual("83.8", ship.EffectiveAntiSubmarine.ToString("f1"), "三種コンビネーションにならない");
            }
        }

        // ReSharper disable once InconsistentNaming
        private static readonly ItemStatus A12cm30連装噴進砲改二 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec
            {
                Id = 274,
                Type = 21,
                IconType = 15,
                AntiAir = 8
            }
        };

        // ReSharper disable once InconsistentNaming
        private static readonly ItemStatus A25mm三連装機銃集中配備 = new ItemStatus
        {
            Id = 1,
            Spec = new ItemSpec{
                Id = 131,
                Type = 21,
                IconType = 15,
                AntiAir = 9
            }
        };

        [TestClass]
        public class AntiAirPropellantBarrageChance
        {
            private ShipStatus _ship;

            [TestInitialize]
            public void Initialize()
            {
                _ship =new ShipStatus
                {
                    AntiAir = 93,
                    Lucky = 46,
                    Spec = new ShipSpec
                    {
                        ShipType = 4,
                    },
                    Slot = new ItemStatus[0]
                };
            }

            [TestMethod]
            public void 噴進砲改二なし()
            {
                Assert.AreEqual(0, _ship.AntiAirPropellantBarrageChance);
            }

            [TestMethod]
            public void 噴進砲改二1つ()
            {
                _ship.AntiAir = 85 + 8;
                _ship.Slot = new[]
                {
                    A12cm30連装噴進砲改二
                };
                Assert.AreEqual("63.1", _ship.AntiAirPropellantBarrageChance.ToString("f1"));
            }

            [TestMethod]
            public void 噴進砲改二2つ()
            {
                _ship.AntiAir = 85 + 16;
                _ship.Slot = new[]
                {
                    A12cm30連装噴進砲改二,
                    A12cm30連装噴進砲改二
                };
                Assert.AreEqual("95.1", _ship.AntiAirPropellantBarrageChance.ToString("f1"));
            }

            [TestMethod]
            public void 噴進砲改二2つと機銃()
            {
                _ship.AntiAir = 85 + 25;
                _ship.Slot = new[]
                {
                    A12cm30連装噴進砲改二,
                    A12cm30連装噴進砲改二,
                    A25mm三連装機銃集中配備
                };
                Assert.AreEqual("114.3", _ship.AntiAirPropellantBarrageChance.ToString("f1"), "噴進砲改二2+機銃");
            }

            [TestMethod]
            public void 伊勢型()
            {
                _ship.AntiAir = 85 + 8;
                _ship.Slot = new[]
                {
                    A12cm30連装噴進砲改二
                };
                _ship.Spec.ShipClass = 2;
                Assert.AreEqual("88.1", _ship.AntiAirPropellantBarrageChance.ToString("f1"));
            }
        }
    }
}