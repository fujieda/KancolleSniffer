using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Web;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class LogServer
    {
        private readonly TcpListener _listener;
        private readonly Thread _thread;
        private readonly string _indexDir = Path.GetDirectoryName(Application.ExecutablePath);
        private string _outputDir = Path.GetDirectoryName(Application.ExecutablePath);

        public string OutputDir
        {
            set { _outputDir = value; }
        }

        public LogServer(int port)
        {
            _listener = new TcpListener(IPAddress.Loopback, port);
            _thread = new Thread(Listen);
        }

        public void Start()
        {
            _thread.Start();
        }

        private void Listen()
        {
            try
            {
                _listener.Start();
                while (true)
                {
                    var data = new byte[4096];
                    var client = _listener.AcceptSocket();
                    try
                    {
                        if (client.Available == 0)
                        {
                            Thread.Sleep(500);
                            if (client.Available == 0)
                                continue;
                        }
                        if (client.Receive(data) == 0)
                            continue;
                        var request = Encoding.UTF8.GetString(data).Split('\r')[0].Split(' ');
                        if (request.Length != 3)
                        {
                            SendError(client, "400 Bad Request");
                            continue;
                        }
                        if (!request[0].StartsWith("GET", StringComparison.OrdinalIgnoreCase))
                        {
                            SendError(client, "501 Not Implemented");
                            continue;
                        }
                        var path = HttpUtility.UrlDecode(request[1].Split('?')[0]);
                        if (path == null || !path.StartsWith("/"))
                        {
                            SendError(client, "400 Bad Request");
                            continue;
                        }

                        path = path == "/" ? "index.html" : path.Substring(1);
                        var full = Path.Combine(_indexDir, path);
                        var csv = Path.Combine(_outputDir, path);
                        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
                        {
                            SendFile(client, full, "text/html");
                            continue;
                        }
                        if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) && File.Exists(csv))
                        {
                            SendFile(client, csv, "text/csv; charset=Shift_JIS");
                            continue;
                        }
                        if (path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                        {
                            SendJsonData(client, csv);
                            continue;
                        }
                        if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase) && File.Exists(full))
                        {
                            SendFile(client, full, "application/javascript");
                            continue;
                        }
                        SendError(client, "404 Not Found");
                    }
                    catch (IOException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                    finally
                    {
                        client.Close();
                    }
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

        private void SendJsonData(Socket client, string path)
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
            if (File.Exists(csv))
            {
                var delimiter = "";
                foreach (var line in File.ReadLines(csv, encoding).Skip(1))
                {
                    client.Send(encoding.GetBytes(delimiter + "[\"" + string.Join("\",\"", line.Split(',')) + "\"]"));
                    delimiter = ",\n";
                }
            }
            client.Send(encoding.GetBytes("]}\n"));
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
            _listener.Stop();
            _thread.Join();
        }
    }
}