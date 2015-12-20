// Copyright (C) 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
// 
// This program is part of KancolleSniffer.
//
// KancolleSniffer is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, see <http://www.gnu.org/licenses/>.

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