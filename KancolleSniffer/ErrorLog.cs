﻿// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using DynaJson;
using KancolleSniffer.Model;
using KancolleSniffer.Util;

namespace KancolleSniffer
{
    public class BattleResultError : Exception
    {
    }

    public class ErrorLog
    {
        private readonly Sniffer _sniffer;
        private BattleState _prevBattleState = BattleState.None;
        private readonly List<Main.Session> _battleApiLog = new List<Main.Session>();

        public ErrorLog(Sniffer sniffer)
        {
            _sniffer = sniffer;
        }

        public void CheckBattleApi(Main.Session session)
        {
            if (_prevBattleState == BattleState.None)
                _battleApiLog.Clear();
            try
            {
                if (_sniffer.Battle.BattleState != BattleState.None)
                {
                    _battleApiLog.Add(session);
                }
                else if (_prevBattleState == BattleState.Result &&
                         // battleresultのあとのship_deckかportでのみエラー判定する
                         _sniffer.IsBattleResultError)
                {
                    throw new BattleResultError();
                }
            }
            finally
            {
                _prevBattleState = _sniffer.Battle.BattleState;
            }
        }

        public string GenerateBattleErrorLog()
        {
            foreach (var s in _battleApiLog)
                Privacy.Remove(s);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            var api = CompressApi(string.Join("\r\n",
                new[] {BattleStartSlots()}.Concat(_battleApiLog.SelectMany(s => s.Lines))));
            var rank = _sniffer.Battle.DisplayedResultRank;
            var status = string.Join("\r\n", new[]
            {
                rank.IsError ? $"{rank.Assumed}->{rank.Actual}" : "",
                HpDiffLog()
            }.Where(s => !string.IsNullOrEmpty(s)));
            var result = $"{{{{{{\r\n{DateTime.Now:g} {version}\r\n{status}\r\n{api}\r\n}}}}}}";
            File.WriteAllText("error.log", result);
            return result;
        }

        private string BattleStartSlots()
        {
            return new JsonObject((from ship in _sniffer.BattleStartStatus
                    group ship by ship.Fleet
                    into fleet
                    select
                        (from s in fleet
                            select (from item in s.AllSlot select item.Spec.Id).ToArray()
                        ).ToArray()
                ).ToArray()).ToString();
        }

        private string HpDiffLog() => string.Join(" ",
            from pair in _sniffer.BattleResultStatusDiff
            let assumed = pair.Assumed // こちらのFleetはnull
            let actual = pair.Actual
            select $"({actual.Fleet.Number}-{actual.DeckIndex}) {assumed.NowHp}->{actual.NowHp}");

        public string GenerateErrorLog(Main.Session s, string exception)
        {
            Privacy.Remove(s);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            var api = CompressApi(string.Join("\r\n", s.Lines));
            var result = $"{{{{{{\r\n{DateTime.Now:g} {version}\r\n{exception}\r\n{api}\r\n}}}}}}";
            File.WriteAllText("error.log", result);
            return result;
        }

        private string CompressApi(string api)
        {
            var output = new MemoryStream();
            var gzip = new GZipStream(output, CompressionLevel.Optimal);
            var bytes = Encoding.UTF8.GetBytes(api);
            gzip.Write(bytes, 0, bytes.Length);
            gzip.Close();
            var ascii85 = Ascii85.Encode(output.ToArray());
            var result = new List<string>();
            var rest = ascii85.Length;
            const int lineLength = 80;
            for (var i = 0; i < ascii85.Length; i += lineLength, rest -= lineLength)
                result.Add(ascii85.Substring(i, Math.Min(rest, lineLength)));
            return string.Join("\r\n", result);
        }
    }
}