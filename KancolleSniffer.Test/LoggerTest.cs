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
using System.Linq;
using Codeplex.Data;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class LoggerTest
    {
        private IEnumerable<string> ReadLogFile(string name)
        {
            using (var file = SnifferTest.OpenLogFile(name))
            {
                string line;
                while ((line = file.ReadLine()) != null)
                    yield return line;
            }
        }

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
            var writer = new LogWriter(mock.Object);

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
            var writer = new LogWriter(mock.Object);

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
    }
}