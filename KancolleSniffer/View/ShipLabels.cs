﻿// Copyright (C) 2019 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using KancolleSniffer.Model;

namespace KancolleSniffer.View
{
    public class ShipLabels : ControlsArranger
    {
        public ShipLabel.Fleet Fleet { get; set; }
        public ShipLabel.Name Name { get; set; }
        public ShipLabel.Hp Hp { get; set; }
        public ShipLabel.Cond Cond { get; set; }
        public ShipLabel.Level Level { get; set; }
        public ShipLabel.Exp Exp { get; set; }
        public Label BackGround { get; set; }

        // Nameが長すぎる場合は他のラベルの下に隠れてほしいのでのZ-orderを下にする。
        // サブクラスで追加するラベルはBackGroundで隠れないようにZ-orderを上にする。
        public sealed override Control[] Controls =>
            AddedControls.Concat(new Control[] {Fleet, Hp, Cond, Level, Exp, Name, BackGround}.Where(c => c != null))
                .ToArray();

        protected virtual Control[] AddedControls => new Control[0];

        public virtual void Set(ShipStatus status, ToolTip toolTip)
        {
            foreach (var label in new ShipLabel[] {Fleet, Name, Hp, Cond, Level, Exp})
                label?.Set(status);
            toolTip.SetToolTip(Name, status.GetEquipString());
        }

        public virtual void Reset()
        {
            foreach (var label in new ShipLabel[] {Fleet, Name, Hp, Cond, Level, Exp})
                label?.Reset();
        }
    }
}