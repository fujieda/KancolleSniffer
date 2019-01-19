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
    public struct AirBaseParams
    {
        public double AirCombat { get; }
        public double Interception { get; }

        public AirBaseParams(double airCombat, double interception)
        {
            AirCombat = airCombat;
            Interception = interception;
        }

        public static AirBaseParams Max(AirBaseParams value1, AirBaseParams value2)
        {
            return new AirBaseParams(Math.Max(value1.AirCombat, value2.AirCombat),
                Math.Max(value1.Interception, value2.Interception));
        }

        public static AirBaseParams operator +(AirBaseParams lhs, AirBaseParams rhs)
        {
            return new AirBaseParams(lhs.AirCombat + rhs.AirCombat, lhs.Interception + rhs.Interception);
        }

        public static AirBaseParams operator +(AirBaseParams lhs, double rhs)
        {
            return new AirBaseParams(lhs.AirCombat + rhs, lhs.Interception + rhs);
        }

        public static AirBaseParams operator *(AirBaseParams lhs, AirBaseParams rhs)
        {
            return new AirBaseParams(lhs.AirCombat * rhs.AirCombat, lhs.Interception * rhs.Interception);
        }

        public static AirBaseParams operator *(AirBaseParams lhs, double rhs)
        {
            return new AirBaseParams(lhs.AirCombat * rhs, lhs.Interception * rhs);
        }

        public AirBaseParams Floor()
        {
            return new AirBaseParams((int)AirCombat, (int)Interception);
        }
    }
}