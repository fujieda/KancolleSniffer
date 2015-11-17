// Copyright (C) 2014, 2015 Kazuhiro Fujieda <fujieda@users.osdn.me>
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

namespace KancolleSniffer
{
    public class ItemTreeView : DbTreeView
    {
        private ItemStatus[] _prevItemList;

        public void SetNodes(ItemStatus[] itemList)
        {
            if (_prevItemList != null && _prevItemList.SequenceEqual(itemList, new ItemStatusComparer()))
                return;
            _prevItemList = itemList.Select(CloneItemStatus).ToArray();
            SetNodes(CreateItemNodes(itemList));
        }

        private TreeNode CreateItemNodes(IEnumerable<ItemStatus> itemList)
        {
            var grouped = from byId in
                (from item in itemList
                    where item.Spec.Id != -1
                    orderby item.Spec.Type, item.Spec.Id, item.Alv, item.Level descending, item.Ship.Spec.Id
                    group item by new {item.Spec.Id, item.Alv, item.Level})
                group byId by byId.First().Spec.Type;
            var root = new TreeNode();
            foreach (var byType in grouped)
            {
                var typeName = byType.First().First().Spec.TypeName;
                var typeNode = new TreeNode();
                typeNode.Name = typeNode.Text = typeName;
                root.Nodes.Add(typeNode);
                foreach (var byItem in byType)
                {
                    var item = byItem.First();
                    var itemNode = new TreeNode();
                    itemNode.Name = itemNode.Text = item.Spec.Name +
                                                    (item.Alv == 0 ? "" : "+" + item.Alv) +
                                                    (item.Level == 0 ? "" : "★" + item.Level);
                    typeNode.Nodes.Add(itemNode);

                    var shipGroup = (from i in byItem group i.Ship by i.Ship.Id).ToArray();
                    foreach (var name in
                        from grp in shipGroup
                        where grp.Key != -1
                        let ship = grp.First()
                        select (ship.Fleet != -1 ? ship.Fleet + 1 + " " : "") +
                               ship.Name + "Lv" + ship.Level + "×" + grp.Count())
                    {
                        itemNode.Nodes.Add(name, name);
                    }
                    foreach (var name in (from grp in shipGroup where grp.Key == -1 select "未装備×" + grp.Count()))
                    {
                        itemNode.Nodes.Add(name, name);
                    }
                }
            }
            return root;
        }

        private ItemStatus CloneItemStatus(ItemStatus org)
        {
            return new ItemStatus
            {
                Level = org.Level,
                Spec = org.Spec,
                Ship = new ShipStatus {Id = org.Ship.Id, Fleet = org.Ship.Fleet}
            };
        }

        private class ItemStatusComparer : IEqualityComparer<ItemStatus>
        {
            public bool Equals(ItemStatus x, ItemStatus y)
                => x.Level == y.Level && x.Spec == y.Spec && x.Ship.Id == y.Ship.Id && x.Ship.Fleet == y.Ship.Fleet;

            public int GetHashCode(ItemStatus obj) => obj.Level + obj.Spec.GetHashCode() + obj.Ship.GetHashCode();
        }

        [DllImport("user32.dll")]
        private static extern int GetScrollPos(IntPtr hWnd, int nBar);

        [DllImport("user32.dll")]
        private static extern int SetScrollPos(IntPtr hWnd, int nBar, int nPos, bool bRedraw);

        private void SetNodes(TreeNode root)
        {
            var y = GetScrollPos(Handle, 1);
            BeginUpdate();
            var save = SaveTreeViewState(Nodes);
            Nodes.Clear();
            foreach (TreeNode child in root.Nodes)
                Nodes.Add(child);
            RestoreTreeViewState(Nodes, save.Nodes);
            EndUpdate();
            SetScrollPos(Handle, 1, y, true);
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
    }
}