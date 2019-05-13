﻿using KancolleSniffer.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class ShipStatusTest
    {
        [TestClass]
        public class OpeningSubmarineAttack
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


            private static readonly ItemStatus 九五式爆雷 = new ItemStatus
            {
                Id = 1,
                Spec = new ItemSpec
                {
                    Id = 47,
                    Name = "九五式爆雷",
                    Type = 15,
                    AntiSubmarine = 4
                }
            };

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
                ship.Slot[0].Spec.AntiSubmarine = 3;
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
                    AntiSubmarine = 7
                }
            };

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
    }
}