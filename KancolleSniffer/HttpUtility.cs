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

namespace KancolleSniffer
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
    }
}