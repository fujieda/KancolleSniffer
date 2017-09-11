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
                _systemProxy.RestoreSettings();
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
                _systemProxy.SetAutoProxyUrl($"http://localhost:{_config.Proxy.Listen}/proxy.pac");
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
                if (WarnConflictPortNumber("プロキシサーバー", _config.Proxy.Listen, _config.Proxy.Auto) == DialogResult.No ||
                    !_config.Proxy.Auto)
                {
                    _systemProxy.RestoreSettings();
                    return false;
                }
                HttpProxy.Startup(0, false, false);
                _config.Proxy.Listen = HttpProxy.LocalPort;
            }
            return true;
        }

        private DialogResult WarnConflictPortNumber(string name, int port, bool auto)
        {
            var msg = $"{name}のポート番号{port}は他のアプリケーションが使用中です。";
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
            Task.Run(() =>
            {
                for (var i = 0; i < 5; i++, Thread.Sleep(15000))
                    SystemProxy.Refresh();
            });
        }

        public void Shutdown()
        {
            Task.Run(() => ShutdownProxy());
            if (_config.Proxy.Auto)
                _systemProxy.RestoreSettings();
            SystemEvents.PowerModeChanged -= SystemEvents_PowerModeChanged;
        }

        private void ShutdownProxy()
        {
            HttpProxy.Shutdown();
        }
    }
}