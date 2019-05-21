// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer.View
{
    /// <summary>
    /// 艦娘名の横幅
    /// 艦娘名のラベルのZ-orderは最下なので名前が長すぎると右隣のラベルの下に隠れるが、
    /// 空装備マークはラベルの右端に表示するので右端が見えるように縮める必要がある。
    /// </summary>
    public enum ShipNameWidth
    {
        MainPanel = 92, // 左端2 HP右端129幅35 129-2-35=92
        AkashiTimer = 53, // 左端2 タイマー左端55 55-2=53 漢字4文字
        NDock = 65, // 左端29 終了時刻右端138幅47 138-47-29=62 空装備マークなし漢字5文字65
        RepairList = 65, // 左端9 時間左端75 75-9=66 漢字5文字65
        RepairListFull = 73, // 左端10 HP右端118幅35 118-10-35=73
        ShipList = 81, // 左端10 HP右端126幅35 126-10-35=81
        GroupConfig = 80, // 左端10 レベル左端90 90-10=80
        Combined = 53, // 左端2 HP右端88幅35 88-2-35=51 空装備マーク犠牲 漢字4文字53
        BattleResult = 65, // 左端2 HP右端101幅35 101-1-35=65
        CiShipName = 65, // 左端168幅236 236-168=68 漢字5文字65
        Max = int.MaxValue
    }
}