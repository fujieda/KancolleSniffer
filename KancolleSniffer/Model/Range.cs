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
    public struct Range
    {
        public int Min { get; }
        public int Max { get; }

        public Range(int min, int max)
        {
            if (max < min)
                throw new ArgumentException();
            Min = min;
            Max = max;
        }

        public Range(double min, double max)
        {
            if (max < min)
                throw new ArgumentException();
            Min = (int)min;
            Max = (int)max;
        }

        public Range(double base_, RangeD range)
        {
            Min = (int)(base_ + range.Min);
            Max = (int)(base_ + range.Max);
        }

        public static Range operator +(Range lhs, Range rhs)
        {
            return new Range(lhs.Min + rhs.Min, lhs.Max + rhs.Max);
        }

        public override bool Equals(object other)
        {
            if (!(other is Range range))
                return false;
            return Max == range.Max && Min == range.Min;
        }

        public override int GetHashCode()
        {
            return 0;
        }

        public static bool operator ==(Range lhs, Range rhs)
        {
            return lhs.Equals(rhs);
        }

        public static bool operator !=(Range lhs, Range rhs)
        {
            return !(lhs == rhs);
        }

        public bool Diff => Min != Max;

        public string RangeString => $"{Min}～{Max}";
    }

    public struct RangeD
    {
        public double Min { get; }
        public double Max { get; }

        public RangeD(double min, double max)
        {
            if (max < min)
                throw new ArgumentException();
            Min = min;
            Max = max;
        }

        public RangeD Sqrt() => new RangeD(Math.Sqrt(Min), Math.Sqrt(Max));

        public static RangeD operator +(RangeD lhs, double rhs)
        {
            return new RangeD(lhs.Min + rhs, lhs.Max + rhs);
        }

        public static explicit operator Range(RangeD range)
        {
            return new Range((int)range.Min, (int)range.Max);
        }
    }
}