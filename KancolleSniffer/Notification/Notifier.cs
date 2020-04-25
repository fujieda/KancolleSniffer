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
using System.Linq;
using System.Threading.Tasks;
using KancolleSniffer.Model;
using KancolleSniffer.Net;
using KancolleSniffer.Util;
using KancolleSniffer.View;

namespace KancolleSniffer.Notification
{
    public class Notifier : IUpdateContext, Sniffer.IRepeatingTimerController
    {
        private readonly Scheduler _scheduler;
        private readonly Method _method;

        private class Method
        {
            public Action FlashWindow;
            public Action<string, string> ShowToaster;
            public Action<string, int> PlaySound;
        }

        public UpdateContext Context { get; set; }

        private TimeStep Step => Context.GetStep();

        private NotificationConfig Notifications => Context.Config.Notifications;

        public Notifier(Action flashWindow, Action<string, string> showToaster, Action<string, int> playSound)
        {
            _method = new Method
            {
                FlashWindow = flashWindow,
                ShowToaster = showToaster,
                PlaySound = playSound,
            };
            _scheduler = new Scheduler(Alarm);
        }

        public void Stop(string key)
        {
            _scheduler.StopRepeat(key,
                (key == "入渠終了" || key == "遠征終了") &&
                (Context.Config.Notifications[key].Flags & NotificationType.Cont) != 0);
        }

        public void Stop(string key, int fleet) => _scheduler.StopRepeat(key, fleet);

        public void Suspend(string exception = null) => _scheduler.SuspendRepeat(exception);

        public void Resume() => _scheduler.ResumeRepeat();

        public void StopAllRepeat() => _scheduler.StopAllRepeat();

        public void StopRepeatingTimer(IEnumerable<string> names)
        {
            foreach (var name in names)
                _scheduler.StopRepeat(name);
        }

        public void NotifyShipItemCount()
        {
            var ship = Context.Sniffer.ShipCounter;
            if (ship.Alarm)
            {
                var message = $"残り{ship.Rest:D}隻";
                _scheduler.Enqueue("艦娘数超過", message);
                ship.Alarm = false;
            }
            var item = Context.Sniffer.ItemCounter;
            if (item.Alarm)
            {
                var message = $"残り{item.Rest:D}個";
                _scheduler.Enqueue("装備数超過", message);
                item.Alarm = false;
            }
            _scheduler.Flush();
        }

        public void NotifyDamagedShip()
        {
            _scheduler.StopRepeat("大破警告");
            if (!Context.Sniffer.BadlyDamagedShips.Any())
                return;
            SetNotification("大破警告", string.Join(" ", Context.Sniffer.BadlyDamagedShips));
            _scheduler.Flush();
        }

        public void NotifyTimers()
        {
            for (var i = 0; i < Context.Sniffer.Missions.Length; i++)
            {
                var entry = Context.Sniffer.Missions[i];
                if (entry.Name == "前衛支援任務" || entry.Name == "艦隊決戦支援任務")
                    continue;
                CheckAlarm("遠征終了", entry.Timer, i + 1, entry.Name);
            }
            for (var i = 0; i < Context.Sniffer.NDock.Length; i++)
            {
                var entry = Context.Sniffer.NDock[i];
                CheckAlarm("入渠終了", entry.Timer, i, entry.Name);
            }
            for (var i = 0; i < Context.Sniffer.KDock.Length; i++)
            {
                var timer = Context.Sniffer.KDock[i];
                CheckAlarm("建造完了", timer, i, "");
            }
            NotifyCondTimers();
            NotifyAkashiTimer();
            _scheduler.Flush();
        }

        private void CheckAlarm(string key, AlarmTimer timer, int fleet, string subject)
        {
            if (timer.CheckAlarm(Step))
            {
                SetNotification(key, fleet, subject);
                return;
            }
            var pre = TimeSpan.FromSeconds(Notifications[key].PreliminaryPeriod);
            if (pre == TimeSpan.Zero)
                return;
            if (timer.CheckAlarm(Step + pre))
                SetPreNotification(key, fleet, subject);
        }

        private void NotifyCondTimers()
        {
            var notice = Context.Sniffer.GetConditionNotice(Step);
            var pre = TimeSpan.FromSeconds(Notifications["疲労回復"].PreliminaryPeriod);
            var preNotice = pre == TimeSpan.Zero
                ? new int[ShipInfo.FleetCount]
                : Context.Sniffer.GetConditionNotice(Step + pre);
            var conditions = Context.Config.NotifyConditions;
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (conditions.Contains(notice[i]))
                {
                    SetNotification("疲労回復" + notice[i], i, "cond" + notice[i]);
                }
                else if (conditions.Contains(preNotice[i]))
                {
                    SetPreNotification("疲労回復" + preNotice[i], i, "cond" + notice[i]);
                }
            }
        }

        private void NotifyAkashiTimer()
        {
            var akashi = Context.Sniffer.AkashiTimer;
            var msgs = akashi.GetNotice(Step);
            if (msgs.Length == 0)
            {
                _scheduler.StopRepeat("泊地修理");
                return;
            }
            if (!akashi.CheckRepairing(Context.GetStep().Now) && !(akashi.CheckPresetRepairing() && Context.Config.UsePresetAkashi))
            {
                _scheduler.StopRepeat("泊地修理");
                return;
            }
            var skipPreliminary = false;
            if (msgs[0].Proceeded == "20分経過しました。")
            {
                SetNotification("泊地修理20分経過", msgs[0].Proceeded);
                msgs[0].Proceeded = "";
                skipPreliminary = true;
                // 修理完了がいるかもしれないので続ける
            }
            for (var i = 0; i < ShipInfo.FleetCount; i++)
            {
                if (msgs[i].Proceeded != "")
                    SetNotification("泊地修理進行", i, msgs[i].Proceeded);
                if (msgs[i].Completed != "")
                    SetNotification("泊地修理完了", i, msgs[i].Completed);
            }
            var pre = TimeSpan.FromSeconds(Notifications["泊地修理20分経過"].PreliminaryPeriod);
            if (skipPreliminary || pre == TimeSpan.Zero)
                return;
            if ((msgs = akashi.GetNotice(Step + pre))[0].Proceeded == "20分経過しました。")
                SetPreNotification("泊地修理20分経過", 0, msgs[0].Proceeded);
        }

        public void NotifyQuestComplete()
        {
            Context.Sniffer.GetQuestNotifications(out var notify, out var stop);
            foreach (var questName in notify)
                SetNotification("任務達成", 0, questName);
            foreach (var questName in stop)
                _scheduler.StopRepeat("任務達成", questName);
            _scheduler.Flush();
        }

        private void SetNotification(string key, string subject)
        {
            SetNotification(key, 0, subject);
        }

        private void SetNotification(string key, int fleet, string subject)
        {
            var spec = Spec(key);
            _scheduler.Enqueue(key, fleet, subject,
                (spec.Flags & Context.Config.NotificationFlags & NotificationType.Repeat) == 0
                    ? 0
                    : spec.RepeatInterval);
        }

        private void SetPreNotification(string key, int fleet, string subject)
        {
            if ((Spec(key).Flags & NotificationType.Preliminary) != 0)
                _scheduler.Enqueue(key, fleet, subject, 0, true);
        }

        private NotificationSpec Spec(string key)
        {
            return Notifications[_scheduler.KeyToName(key)];
        }

        private void Alarm(string balloonTitle, string balloonMessage, string name)
        {
            if (Check(name, NotificationType.FlashWindow))
                _method.FlashWindow();
            if (Check(name, NotificationType.ShowBaloonTip))
                _method.ShowToaster(balloonTitle, balloonMessage);
            if (Check(name, NotificationType.PlaySound))
                _method.PlaySound(Context.Config.Sounds[name], Context.Config.Sounds.Volume);
            if (Context.Config.Pushbullet.On && CheckPush(name))
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushbullet(Context.Config.Pushbullet.Token, balloonTitle,
                        balloonMessage);
                });
            }
            if (Context.Config.Pushover.On && CheckPush(name))
            {
                Task.Run(() =>
                {
                    PushNotification.PushToPushover(Context.Config.Pushover.ApiKey, Context.Config.Pushover.UserKey,
                        balloonTitle, balloonMessage);
                });
            }
        }

        private bool Check(string name, NotificationType type)
        {
            return (Flags(name) & type) != 0;
        }

        private NotificationType Flags(string name)
        {
            return Context.Config.NotificationFlags & Notifications[name].Flags;
        }

        private bool CheckPush(string name)
        {
            return (Notifications[name].Flags & NotificationType.Push) != 0;
        }
    }
}