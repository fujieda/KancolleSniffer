// Copyright (c) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections.Specialized;
using System.Globalization;
using System.Text;

namespace KancolleSniffer.Util
{
    public class HttpUtility
    {
        public static NameValueCollection ParseQueryString(string query)
        {
            var r = new NameValueCollection();
            var seg = UrlDecode(query).Split('&');
            foreach (var st in seg)
            {
                var pair = st.Split('=');
                if (pair.Length <= 0)
                    continue;
                var key = pair[0].Trim('?', ' ');
                var val = pair[1].Trim();
                r.Add(key, val);
            }
            return r;
        }

        public static string UrlDecode(string s)
        {
            return Uri.UnescapeDataString(s.Replace('+', ' '));
        }

        public static string JavascriptStringEncode(string s)
        {
            if (string.IsNullOrEmpty(s))
                return "";
            var sb = new StringBuilder();
            foreach (var ch in s)
            {
                switch (ch)
                {
                    case '\\':
                        sb.Append(@"\\");
                        break;
                    case '"':
                        sb.Append(@"\""");
                        break;
                    case '/':
                        sb.Append(@"\/");
                        break;
                    case '\b':
                        sb.Append(@"\b");
                        break;
                    case '\t':
                        sb.Append(@"\t");
                        break;
                    case '\n':
                        sb.Append(@"\n");
                        break;
                    case '\f':
                        sb.Append(@"\f");
                        break;
                    case '\r':
                        sb.Append(@"\r");
                        break;
                    default:
                        CharEncode(sb, ch);
                        break;
                }
            }
            return sb.ToString();
        }

        private static void CharEncode(StringBuilder sb, char ch)
        {
            if (ch < 0x20 || ch == '<' || ch == '>' || ch == '&')
            {
                sb.Append(@"\u");
                sb.Append(((int)ch).ToString("x04", CultureInfo.InvariantCulture));
                return;
            }
            sb.Append(ch);
        }
    }
}