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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using KancolleSniffer.Model;
using KancolleSniffer.Util;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace KancolleSniffer.Net
{
    public static class LogServer
    {
        private static readonly string IndexDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string _outputDir = AppDomain.CurrentDomain.BaseDirectory;

        public static string OutputDir
        {
            set => _outputDir = value;
        }

        public static int Port;

        public static MaterialCount[] MaterialHistory { private get; set; }

        public static async Task<Response> CreateResponse(Request request)
        {
            var from = DateTime.MinValue;
            var to = DateTime.MaxValue;
            var timestamp = false;

            if (!request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
                return CreateErrorResponse(501, "Not Implemented");

            var tmp = request.OriginalUrl.Split('?');
            var path = HttpUtility.UrlDecode(tmp[0]);
            if (path == null || !path.StartsWith("/"))
                return CreateErrorResponse(400, "Bad Request");

            if (tmp.Length == 2)
            {
                var query = HttpUtility.ParseQueryString(tmp[1]);
                if (query["from"] != null)
                {
                    double.TryParse(query["from"], out var tick);
                    from = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(tick / 1000);
                }
                if (query["to"] != null)
                {
                    double.TryParse(query["to"], out var tick);
                    to = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(tick / 1000);
                }
                if (query["number"] != null)
                    timestamp = query["number"] == "true";
            }

            path = path == "/" ? "index.html" : path.Substring(1);
            var full = Path.Combine(IndexDir, path);
            var csv = Path.Combine(_outputDir, path);
            if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
            {
                return await CreateSendFileResponse(full, "text");
            }
            if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && File.Exists(csv))
            {
                return await CreateSendFileResponse(csv, "text/csv; charset=Shift_JIS");
            }
            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                return await CreateSendJsonResponse(csv, from, to, timestamp);
            }
            if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
            {
                return await CreateSendFileResponse(full, "application/javascript");
            }
            if (path.EndsWith(".pac"))
            {
                return await CreateSendProxyPacResponse(Port);
            }
            if (File.Exists(full))
            {
                return await CreateSendFileResponse(full, "text/plain");
            }
            return CreateErrorResponse(404, "Not Found");
        }

        private static IEnumerable<HttpHeader> CommonHeader => new[]
        {
            new HttpHeader("Server", "KancolleSniffer"),
            new HttpHeader("Date", $"{DateTime.Now:R}")
        };

        private static Response CreateErrorResponse(int statusCode, string statusDescription)
        {
            var body = string.Format(
                "<html><head><title>{0} {1}</title></head>\r\n<body><h4>{0} {1}</h4></body></html>\r\n",
                statusCode, statusDescription);
            var response = new Response(Encoding.ASCII.GetBytes(body))
            {
                HttpVersion = new Version(1, 1),
                StatusCode = statusCode,
                StatusDescription = statusDescription
            };
            response.Headers.AddHeaders(CommonHeader);
            return response;
        }

        private static async Task<Response> CreateSendJsonResponse(string path, DateTime from, DateTime to, bool number)
        {
            var body = new StringBuilder();
            var csv = path.Replace(".json", ".csv");
            var encoding = Encoding.GetEncoding("Shift_JIS");
            body.Append("{ \"data\": [\n");
            var battle = false;
            var material = false;
            if (!File.Exists(csv))
                return CreateErrorResponse(404, "Not Found");
            var records = 0;
            if (path.EndsWith("遠征報告書.json"))
            {
                records = 10;
            }
            else if (path.EndsWith("改修報告書.json"))
            {
                records = 15;
            }
            else if (path.EndsWith("海戦・ドロップ報告書.json"))
            {
                records = 39;
                battle = true;
            }
            else if (path.EndsWith("開発報告書.json"))
            {
                records = 9;
            }
            else if (path.EndsWith("建造報告書.json"))
            {
                records = 12;
            }
            else if (path.EndsWith("資材ログ.json"))
            {
                records = 9;
                material = true;
            }
            else if (path.EndsWith("戦果.json"))
            {
                records = 3;
            }
            try
            {
                using (var file = File.OpenRead(csv))
                using (var reader = new StreamReader(file, encoding))
                {
                    var delimiter = "";
                    await reader.ReadLineAsync(); // drop header
                    while (true)
                    {
                        var line = await reader.ReadLineAsync();
                        if (line == null)
                            break;
                        var data = line.Split(',');
                        var date = ParseDateTime(data[0]);
                        if (date == default)
                            continue;
                        data[0] = Logger.FormatDateTime(date);
                        if (to < date)
                            break;
                        if (date < from)
                            continue;
                        IEnumerable<string> entries = data;
                        if (material)
                            entries = data.Take(9);
                        if (battle)
                            entries = BattleLogProcessor.Process(data);
                        if (entries.Count() != records)
                            continue;
                        body.Append(delimiter);
                        body.Append(number
                            ? "[" + JavaScriptTicks(date) + "," + string.Join(",", entries.Skip(1)) + "]"
                            : "[\"" + string.Join("\",\"", entries) + "\"]"
                        );
                        delimiter = ",\n";
                    }
                    if (material && !number) // Add current value of materials
                    {
                        body.Append(delimiter + "[\"" + string.Join("\",\"", GetCurrentMaterialRecord()) + "\"]");
                    }
                }
            }
            catch
            {
                return CreateErrorResponse(500, "Internal Server Error");
            }
            body.Append("]}\n");
            var response = new Response(encoding.GetBytes(body.ToString()))
            {
                HttpVersion = new Version(1, 1),
                StatusCode = 200,
                StatusDescription = "OK",
                ContentType = "application/json; charset=Shift_JIS"
            };
            response.Headers.AddHeaders(CommonHeader);
            return response;
        }

        private static DateTime ParseDateTime(string dateTime)
        {
            if (DateTime.TryParseExact(dateTime, Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal, out var date))
            {
                return date;
            }
            // システムが和暦に設定されていて和暦が出力されてしまったケースを救う
            if (dateTime[2] == '-')
            {
                if (!int.TryParse(dateTime.Substring(0, 2), out var year))
                    return default;
                dateTime = 1988 + year + dateTime.Substring(2);
                if (!DateTime.TryParseExact(dateTime, Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeLocal, out date))
                {
                    return default;
                }
                return date;
            }
            if (!DateTime.TryParse(dateTime, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date))
                return default;
            return date;
        }

        private static long JavaScriptTicks(DateTime date)
        {
            return ((date.ToUniversalTime().Ticks -
                     new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) /
                    TimeSpan.TicksPerMillisecond);
        }

        private static IEnumerable<string> GetCurrentMaterialRecord()
        {
            return new[] {Logger.FormatDateTime(DateTime.Now)}.Concat(MaterialHistory.Select(c => c.Now.ToString()));
        }

        private static async Task<Response> CreateSendFileResponse(string path, string mime)
        {
            using (var file = File.OpenRead(path))
            using (var memory = new MemoryStream())
            {
                await file.CopyToAsync(memory);
                var response = new Response(memory.ToArray())
                {
                    HttpVersion = new Version(1, 1),
                    StatusCode = 200,
                    StatusDescription = "OK",
                    ContentType = mime
                };
                response.Headers.AddHeaders(CommonHeader);
                return response;
            }
        }

        private static async Task<Response> CreateSendProxyPacResponse(int port)
        {
            string body;
            try
            {
                using (var file = File.OpenRead(Path.Combine(IndexDir, "proxy.pac")))
                using (var reader = new StreamReader(file, Encoding.ASCII))
                {
                    body = (await reader.ReadToEndAsync()).Replace("8080", port.ToString());
                }
            }
            catch
            {
                body = "";
            }
            var response = new Response(Encoding.ASCII.GetBytes(body))
            {
                HttpVersion = new Version(1, 1),
                StatusCode = 200,
                StatusDescription = "OK",
                ContentType = "application/x-ns-proxy-autoconfig"
            };
            response.Headers.AddHeaders(CommonHeader);
            return response;
        }
    }

    public static class BattleLogProcessor
    {
        public static IEnumerable<string> Process(string[] data)
        {
            if (data.Length == 35)
                data = data.Concat(Enumerable.Repeat("", 3)).ToArray();
            if (data.Length == 40)
            {
                data = data.Take(21).Concat(new[] {data[21] + "・" + data[23], data[22] + "・" + data[24]})
                    .Concat(data.Skip(25)).ToArray();
            }
            else if (data.Length != 38)
            {
                return data;
            }
            if (data[5] == "Ｔ字戦(有利)")
                data[5] = "Ｔ字有利";
            if (data[5] == "Ｔ字戦(不利)")
                data[5] = "Ｔ字不利";
            if (data[6].EndsWith("航行序列"))
                data[6] = data[6].Substring(0, 4);
            if (data[7].EndsWith("航行序列"))
                data[7] = data[7].Substring(0, 4);
            data[37] = ShortenAirBattleResult(data[37]);
            return AddDamagedShip(data);
        }

        private static string ShortenAirBattleResult(string result)
        {
            switch (result)
            {
                case "制空均衡":
                    return "均衡";
                case "制空権確保":
                    return "確保";
                case "航空優勢":
                    return "優勢";
                case "航空劣勢":
                    return "劣勢";
                case "制空権喪失":
                    return "喪失";
                default:
                    return "";
            }
        }

        private static IEnumerable<string> AddDamagedShip(string[] data)
        {
            var damaged = new List<string>();
            for (var i = 11; i < 11 + 12; i += 2)
            {
                if (data[i] == "")
                    continue;
                var ship = data[i] = StripKana(data[i]);
                var hp = data[i + 1];
                try
                {
                    damaged.AddRange(from entry in ship.Split('・').Zip(hp.Split('・'), (s, h) => new {s, h})
                        where entry.h.Contains("/")
                        let nm = entry.h.Split('/').Select(int.Parse).ToArray()
                        where ShipStatus.CalcDamage(nm[0], nm[1]) == ShipStatus.Damage.Badly
                        select entry.s);
                }
                catch (FormatException)
                {
                    return data;
                }
            }
            return data.Take(23).Concat(new[] {string.Join("・", damaged)}).Concat(data.Skip(23));
        }

        private static readonly Regex Kana = new Regex(@"\([^)]+\)\(", RegexOptions.Compiled);

        private static string StripKana(string name)
        {
            return Kana.Replace(name, "(");
        }
    }
}