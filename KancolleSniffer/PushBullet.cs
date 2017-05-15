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

using System.Net;
using System.Text;

namespace KancolleSniffer
{
    public class PushBullet
    {
        public static void PushNote(string token, string title, string body)
        {
            using (var wc = new WebClient())
            {
                wc.Headers[HttpRequestHeader.ContentType] = "application/json";
                wc.Credentials = new NetworkCredential(token, "");
                wc.Encoding = Encoding.UTF8;
                wc.UploadString("https://api.pushbullet.com/v2/pushes",
                    $"{{ \"type\": \"note\", \"title\": \"{title}\", \"body\": \"{body}\" }}");
            }
        }
    }
}