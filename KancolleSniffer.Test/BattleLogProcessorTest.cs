﻿using System.Linq;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class BattleLogProcessorTest
    {
        [TestMethod]
        public void NormalLog()
        {
            var input = Enumerable.Repeat("", 38).ToArray();
            input[5] = "Ｔ字戦(有利)";
            input[11] = "龍鳳改(Lv97)";
            input[12] = "3/48";
            input[13] = "夕立改(Lv148)";
            input[14] = "5/36";
            input[15] = "綾波改二(Lv148)";
            input[16] = "20/37";
            input[37] = "制空権確保";
            var result = BattleLogProcessor.Process(input).ToArray();
            PAssert.That(() => result[5] == "Ｔ字有利");
            PAssert.That(() => result[23] == "龍鳳改(Lv97)・夕立改(Lv148)");
            PAssert.That(() => result[38] == "確保");
        }

        [TestMethod]
        public void CombinedLog()
        {
            var input = Enumerable.Repeat("", 38).ToArray();
            input[6] = "第四警戒航行序列";
            input[11] = "龍鳳改(Lv97)・夕立改(Lv148)";
            input[12] = "3/48・5/36";
            input[37] = "航空劣勢";
            var result = BattleLogProcessor.Process(input).ToArray();
            PAssert.That(() => result[6] == "第四警戒");
            PAssert.That(() => result[23] == "龍鳳改(Lv97)・夕立改(Lv148)");
            PAssert.That(() => result[38] == "劣勢");
        }

        [TestMethod]
        public void NormalLogWithKana()
        {
            var input = Enumerable.Repeat("", 38).ToArray();
            input[11] = "Luigi Torelli(ルイージ・トレッリ)(Lv7)";
            input[12] = "2/11";
            var result = BattleLogProcessor.Process(input).ToArray();
            PAssert.That(() => result[11] == "Luigi Torelli(Lv7)");
            PAssert.That(() => result[23] == "Luigi Torelli(Lv7)");
        }

        [TestMethod]
        public void Ship7BattleLog()
        {
            var input = new[]
            {
                "2017-11-20 20:59:39", "台湾沖/ルソン島沖", "1", "出撃", "S", "反航戦", "単横陣", "梯形陣", "深海潜水艦部隊 通商破壊Aライン", "", "",
                "あきつ丸改(Lv81)", "40/40", "那智改二(Lv151)", "63/63", "Roma改(Lv99)", "92/92", "阿武隈改二(Lv98)", "40/45",
                "霞改二(Lv96)", "31/31", "潮改二(Lv94)", "33/33", "龍驤改二(Lv99)", "50/50", "潜水カ級(flagship)", "0/37",
                "潜水カ級(flagship)", "0/37", "潜水カ級(elite)", "0/27", "潜水カ級(elite)", "0/27", "", "", "", "", "590", "0",
                "制空権確保"
            };
            var result = BattleLogProcessor.Process(input).ToArray();
            PAssert.That(() => result[21] == "潮改二(Lv94)・龍驤改二(Lv99)" &&
                               result[22] == "33/33・50/50");
            PAssert.That(() => result.Length == 39);
        }
    }
}