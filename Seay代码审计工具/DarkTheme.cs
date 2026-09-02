using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace Seay代码审计工具
{
    internal class DarkToolStripRenderer : ToolStripProfessionalRenderer
    {
        private DarkColorTable _colorTable;

        public DarkToolStripRenderer() : base(new DarkColorTable())
        {
            _colorTable = new DarkColorTable();
            RoundedEdges = false;
        }

        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(ThemeHelper.ControlColor))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = ThemeHelper.TextColor;
            base.OnRenderItemText(e);
        }

        protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripButton button = e.Item as ToolStripButton;
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (button != null && button.Checked)
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.SelectedColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
            else if (button != null && button.Selected)
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.HoverColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }

            if (button != null && button.Pressed)
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.SelectedColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
        }

        protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
        {
            ToolStripDropDownButton dropDown = e.Item as ToolStripDropDownButton;
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (dropDown != null && (dropDown.Selected || dropDown.Pressed))
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.HoverColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
        }

        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
        {
            e.ArrowColor = ThemeHelper.TextColor;
            base.OnRenderArrow(e);
        }

        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);
            using (Pen pen = new Pen(ThemeHelper.BorderColor))
            {
                e.Graphics.DrawLine(pen, bounds.Left + 4, bounds.Height / 2, bounds.Right - 4, bounds.Height / 2);
            }
        }

        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            Rectangle bounds = new Rectangle(Point.Empty, e.Item.Size);

            if (e.Item.Selected)
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.HoverColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
            else
            {
                using (SolidBrush brush = new SolidBrush(ThemeHelper.ControlColor))
                {
                    e.Graphics.FillRectangle(brush, bounds);
                }
            }
        }

        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
        {
            using (SolidBrush brush = new SolidBrush(ThemeHelper.ControlColor))
            {
                e.Graphics.FillRectangle(brush, e.AffectedBounds);
            }
        }

        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        {
            using (Pen pen = new Pen(ThemeHelper.BorderColor))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1);
            }
        }
    }

    internal class DarkColorTable : ProfessionalColorTable
    {
        public override Color ToolStripGradientBegin => ThemeHelper.ControlColor;
        public override Color ToolStripGradientMiddle => ThemeHelper.ControlColor;
        public override Color ToolStripGradientEnd => ThemeHelper.ControlColor;

        public override Color MenuStripGradientBegin => ThemeHelper.ControlColor;
        public override Color MenuStripGradientEnd => ThemeHelper.ControlColor;

        public override Color MenuItemSelected => ThemeHelper.HoverColor;
        public override Color MenuItemSelectedGradientBegin => ThemeHelper.HoverColor;
        public override Color MenuItemSelectedGradientEnd => ThemeHelper.HoverColor;

        public override Color MenuItemBorder => ThemeHelper.BorderColor;
        public override Color MenuBorder => ThemeHelper.BorderColor;

        public override Color ImageMarginGradientBegin => ThemeHelper.ControlColor;
        public override Color ImageMarginGradientMiddle => ThemeHelper.ControlColor;
        public override Color ImageMarginGradientEnd => ThemeHelper.ControlColor;

        public override Color SeparatorDark => ThemeHelper.BorderColor;
        public override Color SeparatorLight => ThemeHelper.BorderColor;

        public override Color CheckBackground => ThemeHelper.SelectedColor;
        public override Color CheckPressedBackground => ThemeHelper.SelectedColor;
        public override Color CheckSelectedBackground => ThemeHelper.SelectedColor;
    }
}
