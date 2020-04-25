// Copyright (C) 2020 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using DynaJson;

namespace KancolleSniffer.Notification
{
    public class Formatter
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
                    Body = "%f艦隊 %s"
                }
            },
            {
                "入渠終了", new Message
                {
                    Title = "入渠が終わりました",
                    Body = "%fドック %s"
                }
            },
            {
                "建造完了", new Message
                {
                    Title = "建造が終わりました",
                    Body = "%fドック"
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
                    Title = "泊地修理 %f艦隊",
                    Body = "20分経過しました。"
                }
            },
            {
                "泊地修理進行", new Message
                {
                    Title = "泊地修理 %f艦隊",
                    Body = "修理進行：%s"
                }
            },
            {
                "泊地修理完了", new Message
                {
                    Title = "泊地修理 %f艦隊",
                    Body = "修理完了：%s"
                }
            },
            {
                "疲労回復40", new Message
                {
                    Title = "疲労が回復しました",
                    Body = "%f艦隊 残り9分"
                }
            },
            {
                "疲労回復49", new Message
                {
                    Title = "疲労が回復しました",
                    Body = "%f艦隊"
                }
            },
            {
                "任務達成", new Message
                {
                    Title = "任務を達成しました",
                    Body = "%s"
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
                dynamic config = JsonObject.Parse(File.ReadAllText(fileName));
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

        public Message GenerateMessage(Scheduler.Notification notification)
        {
            LoadConfig();
            var format = _config.TryGetValue(notification.Key, out Message value)
                ? value
                : _default[notification.Key];
            var prefix = new[] {"", "[リピート] ", "[継続] ", "[予告] "}[(int)notification.Mode];
            return new Message
            {
                Title = prefix + ProcessFormatString(format.Title, notification.Fleet, notification.Subject),
                Body = ProcessFormatString(format.Body, notification.Fleet, notification.Subject),
                Name = KeyToName(notification.Key)
            };
        }

        private string ProcessFormatString(string format, int fleet, string subject)
        {
            var fn = new[] {"第一", "第二", "第三", "第四"};
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
}