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
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;

namespace KancolleSniffer
{
    public class LogServer
    {
        private static readonly string IndexDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string _outputDir = AppDomain.CurrentDomain.BaseDirectory;

        public static string OutputDir
        {
            set => _outputDir = value;
        }

        public static MaterialCount[] MaterialHistory { private get; set; }

        public static void Process(Socket client, string requestLine)
        {
            var from = DateTime.MinValue;
            var to = DateTime.MaxValue;
            var timestamp = false;

            var request = requestLine.Split(' ');
            if (request.Length != 3)
            {
                SendError(client, "400 Bad Request");
                return;
            }
            if (!request[0].StartsWith("GET", StringComparison.OrdinalIgnoreCase))
            {
                SendError(client, "501 Not Implemented");
                return;
            }
            var tmp = request[1].Split('?');
            var path = HttpUtility.UrlDecode(tmp[0]);
            if (path == null || !path.StartsWith("/"))
            {
                SendError(client, "400 Bad Request");
                return;
            }
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
                SendFile(client, full, "text/html");
                return;
            }
            if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && File.Exists(csv))
            {
                SendFile(client, csv, "text/csv; charset=Shift_JIS");
                return;
            }
            if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            {
                SendJsonData(client, csv, from, to, timestamp);
                return;
            }
            if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
            {
                SendFile(client, full, "application/javascript");
                return;
            }
            if (path.EndsWith(".pac"))
            {
                SendProxyPac(client, HttpProxy.LocalPort);
                return;
            }
            SendError(client, "404 Not Found");
        }

        private static void SendError(Socket client, string error)
        {
            using (var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
                writer.Write("HTTP/1.1 {0}\r\n", error);
                writer.Write("Server: KancolleSniffer\r\n");
                writer.Write("Date: {0:R}\r\n", DateTime.Now);
                writer.Write("Connection: close\r\n\r\n");
                writer.Write("<html><head><title>{0}</title></head>\r\n", error);
                writer.Write("<body><h4>{0}</h4></body></html>\r\n\r\n", error);
                writer.Flush();
                client.Send(((MemoryStream)writer.BaseStream).ToArray());
            }
        }

        private static void SendJsonData(Socket client, string path, DateTime from, DateTime to, bool number)
        {
            using (var header = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
                header.Write("HTTP/1.1 200 OK\r\n");
                header.Write("Server: KancolleSniffer\r\n");
                header.Write("Date: {0:R}\r\n", DateTime.Now);
                header.Write("Content-Type: {0}\r\n", "application/json; charset=Shift_JIS");
                header.Write("Connection: close\r\n\r\n");
                header.Flush();
                client.Send(((MemoryStream)header.BaseStream).ToArray());
            }
            var csv = path.Replace(".json", ".csv");
            var encoding = Encoding.GetEncoding("Shift_JIS");
            client.Send(encoding.GetBytes("{ \"data\": [\n"));
            var battle = false;
            var material = false;
            try
            {
                if (!File.Exists(csv))
                    return;
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
                var delimiter = "";
                foreach (var line in File.ReadLines(csv, encoding).Skip(1))
                {
                    var data = line.Split(',');
                    if (!DateTime.TryParseExact(data[0], Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                        DateTimeStyles.AssumeLocal, out DateTime date))
                    {
                        // システムが和暦に設定されていて和暦が出力されてしまったケースを救う
                        var wareki = CultureInfo.CreateSpecificCulture("ja-JP");
                        wareki.DateTimeFormat.Calendar = new JapaneseCalendar();
                        if (DateTime.TryParseExact(data[0], Logger.DateTimeFormat, wareki,
                            DateTimeStyles.AssumeLocal, out date))
                        {
                            data[0] = Logger.FormatDateTime(date);
                        }
                        else if (DateTime.TryParse(data[0], CultureInfo.CurrentCulture,
                            DateTimeStyles.AssumeLocal, out date))
                        {
                            data[0] = Logger.FormatDateTime(date);
                        }
                        else
                        {
                            continue;
                        }
                    }
                    if (date < from || to < date)
                        continue;
                    IEnumerable<string> entries = data;
                    if (material)
                        entries = data.Take(9);
                    if (battle)
                        entries = BattleLogProcessor.Process(data);
                    if (entries.Count() != records)
                        continue;
                    if (number)
                    {
                        var stamp = ((date.ToUniversalTime().Ticks -
                                      new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks) /
                                     TimeSpan.TicksPerMillisecond).ToString();
                        client.Send(encoding.GetBytes(delimiter + "[" + stamp + "," +
                                                      string.Join(",", entries.Skip(1)) + "]"));
                    }
                    else
                    {
                        client.Send(encoding.GetBytes(delimiter + "[\"" +
                                                      string.Join("\",\"", entries) + "\"]"));
                    }
                    delimiter = ",\n";
                }
                if (material && !number)
                {
                    client.Send(encoding.GetBytes(delimiter + "[\"" +
                                                  string.Join("\",\"", GetCurrentMaterialRecord()) + "\"]"));
                }
            }
            finally
            {
                client.Send(encoding.GetBytes("]}\n"));
            }
        }

        private static IEnumerable<string> GetCurrentMaterialRecord()
        {
            return new[] {Logger.FormatDateTime(DateTime.Now)}.Concat(MaterialHistory.Select(c => c.Now.ToString()));
        }

        private static void SendFile(Socket client, string path, string mime)
        {
            using (var header = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
                header.Write("HTTP/1.1 200 OK\r\n");
                header.Write("Server: KancolleSniffer\r\n");
                header.Write("Date: {0:R}\r\n", DateTime.Now);
                header.Write("Content-Length: {0}\r\n", new FileInfo(path).Length);
                header.Write("Content-Type: {0}\r\n", mime);
                header.Write("Connection: close\r\n\r\n");
                header.Flush();
                client.SendFile(path, ((MemoryStream)header.BaseStream).ToArray(), null,
                    TransmitFileOptions.UseDefaultWorkerThread);
            }
        }

        private static void SendProxyPac(Socket client, int port)
        {
            using (var header = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
                header.Write("HTTP/1.1 200 OK\r\n");
                header.Write("Server: KancolleSniffer\r\n");
                header.Write("Date: {0:R}\r\n", DateTime.Now);
                header.Write("Content-Type: application/x-ns-proxy-autoconfig\r\n");
                header.Write("Connection: close\r\n\r\n");
                header.Flush();
                client.Send(((MemoryStream)header.BaseStream).ToArray());
            }
            string pacFile;
            try
            {
                pacFile = File.ReadAllText("proxy.pac").Replace("8080", port.ToString());
            }
            catch
            {
                pacFile = "";
            }
            client.Send(Encoding.ASCII.GetBytes(pacFile));
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