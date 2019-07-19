// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.Model
{
    public class AirCorpsFighterPower
    {
        public Model.Range AirCombat;
        public Model.Range Interception;
        public bool Difference;

        public AirCorpsFighterPower(Range fighterPower)
        {
            AirCombat = new Model.Range(fighterPower.Min.AirCombat, fighterPower.Max.AirCombat);
            Interception = new Model.Range(fighterPower.Min.Interception, fighterPower.Max.Interception);
            Difference = Interception.Min != AirCombat.Min;
        }

        public struct Pair
        {
            public double AirCombat { get; }
            public double Interception { get; }

            public Pair(double airCombat, double interception)
            {
                AirCombat = airCombat;
                Interception = interception;
            }

            public static Pair Max(Pair value1, Pair value2)
            {
                return new Pair(Math.Max(value1.AirCombat, value2.AirCombat),
                    Math.Max(value1.Interception, value2.Interception));
            }

            public static Pair operator +(Pair lhs, Pair rhs)
            {
                return new Pair(lhs.AirCombat + rhs.AirCombat, lhs.Interception + rhs.Interception);
            }

            public static Pair operator +(Pair lhs, double rhs)
            {
                return new Pair(lhs.AirCombat + rhs, lhs.Interception + rhs);
            }

            public static Pair operator *(Pair lhs, Pair rhs)
            {
                return new Pair(lhs.AirCombat * rhs.AirCombat, lhs.Interception * rhs.Interception);
            }

            public static Pair operator *(Pair lhs, double rhs)
            {
                return new Pair(lhs.AirCombat * rhs, lhs.Interception * rhs);
            }

            public Pair Floor()
            {
                return new Pair((int)AirCombat, (int)Interception);
            }
        }

        public struct Range
        {
            public Pair Min { get; }
            public Pair Max { get; }

            public Range(Pair min, Pair max)
            {
                Min = min;
                Max = max;
            }

            public Range(Pair base_, RangeD range)
            {
                Min = (base_ + range.Min).Floor();
                Max = (base_ + range.Max).Floor();
            }

            public static Range operator +(Range lhs, Range rhs)
            {
                return new Range(lhs.Min + rhs.Min, lhs.Max + rhs.Max);
            }

            public static Range operator *(Range lhs, Pair rhs)
            {
                return new Range(lhs.Min * rhs, lhs.Max * rhs);
            }

            public Range Floor()
            {
                return new Range(Min.Floor(), Max.Floor());
            }
        }
    }
}