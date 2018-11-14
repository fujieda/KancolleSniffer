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
        public static void Remove(ref string url, ref string request, ref string response)
        {
            if (url != null)
                url = RemoveToken(url);
            if (request != null)
                request = RemoveToken(request);
            if (response != null && !(url != null && url.Contains("start2")))
                response = RemoveName(response);
        }

        private static readonly Regex NameRegex = new Regex(
            @"""api_member_id"":""?\d*""?,|""api_(?:nick)?name"":""(?:[^\""]|\\.)*"",""api_(?:nick)?name_id"":""\d*"",",
            RegexOptions.Compiled);

        public static string RemoveName(string response)
        {
            var result = NameRegex.Replace(response, "");
            return result;
        }

        public static string RemoveToken(string query)
        {
            var result = new Regex(@"api(?:%5F|_)token=\w+|api(?:%5F|_)btime=\w+").Replace(query, "");
            result = result.Replace("&&", "&");
            result = result.Replace("?&", "?");
            result = result.Trim('&', '?');
            return result;
        }
    }
}