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

namespace KancolleSniffer
{
    public class LogServer
    {
        private static readonly string IndexDir = AppDomain.CurrentDomain.BaseDirectory;
        private static string _outputDir = AppDomain.CurrentDomain.BaseDirectory;

        public static string OutputDir
        {
            set { _outputDir = value; }
        }

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
                    double tick;
                    double.TryParse(query["from"], out tick);
                    from = new DateTime(1970, 1, 1).ToLocalTime().AddSeconds(tick / 1000);
                }
                if (query["to"] != null)
                {
                    double tick;
                    double.TryParse(query["to"], out tick);
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
            if (path.EndsWith("proxy.pac"))
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
            try
            {
                if (File.Exists(csv))
                {
                    var delimiter = "";
                    var material = path.EndsWith("資材ログ.json"); // 末尾の空データを削除する必要がある
                    var battle = path.EndsWith("海戦・ドロップ報告書.json"); // データを40個にそろえる必要がある
                    foreach (var line in File.ReadLines(csv, encoding).Skip(1))
                    {
                        var data = line.Split(',');
                        DateTime date;
                        if (!DateTime.TryParseExact(data[0], Logger.DateTimeFormat, CultureInfo.InvariantCulture,
                            DateTimeStyles.AssumeLocal, out date) &&
                            DateTime.TryParse(data[0], CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out date))
                        {
                            data[0] = date.ToString(Logger.DateTimeFormat);
                        }
                        if (date < from || to < date)
                            continue;
                        IEnumerable<string> entries = data;
                        if (material)
                            entries = data.Take(9);
                        if (battle)
                            entries = data.Concat(Enumerable.Repeat("", 3)).Take(38);
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
                }
            }
            finally
            {
                client.Send(encoding.GetBytes("]}\n"));
            }
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
            var pacFile = @"
function FindProxyForURL(url, host) {
  if(isInNet(host, ""203.104.209.71"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.184.15"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.184.16"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.187.205"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.187.229"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.187.253"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.188.25"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.248.135"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.7"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.39"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.71"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.103"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.135"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.167"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.215"", ""255.255.255.255"") ||
     isInNet(host, ""125.6.189.247"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.209.23"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.209.39"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.209.55"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.209.102"", ""255.255.255.255"") ||
     isInNet(host, ""203.104.209.87"", ""255.255.255.255"")) {
       return ""PROXY 127.0.0.1:8080"";
    }
  else {
    return ""DIRECT"";
  }
}".Replace("8080", port.ToString());
            client.Send(Encoding.ASCII.GetBytes(pacFile));
        }
    }
}