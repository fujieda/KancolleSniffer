﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using DynaJson;
using ExpressionToCodeLib;
using KancolleSniffer.Log;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KancolleSniffer.Test
{
    using Sniffer = SnifferTest.TestingSniffer;

    [TestClass]
    public class LoggerTest
    {
        [TestInitialize]
        public void Initialize()
        {
            ExpressionToCodeConfiguration.GlobalAssertionConfiguration = ExpressionToCodeConfiguration
                .GlobalAssertionConfiguration.WithPrintedListLengthLimit(200).WithMaximumValueLength(1000);
        }

        [TestMethod]
        public void InspectMissionResult()
        {
            var sniffer = new Sniffer(true);
            var result = "";
            var header = "";
            sniffer.SetLogWriter((path, s, h) =>
            {
                result += s + "|";
                header = h;
            }, () => new DateTime(2015, 1, 1));
            sniffer.EnableLog(LogType.Mission);
            SnifferTest.SniffLogFile(sniffer, "mission_result_001");
            PAssert.That(() => "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材,改修資材" == header);
            PAssert.That(() => "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,0,0,0|" +
                               "2015-01-01 00:00:00,成功,長距離練習航海,0,100,30,0,0,1,0,0|" +
                               "2015-01-01 00:00:00,大成功,MO作戦,0,0,360,420,1,0,0,0|" +
                               "2015-01-01 00:00:00,失敗,東京急行(弐),0,0,0,0,0,0,0,0|"
                               == result);
        }

        [TestMethod]
        public void MissionResultGetScrew()
        {
            var sniffer = new Sniffer(true);
            var result = "";
            var header = "";
            sniffer.SetLogWriter((path, s, h) =>
            {
                result += s + "|";
                header = h;
            }, () => new DateTime(2019, 1, 1));
            sniffer.EnableLog(LogType.Mission);
            SnifferTest.SniffLogFile(sniffer, "mission_result_002");
            PAssert.That(() => "日付,結果,遠征,燃料,弾薬,鋼材,ボーキ,開発資材,高速修復材,高速建造材,改修資材" == header);
            PAssert.That(() => "2019-01-01 00:00:00,大成功,南西諸島離島防衛作戦,0,0,1800,975,0,0,0,1|"
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
        public void FailToMergeTmpFile()
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
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2017, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "battle_001");
            PAssert.That(() =>
                "2017-01-01 00:00:00,カレー洋制圧戦,1,出撃,A,Ｔ字戦(有利),警戒陣,梯形陣,敵潜水教導艦隊,,," +
                "隼鷹改二(Lv157),57/62,利根改二(Lv151),66/66,千代田航改二(Lv159),50/65,千歳航改二(Lv159),65/65,大井改二(Lv57),40/43,秋月改(Lv142),42/42," +
                "潜水ヨ級(elite),34/34,潜水ヨ級,5/24,潜水ヨ級,0/24,潜水カ級(elite),0/27,潜水カ級,0/19,潜水カ級,0/19,248～249,0,制空権確保,4-2|"
                == result);
        }

        /// <summary>
        /// 7隻編成の場合は7隻目を6隻目に重ねる
        /// </summary>
        [TestMethod]
        public void InspectShip7BattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2017, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "ship7battle_001");
            PAssert.That(() =>
                "2017-01-01 00:00:00,台湾沖/ルソン島沖,5,出撃,S,同航戦,単縦陣,単縦陣,深海通商破部隊 前衛水雷戦隊,,," +
                "あきつ丸改(Lv81),40/40,那智改二(Lv151),63/63,Roma改(Lv99),83/92,阿武隈改二(Lv98),40/45,霞改二(Lv96),13/31,潮改二(Lv94)・不知火改(Lv85),31/33・32/32," +
                "軽巡ホ級(flagship),0/53,駆逐ロ級後期型,0/37,駆逐ロ級後期型,0/37,駆逐ロ級後期型,0/37,駆逐イ級,0/20,駆逐イ級,0/20,317～318,0,制空権確保,40-2|"
                == result);
        }

        /// <summary>
        /// 6隻未満の場合
        /// </summary>
        [TestMethod]
        public void InspectShip2BattleResult()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2019, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "battle_003");
            Assert.IsFalse(sniffer.IsBattleResultError);
            Assert.AreEqual("2019-01-01 00:00:00,鎮守府正面海域,1,出撃,S,反航戦,単縦陣,単縦陣,敵偵察艦,駆逐艦,睦月," +
                            "早波改(Lv54),32/32,五月雨(Lv20),16/16,,,,,,,,,駆逐ハ級,0/24,,,,,,,,,,,0,0,,1-1|",
                result);
        }

        /// <summary>
        /// 基地空襲戦
        /// </summary>
        [TestMethod]
        public void InspectAirRaidBattle()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2019, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "airraid_battle_001");
            Assert.AreEqual("2019-01-01 00:00:00,アリューシャン列島沖,12,出撃,S,同航戦,単縦陣,輪形陣,,,," +
                            "基地航空隊1(Lv0),200/200,基地航空隊2(Lv0),200/200,基地航空隊3(Lv0),200/200,,,,,,," +
                            "北方棲妹,790/790,砲台小鬼,130/130,砲台小鬼,130/130,飛行場姫,500/500,飛行場姫,500/500,集積地棲姫,600/600,425～426,231,航空優勢,44-3|",
                result);
        }

        /// <summary>
        /// ボス前の基地空襲戦
        /// </summary>
        [TestMethod]
        public void InspectAirRaidBattleBeforeBoss()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2019, 1, 1));
            sniffer.EnableLog(LogType.Battle);
            SnifferTest.SniffLogFile(sniffer, "airraid_battle_003");
            Assert.AreEqual("2019-01-01 00:00:00,北太平洋ハワイ諸島戦域,22,出撃,A,Ｔ字戦(不利),単縦陣,輪形陣,,,," +
                            "基地航空隊1(Lv0),200/200,基地航空隊2(Lv0),188/200,基地航空隊3(Lv0),200/200,,,,,,," +
                            "中枢棲姫,900/900,飛行場姫,500/500,飛行場姫,500/500,飛行場姫,500/500,砲台小鬼,130/130,集積地棲姫,600/600,471～473,212,航空優勢,44-4|",
                result);
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
        public void InspectCreateItemMulti()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2019, 1, 1));
            sniffer.EnableLog(LogType.CreateItem);
            SnifferTest.SniffLogFile(sniffer, "createitem_002");
            Assert.AreEqual("2019-01-01 00:00:00,失敗,,10,10,30,10,大淀改(167),120|" +
                            "2019-01-01 00:00:00,61cm三連装魚雷,魚雷,10,10,30,10,大淀改(167),120|" +
                            "2019-01-01 00:00:00,失敗,,10,10,30,10,大淀改(167),120|",
                result);
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
                               "2015-01-01 00:00:00,大型艦建造,霧島,巡洋戦艦,1500,1500,2000,1000,1,0,綾波改二(145),120|"
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

        private JsonObject Js(object obj) => new JsonObject(obj);

        [TestMethod]
        public void Achievement()
        {
            var logger = new Logger(null, null, null);
            logger.EnableLog(LogType.Achievement);
            var result = "";
            var dateEnum = new[]
            {
                new DateTime(2017, 3, 31, 21, 0, 0),
                new DateTime(2017, 3, 31, 22, 0, 0),
                new DateTime(2017, 4, 1, 1, 0, 0),
                new DateTime(2017, 4, 1, 2, 0, 0),
                new DateTime(2017, 4, 1, 3, 0, 0),
                new DateTime(2017, 4, 2, 2, 0, 0),
                new DateTime(2017, 4, 2, 3, 0, 0),
                new DateTime(2017, 4, 2, 4, 0, 0)
            }.GetEnumerator();
            logger.SetWriter((path, s, h) => { result += s + "|"; }, () =>
            {
                dateEnum.MoveNext();
                // ReSharper disable once PossibleNullReferenceException
                return (DateTime)dateEnum.Current;
            });
            for (var i = 0; i < 6; i++)
                logger.InspectBasic(Js(new {api_experience = i * 1000}));
            logger.InspectBattleResult(Js(new {api_get_exmap_rate = "100"}));
            logger.InspectMapNext(Js(new
                {api_maparea_id = 0, api_mapinfo_no = 0, api_no = 0, api_event_id = 0, api_get_eo_rate = 75}));
            PAssert.That(() =>
                "2017-03-31 21:00:00,0,0|2017-03-31 21:00:00,0,0|2017-03-31 22:00:00,1000,0|" +
                "2017-04-01 03:00:00,4000,0|2017-04-02 02:00:00,5000,0|" +
                "2017-04-02 03:00:00,5000,100|2017-04-02 04:00:00,5000,75|"
                == result);
        }

        [TestMethod]
        public void InspectClearItemGet()
        {
            var sniffer = new Sniffer();
            var result = "";
            sniffer.SetLogWriter((path, s, h) => { result += s + "|"; }, () => new DateTime(2017, 5, 1));
            sniffer.EnableLog(LogType.Achievement);
            SnifferTest.SniffLogFile(sniffer, "clearitemget_001");
            PAssert.That(() =>
                "2017-05-01 00:00:00,45417045,0|2017-05-01 00:00:00,45417045,350|" == result);
        }

        /// <summary>
        /// 晴嵐(六三一空)任務の場合はapi_bounus_countがない
        /// </summary>
        [TestMethod]
        public void InspectClearItemGetSeiran631Ku()
        {
            var sniffer = new Sniffer(true);
            sniffer.SetLogWriter((path, s, h) => { }, () => new DateTime(2017, 5, 1));
            sniffer.EnableLog(LogType.Achievement);
            sniffer.Sniff("/kcsapi/api_req_quest/clearitemget",
                "api%5Fquest%5Fid=656&api%5Fverno=1",
                JsonObject.Parse(
                    @"{""api_result"":1,""api_result_msg"":""成功"",""api_data"":
                    {""api_material"":[0,0,0,0],""api_bounus"":[
                    {""api_type"":15,""api_count"":1,""api_item"":{""api_id_from"":9999,""api_id_to"":9999,
                    ""api_message"":""第一潜水隊運用航空隊：「晴嵐(六三一空)」の新編成を<br>完了しました！""}}
                    ]}}"));
        }
    }
}