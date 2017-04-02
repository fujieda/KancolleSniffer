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
            PAssert.That(() => "2015-01-01 00:00:00,珊瑚諸島沖,1,出撃,S,同航戦,単縦陣,単縦陣,敵前衛艦隊,重巡洋艦,青葉," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),49/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),74/74,龍鳳改(Lv97),48/48," +
                               "軽巡ヘ級(flagship),0/57,重巡リ級(flagship),0/76,重巡リ級(flagship),0/76,雷巡チ級(elite),0/50,駆逐ニ級(elite),0/45,駆逐ニ級(elite),0/45," +
                               "306～314,0,制空権確保|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,2,,B,反航戦,単横陣,単横陣,敵潜水艦隊,,," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),74/74,龍鳳改(Lv97),48/48," +
                               "潜水ヨ級(flagship),44/44,潜水カ級(elite),27/27,潜水カ級(elite),5/27,潜水カ級(elite),9/27,潜水カ級,0/19,潜水カ級,0/19," +
                               "302～311,0,制空権確保|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,4,,S,反航戦,単縦陣,単縦陣,敵水上打撃部隊,戦艦,扶桑," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),45/74,龍鳳改(Lv97),48/48," +
                               "戦艦タ級(flagship),0/90,重巡リ級(flagship),0/76,重巡リ級(flagship),0/76,雷巡チ級(elite),0/50,駆逐ロ級(flagship),0/43,駆逐ニ級(elite),0/45," +
                               "301～310,0,制空権確保|" +
                               "2015-01-01 00:00:00,珊瑚諸島沖,10,ボス,S,反航戦,単縦陣,単縦陣,敵機動部隊本隊,戦艦,山城," +
                               "武蔵改(Lv133),86/106,扶桑改二(Lv87),77/77,北上改二(Lv113),46/49,飛龍改二(Lv133),63/74,蒼龍改二(Lv133),39/74,龍鳳改(Lv97),48/48," +
                               "装甲空母姫,0/270,空母ヲ級(elite),0/88,戦艦タ級(flagship),0/90,重巡リ級(elite),0/60,軽巡ホ級(flagship),0/53,駆逐ハ級(flagship),0/47," +
                               "294～304,75,制空権確保|"
                               == result);
        }

        [TestMethod]
        public void InspectBattleResultOfSpMidnight()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "sp_midnight_001");
            PAssert.That(() => "2015-01-01 00:00:00,サブ島沖海域,3,出撃,D,反航戦,単縦陣,単縦陣,敵前衛警戒艦隊,,," +
                               "Prinz Eugen改(Lv52),1/63,,,,,,,,,,," +
                               "軽巡ヘ級(flagship),57/57,重巡リ級(flagship),76/76,重巡リ級(flagship),76/76,雷巡チ級(elite),50/50,雷巡チ級(elite),50/50,駆逐ロ級(flagship),0/43," +
                               "0,0,|"
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
            PAssert.That(() =>
                "2015-01-01 00:00:00,南西方面海域,3,出撃,S,同航戦,第四警戒航行序列,単縦陣,ピケット水雷戦隊 A群,,," +
                "あきつ丸改(Lv68)・大淀改(Lv95),40/40・47/47,山城改二(Lv85)・Z1 zwei(Lv84),77/77・35/35,扶桑改二(Lv85)・Z3 zwei(Lv84),77/77・11/35,利根改二(Lv117)・島風改(Lv130),59/66・39/41,筑摩改二(Lv117)・雪風改(Lv130),51/65・37/37,神通改二(Lv97)・北上改二(Lv99),47/50・40/43," +
                "軽巡ツ級(elite),0/66,軽巡ト級(elite),0/55,駆逐イ級後期型,0/35,駆逐イ級後期型,0/35,駆逐イ級後期型,0/35,駆逐イ級後期型,0/35," +
                "144～149,0,制空権確保|" +
                "2015-01-01 00:00:00,南西方面海域,7,,S,同航戦,第四警戒航行序列,複縦陣,任務部隊 D群,駆逐艦,満潮," +
                "あきつ丸改(Lv68)・大淀改(Lv95),40/40・39/47,山城改二(Lv85)・Z1 zwei(Lv84),77/77・35/35,扶桑改二(Lv85)・Z3 zwei(Lv84),77/77・11/35,利根改二(Lv117)・島風改(Lv130),33/66・39/41,筑摩改二(Lv117)・雪風改(Lv130),51/65・37/37,神通改二(Lv97)・北上改二(Lv99),47/50・40/43," +
                "戦艦タ級(flagship),0/90,軽母ヌ級(flagship),0/84,軽巡ト級(elite),0/55,軽巡ト級(elite),0/55,駆逐ロ級後期型,0/37,駆逐ロ級後期型,0/37," +
                "123～131,23,制空権確保|"
                == result);
        }

        [TestMethod]
        public void InspectBothCombinedBattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2017, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "both_combined_001");
            PAssert.That(() =>
                "2017-01-01 00:00:00,本土沖太平洋上,27,出撃&ボス,S,同航戦,第四警戒航行序列,第三警戒航行序列,16th任務部隊 主力機動部隊群,駆逐艦,天津風," +
                "愛宕改(Lv98)・暁改二(Lv84),55/57・28/31,金剛改二(Lv99)・初月改(Lv98),69/82・33/37,飛龍改二(Lv99)・高雄改(Lv98),16/67・57/57,蒼龍改二(Lv99)・神通改二(Lv99),40/67・11/50,龍驤改二(Lv99)・木曾改二(Lv99),50/50・40/44,祥鳳改(Lv76)・北上改二(Lv97),26/45・43/43," +
                "水母水姫・軽巡ヘ級(flagship),0/390・0/57,空母ヲ級改(flagship)・重巡リ級(flagship),0/160・0/76,空母ヲ級改(flagship)・重巡リ級(flagship),0/160・0/76,戦艦タ級(elite)・軽巡ツ級(elite),0/88・0/66,重巡ネ級(elite)・駆逐イ級後期型,0/88・0/35,重巡ネ級(elite)・駆逐イ級後期型,0/88・0/35," +
                "750～751,389,航空優勢|"
                == result);
        }

        [TestMethod]
        public void InspectBattleResultStartAndBoss()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "battle_005");
            PAssert.That(() => "2015-01-01 00:00:00,バシー島沖,7,出撃&ボス,S,同航戦,単縦陣,単縦陣,敵通商破壊艦隊,軽空母,龍驤," +
                               "那珂改二(Lv97),48/48,隼鷹改二(Lv129),62/62,北上改二(Lv129),49/49,大井改二(Lv115),40/49,呂500(Lv62),13/13,伊168改(Lv97),15/15," +
                               "重巡リ級(elite),0/60,重巡リ級,0/58,雷巡チ級(elite),0/50,軽巡ヘ級,0/36,駆逐ニ級,0/28,駆逐ニ級,0/28," +
                               "108～111,0,制空権確保|"
                               == result);
        }

        [TestMethod]
        public void InspectBattleResultDropItem()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "dropitem_001");
            PAssert.That(() =>
                "2015-01-01 00:00:00,南西諸島防衛線,2,出撃,S,Ｔ字戦(有利),単縦陣,単縦陣,敵偵察艦隊,駆逐艦,白雪," +
                "呂500(Lv56),13/13,伊168改(Lv97),15/15,伊58改(Lv97),18/18,伊8改(Lv97),19/19,伊19改(Lv97),18/18,伊401改(Lv99),24/24," +
                "重巡リ級,0/58,軽巡ヘ級,0/36,駆逐イ級,0/20,駆逐イ級,0/20,駆逐イ級,0/20,,," +
                "15,0,制空権確保|" +
                "2015-01-01 00:00:00,南西諸島防衛線,10,ボス,S,反航戦,単縦陣,輪形陣,敵機動部隊,アイテム,菱餅," +
                "呂500(Lv56),13/13,伊168改(Lv97),15/15,伊58改(Lv97),18/18,伊8改(Lv97),19/19,伊19改(Lv97),18/18,伊401改(Lv99),24/24," +
                "空母ヲ級,0/85,空母ヲ級,0/85,重巡リ級,0/58,軽巡ヘ級,0/36,駆逐ハ級,0/24,駆逐ハ級,0/24," +
                "15,20,航空均衡|"
                == result);
        }

        [TestMethod]
        public void InspectBattleResultDropItemAndShip()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "dropitem_002");
            PAssert.That(() =>
                "2015-01-01 00:00:00,アルフォンシーノ方面,1,出撃,S,同航戦,単縦陣,単縦陣,敵前衛艦隊,,," +
                "飛龍改(Lv79),65/65,翔鶴改二(Lv112),85/85,隼鷹改二(Lv139),62/62,足柄改二(Lv139),63/63,大井改二(Lv133),49/49,北上改二(Lv133),49/49," +
                "軽巡ヘ級(flagship),0/57,重巡リ級(elite),0/60,軽巡ト級(elite),0/55,雷巡チ級(elite),0/50,駆逐ロ級(elite),0/35,駆逐ロ級(elite),0/35," +
                "257～258,0,制空権確保|" +
                "2015-01-01 00:00:00,アルフォンシーノ方面,3,,S,同航戦,複縦陣,梯形陣,敵護衛空母群,重巡洋艦,筑摩," +
                "飛龍改(Lv79),65/65,翔鶴改二(Lv112),85/85,隼鷹改二(Lv139),54/62,足柄改二(Lv139),63/63,大井改二(Lv133),46/49,北上改二(Lv133),49/49," +
                "軽母ヌ級(elite),0/70,軽母ヌ級(elite),0/70,軽母ヌ級(elite),0/70,軽巡ホ級(flagship),0/53,駆逐ニ級(elite),0/45,駆逐ニ級(elite),0/45," +
                "255～257,72,制空権確保|" +
                "2015-01-01 00:00:00,アルフォンシーノ方面,11,ボス,S,Ｔ字戦(有利),単縦陣,単縦陣,深海棲艦泊地艦隊,駆逐艦+アイテム,舞風+秋刀魚," +
                "飛龍改(Lv79),65/65,翔鶴改二(Lv112),44/85,隼鷹改二(Lv139),54/62,足柄改二(Lv139),50/63,大井改二(Lv133),46/49,北上改二(Lv133),49/49," +
                "空母ヲ級(flagship),0/96,空母ヲ級(elite),0/88,戦艦ル級(flagship),0/98,軽巡ヘ級(flagship),0/57,軽巡ト級(elite),0/55,駆逐ニ級(elite),0/45," +
                "255～256,55,制空権確保|"
                == result);
        }

        [TestMethod]
        public void ReloadBeforeBattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "reload_002");
            PAssert.That(() => "2015-01-01 00:00:00,鎮守府正面海域,1,出撃,S,同航戦,単縦陣,単縦陣,敵偵察艦,駆逐艦,深雪," +
                               "天龍改(Lv79),40/40,大潮(Lv1),16/16,初霜(Lv1),16/16,,,,,,,駆逐イ級,0/20,,,,,,,,,,,0,0,|" +
                               "2015-01-01 00:00:00,鎮守府正面海域,1,出撃,S,同航戦,単縦陣,単縦陣,敵偵察艦,,," +
                               "天龍改(Lv79),40/40,大潮(Lv1),6/16,初霜(Lv1),16/16,,,,,,,駆逐イ級,0/20,,,,,,,,,,,0,0,|"
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
            PAssert.That(() => "2015-01-01 00:00:00,26178,26742,21196,33750,1426,1574,2185,10|" +
                               "2015-01-01 00:10:00,24595,25353,18900,32025,1427,1576,2187,10|" +
                               "2015-01-01 00:20:00,23463,25064,17314,31765,1427,1572,2187,10|"
                               == result);
        }

        [TestMethod]
        public void WriteMaterialLogOnSortie()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Material);
            SnifferTest.SniffLogFile(sniffer, "battle_001");
            PAssert.That(() => "2015-01-01 00:00:00,39636,36912,43064,47519,1329,1424,2030,19|" +
                               "2015-01-01 00:00:00,39636,36912,43064,47519,1329,1424,2030,19|"
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
            PAssert.That(() => "2015-01-01 00:00:00,12cm単装砲,小口径主砲,10,10,30,10,綾波改二(145),120|" +
                               "2015-01-01 00:00:00,失敗,,10,10,30,10,綾波改二(145),120|"
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
            PAssert.That(() => "2015-01-01 00:00:00,通常艦建造,雷,駆逐艦,30,30,30,30,1,1,綾波改二(145),120|" +
                               "2015-01-01 00:00:00,大型艦建造,霧島,高速戦艦,1500,1500,2000,1000,1,0,綾波改二(145),120|"
                               == result);
        }

        [TestMethod]
        public void InspectRemodelSlot()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.RemodelSlot);
            SnifferTest.SniffLogFile(sniffer, "remodel_slot_001");
            PAssert.That(() => "2015-01-01 00:00:00,12.7cm連装砲,4,○,,,,10,30,60,0,1,1,明石改(50),島風改(131)|" +
                               "2015-01-01 00:00:00,12.7cm連装砲,5,×,,,,10,30,60,0,1,1,明石改(50),|" +
                               "2015-01-01 00:00:00,12.7cm連装砲,7,○,,12.7cm連装砲,1,10,30,60,0,1,1,明石改(50),島風改(131)|" +
                               "2015-01-01 00:00:00,91式高射装置,10,○,○,10cm連装高角砲,2,0,0,60,40,9,7,明石改(50),摩耶改(98)|"
                               == result);
        }

        [TestMethod]
        public void Achievement()
        {
            var logger = new Logger(null, null, null);
            logger.EnableLog(LogType.Achivement);
            var result = "";
            var dateEnum = new[]
            {
                new DateTime(2017, 3, 31, 21, 0, 0),
                new DateTime(2017, 3, 31, 22, 0, 0),
                new DateTime(2017, 4, 1, 4, 0, 0),
                new DateTime(2017, 4, 1, 5, 0, 0),
                new DateTime(2017, 4, 1, 6, 0, 0),
                new DateTime(2017, 4, 2, 5, 0, 0),
                new DateTime(2017, 4, 2, 6,0,0)
            }.GetEnumerator();
            logger.SetWriter((path, s, h) => { result += s + "|"; }, () =>
            {
                dateEnum.MoveNext();
                return (DateTime)dateEnum.Current;
            });
            for (var i = 0; i < 6; i++)
                logger.InspectBasic(JsonParser.Parse($"{{\"api_experience\": {i * 1000}}}"));
            logger.InspectBattleResult(JsonParser.Parse("{\"api_get_exmap_rate\": \"100\"}"));
            PAssert.That(() =>
                "2017-03-31 21:00:00,0,0|2017-03-31 21:00:00,0,0|2017-03-31 22:00:00,1000,0|"+
                "2017-04-01 06:00:00,4000,0|2017-04-02 05:00:00,5000,0|2017-04-02 06:00:00,5000,100|"
                == result);
        }
    }
}