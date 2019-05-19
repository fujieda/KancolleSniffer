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

using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ShipLabels : ControlsArranger
    {
        public ShipLabel Fleet { get; set; }
        public ShipLabel.Name Name { get; set; }
        public ShipLabel.Hp Hp { get; set; }
        public ShipLabel.Cond Cond { get; set; }
        public ShipLabel Level { get; set; }
        public ShipLabel Exp { get; set; }
        public Label BackGround { get; set; }

        public override Control[] Controls =>
            new Control[] {Hp, Cond, Level, Exp, Name, Fleet, BackGround}.Where(c => c != null)
                .ToArray(); // 名前のZ-orderを下に
    }
}