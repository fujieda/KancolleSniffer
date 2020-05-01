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

using ExpressionToCodeLib;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace KancolleSniffer.Test
{
    [TestClass]
    public class PrivacyTest
    {
        [ClassInitialize]
        public static void Initialize(TestContext context)
        {
            SnifferTest.Initialize(context);
        }

        [TestMethod]
        public void RemoveToken()
        {
            const string request = "api%5Fverno=1&api%5Ftoken=0123456abcdef&api%5Fport=0123456789";
            PAssert.That(() => RemoveToken(request) == "api%5Fverno=1&api%5Fport=0123456789", "トークンが中間");
            const string request2 = "api%5Fverno=1&api%5Ftoken=0123456abcdef";
            PAssert.That(() => RemoveToken(request2) == @"api%5Fverno=1", "トークンが末尾");
            const string request3 = "api%5Ftoken=0123456abcdef&api%5Fverno=1";
            PAssert.That(() => RemoveToken(request3) == @"api%5Fverno=1", "トークンが先頭");
            const string request4 = "api%5Ftoken=0123456abcdef";
            PAssert.That(() => RemoveToken(request4) == "", "トークン単独");
            const string request5 = "api%5Fbtime=83026279&api%5Ftoken=0123456abcdef&api%5Fverno=1";
            PAssert.That(() => RemoveToken(request5) == "api%5Fverno=1", "戦闘APIの時刻印を削除");
        }

        /// <summary>
        /// 二期は%エンコードされていない
        /// </summary>
        [TestMethod]
        public void RemoveToken2()
        {
            var request = "api_verno=1&api_token=0123456abcdef&api_port=0123456789";
            PAssert.That(() => RemoveToken(request) == "api_verno=1&api_port=0123456789", "トークンが中間");
            var request5 = "api_btime=83026279&api_token=0123456abcdef&api_verno=1";
            PAssert.That(() => RemoveToken(request5) == "api_verno=1", "戦闘APIの時刻印を削除");
        }

        [TestMethod]
        public void RemoveName()
        {
            const string response1 =
                @"{""api_result"":1,""api_result_msg"":""成功"",""api_data"":{""api_basic"":{""api_member_id"":""123456""," +
                @"""api_nickname"":""ぱんなこった"",""api_nickname_id"":""12345678"",""api_active_flag"":1}}}";
            const string result1 =
                @"{""api_result"":1,""api_result_msg"":""成功"",""api_data"":{""api_basic"":{""api_active_flag"":1}}}";
            PAssert.That(() => RemoveName(response1) == result1);
            const string response2 =
                @"{""api_deck_data"":[{""api_member_id"":123456,""api_id"":1,""api_name"":""第一艦隊"",""api_name_id"":""123456"",""api_mission"":[0,0,0,0],""api_flagship"":""0""}]}";
            const string result2 =
                @"{""api_deck_data"":[{""api_id"":1,""api_mission"":[0,0,0,0],""api_flagship"":""0""}]}";
            PAssert.That(() => RemoveName(response2) == result2);
            const string response3 =
                @"{""api_deck_data"":[{""api_member_id"":123456,""api_id"":1,""api_name"":""第\\/一艦\\""隊\\"""",""api_name_id"":""123456"",""api_mission"":[0,0,0,0],""api_flagship"":""0""}]}";
            PAssert.That(() => RemoveName(response3) == result2);
        }

        private string RemoveToken(string query)
        {
            var s = new Main.Session(query, null, null);
            Privacy.Remove(s);
            return s.Url;
        }

        private string RemoveName(string response)
        {
            var s = new Main.Session(null, null, response);
            Privacy.Remove(s);
            return s.Response;
        }
    }
}