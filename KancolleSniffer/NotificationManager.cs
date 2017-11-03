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

        private class Notification
        {
            public string Key { get; set; }
            public int Fleet { get; set; }
            public string Subject { get; set; }
        }

        public NotificationManager(Action<string, string, string> ring, ITimer timer = null)
        {

            _notificationQueue = new NotificationQueue(ring, timer);
        }

        public void Enqueue(string key, int fleet, string subject)
        {
            _notificationQueue.Enqueue(new Notification
            {
                Key = key,
                Fleet = fleet,
                Subject = subject
            });
        }

        public void Enqueue(string key, string subject)
        {
            Enqueue(key, 0, subject);
        }

        private class NotificationConfig
        {
            public class Message
            {
                public string Title { get; set; }
                public string Body { get; set; }
                public string Name { get; set; }
            }

            private readonly Dictionary<string, Message> _config = new Dictionary<string, Message>();

            private readonly Dictionary<string, Message> _default = new Dictionary<string, Message>
            {
                {
                    "遠征終了", new Message
                    {
                        Title = "遠征が終わりました",
                        Body = "%s"
                    }
                },
                {
                    "入渠終了", new Message
                    {
                        Title = "入渠が終わりました",
                        Body = "%s"
                    }
                },
                {
                    "建造完了", new Message
                    {
                        Title = "建造が終わりました",
                        Body = "%s"
                    }
                },
                {
                    "艦娘数超過", new Message
                    {
                        Title = "艦娘が多すぎます",
                        Body = "%s"
                    }
                },
                {
                    "装備数超過", new Message
                    {
                        Title = "装備が多すぎます",
                        Body = "%s"
                    }
                },
                {
                    "大破警告", new Message
                    {
                        Title = "大破した艦娘がいます",
                        Body = "%s"
                    }
                },
                {
                    "泊地修理20分経過", new Message
                    {
                        Title = "泊地修理 %f",
                        Body = "20分経過しました。"
                    }
                },
                {
                    "泊地修理進行", new Message
                    {
                        Title = "泊地修理 %f",
                        Body = "修理進行：%s"
                    }
                },
                {
                    "泊地修理完了", new Message
                    {
                        Title = "泊地修理 %f",
                        Body = "修理完了：%s"
                    }
                },
                {
                    "疲労回復40", new Message
                    {
                        Title = "疲労が回復しました",
                        Body = "%f 残り9分"
                    }
                },
                {
                    "疲労回復49", new Message
                    {
                        Title = "疲労が回復しました",
                        Body = "%f"
                    }
                }
            };

            public static string KeyToName(string key) => key.StartsWith("疲労回復") ? key.Substring(0, 4) : key;

            private void LoadConfig()
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
                        _config[entry.key] = new Message
                        {
                            Title = entry.title,
                            Body = entry.body
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

            public Message GenerateMessage(Notification notification)
            {
                LoadConfig();
                var format = _config.TryGetValue(notification.Key, out Message value) ? value : _default[notification.Key];
                return new Message
                {
                    Title = ProcessFormatString(format.Title, notification.Fleet, notification.Subject),
                    Body = ProcessFormatString(format.Body, notification.Fleet, notification.Subject),
                    Name = KeyToName(notification.Key)
                };
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
        }

        public interface ITimer
        {
            int Interval { get; set; }
            bool Enabled { get; set; }
            event EventHandler Tick;
            void Start();
            void Stop();
        }

        public class TimerWrapper : ITimer
        {
            private readonly Timer _timer = new Timer();

            public int Interval
            {
                get => _timer.Interval;
                set => _timer.Interval = value;
            }

            public bool Enabled
            {
                get => _timer.Enabled;
                set => _timer.Enabled = value;
            }

            public event EventHandler Tick
            {
                add => _timer.Tick += value;
                remove => _timer.Tick -= value;
            }

            public void Start() => _timer.Start();

            public void Stop() => _timer.Stop();
        }

        private class NotificationQueue
        {
            private readonly Action<string, string, string> _ring;
            private readonly Queue<Notification> _queue = new Queue<Notification>();
            private readonly ITimer _timer;
            private readonly NotificationConfig _notificationConfig = new NotificationConfig();

            public NotificationQueue(Action<string, string, string> ring, ITimer timer)
            {
                _ring = ring;
                _timer = timer ?? new TimerWrapper();
                _timer.Interval = 2000;
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
                Ring(notification);
            }

            public void Enqueue(Notification notification)
            {
                if (_timer.Enabled)
                {
                    _queue.Enqueue(notification);
                }
                else
                {
                    Ring(notification);
                    _timer.Start();
                }
            }

            private void Ring(Notification notification)
            {
                var message =
                    _notificationConfig.GenerateMessage(notification);
                _ring(message.Title, message.Body, message.Name);
            }
        }
    }
}