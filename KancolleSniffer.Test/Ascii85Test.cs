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

using System.Collections.Generic;
using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class Ascii85Test
    {
        [TestMethod]
        public void Encode()
        {
            PAssert.That(() => Ascii85.Encode(new byte [0]) == "<~~>");
            var expected = new[]
            {
                "",
                "!!",
                "!!*",
                "!!*-",
                "!!*-'",
                "!!*-'\"9",
                "!!*-'\"9e",
                "!!*-'\"9eu"
            };
            var input = new List<byte>();
            for (var i = 0; i < expected.Length; input.Add((byte)i), i++)
            {
                var i1 = i;
                PAssert.That(() => Ascii85.Encode(input.ToArray()) == "<~" + expected[i1] + "~>", $"Length: {i1}");
            }
            PAssert.That(() => Ascii85.Encode(new byte[] {0, 0, 0, 0}) == "<~" + "z" + "~>");
        }
    }
}