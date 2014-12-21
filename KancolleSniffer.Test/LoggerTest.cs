// Copyright (C) 2014 Kazuhiro Fujieda <fujieda@users.sourceforge.jp>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.IO;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class LoggerTest
    {
        [TestMethod]
        public void InspectMissionResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            var header = "";
            sniffer.SetLogWriter((path, s, h) =>
            {
                result += s + "|";
                header = h;
            }, () => new DateTime(2015, 1, 1));
            sniffer.SkipMaster();
            sniffer.EnableLog(LogType.Mission);
            SnifferTest.SniffLogFile(sniffer, "mission_result_001");
            PAssert.That(() => "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材" == header);
            PAssert.That(() => "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0|" +
                               "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,1,0|" +
                               "2015-01-01 00:00:00,大成功,MO作戦,0,0,360,420,1,0,0|" +
                               "2015-01-01 00:00:00,失敗,東京急行(弐),0,0,0,0,0,0,0|"
                               == result);
        }

        /// <summary>
        /// 出力先に書けないときは一時ファイルに書き込む
        /// </summary>
        [TestMethod]
        public void WriteTmpFileWhenCantWriteTarget()
        {
            var mock = new Mock<LogWriter.IFile>();
            var writer = new LogWriter(null, mock.Object);

            const string str = "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0";
            var tmp = "";
            mock.Setup(l => l.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((file, s) =>
                {
                    if (file.EndsWith(".csv"))
                        throw new IOException();
                    if (file.EndsWith(".tmp"))
                        tmp += s;
                });
            mock.SetupSequence(l => l.Exists(It.IsAny<string>())).Returns(false).Returns(true);
            writer.Write("遠征報告書", str, "");
            PAssert.That(() => "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0\r\n" == tmp);
        }

        [TestMethod]
        public void MergeTmpFile()
        {
            MergeTmpFileMain(false);
        }

        [TestMethod]
        public void FaileToMergeTmpFile()
        {
            MergeTmpFileMain(true);
        }

        private void MergeTmpFileMain(bool failToMerge)
        {
            var mock = new Mock<LogWriter.IFile>();
            var writer = new LogWriter(null, mock.Object);

            var tmp = "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0\r\n";
            var csv = "";
            mock.Setup(l => l.Exists(It.IsAny<string>())).Returns(true);
            mock.Setup(l => l.ReadAllText(It.IsAny<string>()))
                .Returns((string path) => path.EndsWith(".tmp") ? tmp : "");
            mock.Setup(l => l.AppendAllText(It.IsAny<string>(), It.IsAny<string>()))
                .Callback<string, string>((file, s) =>
                {
                    if (file.EndsWith(".tmp"))
                        tmp += s;
                    if (file.EndsWith(".csv"))
                    {
                        if (failToMerge)
                            throw new IOException();
                        csv += s;
                    }
                });
            const string str = "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,1,0";
            writer.Write("遠征報告書", str, "");
            var result = failToMerge ? tmp : csv;
            PAssert.That(() =>
                "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0\r\n" +
                "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,1,0\r\n" == result);
        }

        [TestMethod]
        public void InspectBattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "battle_004");
            PAssert.That(() => "2015-01-01 00:00:00,珊瑚諸島沖,1,,S,同航戦,単縦陣,単縦陣,敵前衛艦隊,重巡洋艦,青葉," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),49/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),74/74,龍鳳改(Lv97),48/48," +
                               "軽巡ヘ級(flagship),57/57,重巡リ級(flagship),76/76,重巡リ級(flagship),76/76,雷巡チ級(elite),50/50,駆逐ニ級(elite),45/45,駆逐ニ級(elite),45/45|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,2,,B,反航戦,単横陣,単横陣,敵潜水艦隊,,," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),74/74,龍鳳改(Lv97),48/48," +
                               "潜水ヨ級(flagship),44/44,潜水カ級(elite),27/27,潜水カ級(elite),27/27,潜水カ級(elite),27/27,潜水カ級,19/19,潜水カ級,19/19|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,4,,S,反航戦,単縦陣,単縦陣,敵水上打撃部隊,戦艦,扶桑," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),45/74,龍鳳改(Lv97),48/48," +
                               "戦艦タ級(flagship),90/90,重巡リ級(flagship),76/76,重巡リ級(flagship),76/76,雷巡チ級(elite),50/50,駆逐ロ級(flagship),43/43,駆逐ニ級(elite),45/45|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,10,ボス,S,反航戦,単縦陣,単縦陣,敵機動部隊本隊,戦艦,山城," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),39/74,龍鳳改(Lv97),48/48," +
                               "装甲空母姫,270/270,空母ヲ級(elite),88/88,戦艦タ級(flagship),90/90,重巡リ級(elite),60/60,軽巡ホ級(flagship),53/53,駆逐ハ級(flagship),47/47|"
                               == result);
        }

        [TestMethod]
        public void InspectCombinedBattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "combined_surface_001");
            PAssert.That(() => "2015-01-01 00:00:00,南西方面海域,3,,S,同航戦,第四警戒航行序列,単縦陣,ピケット水雷戦隊 A群,,," +
                               "あきつ丸改(Lv68),40/40,山城改二(Lv85),77/77,扶桑改二(Lv85),77/77,利根改二(Lv117),59/66,筑摩改二(Lv117),51/65,神通改二(Lv97),47/50," +
                               "軽巡ツ級(elite),66/66,軽巡ト級(elite),55/55,駆逐イ級後期型,35/35,駆逐イ級後期型,35/35,駆逐イ級後期型,35/35,駆逐イ級後期型,35/35|" +
                               "2015-01-01 00:00:00,南西方面海域,7,,S,同航戦,第四警戒航行序列,複縦陣,任務部隊 D群,駆逐艦,満潮," +
                               "あきつ丸改(Lv68),40/40,山城改二(Lv85),77/77,扶桑改二(Lv85),77/77,利根改二(Lv117),33/66,筑摩改二(Lv117),51/65,神通改二(Lv97),47/50," +
                               "戦艦タ級(flagship),90/90,軽母ヌ級(flagship),84/84,軽巡ト級(elite),55/55,軽巡ト級(elite),55/55,駆逐ロ級後期型,37/37,駆逐ロ級後期型,37/37|"
                               == result);
        }

        [TestMethod]
        public void InspectMaterial()
        {
            var sniffer = new Sniffer();
            var result = "";
            var first = new DateTime(2015, 1, 1, 0, 0, 0);
            var queue =
                new Queue<DateTime>(new[] {first, first.AddMinutes(10), first.AddMinutes(15), first.AddMinutes(20)});
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, queue.Dequeue);
            sniffer.EnableLog(LogType.Material);
            SnifferTest.SniffLogFile(sniffer, "material_001");
            PAssert.That(() => "2015-01-01 00:00:00,26178,26742,21196,33750,1426,1574,2185,10,|" +
                               "2015-01-01 00:10:00,24595,25353,18900,32025,1427,1576,2187,10,|"
                               == result);
            SnifferTest.SniffLogFile(sniffer, "material_001");
            PAssert.That(() => "2015-01-01 00:00:00,26178,26742,21196,33750,1426,1574,2185,10,|" +
                               "2015-01-01 00:10:00,24595,25353,18900,32025,1427,1576,2187,10,|" +
                               "2015-01-01 00:20:00,24595,25353,18900,32025,1427,1576,2187,10,|"
                               == result);
        }

        [TestMethod]
        public void InspectCreateItem()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.CreateItem);
            SnifferTest.SniffLogFile(sniffer, "createitem_001");
            PAssert.That(() => "2015-01-01 00:00:00,7.7mm機銃,対空機銃,10,20,20,10,天津風改(127),114|" +
                               "2015-01-01 00:00:00,失敗,,10,20,20,10,天津風改(127),114|"
                               == result);
        }

        [TestMethod]
        public void InspectGetShip()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.CreateShip);
            SnifferTest.SniffLogFile(sniffer, "createship_001");
            PAssert.That(() => "2015-01-01 00:00:00,通常艦建造,球磨,軽巡洋艦,30,30,30,30,1,1,明石改(50),116|" +
                               "2015-01-01 00:00:00,大型艦建造,筑摩,重巡洋艦,1500,1500,2000,1000,1,0,明石改(50),116|"
                               == result);
        }
    }
}