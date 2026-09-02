using System;
using System.Drawing;
using System.Windows.Forms;

namespace Seay代码审计工具
{
    internal class DarkTreeView : TreeView
    {
        private int _hoverItemY = -1;

        public DarkTreeView()
        {
            this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
            this.ShowLines = false;
            this.FullRowSelect = true;
            this.HideSelection = false;
            this.BorderStyle = BorderStyle.None;
            this.BackColor = ThemeHelper.ControlColor;
            this.ForeColor = ThemeHelper.TextColor;

            this.DrawNode += DarkTreeView_DrawNode;
            this.MouseMove += DarkTreeView_MouseMove;
            this.MouseLeave += DarkTreeView_MouseLeave;
        }

        private void DarkTreeView_MouseMove(object sender, MouseEventArgs e)
        {
            TreeNode hoverNode = this.GetNodeAt(e.X, e.Y);
            if (hoverNode != null)
            {
                _hoverItemY = hoverNode.Bounds.Y;
            }
            else
            {
                _hoverItemY = -1;
            }
            this.Invalidate();
        }

        private void DarkTreeView_MouseLeave(object sender, EventArgs e)
        {
            _hoverItemY = -1;
            this.Invalidate();
        }

        private void DarkTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
        {
            Rectangle bounds = e.Bounds;
            if (bounds.IsEmpty) return;

            Color backColor = ThemeHelper.ControlColor;
            Color foreColor = ThemeHelper.TextColor;

            if ((e.State & TreeNodeStates.Selected) != 0)
            {
                backColor = ThemeHelper.SelectedColor;
            }
            else if (bounds.Y == _hoverItemY)
            {
                backColor = ThemeHelper.HoverColor;
            }

            using (SolidBrush backBrush = new SolidBrush(backColor))
            {
                e.Graphics.FillRectangle(backBrush, new Rectangle(0, bounds.Y, this.Width, bounds.Height));
            }

            int imageIndex = e.Node.ImageIndex;
            if (imageIndex >= 0 && this.ImageList != null && imageIndex < this.ImageList.Images.Count)
            {
                int imageX = bounds.X - 20;
                int imageY = bounds.Y + (bounds.Height - this.ImageList.ImageSize.Height) / 2;
                e.Graphics.DrawImage(this.ImageList.Images[imageIndex], imageX, imageY);
            }

            TextRenderer.DrawText(
                e.Graphics,
                e.Node.Text,
                this.Font,
                new Point(bounds.X, bounds.Y),
                foreColor,
                TextFormatFlags.VerticalCenter | TextFormatFlags.Left
            );
        }
    }
}
