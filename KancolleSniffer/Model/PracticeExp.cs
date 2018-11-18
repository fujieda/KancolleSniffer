using System.Collections.Generic;
using System.Linq;
using static System.Math;

namespace KancolleSniffer.Model
{
    public static class PracticeExp
    {
        private static readonly int[] ExpTable =
        {
            0, 100, 300, 600, 1000, 1500, 2100, 2800, 3600, 4500,
            5500, 6600, 7800, 9100, 10500, 12000, 13600, 15300, 17100, 19000,
            21000, 23100, 25300, 27600, 30000, 32500, 35100, 37800, 40600, 43500,
            46500, 49600, 52800, 56100, 59500, 63000, 66600, 70300, 74100, 78000,
            82000, 86100, 90300, 94600, 99000, 103500, 108100, 112800, 117600, 122500,
            127500, 132700, 138100, 143700, 149500, 155500, 161700, 168100, 174700, 181500,
            188500, 195800, 203400, 211300, 219500, 228000, 236800, 245900, 255300, 265000,
            275000, 285400, 296200, 307400, 319000, 331000, 343400, 356200, 369400, 383000,
            397000, 411500, 426500, 442000, 458000, 474500, 491500, 509000, 527000, 545500,
            564500, 584500, 606500, 631500, 661500, 701500, 761500, 851500, 1000000, 1000000,
            1010000, 1011000, 1013000, 1016000, 1020000, 1025000, 1031000, 1038000, 1046000, 1055000,
            1065000, 1077000, 1091000, 1107000, 1125000, 1145000, 1168000, 1194000, 1223000, 1255000,
            1290000, 1329000, 1372000, 1419000, 1470000, 1525000, 1584000, 1647000, 1714000, 1785000,
            1860000, 1940000, 2025000, 2115000, 2210000, 2310000, 2415000, 2525000, 2640000, 2760000,
            2887000, 3021000, 3162000, 3310000, 3465000, 3628000, 3799000, 3978000, 4165000, 4360000,
            4564000, 4777000, 4999000, 5230000, 5470000, 5720000, 5780000, 5860000, 5970000, 6120000,
            6320000, 6580000, 6910000, 7320000, 7820000, 7920000, 8033000, 8172000, 8350000, 8580000,
            8875000, 9248000, 9705000, 10266000, 10950000
        };

        public static double GetExp(int ship1Lv, int ship2Lv)
        {
            var raw = ExpTable[Min(ship1Lv, ExpTable.Length) - 1] / 100.0 +
                      ExpTable[Min(ship2Lv, ExpTable.Length) - 1] / 300.0;
            return raw >= 500 ? 500 + (int)Sqrt(raw - 500) : (int)raw;
        }

        public static double TrainingCruiserBonus(IReadOnlyList<ShipStatus> fleet) =>
            1.0 + TrainingCruiserBonusRate(fleet);

        private static double TrainingCruiserBonusRate(IReadOnlyList<ShipStatus> fleet)
        {
            if (fleet[0].Spec.IsTrainingCruiser)
            {
                var fsLevel = fleet[0].Level;
                if (fleet.Skip(1).Any(s => s.Spec.IsTrainingCruiser))
                {
                    if (fsLevel < 10)
                        return 0.10;
                    if (fsLevel < 30)
                        return 0.13;
                    if (fsLevel < 60)
                        return 0.16;
                    if (fsLevel < 100)
                        return 0.20;
                    return 0.25;
                }
                if (fsLevel < 10)
                    return 0.05;
                if (fsLevel < 30)
                    return 0.08;
                if (fsLevel < 60)
                    return 0.12;
                if (fsLevel < 100)
                    return 0.15;
                return 0.20;
            }
            var tc = fleet.Count(s => s.Spec.IsTrainingCruiser);
            if (tc == 0)
                return 0;
            var level = fleet.Where(s => s.Spec.IsTrainingCruiser).Max(s => s.Level);
            if (tc == 1)
            {
                if (level < 10)
                    return 0.03;
                if (level < 30)
                    return 0.05;
                if (level < 60)
                    return 0.07;
                if (level < 100)
                    return 0.10;
                return 0.15;
            }
            if (level < 10)
                return 0.04;
            if (level < 30)
                return 0.06;
            if (level < 60)
                return 0.08;
            if (level < 100)
                return 0.12;
            return 0.175;
        }
    }
}