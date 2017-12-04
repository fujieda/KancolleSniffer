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
    public class ErrorLogTest
    {
        [TestMethod]
        public void RemoveTokenFromRequest()
        {
            var request =
                "api%5Fverno=1&api%5Ftoken=0123456abcdef&api%5Fport=0123456789";
            var response = "";
            ErrorLog.RemoveUnwantedInformation(ref request, ref response);
            PAssert.That(() => request == "api%5Fverno=1&api%5Fport=0123456789", "トークンが中間");
            var request2 = "api%5Fverno=1&api%5Ftoken=0123456abcdef";
            ErrorLog.RemoveUnwantedInformation(ref request2, ref response);
            PAssert.That(() => request2 == @"api%5Fverno=1", "トークンが末尾");
            var request3 = "api%5Ftoken=0123456abcdef&api%5Fverno=1";
            ErrorLog.RemoveUnwantedInformation(ref request3, ref response);
            PAssert.That(() => request3 == @"api%5Fverno=1", "トークンが先頭");
            var request4 = "api%5Ftoken=0123456abcdef";
            ErrorLog.RemoveUnwantedInformation(ref request4, ref response);
            PAssert.That(() => request4 == "", "トークン単独");
        }

        [TestMethod]
        public void RemoveUnwantedInformationFromResponse()
        {
            var request = "";
            var response = @"{""api_result"":1,""api_result_msg"":""成功"",""api_data"":"+
                            @"{""api_basic"":{""api_member_id"":""123456""," +
                            @"""api_nickname"":""ぱんなこった"",""api_nickname_id"":""12345678"",""api_active_flag"":1}}}";
            ErrorLog.RemoveUnwantedInformation(ref request, ref response);
            PAssert.That(() => response == @"{""api_basic"":{""api_active_flag"":1}}");
            var response2 = @"{""api_result"":1,""api_result_msg"":""成功""}";
            ErrorLog.RemoveUnwantedInformation(ref request, ref response2);
            PAssert.That(() => response2 == @"");
        }
    }
}