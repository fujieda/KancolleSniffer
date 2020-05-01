// Copyright (C) 2018 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

using System.Text.RegularExpressions;

namespace KancolleSniffer
{
    public static class Privacy
    {
        public static void Remove(MainForm.Session s)
        {
            RemoveToken(s);
            RemoveName(s);
        }

        private static void RemoveToken(MainForm.Session s)
        {
            s.Url = RemoveToken(s.Url);
            s.Request = RemoveToken(s.Request);
        }

        private static string RemoveToken(string query)
        {
            if (query == null)
                return null;
            var result = new Regex(@"api(?:%5F|_)token=\w+|api(?:%5F|_)btime=\w+").Replace(query, "");
            return result.Replace("&&", "&").Replace("?&", "?").Trim('&', '?');
        }

        private static readonly Regex NameRegex = new Regex(
            @"""api_member_id"":""?\d*""?,|""api_(?:nick)?name"":""(?:[^\""]|\\.)*"",""api_(?:nick)?name_id"":""\d*"",",
            RegexOptions.Compiled);

        private static void RemoveName(MainForm.Session s)
        {
            if (s.Response != null && !(s.Url != null && s.Url.Contains("start2")))
                s.Response = NameRegex.Replace(s.Response, "");
        }
    }
}