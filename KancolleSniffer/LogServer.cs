using System;
using System.IO;
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
                        // ReSharper disable once AssignNullToNotNullAttribute
                        var full = Path.Combine(Path.GetDirectoryName(Application.ExecutablePath), path);
                        if (!File.Exists(full))
                        {
                            SendError(client, "404 Not Found");
                            continue;
                        }
                        if (path.EndsWith(".html", StringComparison.OrdinalIgnoreCase))
                        {
                            SendFile(client, full, "text/html");
                            continue;
                        }
                        if (path.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
                        {
                            SendFile(client, full, "text/csv; charset=Shift_JIS");
                            continue;
                        }
                        if (path.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
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
                client.SendFile(path, ((MemoryStream)writer.BaseStream).ToArray(), null, TransmitFileOptions.UseDefaultWorkerThread);
            }
        }

        public void Stop()
        {
            _listener.Stop();
            _thread.Join();
        }
    }
}