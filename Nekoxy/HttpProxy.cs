﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TrotiNet;

namespace Nekoxy
{
    /// <summary>
    /// HTTPプロキシサーバー。
    /// HTTPプロトコルにのみ対応し、HTTPS等はサポートしない。
    /// </summary>
    public static class HttpProxy
    {
        private static TcpServer server;

        /// <summary>
        /// HTTPレスポンスをプロキシクライアントに送信完了した際に発生。
        /// </summary>
        public static event Action<Session> AfterSessionComplete;

        /// <summary>
        /// アップストリームプロキシのホスト名。
        /// Startupメソッド時に設定されたシステムプロキシより優先して利用される。
        /// アップストリームプロキシは UpstreamProxyHost が null の場合無効となる。
        /// TrotiNet は Dns.GetHostAddresses で取得されたアドレスを順番に接続試行するため、
        /// 接続先によっては動作が遅くなる可能性がある。
        /// 例えば 127.0.0.1 で待ち受けているローカルプロキシに対して接続したい場合、
        /// localhost を指定するとまず ::1 へ接続試行するため、動作が遅くなってしまう。
        /// </summary>
        public static string UpstreamProxyHost
        {
            get { return TransparentProxyLogic.UpstreamProxyHost; }
            set { TransparentProxyLogic.UpstreamProxyHost = value; }
        }

        /// <summary>
        /// アップストリームプロキシのポート番号。
        /// アップストリームプロキシは UpstreamProxyHost が null の場合無効となる。
        /// </summary>
        public static int UpstreamProxyPort
        {
            get { return TransparentProxyLogic.UpstreamProxyPort; }
            set { TransparentProxyLogic.UpstreamProxyPort = value; }
        }

        /// <summary>
        /// プロキシサーバーが Listening 中かどうかを取得。
        /// </summary>
        public static bool IsInListening
        {
            get { return server != null && server.IsListening; }
        }

        /// <summary>
        /// 指定ポートで Listening を開始する。
        /// Shutdown() を呼び出さずに2回目の Startup() を呼び出した場合、InvalidOperationException が発生する。
        /// </summary>
        /// <param name="listeningPort">Listeningするポート。</param>
        /// <param name="useIpV6">falseの場合、127.0.0.1で待ち受ける。trueの場合、::1で待ち受ける。既定false。</param>
        /// <param name="isSetIEProxySettings">trueの場合、プロセス内IEプロキシの設定を実施し、アップストリームプロキシにシステム設定プロキシを設定する
        /// (ただしUpstreamProxyHostプロパティの方が優先される)。既定true。</param>
        public static void Startup(int listeningPort, bool useIpV6 = false, bool isSetIEProxySettings = true)
        {
            if (server != null) throw new InvalidOperationException("Calling Startup() twice without calling Shutdown() is not permitted.");

            TransparentProxyLogic.AfterSessionComplete += InvokeAfterSessionComplete;
            try
            {
                if (isSetIEProxySettings)
                {
                    WinInetUtil.SetProxyInProcessByUrlmon(listeningPort);
                    var systemProxyHost = WinInetUtil.GetSystemHttpProxyHost();
                    var systemProxyPort = WinInetUtil.GetSystemHttpProxyPort();
                    if (systemProxyPort != listeningPort || systemProxyHost.IsLoopbackHost())
                    {
                        //自身が指定されていた場合上流には指定しない
                        TransparentProxyLogic.DefaultUpstreamProxyHost = systemProxyHost;
                        TransparentProxyLogic.DefaultUpstreamProxyPort = systemProxyPort;
                    }
                }

                server = new TcpServer(listeningPort, useIpV6);
                server.Start(TransparentProxyLogic.CreateProxy);
                server.InitListenFinished.WaitOne();
                if (server.InitListenException != null) throw server.InitListenException;
            }
            catch (Exception)
            {
                Shutdown();
                throw;
            }
        }

        /// <summary>
        /// Listening しているスレッドを終了し、ソケットを閉じる。
        /// </summary>
        public static void Shutdown()
        {
            TransparentProxyLogic.AfterSessionComplete -= InvokeAfterSessionComplete;
            if (server == null) return;
            server.Stop();
            server = null;
        }

        private static void InvokeAfterSessionComplete(Session session)
        {
            if (AfterSessionComplete != null)
                AfterSessionComplete.Invoke(session);
        }
    }
}
