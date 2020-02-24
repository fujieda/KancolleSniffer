// Copyright (C) 2020 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer
{
    public partial class MainForm
    {
        private class ListFormGroup
        {
            private readonly MainForm _mainForm;
            private readonly List<ListForm> _listForms = new List<ListForm>();

            public ListForm Main => _listForms[0];

            public ListFormGroup(MainForm mainForm)
            {
                _mainForm = mainForm;
                _listForms.Add(new ListForm(mainForm) {IsMaster = true});
                for (var i = 0; i < mainForm.Config.ListFormGroup.Count; i++)
                    _listForms.Add(new ListForm(mainForm) {Owner = Main});
            }

            public void ShowOrCreate()
            {
                foreach (var listForm in _listForms)
                {
                    if (listForm.WindowState == FormWindowState.Minimized)
                    {
                        listForm.WindowState = FormWindowState.Normal;
                        return;
                    }
                    if (!listForm.Visible)
                    {
                        listForm.Show();
                        return;
                    }
                }
                var newForm = new ListForm(_mainForm) {Owner = Main, TopMost = Main.TopMost, Font = Main.Font};
                newForm.UpdateList();
                newForm.Show();
                _listForms.Add(newForm);
            }

            public void UpdateList()
            {
                InvokeAll(listForm => listForm.UpdateList());
            }

            public void UpdateAirBattleResult()
            {
                InvokeAll(listForm => listForm.UpdateAirBattleResult());
            }

            public void UpdateBattleResult()
            {
                InvokeAll(listForm => listForm.UpdateBattleResult());
            }

            public void UpdateCellInfo()
            {
                InvokeAll(listForm => listForm.UpdateCellInfo());
            }

            public bool Visible => _listForms.Any(listForm => listForm.Visible);

            public bool TopMost
            {
                set { InvokeAll(listFrom => { listFrom.TopMost = value; }); }
            }

            public Font Font
            {
                get => _listForms[0].Font;
                set { InvokeAll(listForm => { listForm.Font = value; }); }
            }

            public void ShowShip(int id)
            {
                InvokeAll(listForm => listForm.ShowShip(id));
            }

            public void Show()
            {
                InvokeAll(listForm => listForm.Show());
            }

            public void Close()
            {
                InvokeAll(listForm => listForm.Close());
            }

            private void InvokeAll(Action<ListForm> action)
            {
                foreach (var listForm in _listForms)
                    action(listForm);
            }
        }
    }
}