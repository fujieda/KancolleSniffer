// Copyright (C) 2017 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Titanium.Web.Proxy;
using Titanium.Web.Proxy.EventArguments;
using Titanium.Web.Proxy.Http;
using Titanium.Web.Proxy.Models;

namespace KancolleSniffer.Net
{
    public class ProxyManager
    {
        private readonly Config _config;
        private readonly Control _parent;
        private readonly SystemProxy _systemProxy = new SystemProxy();
        private readonly ProxyServer _proxyServer = new ProxyServer("KancolleSniffer", "KancolleSniffer Root Certificate Authority");
        private ExplicitProxyEndPoint _proxyEndPoint;
        private int _autoConfigRetryCount;
        private readonly Timer _timer = new Timer {Interval = 1000};
        private bool _initiated;
        private DateTime _pacFileTime;

        public event Action<HttpWebClient> BeforeResponse;

        public ProxyManager(Config config, Control parent)
        {
            _config = config;
            _parent = parent;
            _proxyServer.CertificateManager.SaveFakeCertificates = true;
            _proxyServer.BeforeResponse += ProxyServer_BeforeResponse;
            _proxyServer.BeforeRequest += ProxyServer_BeforeRequest;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
            _timer.Tick += CheckProxy;
        }

        private async Task ProxyServer_BeforeResponse(object sender, SessionEventArgs e)
        {
            e.WebSession.Response.KeepBody = true;
            await e.GetResponseBodyAsString();
            BeforeResponse?.Invoke(e.WebSession);
        }

        private async Task ProxyServer_BeforeRequest(object sender, SessionEventArgs e)
        {
            var request = e.WebSession.Request;
            if (!(request.Host.StartsWith("127.0.0.1") || request.Host.StartsWith("localhost")))
                return;
            var response = await LogServer.CreateResponse(request);
            e.Respond(response);
        }

        public bool ApplyConfig()
        {
            if (!_config.Proxy.Auto)
                RestoreSystemProxy();
            if (_config.Proxy.UseUpstream)
            {
                _proxyServer.UpStreamHttpsProxy = _proxyServer.UpStreamHttpProxy =
                    new ExternalProxy {HostName = "127.0.0.1", Port = _config.Proxy.UpstreamPort};
            }
            else
            {
                _proxyServer.UpStreamHttpsProxy = _proxyServer.UpStreamHttpProxy = null;
            }
            var result = true;
            if (_proxyEndPoint != null && _proxyEndPoint.Port != _config.Proxy.Listen)
            {
                _proxyServer.RemoveEndPoint(_proxyEndPoint);
                result = StartProxy();
            }
            if (_proxyEndPoint == null)
            {
                result = StartProxy();
            }
            if (_config.Proxy.Auto && result)
            {
                SetAndCheckAutoConfigUrl();
            }
            return result;
        }

        private bool StartProxy()
        {
            try
            {
                _proxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, _config.Proxy.Listen);
                _proxyServer.AddEndPoint(_proxyEndPoint);
                if (!_proxyServer.ProxyRunning)
                    _proxyServer.Start();
            }
            catch (SocketException e)
            {
                var sockex = (SocketException)e.InnerException;
                if (sockex == null ||
                    sockex.SocketErrorCode != SocketError.AddressAlreadyInUse &&
                    sockex.SocketErrorCode != SocketError.AccessDenied)
                {
                    throw;
                }
                if (WarnConflictPortNumber(_config.Proxy.Listen, _config.Proxy.Auto) == DialogResult.No ||
                    !_config.Proxy.Auto)
                {
                    RestoreSystemProxy();
                    return false;
                }
                _proxyServer.RemoveEndPoint(_proxyEndPoint);
                _proxyEndPoint = new ExplicitProxyEndPoint(IPAddress.Loopback, 0);
                _proxyServer.AddEndPoint(_proxyEndPoint);
                _config.Proxy.Listen = _proxyEndPoint.Port;
            }
            LogServer.Port = _proxyEndPoint.Port;
            return true;
        }

        private DialogResult WarnConflictPortNumber(int port, bool auto)
        {
            var msg = $"ポート番号{port}は他のアプリケーションが使用中です。";
            var cap = "ポート番号の衝突";
            return auto
                ? MessageBox.Show(_parent, msg + "自動的に別の番号を割り当てますか？", cap,
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                : MessageBox.Show(_parent, msg + "設定ダイアログでポート番号を変更してください。", cap,
                    MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }

        private void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
        {
            if (e.Mode != PowerModes.Resume || !_config.Proxy.Auto)
                return;
            SystemProxy.Refresh();
        }

        public void Shutdown()
        {
            ShutdownProxy();
            if (_config.Proxy.Auto)
                RestoreSystemProxy();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void ShutdownProxy()
        {
            _proxyServer.Stop();
        }

        private void SetAndCheckAutoConfigUrl()
        {
            SetAutoConfigUrl();
            _initiated = false;
            _timer.Start();
        }

        private void CheckProxy(object sender, EventArgs ev)
        {
            if (_initiated)
            {
                // Windows 10でプロキシ設定がいつの間にか消えるのに対応するために、
                // 設定が消えていないか毎秒確認して、消えていたら再設定する。
                if (IsProxyWorking)
                    return;
                File.AppendAllText("proxy.log", $"[{DateTime.Now:G}] proxy setting vanished.\r\n");
                SetAutoConfigUrl();
                return;
            }
            if (IsProxyWorking)
            {
                _initiated = true;
                return;
            }
            if (_autoConfigRetryCount > 0 && _autoConfigRetryCount % 5 == 0)
            {
                _timer.Stop();
                switch (MessageBox.Show(_parent, "プロキシの自動設定に失敗しました。", "エラー", MessageBoxButtons.AbortRetryIgnore,
                    MessageBoxIcon.Error))
                {
                    case DialogResult.Abort:
                        Shutdown();
                        Environment.Exit(1);
                        break;
                    case DialogResult.Ignore:
                        return;
                }
                _timer.Start();
            }
            _autoConfigRetryCount++;
            SetAutoConfigUrl();
        }

        private bool IsProxyWorking =>
            WebRequest.GetSystemWebProxy().GetProxy(new Uri("http://125.6.184.16/")).IsLoopback;

        private void SetAutoConfigUrl()
        {
            var suffix = (DateTime.Now - _pacFileTime < TimeSpan.FromHours(6)
                ? (int)_pacFileTime.TimeOfDay.TotalSeconds
                : 0) + _autoConfigRetryCount;
            _systemProxy.SetAutoConfigUrl(
                $"http://localhost:{_config.Proxy.Listen}/proxy{(suffix == 0 ? "" : suffix.ToString("x"))}.pac");
        }

        private void RestoreSystemProxy()
        {
            _timer.Stop();
            _systemProxy.RestoreSettings();
        }

        public void UpdatePacFile()
        {
            var pacFile = "proxy.pac";
            var request = (HttpWebRequest)WebRequest.Create($"https://kancollesniffer.osdn.jp/{pacFile}");
            if (File.Exists(pacFile))
            {
                _pacFileTime = File.GetLastWriteTime(pacFile);
                request.IfModifiedSince = _pacFileTime;
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                var mem = new MemoryStream();
                using (var stream = response.GetResponseStream())
                    stream?.CopyTo(mem);
                mem.Position = 0;
                using (var file = new FileStream(pacFile, FileMode.Create))
                    mem.CopyTo(file);
                _pacFileTime = File.GetLastWriteTime(pacFile);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
    }
}