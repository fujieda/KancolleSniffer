// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class BattleResultError : Exception
    {
    }

    public class ErrorLog
    {
        private readonly Sniffer _sniffer;
        private BattleState _prevBattleState = BattleState.None;
        private readonly List<string[]> _battleApiLog = new List<string[]>();

        public ErrorLog(Sniffer sniffer)
        {
            _sniffer = sniffer;
        }

        public void CheckBattleApi(string url, string request, string response)
        {
            if (!url.EndsWith("api_port/port"))
                _battleApiLog.Add(new[] {url, request, response});
            try
            {
                if (_prevBattleState == BattleState.Result &&
                         // battleresultのあとのship_deckかportでのみエラー判定する
                         IsBattleResultError)
                {
                    throw new BattleResultError();
                }
            }
            finally
            {
                _prevBattleState = _sniffer.Battle.BattleState;
            }
            if (url.EndsWith("api_port/port"))
                _battleApiLog.Clear();
        }

        private bool IsBattleResultError =>
            _sniffer.Battle.DisplayedResultRank.IsError || _sniffer.IsBattleResultStatusError;

        public string GenerateBattleErrorLog()
        {
            foreach (var logs in _battleApiLog)
                RemoveUnwantedInformation(ref logs[1], ref logs[2]);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            var api = CompressApi(string.Join("\r\n",
                new[] {BattleStartSlots()}.Concat(_battleApiLog.SelectMany(logs => logs))));
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
            return JsonObject.CreateJsonObject((from ship in _sniffer.BattleStartStatus
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
            let assumed = pair.Assumed
            let actual = pair.Actual
            select $"({assumed.Fleet}-{assumed.DeckIndex}) {assumed.NowHp}->{actual.NowHp}");

        public string GenerateErrorLog(string url, string request, string response, string exception)
        {
            RemoveUnwantedInformation(ref request, ref response);
            var version = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            var api = CompressApi($"{url}\r\n{request}\r\n{response}");
            var result = $"{{{{{{\r\n{DateTime.Now:g} {version}\r\n{exception}\r\n{api}\r\n}}}}}}";
            File.WriteAllText("error.log", result);
            return result;
        }

        public static void RemoveUnwantedInformation(ref string request, ref string response)
        {
            var token = new Regex("&api%5Ftoken=.+?(?=&|$)|api%5Ftoken=.+?(?:&|$)");
            request = token.Replace(request, "");
            var id = new Regex(@"""api_member_id"":""\d+?"",?|""api_nickname"":"".+?"",?|""api_nickname_id"":""\d+"",?");
            response = id.Replace(response, "");
            var preamble = new Regex(@"^{""api_result"":.+?({.*})?}$");
            response = preamble.Replace(response, match => match.Groups[1].Value);
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