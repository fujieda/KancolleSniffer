using System.Linq;
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
    }


}