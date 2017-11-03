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
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace KancolleSniffer
{
    public class ProxyManager
    {
        private readonly Config _config;
        private readonly Control _parent;
        private readonly SystemProxy _systemProxy = new SystemProxy();
        private int _prevProxyPort;

        public ProxyManager(Config config, Control parent)
        {
            _config = config;
            _parent = parent;
            SystemEvents.PowerModeChanged += SystemEvents_PowerModeChanged;
        }

        public bool ApplyConfig()
        {
            if (!_config.Proxy.Auto)
                RestoreSystemProxy();
            if (_config.Proxy.UseUpstream)
            {
                HttpProxy.UpstreamProxyHost = "127.0.0.1";
                HttpProxy.UpstreamProxyPort = _config.Proxy.UpstreamPort;
            }
            HttpProxy.IsEnableUpstreamProxy = _config.Proxy.UseUpstream;
            var result = true;
            if (!HttpProxy.IsInListening || _config.Proxy.Listen != _prevProxyPort)
            {
                ShutdownProxy();
                result = StartProxy();
            }
            if (_config.Proxy.Auto && result)
            {
                SetAutoProxyUrl();
            }
            _prevProxyPort = _config.Proxy.Listen;
            return result;
        }

        private bool StartProxy()
        {
            try
            {
                HttpProxy.Startup(_config.Proxy.Listen, false, false);
            }
            catch (SocketException e)
            {
                if (e.SocketErrorCode != SocketError.AddressAlreadyInUse &&
                    e.SocketErrorCode != SocketError.AccessDenied)
                {
                    throw;
                }
                if (WarnConflictPortNumber(_config.Proxy.Listen, _config.Proxy.Auto) == DialogResult.No ||
                    !_config.Proxy.Auto)
                {
                    RestoreSystemProxy();
                    return false;
                }
                HttpProxy.Startup(0, false, false);
                _config.Proxy.Listen = HttpProxy.LocalPort;
            }
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
            Task.Run(() => ShutdownProxy());
            if (_config.Proxy.Auto)
                RestoreSystemProxy();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void ShutdownProxy()
        {
            HttpProxy.Shutdown();
        }

        private readonly AutoResetEvent _stopEvent = new AutoResetEvent(false);
        private Task _checkerTask;

        private void SetAutoProxyUrl()
        {
            var url = $"http://localhost:{_config.Proxy.Listen}/proxy.pac";
            _systemProxy.SetAutoProxyUrl(url);
            if (_checkerTask != null && !_checkerTask.IsCompleted)
            {
                _stopEvent.Set();
                _checkerTask.Wait();
            }
            _checkerTask = Task.Run(() =>
            {
                // Windows 10でプロキシ設定がいつの間にか消えるのに対応するために、
                // 設定が消えていないか毎秒確認して、消えていたら再設定する。
                do
                {
                    var proxy = WebRequest.GetSystemWebProxy().GetProxy(new Uri("http://125.6.184.16/"));
                    if (!proxy.IsLoopback)
                    {
                        File.AppendAllText("proxy.log", $"[{DateTime.Now:g}] proxy setting vanished.\r\n");
                        _systemProxy.SetAutoProxyUrl(url);
                    }
                } while (!_stopEvent.WaitOne(1000));
            });
        }

        private void RestoreSystemProxy()
        {
            if (_checkerTask != null && !_checkerTask.IsCompleted)
            {
                _stopEvent.Set();
                _checkerTask.Wait();
            }
            _systemProxy.RestoreSettings();
        }

        public void UpdatePacFile()
        {
            var request = (HttpWebRequest)WebRequest.Create("https://kancollesniffer.osdn.jp/proxy.pac");
            if (File.Exists("proxy.pac"))
            {
                var date = File.GetLastWriteTime("proxy.pac");
                request.IfModifiedSince = date;
            }
            try
            {
                var response = (HttpWebResponse)request.GetResponse();
                using (var stream = response.GetResponseStream())
                using (var file = new FileStream("proxy.pac", FileMode.OpenOrCreate))
                    stream?.CopyTo(file);
            }
            // ReSharper disable once EmptyGeneralCatchClause
            catch
            {
            }
        }
    }
}