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
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class NotificationManagerTest
    {
        private class MockTimer : NotificationManager.ITimer
        {
            private int _elapsed, _totalElapsed;
            private bool _enabled;
            private DateTime _start = new DateTime(2017, 11, 1);
            private DateTime _now;

            public MockTimer()
            {
                _now = _start;
            }

            public int Interval { get; set; }

            public bool Enabled
            {
                get => _enabled;
                set
                {
                    _enabled = value;
                    _start += TimeSpan.FromMilliseconds(_elapsed);
                    _elapsed = 0;
                }
            }

            public event EventHandler Tick;

            public void Start()
            {
                Enabled = true;
            }

            public void Stop()
            {
                Enabled = false;
            }

            public DateTime Now => _now;

            public int Elapsed => _totalElapsed;

            public void ElapseTime(int millis)
            {
                _totalElapsed += millis;
                if (!Enabled)
                {
                    _now = _start += TimeSpan.FromMilliseconds(millis);
                    return;
                }
                var after = _elapsed + millis;
                for (var n = _elapsed / Interval; n < after / Interval; n++)
                {
                    _now = _start + TimeSpan.FromMilliseconds((n + 1) * Interval);
                    Tick?.Invoke(this, EventArgs.Empty);
                }
                _elapsed = after;
                _now = _start + TimeSpan.FromMilliseconds(_elapsed);
            }
        }

        private class Message
        {
            public string Title { private get; set; }
            public string Body { private get; set; }
            public string Name { private get; set; }

            public bool Equals(Message other) =>
                other != null && Title == other.Title && Body == other.Body && Name == other.Name;
        }

        /// <summary>
        /// 単発
        /// </summary>
        [TestMethod]
        public void SingleNotification()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            manager.Enqueue("遠征終了", "防空射撃演習");
            PAssert.That(() => new Message {Title = "遠征が終わりました", Body = "防空射撃演習", Name = "遠征終了"}.Equals(result));
        }

        /// <summary>
        /// 連続した通知の間隔を二秒空ける
        /// </summary>
        [TestMethod]
        public void TwoNotificationAtSameTime()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            manager.Enqueue("疲労回復40", 0, "cond40");
            manager.Enqueue("疲労回復49", 1, "cond49");
            PAssert.That(() => new Message {Title = "疲労が回復しました", Body = "第一艦隊 残り9分", Name = "疲労回復"}.Equals(result));
            result = null;
            timer.ElapseTime(1000);
            PAssert.That(() => result == null);
            timer.ElapseTime(1000);
            PAssert.That(() => new Message {Title = "疲労が回復しました", Body = "第二艦隊", Name = "疲労回復"}.Equals(result));
            timer.ElapseTime(2000);
            PAssert.That(() => !timer.Enabled);
        }

        /// <summary>
        /// 一つ目の通知の一秒後に投入された通知は一秒ずらす
        /// </summary>
        [TestMethod]
        public void TwoNotification1SecDelay()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            manager.Enqueue("建造完了", "第一ドック");
            PAssert.That(() => new Message {Title = "建造が終わりました", Body = "第一ドック", Name = "建造完了"}.Equals(result));
            timer.ElapseTime(1000);
            manager.Enqueue("建造完了", "第二ドック");
            timer.ElapseTime(1000);
            PAssert.That(() => new Message {Title = "建造が終わりました", Body = "第二ドック", Name = "建造完了"}.Equals(result));
        }

        /// <summary>
        /// 通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void SingleRepeatableNotification()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            var expected = new Message {Title = "遠征が終わりました", Body = "防空射撃演習", Name = "遠征終了"};
            while (true)
            {
                switch (timer.Elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", "防空射撃演習", 2);
                        PAssert.That(() => expected.Equals(result));
                        break;
                    case 2000:
                        PAssert.That(() => expected.Equals(result));
                        break;
                    case 4000:
                        PAssert.That(() => expected.Equals(result));
                        return;
                    default:
                        PAssert.That(() => result == null, timer.Elapsed.ToString());
                        break;
                }
                result = null;
                timer.ElapseTime(1000);
            }
        }

        /// <summary>
        /// 二つの通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void TwoRepeatableNotofication()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            var ensei = new Message {Title = "遠征が終わりました", Body = "防空射撃演習", Name = "遠征終了"};
            var hakuchi = new Message {Title = "泊地修理 第一艦隊", Body = "20分経過しました。", Name = "泊地修理20分経過"};
            while (true)
            {
                switch (timer.Elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", "防空射撃演習", 10);
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 2000:
                        manager.Enqueue("泊地修理20分経過", 0, "", 5);
                        PAssert.That(() => hakuchi.Equals(result));
                        break;
                    case 7000:
                        PAssert.That(() => hakuchi.Equals(result), "泊地修理2回目");
                        break;
                    case 10000:
                        PAssert.That(() => ensei.Equals(result), "遠征終了2回目");
                        return;
                    default:
                        PAssert.That(() => result == null, timer.Elapsed.ToString());
                        break;
                }
                result = null;
                timer.ElapseTime(1000);
            }
        }

        /// <summary>
        /// スケジュールがぶつかる二つの通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void TwoRepeatableNotification1SecDelay()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            var ensei = new Message {Title = "遠征が終わりました", Body = "防空射撃演習", Name = "遠征終了"};
            var hakuchi = new Message {Title = "泊地修理 第一艦隊", Body = "20分経過しました。", Name = "泊地修理20分経過"};
            while (true)
            {
                switch (timer.Elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", "防空射撃演習", 3);
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 1000:
                        manager.Enqueue("泊地修理20分経過", 0, "", 2);
                        break;
                    case 2000:
                        PAssert.That(() => hakuchi.Equals(result));
                        break;
                    case 4000:
                        PAssert.That(() => ensei.Equals(result), "遠征終了2回目");
                        break;
                    case 6000:
                        PAssert.That(() => hakuchi.Equals(result), "泊地修理2回目");
                        return;
                    default:
                        PAssert.That(() => result == null, timer.Elapsed.ToString());
                        break;
                }
                result = null;
                timer.ElapseTime(1000);
            }
        }

        /// <summary>
        /// リピートしている通知を止める
        /// </summary>
        [TestMethod]
        public void RemoveRepeatableNotification()
        {
            var timer = new MockTimer();
            Message result = null;
            var manager =
                new NotificationManager((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, timer);
            var ensei = new Message {Title = "遠征が終わりました", Body = "防空射撃演習", Name = "遠征終了"};
            var hakuchi = new Message {Title = "入渠が終わりました", Body = "綾波改二", Name = "入渠終了"};
            while (true)
            {
                switch (timer.Elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", "防空射撃演習", 10);
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 2000:
                        manager.Enqueue("入渠終了", "綾波改二", 5);
                        PAssert.That(() => hakuchi.Equals(result));
                        break;
                    case 3000:
                        manager.StopRepeat("入渠終了");
                        break;
                    case 7000:
                        PAssert.That(() => result == null, "入渠終了2回目はない");
                        break;
                    case 10000:
                        PAssert.That(() => ensei.Equals(result), "遠征終了2回目");
                        return;
                    default:
                        PAssert.That(() => result == null, timer.Elapsed.ToString());
                        break;
                }
                result = null;
                timer.ElapseTime(1000);
            }
        }
    }
}