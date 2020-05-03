using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ContextMenuMain : ContextMenuStrip
    {
        private readonly ToolStripMenuItem[] _menuItems =
        {
            new ToolStripMenuItem
            {
                Name = "listToolStripMenuItem",
                Size = new Size(125, 22),
                Text = "一覧(&L)"
            },
            new ToolStripMenuItem
            {
                Name = "LogToolStripMenuItem",
                Size = new Size(125, 22),
                Text = "報告書(&R)"
            },
            new ToolStripMenuItem
            {
                Name = "CaptureToolStripMenuItem",
                Size = new Size(125, 22),
                Text = "撮影(&C)"
            },
            new ToolStripMenuItem
            {
                Name = "ConfigToolStripMenuItem",
                Size = new Size(125, 22),
                Text = "設定(&O)"
            },
            new ToolStripMenuItem
            {
                Name = "ExitToolStripMenuItem",
                Size = new Size(125, 22),
                Text = "終了(&X)"
            }
        };

        public ContextMenuMain()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Items.AddRange(_menuItems.ToArray<ToolStripItem>());
        }

        public void SetClickHandlers(Action list, Action report, Action capture,
            Action config, Action exit)
        {
            foreach (var entry in _menuItems.Zip(new []{list, report, capture, config, exit}, (item, handler) => new {item, handler}))
                entry.item.Click += (sender, e) => entry.handler();
        }
    }
}