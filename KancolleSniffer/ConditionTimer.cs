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

        public ConditionTimer(ShipInfo shipInfo)
        {
            _shipInfo = shipInfo;
        }

        public void SetTimer()
        {
            for (var fleet = 0; fleet < ShipInfo.FleetCount; fleet++)
            {
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

        private int CondMin(int fleet)
        {
            return (from id in _shipInfo.GetDeck(fleet) where id != -1 select _shipInfo[id].Cond)
                .DefaultIfEmpty(49).Min();
        }

        public DateTime GetTimer(int fleet)
        {
            return _enable[fleet] ? _times[fleet] : DateTime.MinValue;
        }
    }
}