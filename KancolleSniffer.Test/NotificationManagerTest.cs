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
using KancolleSniffer.Notification;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class NotificationManagerTest
    {
        private class TimeProvider
        {
            private DateTime _now = new DateTime(2017, 11, 1);

            public DateTime GetNow()
            {
                    var now = _now;
                    _now += TimeSpan.FromSeconds(1);
                    return now;
            }
        }

        private class Message
        {
            public string Title { private get; set; }
            public string Body { private get; set; }
            public string Name { private get; set; }

            public bool Equals(Message other) =>
                other != null && Title == other.Title && Body == other.Body && Name == other.Name;

            public Message Repeat => new Message {Title = "[リピート] " + Title, Body = Body, Name = Name};
            public Message Cont => new Message {Title = "[継続] " + Title, Body = Body, Name = Name};
        }

        /// <summary>
        /// 単発
        /// </summary>
        [TestMethod]
        public void SingleNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            manager.Enqueue("遠征終了", 1, "防空射撃演習");
            manager.Flush();
            PAssert.That(() => new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"}.Equals(result));
        }

        /// <summary>
        /// 連続した通知の間隔を二秒空ける
        /// </summary>
        [TestMethod]
        public void TwoNotificationAtSameTime()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            manager.Enqueue("遠征終了", 1, "防空射撃演習");
            manager.Enqueue("疲労回復49", 1, "cond49");
            manager.Flush();
            PAssert.That(() => new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"}.Equals(result));
            result = null;
            manager.Flush();
            PAssert.That(() => result == null);
            manager.Flush();
            PAssert.That(() => new Message {Title = "疲労が回復しました", Body = "第二艦隊", Name = "疲労回復"}.Equals(result));
        }

        /// <summary>
        /// 一つ目の通知の一秒後に投入された通知は一秒ずらす
        /// </summary>
        [TestMethod]
        public void TwoNotification1SecDelay()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            manager.Enqueue("建造完了", 0, "");
            manager.Flush();
            PAssert.That(() => new Message {Title = "建造が終わりました", Body = "第一ドック", Name = "建造完了"}.Equals(result));
            manager.Flush();
            manager.Enqueue("建造完了", 1, "");
            manager.Flush();
            PAssert.That(() => new Message {Title = "建造が終わりました", Body = "第二ドック", Name = "建造完了"}.Equals(result));
        }

        /// <summary>
        /// 通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void SingleRepeatableNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 2);
                        manager.Flush();
                        PAssert.That(() => expected.Equals(result));
                        break;
                    case 2000:
                        manager.Flush();
                        PAssert.That(() => expected.Repeat.Equals(result));
                        break;
                    case 4000:
                        manager.Flush();
                        PAssert.That(() => expected.Repeat.Equals(result));
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// 二つの通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void TwoRepeatableNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var ensei = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var hakuchi = new Message {Title = "泊地修理 第一艦隊", Body = "20分経過しました。", Name = "泊地修理20分経過"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 2000:
                        manager.Enqueue("泊地修理20分経過", 0, "", 5);
                        manager.Flush();
                        PAssert.That(() => hakuchi.Equals(result));
                        break;
                    case 7000:
                        manager.Flush();
                        PAssert.That(() => hakuchi.Repeat.Equals(result), "泊地修理2回目");
                        break;
                    case 10000:
                        manager.Flush();
                        PAssert.That(() => ensei.Repeat.Equals(result), "遠征終了2回目");
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// スケジュールがぶつかる二つの通知をリピートさせる
        /// </summary>
        [TestMethod]
        public void TwoRepeatableNotification1SecDelay()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var ensei = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var hakuchi = new Message {Title = "泊地修理 第一艦隊", Body = "20分経過しました。", Name = "泊地修理20分経過"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 3);
                        manager.Flush();
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 1000:
                        manager.Enqueue("泊地修理20分経過", 0, "", 2);
                        manager.Flush();
                        break;
                    case 2000:
                        manager.Flush();
                        PAssert.That(() => hakuchi.Equals(result));
                        break;
                    case 4000:
                        manager.Flush();
                        PAssert.That(() => ensei.Repeat.Equals(result), "遠征終了2回目");
                        break;
                    case 6000:
                        manager.Flush();
                        PAssert.That(() => hakuchi.Repeat.Equals(result), "泊地修理2回目");
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// リピートしている通知を止める
        /// </summary>
        [TestMethod]
        public void RemoveRepeatableNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var ensei = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var nyukyo = new Message {Title = "入渠が終わりました", Body = "第一ドック 綾波改二", Name = "入渠終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 2000:
                        manager.Enqueue("入渠終了", 0, "綾波改二", 5);
                        manager.Flush();
                        PAssert.That(() => nyukyo.Equals(result));
                        break;
                    case 3000:
                        manager.StopRepeat("入渠終了");
                        manager.Flush();
                        break;
                    case 7000:
                        manager.Flush();
                        PAssert.That(() => result == null, "入渠終了2回目はない");
                        break;
                    case 10000:
                        manager.Flush();
                        PAssert.That(() => ensei.Repeat.Equals(result), "遠征終了2回目");
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// リピートを中断・再開する
        /// </summary>
        [TestMethod]
        public void SuspendRepeat()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => expected.Equals(result));
                        break;
                    case 1000:
                        manager.Flush();
                        manager.SuspendRepeat();
                        break;
                    case 11000:
                        manager.Flush();
                        manager.ResumeRepeat();
                        break;
                    case 12000:
                        manager.Flush();
                        PAssert.That(() => expected.Repeat.Equals(result));
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// リピートを例外付きで中断・再開する
        /// </summary>
        [TestMethod]
        public void SuspendRepeatWithException()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var ensei = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var taiha = new Message {Title = "大破した艦娘がいます", Body = "摩耶改二", Name = "大破警告"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => ensei.Equals(result));
                        break;
                    case 1000:
                        manager.Flush();
                        manager.SuspendRepeat("大破警告");
                        break;
                    case 2000:
                        manager.Enqueue("大破警告", "摩耶改二", 8);
                        manager.Flush();
                        PAssert.That(() => taiha.Equals(result));
                        break;
                    case 10000:
                        manager.Flush();
                        PAssert.That(() => taiha.Repeat.Equals(result));
                        break;
                    case 11000:
                        manager.Flush();
                        manager.ResumeRepeat();
                        break;
                    case 12000:
                        manager.Flush();
                        PAssert.That(() => ensei.Repeat.Equals(result));
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// リピート中の特定の通知を止める
        /// </summary>
        [TestMethod]
        public void StopSpecificRepeatingNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected1 = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var expected2 = new Message {Title = "遠征が終わりました", Body = "第三艦隊 海上護衛任務", Name = "遠征終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => expected1.Equals(result));
                        break;
                    case 1000:
                        manager.Enqueue("遠征終了", 2, "海上護衛任務", 10);
                        manager.Flush();
                        break;
                    case 2000:
                        manager.Flush();
                        PAssert.That(() => expected2.Equals(result));
                        break;
                    case 5000:
                        manager.Flush();
                        manager.StopRepeat("遠征終了", 1);
                        break;
                    case 12000:
                        manager.Flush();
                        PAssert.That(() => expected2.Repeat.Equals(result));
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// 継続中のリピートは艦隊やドックの番号だけ通知する
        /// </summary>
        [TestMethod]
        public void ContinueRepeatWithoutSubject()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected1 = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            var expected2 = new Message {Title = "遠征が終わりました", Body = "第二艦隊 ", Name = "遠征終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Flush();
                        PAssert.That(() => expected1.Equals(result));
                        break;
                    case 2000:
                        manager.Flush();
                        manager.StopRepeat("遠征終了", true);
                        break;
                    case 10000:
                        manager.Flush();
                        PAssert.That(() => expected2.Cont.Equals(result));
                        break;
                    case 11000:
                        manager.Flush();
                        manager.StopRepeat("遠征終了", 1);
                        break;
                    case 21000:
                        manager.Flush();
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }

        /// <summary>
        /// 予告する
        /// </summary>
        [TestMethod]
        public void PreliminaryNotification()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected = new Message {Title = "[予告] 遠征が終わりました", Body = "第二艦隊 防空射撃演習", Name = "遠征終了"};
            manager.Enqueue("遠征終了", 1, "防空射撃演習", 0, true);
            manager.Flush();
            PAssert.That(() => expected.Equals(result));
        }

        /// <summary>
        /// 同時に通知されるタイトルが同じ通知をマージする
        /// </summary>
        [TestMethod]
        public void MergeTwoNotificationsWithSameTitle()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
            manager.Enqueue("遠征終了", 2, "海上護衛任務", 10);
            manager.Flush();
            PAssert.That(() =>
                new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習\r\n第三艦隊 海上護衛任務", Name = "遠征終了"}.Equals(result));
        }

        /// <summary>
        /// マージされた二つの通知の一方を止める
        /// </summary>
        [TestMethod]
        public void StopOneOfMergedNotifications()
        {
            var time = new TimeProvider();
            Message result = null;
            var manager =
                new Scheduler((t, b, n) => { result = new Message {Title = t, Body = b, Name = n}; }, time.GetNow);
            var expected1 = new Message {Title = "遠征が終わりました", Body = "第二艦隊 防空射撃演習\r\n第三艦隊 海上護衛任務", Name = "遠征終了"};
            var expected2 = new Message {Title = "遠征が終わりました", Body = "第三艦隊 海上護衛任務", Name = "遠征終了"};
            var elapsed = 0;
            while (true)
            {
                switch (elapsed)
                {
                    case 0:
                        manager.Enqueue("遠征終了", 1, "防空射撃演習", 10);
                        manager.Enqueue("遠征終了", 2, "海上護衛任務", 10);
                        manager.Flush();
                        PAssert.That(() => expected1.Equals(result));
                        break;
                    case 5000:
                        manager.Flush();
                        manager.StopRepeat("遠征終了", 1);
                        break;
                    case 10000:
                        manager.Flush();
                        PAssert.That(() => expected2.Repeat.Equals(result));
                        return;
                    default:
                        manager.Flush();
                        PAssert.That(() => result == null, elapsed.ToString());
                        break;
                }
                result = null;
                elapsed += 1000;
            }
        }
    }
}