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
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using KancolleSniffer.Log;
using KancolleSniffer.Util;

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

        public static LogProcessor LogProcessor { private get; set; }

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
            if (File.Exists(full))
            {
                SendFile(client, full, "text/plain");
                return;
            }
            SendError(client, "404 Not Found");
        }

        private static void SendError(Socket client, string error)
        {
            using var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII);
            writer.Write("HTTP/1.1 {0}\r\n", error);
            writer.Write("Server: KancolleSniffer\r\n");
            writer.Write("Date: {0:R}\r\n", DateTime.Now);
            writer.Write("Connection: close\r\n\r\n");
            writer.Write("<html><head><title>{0}</title></head>\r\n", error);
            writer.Write("<body><h4>{0}</h4></body></html>\r\n\r\n", error);
            writer.Flush();
            client.Send(((MemoryStream)writer.BaseStream).ToArray());
        }

        private static void SendJsonData(Socket client, string path, DateTime from, DateTime to, bool number)
        {
            SendJsonDataHeader(client);
            var csv = path.Replace(".json", ".csv");
            if (!File.Exists(csv))
                return;
            var encoding = Encoding.GetEncoding("Shift_JIS");
            client.Send(encoding.GetBytes("{ \"data\": [\n"));
            try
            {
                foreach (var record in LogProcessor.Process(File.ReadLines(csv, encoding).Skip(1), csv, from, to,
                    number))
                    client.Send(encoding.GetBytes(record));
            }
            finally
            {
                client.Send(encoding.GetBytes("]}\n"));
            }
        }

        private static void SendJsonDataHeader(Socket client)
        {
            using var header = new StreamWriter(new MemoryStream(), Encoding.ASCII);
            header.Write("HTTP/1.1 200 OK\r\n");
            header.Write("Server: KancolleSniffer\r\n");
            header.Write("Date: {0:R}\r\n", DateTime.Now);
            header.Write("Content-Type: {0}\r\n", "application/json; charset=Shift_JIS");
            header.Write("Connection: close\r\n\r\n");
            header.Flush();
            client.Send(((MemoryStream)header.BaseStream).ToArray());
        }

        private static void SendFile(Socket client, string path, string mime)
        {
            using var header = new StreamWriter(new MemoryStream(), Encoding.ASCII);
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

        private static void SendProxyPac(Socket client, int port)
        {
            SendProxyPacHeader(client);
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

        private static void SendProxyPacHeader(Socket client)
        {
            using var header = new StreamWriter(new MemoryStream(), Encoding.ASCII);
            header.Write("HTTP/1.1 200 OK\r\n");
            header.Write("Server: KancolleSniffer\r\n");
            header.Write("Date: {0:R}\r\n", DateTime.Now);
            header.Write("Content-Type: application/x-ns-proxy-autoconfig\r\n");
            header.Write("Connection: close\r\n\r\n");
            header.Flush();
            client.Send(((MemoryStream)header.BaseStream).ToArray());
        }
    }
}