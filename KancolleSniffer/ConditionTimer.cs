using System;
using System.Linq;

namespace KancolleSniffer
{
    public class ConditionTimer
    {
        private readonly ShipInfo _shipInfo;
        private readonly DateTime[][] _times = new DateTime[ShipInfo.FleetCount][];
        private readonly bool[] _enable = new bool[ShipInfo.FleetCount];

        public ConditionTimer(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
            for (var f = 0; f < _times.Length; f++)
                _times[f] = new DateTime[3];
        }

        public void SetTimer()
        {
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
            {
                _enable[fleet] = true;
                var cond =
                    (from id in _shipInfo.GetDeck(fleet) where id != -1 select _shipInfo[id].Cond)
                        .DefaultIfEmpty(49).Min();
                var time49 = _times[fleet][2];
                if (cond < 49 && time49 != DateTime.MinValue) // 計時中
                {
                    // コンディション値から推定される残り時刻と経過時間の差
                    var diff = TimeSpan.FromMinutes((49 - cond + 2) / 3 * 3) - (time49 - DateTime.Now);
                    if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(3)) // 差が0以上3分以内ならタイマーを更新しない。
                        return;
                }
                var thresh = new[] {30, 40, 49};
                for (var i = 0; i < thresh.Length; i++)
                    _times[fleet][i] = cond < thresh[i]
                        ? DateTime.Now.AddMinutes((thresh[i] - cond + 2) / 3 * 3)
                        : DateTime.MinValue;
            }
        }

        public void Disable(int fleet)
        {
            _enable[fleet] = false;
        }

        public string[] GetTimerStrings(int fleet)
        {
            if (!_enable[fleet])
                return new[] {"無効", "無効", "無効"};
            var now = DateTime.Now;
            return
                (from time in _times[fleet] select time > now ? (time - now).ToString(@"mm\:ss") : "00:00")
                    .ToArray();
        }
    }
}