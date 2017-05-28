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
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public class NotificationManager
    {
        private readonly NotificationQueue _notificationQueue;
        private readonly NotificationConfig _notificationConfig = new NotificationConfig();

        public class Notification
        {
            public string Title;
            public string Body;
            public string Name;
        }

        public NotificationManager(Action<string, string, string> ring)
        {
            _notificationQueue = new NotificationQueue(ring);
        }

        public void Enqueue(string key, int fleet, string subject)
        {
            _notificationConfig.LoadConfig();
            var notification = _notificationConfig[key];
            _notificationQueue.Enqueue(new Notification
            {
                Title = ProcessFormatString(notification.Title, fleet, subject),
                Body = ProcessFormatString(notification.Body, fleet, subject),
                Name = notification.Name
            });
        }

        public void Enqueue(string key, string subject)
        {
            Enqueue(key, 0, subject);
        }

        private string ProcessFormatString(string format, int fleet, string subject)
        {
            var fn = new[] {"第一艦隊", "第二艦隊", "第三艦隊", "第四艦隊"};
            var result = "";
            var percent = false;
            foreach (var ch in format)
            {
                if (ch == '%')
                {
                    if (percent)
                    {
                        percent = false;
                        result += '%';
                    }
                    else
                    {
                        percent = true;
                    }
                }
                else if (percent)
                {
                    percent = false;
                    switch (ch)
                    {
                        case 'f':
                            result += fn[fleet];
                            break;
                        case 's':
                            result += subject;
                            break;
                        default:
                            result += '%'.ToString() + ch;
                            break;
                    }
                }
                else
                {
                    result += ch;
                }
            }
            return result;
        }

        private class NotificationConfig
        {
            private readonly Dictionary<string, Notification> _config = new Dictionary<string, Notification>();

            private readonly Dictionary<string, Notification> _default = new Dictionary<string, Notification>
            {
                {
                    "遠征終了", new Notification
                    {
                        Title = "遠征が終わりました",
                        Body = "%s",
                        Name = "遠征終了"
                    }
                },
                {
                    "入渠終了", new Notification
                    {
                        Title = "入渠が終わりました",
                        Body = "%s",
                        Name = "入渠終了"
                    }
                },
                {
                    "建造完了", new Notification
                    {
                        Title = "建造が終わりました",
                        Body = "%s",
                        Name = "建造完了"
                    }
                },
                {
                    "艦娘数超過", new Notification
                    {
                        Title = "艦娘が多すぎます",
                        Body = "%s",
                        Name = "艦娘数超過"
                    }
                },
                {
                    "装備数超過", new Notification
                    {
                        Title = "装備が多すぎます",
                        Body = "%s",
                        Name = "装備数超過"
                    }
                },
                {
                    "大破警告", new Notification
                    {
                        Title = "大破した艦娘がいます",
                        Body = "%s",
                        Name = "大破警告"
                    }
                },
                {
                    "泊地修理20分経過", new Notification
                    {
                        Title = "泊地修理 %f",
                        Body = "20分経過しました。",
                        Name = "泊地修理20分経過"
                    }
                },
                {
                    "泊地修理進行", new Notification
                    {
                        Title = "泊地修理 %f",
                        Body = "修理進行：%s",
                        Name = "泊地修理進行"
                    }
                },
                {
                    "泊地修理完了", new Notification
                    {
                        Title = "泊地修理 %f",
                        Body = "修理完了：%s",
                        Name = "泊地修理完了"
                    }
                },
                {
                    "疲労回復40", new Notification
                    {
                        Title = "疲労が回復しました",
                        Body = "%f 残り9分",
                        Name = "疲労回復"
                    }
                },
                {
                    "疲労回復49", new Notification
                    {
                        Title = "疲労が回復しました",
                        Body = "%f",
                        Name = "疲労回復"
                    }
                }
            };

            public Notification this[string key] => _config.TryGetValue(key, out Notification value) ? value : _default[key];

            public void LoadConfig()
            {
                const string fileName = "notification.json";
                _config.Clear();
                try
                {
                    dynamic config = JsonParser.Parse(File.ReadAllText(fileName));
                    foreach (var entry in config)
                    {
                        if (!_default.ContainsKey(entry.key))
                            continue;
                        _config[entry.key] = new Notification
                        {
                            Title = entry.title,
                            Body = entry.body,
                            Name = _default[entry.key].Name
                        };
                    }
                }
                catch (FileNotFoundException)
                {
                }
                catch (Exception ex)
                {
                    throw new Exception($"{fileName}に誤りがあります。: ${ex.Message}", ex);
                }
            }
        }

        private class NotificationQueue
        {
            private readonly Action<string, string, string> _ring;
            private readonly Queue<Notification> _queue = new Queue<Notification>();
            private readonly Timer _timer = new Timer {Interval = 2000};

            public NotificationQueue(Action<string, string, string> ring)
            {
                _ring = ring;
                _timer.Tick += TimerOnTick;
            }

            private void TimerOnTick(object obj, EventArgs e)
            {
                if (_queue.Count == 0)
                {
                    _timer.Stop();
                    return;
                }
                var notification = _queue.Dequeue();
                _ring(notification.Title, notification.Body, notification.Name);
            }

            public void Enqueue(Notification notification)
            {
                if (_timer.Enabled)
                {
                    _queue.Enqueue(notification);
                }
                else
                {
                    _ring(notification.Title, notification.Body, notification.Name);
                    _timer.Start();
                }
            }
        }
    }
}