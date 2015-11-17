// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace KancolleSniffer
{
    public class KancolleDb
    {
        private readonly BlockingCollection<Tuple<string, string, string>> _queue =
            new BlockingCollection<Tuple<string, string, string>>(10);

        private bool _started;
        private string _token;

        public void Start(string token)
        {
            _token = token;
            if (_started)
                return;
            _started = true;
            Task.Run(() =>
            {
                try
                {
                    while (true)
                    {
                        var e = _queue.Take();
                        try
                        {
                            using (var wc = new WebClient())
                            {
                                var values = new NameValueCollection
                                {
                                    {"token", _token},
                                    {"agent", "KdpR5STmwYTaFpNCbD4N"},
                                    {"url", e.Item1},
                                    {"requestbody", e.Item2},
                                    {"responsebody", e.Item3}
                                };
                                wc.UploadValues("http://api.kancolle-db.net/2/", values);
                            }
                        }
                        catch (WebException)
                        {
                        }
                    }
                }
                catch (InvalidOperationException)
                {
                }
            });
        }

        private readonly HashSet<string> _urlSet = new HashSet<string>()
        {
            "/kcsapi/api_port/port",
            "/kcsapi/api_get_member/ship2",
            "/kcsapi/api_get_member/ship3",
            "/kcsapi/api_get_member/slot_item",
            "/kcsapi/api_get_member/kdock",
            "/kcsapi/api_get_member/mapinfo",
            "/kcsapi/api_req_hensei/change",
            "/kcsapi/api_req_kousyou/createship",
            "/kcsapi/api_req_kousyou/getship",
            "/kcsapi/api_req_kousyou/createitem",
            "/kcsapi/api_req_map/start",
            "/kcsapi/api_req_map/next",
            "/kcsapi/api_req_map/select_eventmap_rank",
            "/kcsapi/api_req_sortie/battle",
            "/kcsapi/api_req_battle_midnight/battle",
            "/kcsapi/api_req_battle_midnight/sp_midnight",
            "/kcsapi/api_req_sortie/night_to_day",
            "/kcsapi/api_req_sortie/battleresult",
            "/kcsapi/api_req_combined_battle/battle",
            "/kcsapi/api_req_combined_battle/airbattle",
            "/kcsapi/api_req_combined_battle/midnight_battle",
            "/kcsapi/api_req_combined_battle/battleresult",
            "/kcsapi/api_req_sortie/airbattle",
            "/kcsapi/api_req_combined_battle/battle_water",
            "/kcsapi/api_req_combined_battle/sp_midnight"
        };

        public void Send(string url, string request, string response)
        {
            if (!_urlSet.Contains(url))
                return;
            request = Regex.Replace(request, @"&api%5Ftoken=[^&]+|api%5Ftoken=[^&]+&?", "");
            response = response.Replace("svdata=", "");
            _queue.TryAdd(new Tuple<string, string, string>(url, request, response));
        }

        public void Stop()
        {
            _queue.CompleteAdding();
        }
    }
}