﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using TrotiNet;

namespace Nekoxy
{
    /// <summary>
    /// 通信データを透過し読み取るためのProxyLogic。
    /// Transfer-Encoding: chunked なHTTPリクエストの RequestBody の読み取りは未対応。
    /// </summary>
    internal class TransparentProxyLogic : ProxyLogic
    {
        /// <summary>
        /// レスポンス送信後に発生するイベント。
        /// </summary>
        public static event Action<Session> AfterSessionComplete;

        /// <summary>
        /// アップストリームプロキシの指定を有効にする。既定値false。
        /// trueの場合、デフォルトプロキシを無視し、UpstreamProxyHost プロパティと UpstreamProxyPort プロパティをアップストリームプロキシに設定する。
        /// </summary>
        public static bool IsEnableUpstreamProxy { get; set; }

        /// <summary>
        /// インスタンス初期化時にRelayHttpProxyHostに設定される値。
        /// </summary>
        public static string UpstreamProxyHost { get; set; }

        /// <summary>
        /// インスタンス初期化時にRelayHttpProxyPortに設定される値。
        /// </summary>
        public static int UpstreamProxyPort { get; set; }

        /// <summary>
        /// UpstreamProxyHostがnullの場合に用いられるデフォルトホスト。
        /// </summary>
        public static string DefaultUpstreamProxyHost { get; set; }

        /// <summary>
        /// UpstreamProxyHostがnullの場合に用いられるデフォルトポート番号。
        /// </summary>
        public static int DefaultUpstreamProxyPort { get; set; }

        /// <summary>
        /// TcpServerがインスタンスを生成する際に使用するメソッド。
        /// 接続(AcceptCallback)の都度呼び出され、インスタンスが生成される。
        /// </summary>
        /// <param name="clientSocket">Browser-Proxy間Socket。SocketBP。</param>
        /// <returns>ProxyLogicインスタンス。</returns>
        public new static TransparentProxyLogic CreateProxy(HttpSocket clientSocket)
            => new TransparentProxyLogic(clientSocket);

        /// <summary>
        /// SocketBPからインスタンスを初期化。
        /// 接続(AcceptCallback)の都度インスタンスが生成される。
        /// </summary>
        /// <param name="clientSocket">Browser-Proxy間Socket。SocketBP。</param>
        public TransparentProxyLogic(HttpSocket clientSocket) : base(clientSocket)
        {
            this.RelayHttpProxyHost = IsEnableUpstreamProxy ? UpstreamProxyHost : DefaultUpstreamProxyHost;
            this.RelayHttpProxyPort = IsEnableUpstreamProxy ? UpstreamProxyPort : DefaultUpstreamProxyPort;
        }

        /// <summary>
        /// SendResponseをoverrideし、リクエストデータを読み取る。
        /// </summary>
        protected override void SendRequest()
        {
            this.currentSession = new Session();

            //HTTPメソッド送信
            this.SocketPS.WriteAsciiLine(this.RequestLine.RequestLine);
            //HTTPリクエストヘッダ送信
            this.SocketPS.WriteAsciiLine(this.RequestHeaders.HeadersInOrder);

            byte[] request = null;
            if (this.State.bRequestHasMessage)
            {
                if (this.State.bRequestMessageChunked)
                {
                    //FIXME: chunked request のデータ読み取りは未対応
                    this.SocketBP.TunnelChunkedDataTo(this.SocketPS);
                }
                else
                {
                    //Requestデータを読み取って流す
                    request = new byte[this.State.RequestMessageLength];
                    this.SocketBP.TunnelDataTo(request, this.State.RequestMessageLength);
                    this.SocketPS.TunnelDataTo(this.TunnelPS, request);
                }
            }
            this.currentSession.Request = new HttpRequest(this.RequestLine, this.RequestHeaders, request);

            //ReadResponseへ移行
            this.State.NextStep = this.ReadResponse;
        }

        /// <summary>
        /// OnReceiveResponseをoverrideし、レスポンスデータを読み取る。
        /// </summary>
        protected override void OnReceiveResponse()
        {
            //200だけ
            if (this.ResponseStatusLine.StatusCode != 200) return;

            // GetContentだけやるとサーバーからデータ全部読み込むけどクライアントに送らないってことになる。
            // のでTransferEncodingとContentLengthを書き換えてchunkedじゃないレスポンスとしてクライアントに送信してやる必要がある。
            // 
            // RFC 7230 の 3.3.1 を見る限りだと、Transfer-Encoding はリクエスト/レスポンスチェーンにいるどの recipient も
            // デコードしておっけーみたいに書いてあるから、HTTP的には問題なさそう。
            // https://tools.ietf.org/html/rfc7230#section-3.3.1
            // 
            // ただ4.1.3のロジックとTrotiNetのとを見比べると trailer フィールドへの対応が足りてるのかどうか疑問が残る。
            // https://tools.ietf.org/html/rfc7230#section-4.1.3
            var response = this.ResponseHeaders.IsUnknownLength()
                ? this.GetContentWhenUnknownLength()
                : this.GetContent();
            this.State.NextStep = null; //既定の後続動作(SendResponse)をキャンセル(自前で送信処理を行う)

            //Content-Encoding対応っぽい
            using (var ms = new MemoryStream())
            {
                var stream = this.GetResponseMessageStream(response);
                stream.CopyTo(ms);
                var content = ms.ToArray();
                this.currentSession.Response = new HttpResponse(this.ResponseStatusLine, this.ResponseHeaders, content);
            }

            // Transfer-Encoding: Chunked をやめて Content-Length を使うようヘッダ書き換え
            this.ResponseHeaders.TransferEncoding = null;
            this.ResponseHeaders.ContentLength = (uint)response.Length;

            this.SendResponseStatusAndHeaders(); //クライアントにHTTPステータスとヘッダ送信
            this.SocketBP.TunnelDataTo(this.TunnelBP, response); //クライアントにレスポンスボディ送信

            //keep-aliveとかじゃなかったら閉じる
            if (!this.State.bPersistConnectionPS)
            {
                this.SocketPS?.CloseSocket();
                this.SocketPS = null;
            }

            //AfterSessionCompleteイベント
            AfterSessionComplete?.Invoke(this.currentSession);
        }

        /// <summary>
        /// Transfer-Encoding も Content-Length も不明の場合、TrotiNet の SendResponse() にならい、Socket.Receive() が 0 になるまで受ける。
        /// </summary>
        /// <returns></returns>
        private byte[] GetContentWhenUnknownLength()
        {
            var buffer = new byte[512];
            this.SocketPS.TunnelDataTo(ref buffer); // buffer の長さは内部で調整される
            return buffer;
        }

        private Session currentSession; //SendRequestで初期化してOnReceiveResponseの最後でイベントに投げる
    }
}
