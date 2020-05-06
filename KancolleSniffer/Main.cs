using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using DynaJson;
using KancolleSniffer.Forms;
using KancolleSniffer.Log;
using KancolleSniffer.Net;
using KancolleSniffer.Util;
using Microsoft.CSharp.RuntimeBinder;

namespace KancolleSniffer
{
    public class Main
    {
        private ProxyManager _proxyManager;
        private Form _form;
        private MainWindow _mainBehavior;
        private readonly ErrorDialog _errorDialog = new ErrorDialog();
        private ConfigDialog _configDialog;
        private ErrorLog _errorLog;
        private IEnumerator<string> _playLog;
        private string _debugLogFile;
        private bool _timerEnabled;
        private readonly Timer _mainTimer = new Timer {Interval = 1000, Enabled = true};

        public TimeStep Step { get; } = new TimeStep();

        public Config Config { get; } = new Config();
        public Sniffer Sniffer { get; } = new Sniffer();

        public static void Run()
        {
            new Main().RunInternal();
        }

        private void RunInternal()
        {
            Config.Load();
            _configDialog = new ConfigDialog(this);
            var form = Config.Shape == "横長" ? (Form)new HorizontalMainForm() : new VerticalMainForm();
            _form = form;
            _mainBehavior = new MainWindow(this, form);
            _proxyManager = new ProxyManager(_form, Config);
            _proxyManager.UpdatePacFile();
            _errorLog = new ErrorLog(Sniffer);
            Sniffer.RepeatingTimerController = _mainBehavior.Notifier;
            LoadData();
            ApplyConfig();
            ApplySettings();
            HttpProxy.AfterSessionComplete += HttpProxy_AfterSessionComplete;
            _mainTimer.Tick += TimerTick;
            Application.Run(_form);
            Terminate();
        }

        public void CheckVersionUpMain(LinkLabel guide)
        {
            CheckVersionUp((current, latest) =>
            {
                if (latest == current)
                    return;
                guide.Text = $"バージョン{latest}があります。";
                guide.LinkArea = new LinkArea(0, guide.Text.Length);
                guide.Click += (obj, ev) => { Process.Start("https://ja.osdn.net/rel/kancollesniffer/" + latest); };
            });
        }

        private readonly FileSystemWatcher _watcher = new FileSystemWatcher
        {
            Path = AppDomain.CurrentDomain.BaseDirectory,
            NotifyFilter = NotifyFilters.LastWrite
        };

        private readonly Timer _watcherTimer = new Timer {Interval = 1000};

        private void LoadData()
        {
            var target = "";
            Sniffer.LoadState();
            _watcher.SynchronizingObject = _form;
            _watcherTimer.Tick += (sender, ev) =>
            {
                _watcherTimer.Stop();
                switch (target)
                {
                    case "status.xml":
                        Sniffer.LoadState();
                        break;
                    case "TP.csv":
                        Sniffer.AdditionalData.LoadTpSpec();
                        break;
                }
            };
            _watcher.Changed += (sender, ev) =>
            {
                target = ev.Name;
                _watcherTimer.Stop();
                _watcherTimer.Start();
            };
            _watcher.EnableRaisingEvents = true;
        }

        private void HttpProxy_AfterSessionComplete(HttpProxy.Session session)
        {
            _form.BeginInvoke(new Action<HttpProxy.Session>(ProcessRequest), session);
        }

        public class Session
        {
            public string Url { get; set; }
            public string Request { get; set; }
            public string Response { get; set; }

            public Session(string url, string request, string response)
            {
                Url = url;
                Request = request;
                Response = response;
            }

            public string[] Lines => new[] {Url, Request, Response};
        }

        private void ProcessRequest(HttpProxy.Session session)
        {
            var url = session.Request.PathAndQuery;
            if (!url.Contains("kcsapi/"))
                return;
            var s = new Session(url, session.Request.BodyAsString, session.Response.BodyAsString);
            Privacy.Remove(s);
            if (s.Response == null || !s.Response.StartsWith("svdata="))
            {
                WriteDebugLog(s);
                return;
            }
            s.Response = UnEscapeString(s.Response.Remove(0, "svdata=".Length));
            WriteDebugLog(s);
            ProcessRequestMain(s);
        }

        private void ProcessRequestMain(Session s)
        {
            try
            {
                var update = (Sniffer.Update)Sniffer.Sniff(s.Url, s.Request, JsonObject.Parse(s.Response));
                _mainBehavior.UpdateInfo(update);
                if (!Sniffer.Started)
                    return;
                Step.SetNowIfNeeded();
                if ((update & Sniffer.Update.Timer) != 0)
                    _timerEnabled = true;
                _errorLog.CheckBattleApi(s);
            }

            catch (RuntimeBinderException e)
            {
                if (_errorDialog.ShowDialog(_form,
                        "艦これに仕様変更があったか、受信内容が壊れています。",
                        _errorLog.GenerateErrorLog(s, e.ToString())) == DialogResult.Abort)
                    Exit();
            }
            catch (LogIOException e)
            {
                // ReSharper disable once PossibleNullReferenceException
                if (_errorDialog.ShowDialog(_form, e.Message, e.InnerException.ToString()) == DialogResult.Abort)
                    Exit();
            }
            catch (BattleResultError)
            {
                if (_errorDialog.ShowDialog(_form, "戦闘結果の計算に誤りがあります。",
                        _errorLog.GenerateBattleErrorLog()) == DialogResult.Abort)
                    Exit();
            }
            catch (Exception e)
            {
                if (_errorDialog.ShowDialog(_form, "エラーが発生しました。",
                        _errorLog.GenerateErrorLog(s, e.ToString())) == DialogResult.Abort)
                    Exit();
            }
        }

        private void Exit()
        {
            _proxyManager.Shutdown();
            Environment.Exit(1);
        }

        private void WriteDebugLog(Session s)
        {
            if (_debugLogFile != null)
            {
                File.AppendAllText(_debugLogFile,
                    $"date: {DateTime.Now:g}\nurl: {s.Url}\nrequest: {s.Request}\nresponse: {s.Response ?? "(null)"}\n");
            }
        }

        private string UnEscapeString(string s)
        {
            try
            {
                var rx = new Regex(@"\\[uU]([0-9A-Fa-f]{4})");
                return rx.Replace(s,
                    match => ((char)int.Parse(match.Value.Substring(2), NumberStyles.HexNumber)).ToString());
            }
            catch (ArgumentException)
            {
                return s;
            }
        }

        public async void CheckVersionUp(Action<string, string> action)
        {
            var current = string.Join(".", Application.ProductVersion.Split('.').Take(2));
            try
            {
                var latest = (await new WebClient().DownloadStringTaskAsync("http://kancollesniffer.osdn.jp/version"))
                    .TrimEnd();
                try
                {
                    action(current, latest);
                }
                catch (InvalidOperationException)
                {
                }
            }
            catch (WebException)
            {
            }
        }

        private void ApplySettings()
        {
            ApplyDebugLogSetting();
            ApplyLogSetting();
            ApplyProxySetting();
        }

        public void ApplyDebugLogSetting()
        {
            _debugLogFile = Config.DebugLogging ? Config.DebugLogFile : null;
        }

        public bool ApplyProxySetting()
        {
            return _proxyManager.ApplyConfig();
        }

        public void ApplyLogSetting()
        {
            LogServer.OutputDir = Config.Log.OutputDir;
            LogServer.LogProcessor = new LogProcessor(Sniffer.Material.MaterialHistory, Sniffer.MapDictionary);
            Sniffer.EnableLog(Config.Log.On ? LogType.All : LogType.None);
            Sniffer.MaterialLogInterval = Config.Log.MaterialLogInterval;
            Sniffer.LogOutputDir = Config.Log.OutputDir;
        }

        public void SetPlayLog(string file)
        {
            _playLog = File.ReadLines(file).GetEnumerator();
        }

        private void TimerTick(object sender, EventArgs ev)
        {
            if (_timerEnabled)
            {
                try
                {
                    Step.SetNow();
                    _mainBehavior.UpdateTimers();
                    _mainBehavior.Notifier.NotifyTimers();
                    Step.SetPrev();
                }
                catch (Exception ex)
                {
                    if (_errorDialog.ShowDialog(_form, "エラーが発生しました。", ex.ToString()) == DialogResult.Abort)
                        Exit();
                }
            }
            if (_playLog == null || _configDialog.Visible)
            {
                _mainBehavior.PlayLogSign.Visible = false;
                return;
            }
            PlayLog();
        }

        public void ResetAchievement()
        {
            Sniffer.Achievement.Reset();
            _mainBehavior.UpdateItemInfo();
        }

        private void PlayLog()
        {
            var lines = new List<string>();
            var sign = _mainBehavior.PlayLogSign;
            foreach (var s in new[] {"url: ", "request: ", "response: "})
            {
                do
                {
                    if (!_playLog.MoveNext() || _playLog.Current == null)
                    {
                        sign.Visible = false;
                        return;
                    }
                } while (!_playLog.Current.StartsWith(s));
                lines.Add(_playLog.Current.Substring(s.Length));
            }
            sign.Visible = !sign.Visible;
            ProcessRequestMain(new Session(lines[0], lines[1], lines[2]));
        }

        public void ShowConfigDialog()
        {
            if (_configDialog.ShowDialog(_form) == DialogResult.OK)
            {
                Config.Save();
                ApplyConfig();
                _mainBehavior.Notifier.StopRepeatingTimer(_configDialog.RepeatSettingsChanged);
            }
        }

        private void ApplyConfig()
        {
            Sniffer.ShipCounter.Margin = Config.MarginShips;
            Sniffer.ItemCounter.Margin = Config.MarginEquips;
            _mainBehavior.Notifier.NotifyShipItemCount();
            Sniffer.Achievement.ResetHours = Config.ResetHours;
            Sniffer.WarnBadDamageWithDameCon = Config.WarnBadDamageWithDameCon;
            _mainBehavior.ApplyConfig();
        }

        public IEnumerable<Control> Controls =>
            new Control[] {_errorDialog, _configDialog, _configDialog.NotificationConfigDialog};

        private void Terminate()
        {
            _proxyManager.Shutdown();
            Config.Save();
            Sniffer.FlashLog();
            Sniffer.SaveState();
        }

        public void ShowReport()
        {
            Process.Start("http://localhost:" + Config.Proxy.Listen + "/");
        }

        public void StartCapture()
        {
            try
            {
                var proc = new ProcessStartInfo("BurageSnap.exe") {WorkingDirectory = "Capture"};
                Process.Start(proc);
            }
            catch (FileNotFoundException)
            {
            }
            catch (Win32Exception)
            {
            }
        }
    }
}