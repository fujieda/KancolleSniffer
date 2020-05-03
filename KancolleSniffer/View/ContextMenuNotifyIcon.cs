using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace KancolleSniffer.View
{
    public class ContextMenuNotifyIcon : ContextMenuStrip
    {
        private readonly ToolStripMenuItem[] _menuItems =
        {
            new ToolStripMenuItem
            {
                Font = new Font("メイリオ", 9F, FontStyle.Bold,
                    GraphicsUnit.Point, 128),
                Size = new Size(121, 22),
                Text = "開く(&O)"
            },
            new ToolStripMenuItem
            {
                Size = new Size(121, 22),
                Text = "終了(&X)"
            }
        };

        public ContextMenuNotifyIcon()
        {
            // ReSharper disable once VirtualMemberCallInConstructor
            Items.AddRange(_menuItems.ToArray<ToolStripItem>());
        }

        public void SetEventHandlers(Action open, Action exit)
        {
            _menuItems[0].Click += (sender , e) => open();
            _menuItems[1].Click += (sender , e) => exit();
        }
    }
}