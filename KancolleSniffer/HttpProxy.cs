// Copyright (c) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KancolleSniffer
{
    public class HttpProxy
    {
        private static HttpProxy _httpProxy;
        public static int LocalPort { get; set; }
        public static string UpstreamProxyHost { get; set; }
        public static int UpstreamProxyPort { get; set; }
        public static bool IsEnableUpstreamProxy { get; set; }
        public static bool IsInListening { get; private set; }
        public static event Action<Session> AfterSessionComplete;

        private TcpListener _listener;

        public static void Startup(int port, bool dummy0, bool dummy1)
        {
            LocalPort = port;
            _httpProxy = new HttpProxy();
            _httpProxy.Start();
        }

        public void Start()
        {
            _listener = new TcpListener(IPAddress.Loopback, LocalPort);
            _listener.Start();
            LocalPort = ((IPEndPoint)(_listener.LocalEndpoint)).Port;
            IsInListening = true;
            Task.Run(() => AcceptClient());
        }

        public static void Shutdown()
        {
            _httpProxy?.Stop();
        }

        public void Stop()
        {
            IsInListening = false;
            _listener.Server.Close();
            _listener.Stop();
        }

        public void AcceptClient()
        {
            try
            {
                while (true)
                {
                    var client = _listener.AcceptSocket();
                    Task.Run(() => new HttpClient(client).ProcessRequest());
                }
            }
            catch (SocketException)
            {
            }
            finally
            {
                Stop();
            }
        }

        private class HttpClient
        {
            private readonly Socket _client;
            private Socket _server;
            private readonly Session _session;
            private readonly HttpStream _clientStream;
            private HttpStream _serverStream;

            public HttpClient(Socket client)
            {
                _client = client;
                _clientStream = new HttpStream(client);
                _session = new Session();
            }

            public void ProcessRequest()
            {
                try
                {
                    ReceiveRequest();
                    if (_session.Request.Method == "CONNECT")
                    {
                        HandleConnect();
                        return;
                    }
                    SendRequest();
                    ReceiveRequestBody();
                    SendRequestBody();
                    ReceiveResponse();
                    SendResponse();
                    Close();
                    AfterSessionComplete?.Invoke(_session);
                }
                catch (SocketException)
                {
                }
                catch (IOException)
                {
                }
                catch (HttpProxyAbort)
                {
                }
                finally
                {
                    Close();
                }
            }

            private void ReceiveRequest()
            {
                var requestLine = _clientStream.ReadLine();
                _session.Request.RequestLine = requestLine;
                _session.Request.Headers = _clientStream.ReadHeaders();
            }

            private void ReceiveRequestBody()
            {
                if (_session.Request.ContentLength != -1 || _session.Request.TransferEncoding != null)
                    _session.Request.ReadBody(_clientStream);
            }

            private void SendRequest()
            {
                _server = ConnectServer();
                _serverStream = new HttpStream(_server).
                    WriteLines(_session.Request.RequestLine + _session.Request.ModifiedHeaders);
            }

            private void SendRequestBody()
            {
                _serverStream.Write(_session.Request.Body);
            }

            private void ReceiveResponse()
            {
                _session.Response.StatusLine = _serverStream.ReadLine();
                _session.Response.Headers = _serverStream.ReadHeaders();
                if (HasBody)
                    _session.Response.ReadBody(_serverStream);
            }

            private bool HasBody
            {
                get
                {
                    var code = _session.Response.StatusCode;
                    return (!(_session.Request.Method == "HEAD" ||
                              code.StartsWith("1") || code == "204" || code == "304"));
                }
            }

            private void SendResponse()
            {
                _clientStream.WriteLines(_session.Response.StatusLine + _session.Response.ModifiedHeaders)
                    .Write(_session.Response.Body);
            }

            private void HandleConnect()
            {
                var host = "";
                var port = 443;
                if (!ParseAuthority(_session.Request.PathAndQuery, ref host, ref port))
                    return;
                _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                _server.Connect(host, port);
                _clientStream.WriteLines("HTTP/1.0 200 Connection established\r\n\r\n");
                Task[] tasks =
                {
                    Task.Run(() => { TunnnelSockets(_client, _server); }),
                    Task.Run(() => { TunnnelSockets(_server, _client); })
                };
                Task.WaitAll(tasks);
            }

            private void TunnnelSockets(Socket from, Socket to)
            {
                try
                {
                    var buf = new byte[8192];
                    while (true)
                    {
                        var n = from.Receive(buf);
                        if (n == 0)
                            break;
                        var sent = to.Send(buf, n, SocketFlags.None);
                        if (sent < n)
                            break;
                    }
                    to.Shutdown(SocketShutdown.Send);
                }
                catch (SocketException)
                {
                }
            }

            private static readonly Regex HostAndPortRegex =
                new Regex("http://([^:/]+)(?::(\\d+))?/", RegexOptions.Compiled);

            private Socket ConnectServer()
            {
                string host = null;
                var port = 80;
                if (IsEnableUpstreamProxy)
                {
                    host = UpstreamProxyHost;
                    port = UpstreamProxyPort;
                    goto connect;
                }
                var m = HostAndPortRegex.Match(_session.Request.RequestLine);
                if (m.Success)
                {
                    host = m.Groups[1].Value;
                    if (m.Groups[2].Success)
                        port = int.Parse(m.Groups[2].Value);
                    _session.Request.RequestLine = _session.Request.RequestLine.Remove(m.Index, m.Length - 1);
                }
                if (host == null && !ParseAuthority(_session.Request.Host, ref host, ref port))
                    throw new HttpProxyAbort("Can't find destination host");
                connect:
                var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                socket.Connect(host, port);
                return socket;
            }

            private static readonly Regex AuthorityRegex = new Regex("([^:]+)(?::(\\d+))?");

            private bool ParseAuthority(string authority, ref string host, ref int port)
            {
                if (string.IsNullOrEmpty(authority))
                    return false;
                var m = AuthorityRegex.Match(authority);
                if (!m.Success)
                    return false;
                host = m.Groups[1].Value;
                if (m.Groups[2].Success)
                    port = int.Parse(m.Groups[2].Value);
                return true;
            }

            private void Close()
            {
                _serverStream?.Close();
                _clientStream?.Close();
                _server?.Close();
                _client.Close();
            }
        }

        public class Session
        {
            public Request Request { get; set; } = new Request();
            public Response Response { get; set; } = new Response();
        }

        public class Message
        {
            private string _headers;
            public byte[] Body { get; set; }

            private static readonly Regex CharsetRegx = new Regex("charset=([\\w-]+)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

            public int ContentLength { get; set; } = -1;
            public string TransferEncoding { get; set; }
            public string ContentType { get; set; }
            public string ContentEncoding { get; set; }
            public string Host { get; set; }

            public string Headers
            {
                get { return _headers; }
                set
                {
                    _headers = value;
                    SetHeaders(_headers);
                }
            }

            public virtual string ModifiedHeaders => SetConnectionClose(Headers);

            private string SetConnectionClose(string headers)
            {
                return InsertHeader(RemoveHeaders(headers,
                    new[] {"connection", "keep-alive", "proxy-connection"}), "Connection: close\r\n");
            }

            protected string RemoveHeaders(string headers, string[] fields)
            {
                foreach (var f in fields)
                {
                    var m = MatchField(f, headers);
                    if (!m.Success)
                        continue;
                    headers = headers.Remove(m.Index, m.Length);
                }
                return headers;
            }

            protected string InsertHeader(string headers, string header)
            {
                return headers.Insert(headers.Length - 2, header);
            }

            protected virtual void SetHeaders(string headers)
            {
                var s = GetField("content-length");
                if (s != null)
                {
                    int len;
                    ContentLength = int.TryParse(s, out len) ? len : -1;
                }
                TransferEncoding = GetField("transfer-encoding")?.ToLower(CultureInfo.InvariantCulture);
                ContentType = GetField("content-type");
                ContentEncoding = GetField("content-encoding");
                Host = GetField("host");
            }

            protected Match MatchField(string name, string headers)
            {
                var regex = new Regex("^" + name + ":\\s*([^\r]+)\r\n",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase | RegexOptions.Multiline);
                return regex.Match(headers);
            }

            protected string GetField(string name)
            {
                var m = MatchField(name, Headers);
                return m.Success ? m.Groups[1].Value : null;
            }

            public string BodyAsString
            {
                get
                {
                    if (Body == null)
                        return "";
                    var m = CharsetRegx.Match(ContentType ?? "");
                    var encoding = Encoding.ASCII;
                    if (m.Success)
                    {
                        var name = m.Groups[1].Value;
                        if (name == "utf8")
                            name = "UTF-8";
                        encoding = Encoding.GetEncoding(name);
                    }
                    return encoding.GetString(Body);
                }
            }

            public void ReadBody(HttpStream stream)
            {
                if (TransferEncoding != null && TransferEncoding.Contains("chunked"))
                {
                    Body = stream.ReadChunked();
                }
                else if (ContentLength == 0)
                {
                }
                else if (ContentLength > 0)
                {
                    var buf = new byte[ContentLength];
                    stream.Read(buf, 0, ContentLength);
                    Body = buf;
                }
                else
                {
                    Body = stream.ReadToEnd();
                }
                if (ContentEncoding == null)
                    return;
                var dc = new MemoryStream();
                if (ContentEncoding == "gzip")
                    new GZipStream(new MemoryStream(Body), CompressionMode.Decompress).CopyTo(dc);
                else if (ContentEncoding == "deflate")
                    new DeflateStream(new MemoryStream(Body), CompressionMode.Decompress).CopyTo(dc);
                Body = dc.ToArray();
            }
        }

        public class Request : Message
        {
            private string _requestLine;

            public string RequestLine
            {
                get { return _requestLine; }
                set
                {
                    _requestLine = value;
                    var f = _requestLine.Split(' ');
                    if (f.Length < 3)
                        throw new HttpProxyAbort("Invalid request line");
                    Method = f[0];
                    PathAndQuery = f.Length < 2 ? "" : f[1];
                }
            }

            public string Method { get; private set; }
            public string PathAndQuery { get; private set; }
        }

        public class Response : Message
        {
            private string _statusLine;

            public override string ModifiedHeaders =>
                InsertContentLength(RemoveHeaders(base.ModifiedHeaders,
                    new[] {"transfer-encoding", "content-encoding", "content-length"}));

            private string InsertContentLength(string headers)
            {
                return Body == null ? headers : InsertHeader(headers, $"Content-Length: {Body.Length}\r\n");
            }

            public string StatusLine
            {
                get { return _statusLine; }
                set
                {
                    _statusLine = value;
                    var f = _statusLine.Split(' ');
                    if (f.Length < 3)
                        throw new HttpProxyAbort("Invalid status line");
                    StatusCode = _statusLine.Split(' ')[1];
                }
            }

            public string StatusCode { get; private set; }
        }

        private class HttpProxyAbort : Exception
        {
            public HttpProxyAbort(string message) : base(message)
            {
            }
        }

        public class HttpStream
        {
            private readonly Socket _socket;
            private readonly byte[] _buffer = new byte[4096];
            private int _available;
            private int _position;

            public HttpStream(Socket socket)
            {
                _socket = socket;
                socket.NoDelay = true;
            }

            public string ReadLine()
            {
                var sb = new StringBuilder();
                int ch;
                while ((ch = ReadByte()) != -1)
                {
                    sb.Append((char)ch);
                    if (ch == '\n')
                        break;
                }
                return sb.ToString();
            }

            private int ReadByte()
            {
                if (_position < _available)
                    return _buffer[_position++];
                _available = _socket.Receive(_buffer, 0, _buffer.Length, SocketFlags.None);
                _position = 0;
                return _available == 0 ? -1 : _buffer[_position++];
            }

            public HttpStream WriteLines(string s)
            {
                var buf = Encoding.ASCII.GetBytes(s);
                Write(buf, 0, buf.Length);
                return this;
            }

            public string ReadHeaders()
            {
                var sb = new StringBuilder();
                string line;
                do
                {
                    line = ReadLine();
                    sb.Append(line);
                } while (line != "\r\n");
                return sb.ToString();
            }

            public byte[] ReadChunked()
            {
                var buf = new MemoryStream();
                while (true)
                {
                    var size = ReadLine();
                    if (size.Length < 3)
                        break;
                    int val;
                    if (!int.TryParse(size.Substring(0, size.Length - 2),
                        NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out val))
                        break;
                    if (val == 0)
                    {
                        ReadLine();
                        break;
                    }
                    var chunk = new byte[val];
                    Read(chunk, 0, chunk.Length);
                    buf.Write(chunk, 0, chunk.Length);
                    ReadLine();
                }
                return buf.ToArray();
            }

            public byte[] ReadToEnd()
            {
                var result = new MemoryStream();
                var buf = new byte[4096];
                int len;
                while ((len = Read(buf, 0, buf.Length)) > 0)
                    result.Write(buf, 0, len);
                return result.ToArray();
            }

            public HttpStream Write(byte[] body)
            {
                if (body != null)
                    Write(body, 0, body.Length);
                return this;
            }

            public int Read(byte[] buf, int offset, int count)
            {
                try
                {
                    var total = 0;
                    do
                    {
                        int n;
                        if (_position < _available)
                        {
                            n = Math.Min(count, _available - _position);
                            Buffer.BlockCopy(_buffer, _position, buf, 0, n);
                            _position += n;
                        }
                        else
                        {
                            n = _socket.Receive(buf, offset, count, SocketFlags.None);
                            if (n == 0)
                                return total == 0 ? n : total;
                        }
                        count -= n;
                        offset += n;
                        total += n;
                    } while (count > 0);
                    return total;
                }
                catch (IOException)
                {
                    return -1;
                }
            }

            public void Write(byte[] buf, int offset, int count)
            {
                try
                {
                    do
                    {
                        var n = _socket.Send(buf, offset, count, SocketFlags.None);
                        if (n == 0)
                            return;
                        count -= n;
                        offset += n;
                    } while (count > 0);
                }
                catch (IOException)
                {
                }
            }

            public HttpStream Close()
            {
                _socket.Close();
                return this;
            }
        }
    }
}