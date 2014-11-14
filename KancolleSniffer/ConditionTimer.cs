using System;
using System.Linq;

namespace KancolleSniffer
{
    public class ConditionTimer
    {
        private readonly ShipInfo _shipInfo;
        private readonly DateTime[] _times = new DateTime[ShipInfo.FleetCount];
        private readonly bool[] _enable = new bool[ShipInfo.FleetCount];
        private readonly int[] _cond = new int[ShipInfo.FleetCount];
        private readonly TimeSpan[] _prevLeftTimes = new TimeSpan[ShipInfo.FleetCount];

        public ConditionTimer(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
        }

        public void SetTimer()
        {
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
            {
                if (!_enable[fleet]) // タイマーが無効なら前回の残り時間を無効にする
                    _prevLeftTimes[fleet] = TimeSpan.MinValue;
                if (_shipInfo.InMission(fleet) || _shipInfo.InSortie(fleet))
                    continue;
                _enable[fleet] = true;
                var cond = _cond[fleet] = CondMin(fleet);
                if (cond < 49 && _times[fleet] != DateTime.MinValue) // 計時中
                {
                    // コンディション値から推定される残り時刻と経過時間の差
                    var diff = TimeSpan.FromMinutes((49 - cond + 2) / 3 * 3) - (_times[fleet] - DateTime.Now);
                    if (diff >= TimeSpan.Zero && diff <= TimeSpan.FromMinutes(3)) // 差が0以上3分以内ならタイマーを更新しない。
                        return;
                }
                _times[fleet] = cond < 49
                    ? DateTime.Now.AddMinutes((49 - cond + 2) / 3 * 3)
                    : DateTime.MinValue;
            }
        }

        public void Invalidate(int fleet)
        {
            _enable[fleet] = _cond[fleet] == CondMin(fleet);
        }

        public void Disable(int fleet)
        {
            _enable[fleet] = false;
        }

        private int CondMin(int fleet)
        {
            return (from id in _shipInfo.GetDeck(fleet) where id != -1 select _shipInfo[id].Cond)
                .DefaultIfEmpty(49).Min();
        }

        public DateTime GetTimer(int fleet)
        {
            return _enable[fleet] ? _times[fleet] : DateTime.MinValue;
        }

        public int[] GetNotice()
        {
            var result = new int[_times.Length];
            for (var f = 0; f < _times.Length; f++)
            {
                var now = _times[f] == DateTime.MinValue ? TimeSpan.Zero : _times[f] - DateTime.Now;
                var prev = _prevLeftTimes[f];
                _prevLeftTimes[f] = now;
                if (prev == TimeSpan.MinValue)
                    continue;
                if (prev > TimeSpan.FromMinutes(9) && now <= TimeSpan.FromMinutes(9))
                    result[f] = 40;
                else if (prev > TimeSpan.Zero && now <= TimeSpan.Zero)
                    result[f] = 49;
            }
            return result;
        }
    }
}