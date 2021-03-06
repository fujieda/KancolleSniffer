﻿// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using KancolleSniffer.Model;

namespace KancolleSniffer.View.ListWindow
{
    public class ItemTreeView : TreeView
    {
        public void SetNodes(ItemStatus[] itemList)
        {
            SetNodes(CreateItemNodes(itemList));
        }

        private TreeNode CreateItemNodes(IEnumerable<ItemStatus> itemList)
        {
            var grouped = from item in itemList
                where !item.Spec.Empty
                orderby item.Spec.Type, item.Spec.Id, item.Alv, item.Level
                group item by item.Spec.Type
                into byTypeGroup
                from bySpec in (from item in byTypeGroup
                    group item by item.Spec.Id
                    into bySpecGroup
                    from byParam in (from item in bySpecGroup
                        group item by new {item.Alv, item.Level}
                        into byParamGroup
                        from byHolder in (from item in byParamGroup group item by item.Holder.Id)
                        group byHolder by byParamGroup.Key)
                    group byParam by bySpecGroup.Key)
                group bySpec by byTypeGroup.Key;

            var root = new TreeNode();
            foreach (var byType in grouped)
            {
                var typeName = byType.First().First().First().First().Spec.TypeName;
                var typeNode = new TreeNode();
                typeNode.Name = typeNode.Text = typeName;
                root.Nodes.Add(typeNode);
                foreach (var bySpec in byType)
                {
                    var item = bySpec.First().First().First();
                    var itemNode = new TreeNode();
                    itemNode.Name = itemNode.Text = item.Spec.Name + "x" +
                                                    bySpec.SelectMany(spec => spec).SelectMany(param => param).Count();
                    typeNode.Nodes.Add(itemNode);
                    foreach (var byParam in bySpec)
                    {
                        TreeNode paramNode;
                        if (bySpec.Count() == 1 && byParam.Key.Alv == 0 && byParam.Key.Level == 0)
                        {
                            paramNode = itemNode;
                        }
                        else
                        {
                            paramNode = new TreeNode();
                            item = byParam.First().First();
                            paramNode.Name = paramNode.Text =
                                (item.Spec.IsAircraft ? "+" + item.Alv : "") + "★" + item.Level + "x" +
                                byParam.SelectMany(param => param).Count();
                            itemNode.Nodes.Add(paramNode);
                        }
                        foreach (var byShip in byParam)
                        {
                            var ship = byShip.First().Holder;
                            var name = ship.Empty
                                ? "未装備x" + byShip.Count()
                                : (ship.Fleet == null ? "" : ship.Fleet.Number + 1 + " ") +
                                  ship.Name + (ship.Level > 0 ? "Lv" + ship.Level : "") + "x" + byShip.Count();
                            paramNode.Nodes.Add(name, name);
                        }
                    }
                }
            }
            return root;
        }

        private void SetNodes(TreeNode root)
        {
            var save = SaveTreeViewState(Nodes);
            UpdateNodes(Nodes, root.Nodes);
            RestoreTreeViewState(Nodes, save.Nodes);
        }

        private TreeNode SaveTreeViewState(IEnumerable nodes)
        {
            var result = new TreeNode();
            foreach (TreeNode child in nodes)
            {
                var copy = SaveTreeViewState(child.Nodes);
                if (child.IsExpanded)
                    copy.Expand();
                copy.Name = child.Name;
                result.Nodes.Add(copy);
            }
            return result;
        }

        private void RestoreTreeViewState(TreeNodeCollection dst, TreeNodeCollection src)
        {
            foreach (TreeNode d in dst)
            {
                var s = src[d.Name];
                if (s == null)
                    continue;
                if (s.IsExpanded)
                    d.Expand();
                RestoreTreeViewState(d.Nodes, s.Nodes);
            }
        }

        private void UpdateNodes(TreeNodeCollection prev, TreeNodeCollection now)
        {
            for (var i = 0; i < now.Count; i++)
            {
                if (i < prev.Count)
                {
                    if (prev[i].Name == now[i].Name)
                    {
                        UpdateNodes(prev[i].Nodes, now[i].Nodes);
                        continue;
                    }
                    prev.RemoveAt(i);
                }
                prev.Insert(i, now[i]);
            }
            for (var i = now.Count; i < prev.Count; i++)
                prev.RemoveAt(i);
        }

        // ReSharper disable InconsistentNaming
        // ReSharper disable IdentifierTypo

        private const int TV_FIRST = 0x1100;

        private const int TVM_SETEXTENDEDSTYLE = TV_FIRST + 44;

        private const int TVS_EX_DOUBLEBUFFER = 0x0004;

        // ReSharper restore IdentifierTypo
        // ReSharper restore InconsistentNaming

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);

        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            // Enable double buffer
            SendMessage(Handle, TVM_SETEXTENDEDSTYLE, (IntPtr)TVS_EX_DOUBLEBUFFER, (IntPtr)TVS_EX_DOUBLEBUFFER);
        }
    }
}