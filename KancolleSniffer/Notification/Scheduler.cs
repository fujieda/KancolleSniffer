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
using System.Linq;

namespace KancolleSniffer.Notification
{
    public class Scheduler
    {
        private readonly Action<string, string, string> _alarm;
        private readonly List<Notification> _queue = new List<Notification>();
        private readonly Func<DateTime> _nowFunc = () => DateTime.Now;
        private readonly Formatter _formatter = new Formatter();
        private DateTime _lastAlarm;
        private bool _suspend;
        private string _suspendException;

        public enum Mode
        {
            Normal,
            Repeat,
            Cont,
            Preliminary
        }

        public class Notification
        {
            public string Key { get; set; }
            public int Fleet { get; set; }
            public string Subject { get; set; }
            public int Repeat { get; set; }
            public Mode Mode { get; set; }
            public DateTime Schedule { get; set; }
        }

        public Scheduler(Action<string, string, string> alarm, Func<DateTime> nowFunc = null)
        {
            _alarm = alarm;
            if (nowFunc != null)
                _nowFunc = nowFunc;
        }

        public void Enqueue(string key, int fleet, string subject, int repeat = 0, bool preliminary = false)
        {
            _queue.Add(new Notification
            {
                Key = key,
                Fleet = fleet,
                Subject = subject,
                Repeat = repeat,
                Mode = preliminary ? Mode.Preliminary : Mode.Normal
            });
        }

        public void Enqueue(string key, string subject, int repeat = 0)
        {
            Enqueue(key, 0, subject, repeat);
        }

        public void Flush()
        {
            Alarm();
        }

        public void StopRepeat(string key, bool cont = false)
        {
            if (!cont)
            {
                _queue.RemoveAll(n => IsMatch(n, key));
            }
            else
            {
                foreach (var n in _queue.Where(n => IsMatch(n, key)))
                {
                    n.Subject = "";
                    n.Mode = Mode.Cont;
                }
            }
        }

        public void StopRepeat(string key, int fleet)
        {
            _queue.RemoveAll(n => IsMatch(n, key) && n.Fleet == fleet);
        }

        public void StopRepeat(string key, string subject)
        {
            _queue.RemoveAll(n => IsMatch(n, key) && n.Subject == subject);
        }

        public void StopAllRepeat()
        {
            _queue.RemoveAll(n => n.Schedule != default);
        }

        public void SuspendRepeat(string exception = "")
        {
            _suspend = true;
            _suspendException = exception;
        }

        public void ResumeRepeat()
        {
            _suspend = false;
        }

        public string KeyToName(string key) => Formatter.KeyToName(key);

        private bool IsMatch(Notification n, string key) =>
            n.Key.Substring(0, 4) == key.Substring(0, 4) && n.Schedule != default;

        private void Alarm()
        {
            var now = _nowFunc();
            if (now - _lastAlarm < TimeSpan.FromSeconds(2))
                return;
            var first = _queue.FirstOrDefault(n => n.Schedule.CompareTo(now) <= 0 &&
                                                   !(n.Schedule != default && _suspend && n.Key != _suspendException));
            if (first == null)
                return;
            var message = _formatter.GenerateMessage(first);
            var similar = _queue.Where(n =>
                    _formatter.GenerateMessage(n).Name == message.Name && n.Schedule.CompareTo(now) <= 0)
                .ToArray();
            var body = string.Join("\r\n", similar.Select(n => _formatter.GenerateMessage(n).Body));
            foreach (var n in similar)
            {
                if (n.Repeat == 0)
                {
                    _queue.Remove(n);
                }
                else
                {
                    n.Schedule = now + TimeSpan.FromSeconds(n.Repeat);
                    if (n.Mode == Mode.Normal)
                        n.Mode = Mode.Repeat;
                }
            }
            _alarm(message.Title, body, message.Name);
            _lastAlarm = now;
        }
    }
}