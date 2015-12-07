// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;

namespace KancolleSniffer
{
    public class LogServer
    {
        private readonly TcpListener _listener;
        private readonly string _indexDir = AppDomain.CurrentDomain.BaseDirectory;
        private string _outputDir = AppDomain.CurrentDomain.BaseDirectory;
        private readonly List<Socket> _sockets = new List<Socket>();

        public int Port { get; private set; }

        public bool IsListening { get; private set; }

        public string OutputDir
        {
            set { _outputDir = value; }
        }

        public LogServer(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
        }

        public void Start()
        {
            _listener.Start();
            Port = ((IPEndPoint)_listener.LocalEndpoint).Port;
            IsListening = true;
            new Thread(Listen).Start();
        }

        private void Listen()
        {
            try
            {
                while (true)
                {
                    var socket = _listener.AcceptSocket();
                    lock (_sockets)
                        _sockets.Add(socket);
                    new Thread(Process).Start(socket);
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                _listener.Stop();
            }
        }

        private void Process(Object obj)
        {
            var client = (Socket)obj;
            var data = new byte[4096];
            var from = DateTime.MinValue;
            var to = DateTime.MaxValue;
            try
            {
                if (client.Receive(data) == 0)
                    return;
                var request = Encoding.UTF8.GetString(data).Split('\r')[0].Split(' ');
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
                }

                path = path == "/" ? "index.html" : path.Substring(1);
                var full = Path.Combine(_indexDir, path);
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
                    SendJsonData(client, csv, from, to);
                    return;
                }
                if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
                {
                    SendFile(client, full, "application/javascript");
                    return;
                }
                SendError(client, "404 Not Found");
            }
            catch (IOException)
            {
            }
            catch (SocketException)
            {
            }
            finally
            {
                lock (_sockets)
                    _sockets.Remove(client);
                client.Close();
            }
        }

        private void SendError(Socket client, string error)
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

        private void SendJsonData(Socket client, string path, DateTime from, DateTime to)
        {
            var header = new StreamWriter(new MemoryStream(), Encoding.ASCII);
            header.Write("HTTP/1.1 200 OK\r\n");
            header.Write("Server: KancolleSniffer\r\n");
            header.Write("Date: {0:R}\r\n", DateTime.Now);
            header.Write("Content-Type: {0}\r\n", "application/json; charset=Shift_JIS");
            header.Write("Connection: close\r\n\r\n");
            header.Flush();
            client.Send(((MemoryStream)header.BaseStream).ToArray());

            var csv = path.Replace(".json", ".csv");
            var encoding = Encoding.GetEncoding("Shift_JIS");
            client.Send(encoding.GetBytes("{ \"data\": [\n"));
            try
            {
                if (File.Exists(csv))
                {
                    var delimiter = "";
                    var material = path.EndsWith("資材ログ.json"); // 末尾の空データを削除する必要がある
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
                        client.Send(encoding.GetBytes(delimiter + "[\"" +
                                                      string.Join("\",\"", (material ? data.Take(9) : data)) + "\"]"));
                        delimiter = ",\n";
                    }
                }
            }
            finally
            {
                client.Send(encoding.GetBytes("]}\n"));
            }
        }

        private void SendFile(Socket client, string path, string mime)
        {
            using (var writer = new StreamWriter(new MemoryStream(), Encoding.ASCII))
            {
                writer.Write("HTTP/1.1 200 OK\r\n");
                writer.Write("Server: KancolleSniffer\r\n");
                writer.Write("Date: {0:R}\r\n", DateTime.Now);
                writer.Write("Content-Length: {0}\r\n", new FileInfo(path).Length);
                writer.Write("Content-Type: {0}\r\n", mime);
                writer.Write("Connection: close\r\n\r\n");
                writer.Flush();
                client.SendFile(path, ((MemoryStream)writer.BaseStream).ToArray(), null,
                    TransmitFileOptions.UseDefaultWorkerThread);
            }
        }

        public void Stop()
        {
            IsListening = false;
            _listener.Server.Close();
            lock (_sockets)
                _sockets.ForEach(s => s.Close());
        }
    }
}