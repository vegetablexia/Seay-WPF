using System.Drawing;
using System.Windows.Forms;

namespace Seay代码审计工具
{
    internal static class ThemeHelper
    {
        public static Color BgColor = Color.FromArgb(37, 37, 38);
        public static Color BgLightColor = Color.FromArgb(45, 45, 48);
        public static Color ControlColor = Color.FromArgb(51, 51, 55);
        public static Color ControlLightColor = Color.FromArgb(62, 62, 66);
        public static Color TextColor = Color.FromArgb(241, 241, 241);
        public static Color TextDimColor = Color.FromArgb(160, 160, 160);
        public static Color AccentColor = Color.FromArgb(0, 122, 204);
        public static Color AccentHoverColor = Color.FromArgb(28, 150, 224);
        public static Color BorderColor = Color.FromArgb(68, 68, 72);
        public static Color HoverColor = Color.FromArgb(70, 70, 74);
        public static Color SelectedColor = Color.FromArgb(0, 122, 204);
        public static Color ErrorColor = Color.FromArgb(220, 53, 69);
        public static Color SuccessColor = Color.FromArgb(40, 167, 69);
        public static Color WarningColor = Color.FromArgb(255, 193, 7);

        public static void ApplyForm(Form form)
        {
            form.BackColor = BgColor;
            form.ForeColor = TextColor;
        }

        public static void ApplyToolStrip(ToolStrip toolStrip)
        {
            toolStrip.BackColor = ControlColor;
            toolStrip.ForeColor = TextColor;
            toolStrip.Renderer = new DarkToolStripRenderer();
            toolStrip.Padding = new Padding(8, 4, 8, 4);
        }

        public static void ApplyTreeView(TreeView tv)
        {
            tv.BackColor = ControlColor;
            tv.ForeColor = TextColor;
            tv.BorderStyle = BorderStyle.None;
        }

        public static void ApplyTextBox(TextBox txt)
        {
            txt.BackColor = ControlColor;
            txt.ForeColor = TextColor;
            txt.BorderStyle = BorderStyle.FixedSingle;
        }

        public static void ApplyComboBox(ComboBox cmb)
        {
            cmb.BackColor = ControlColor;
            cmb.ForeColor = TextColor;
            cmb.FlatStyle = FlatStyle.Flat;
        }

        public static void ApplyListView(ListView lv)
        {
            lv.BackColor = ControlColor;
            lv.ForeColor = TextColor;
            lv.BorderStyle = BorderStyle.None;
            lv.GridLines = false;
        }

        public static void ApplyRichTextBox(RichTextBox rtb)
        {
            rtb.BackColor = ControlColor;
            rtb.ForeColor = TextColor;
            rtb.BorderStyle = BorderStyle.None;
        }

        public static void ApplyTabControl(TabControl tab)
        {
            tab.BackColor = BgColor;
            tab.ForeColor = TextColor;
        }

        public static void ApplyPanel(Panel panel)
        {
            panel.BackColor = BgColor;
            panel.ForeColor = TextColor;
        }

        public static void ApplyGroupBox(GroupBox gb)
        {
            gb.BackColor = BgColor;
            gb.ForeColor = TextColor;
        }

        public static void ApplyLabel(Label label)
        {
            label.BackColor = Color.Transparent;
            label.ForeColor = TextColor;
        }

        public static void ApplyButton(Button btn)
        {
            btn.BackColor = AccentColor;
            btn.ForeColor = Color.White;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 0;
            btn.FlatAppearance.MouseOverBackColor = AccentHoverColor;
            btn.FlatAppearance.MouseDownBackColor = Color.FromArgb(0, 100, 180);
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplySecondaryButton(Button btn)
        {
            btn.BackColor = ControlColor;
            btn.ForeColor = TextColor;
            btn.FlatStyle = FlatStyle.Flat;
            btn.FlatAppearance.BorderSize = 1;
            btn.FlatAppearance.BorderColor = BorderColor;
            btn.FlatAppearance.MouseOverBackColor = HoverColor;
            btn.Cursor = Cursors.Hand;
        }

        public static void ApplyCheckBox(CheckBox cb)
        {
            cb.BackColor = Color.Transparent;
            cb.ForeColor = TextColor;
        }

        public static void ApplyRadioButton(RadioButton rb)
        {
            rb.BackColor = Color.Transparent;
            rb.ForeColor = TextColor;
        }

        public static void ApplyLinkLabel(LinkLabel ll)
        {
            ll.BackColor = Color.Transparent;
            ll.LinkColor = AccentColor;
            ll.ActiveLinkColor = AccentHoverColor;
        }

        public static void ApplyAllControls(Control parent)
        {
            foreach (Control ctrl in parent.Controls)
            {
                if (ctrl is Label)
                    ApplyLabel((Label)ctrl);
                else if (ctrl is TextBox)
                    ApplyTextBox((TextBox)ctrl);
                else if (ctrl is ComboBox)
                    ApplyComboBox((ComboBox)ctrl);
                else if (ctrl is Button)
                    ApplyButton((Button)ctrl);
                else if (ctrl is CheckBox)
                    ApplyCheckBox((CheckBox)ctrl);
                else if (ctrl is RadioButton)
                    ApplyRadioButton((RadioButton)ctrl);
                else if (ctrl is LinkLabel)
                    ApplyLinkLabel((LinkLabel)ctrl);
                else if (ctrl is Panel)
                    ApplyPanel((Panel)ctrl);
                else if (ctrl is GroupBox)
                    ApplyGroupBox((GroupBox)ctrl);
                else if (ctrl is ListView)
                    ApplyListView((ListView)ctrl);
                else if (ctrl is RichTextBox)
                    ApplyRichTextBox((RichTextBox)ctrl);
                else if (ctrl is ToolStrip)
                    ApplyToolStrip((ToolStrip)ctrl);
                else if (ctrl is TabControl)
                    ApplyTabControl((TabControl)ctrl);

                if (ctrl.HasChildren)
                    ApplyAllControls(ctrl);
            }
        }
    }
}
