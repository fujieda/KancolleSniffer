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
using System.Text;

namespace KancolleSniffer
{
    public static class Ascii85
    {
        public static string Encode(byte[] input)
        {
            var count = 0;
            var tuple = 0u;
            var output = new StringBuilder();
            output.Append("<~");
            foreach (byte ch in input)
            {
                tuple |=  (uint)ch << (24 - 8 * count++);
                if (count < 4)
                    continue;
                output.Append(tuple == 0 ? "z" : Encode85(tuple, count));
                tuple = 0;
                count = 0;
            }
            if (count > 0)
                output.Append(Encode85(tuple, count));
            output.Append("~>");
            return output.ToString();
        }

        private static string Encode85(uint tuple, int count)
        {
            var buf = new char[5];
            for (var j = 4; j >= 0; j--)
            {
                buf[j] = (char)(tuple % 85 + '!');
                tuple /= 85;
            }
            Array.Resize(ref buf, count + 1);
            return new string(buf);
        }
    }
}