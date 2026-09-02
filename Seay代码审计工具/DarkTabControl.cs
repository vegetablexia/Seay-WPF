using System;
using System.Drawing;
using System.Windows.Forms;

namespace Seay代码审计工具
{
    internal class DarkTabControl : TabControl
    {
        private int _hoverTabIndex = -1;

        public DarkTabControl()
        {
            this.DrawMode = TabDrawMode.OwnerDrawFixed;
            this.Padding = new Point(12, 6);
            this.BackColor = ThemeHelper.BgColor;
            this.ForeColor = ThemeHelper.TextColor;

            this.DrawItem += DarkTabControl_DrawItem;
            this.MouseMove += DarkTabControl_MouseMove;
            this.MouseLeave += DarkTabControl_MouseLeave;
        }

        private void DarkTabControl_MouseMove(object sender, MouseEventArgs e)
        {
            int newHoverIndex = -1;
            for (int i = 0; i < this.TabCount; i++)
            {
                if (this.GetTabRect(i).Contains(e.Location))
                {
                    newHoverIndex = i;
                    break;
                }
            }

            if (newHoverIndex != _hoverTabIndex)
            {
                _hoverTabIndex = newHoverIndex;
                this.Invalidate();
            }
        }

        private void DarkTabControl_MouseLeave(object sender, EventArgs e)
        {
            _hoverTabIndex = -1;
            this.Invalidate();
        }

        private void DarkTabControl_DrawItem(object sender, DrawItemEventArgs e)
        {
            Rectangle tabBounds = this.GetTabRect(e.Index);

            Color backColor = ThemeHelper.ControlColor;
            if (e.Index == this.SelectedIndex)
            {
                backColor = ThemeHelper.SelectedColor;
            }
            else if (e.Index == _hoverTabIndex)
            {
                backColor = ThemeHelper.HoverColor;
            }

            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, tabBounds);
            }

            SizeF textSize = e.Graphics.MeasureString(this.TabPages[e.Index].Text, this.Font);
            int textX = (int)(tabBounds.X + (tabBounds.Width - textSize.Width) / 2);
            int textY = (int)(tabBounds.Y + (tabBounds.Height - textSize.Height) / 2);

            TextRenderer.DrawText(
                e.Graphics,
                this.TabPages[e.Index].Text,
                this.Font,
                new Point(textX, textY),
                ThemeHelper.TextColor
            );
        }
    }
}
