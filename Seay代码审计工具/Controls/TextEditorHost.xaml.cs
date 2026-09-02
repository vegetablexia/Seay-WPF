using System;
using System.Drawing;
using System.Windows.Controls;
using ICSharpCode.TextEditor;
using ICSharpCode.TextEditor.Document;

namespace Seay代码审计工具.Controls
{
    public partial class TextEditorHost : UserControl
    {
        private TextEditorControl editor;

        public TextEditorHost()
        {
            InitializeComponent();
            CreateEditor();
        }

        private void CreateEditor()
        {
            editor = new TextEditorControl();
            editor.Dock = System.Windows.Forms.DockStyle.Fill;
            editor.BackColor = Color.FromArgb(30, 30, 30);
            editor.ForeColor = Color.FromArgb(241, 241, 241);
            editor.Font = new Font("Consolas", 13);
            editor.ShowLineNumbers = true;
            editor.IsIconBarVisible = false;
            editor.Encoding = System.Text.Encoding.Default;

            editor.ActiveTextAreaControl.TextArea.KeyDown += (s, e) =>
            {
                if (e.Control && e.KeyCode == System.Windows.Forms.Keys.S)
                {
                    FileSaved?.Invoke(this, EventArgs.Empty);
                    e.Handled = true;
                }
            };

            ApplyDarkScrollbars();

            wfhHost.Child = editor;
        }

        [System.Runtime.InteropServices.DllImport("uxtheme.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr hWnd, string subAppName, string subIdList);

        private void ApplyDarkScrollbars()
        {
            var tac = editor.ActiveTextAreaControl;
            if (tac == null) return;
            DarkenScrollbar(tac.VScrollBar);
            DarkenScrollbar(tac.HScrollBar);
        }

        private void DarkenScrollbar(System.Windows.Forms.Control scrollbar)
        {
            if (scrollbar == null) return;
            scrollbar.HandleCreated += (s, e) =>
                SetWindowTheme(((System.Windows.Forms.Control)s).Handle, "DarkMode_Explorer", null);
            if (scrollbar.IsHandleCreated)
                SetWindowTheme(scrollbar.Handle, "DarkMode_Explorer", null);
        }

        public TextEditorControl Editor => editor;

        public string Text
        {
            get { return editor?.Text ?? ""; }
            set { if (editor != null) editor.Text = value; }
        }

        public string SelectedText => editor?.ActiveTextAreaControl?.SelectionManager?.SelectedText ?? "";

        public event EventHandler FileSaved;

        public void SetHighlighting(string language)
        {
            if (editor != null)
            {
                editor.Document.HighlightingStrategy =
                    HighlightingStrategyFactory.CreateHighlightingStrategy(language);
            }
        }

        public void SetHighlightingByExtension(string ext)
        {
            if (editor == null) return;
            string lang = "PHP";
            switch (ext.ToLower())
            {
                case ".php": case ".php3": case ".php4": case ".php5": case ".phtml":
                    lang = "PHP"; break;
                case ".js": case ".json":
                    lang = "JavaScript"; break;
                case ".html": case ".htm": case ".xhtml":
                    lang = "HTML"; break;
                case ".css":
                    lang = "CSS"; break;
                case ".xml": case ".xaml": case ".config":
                    lang = "XML"; break;
                case ".sql":
                    lang = "SQL"; break;
                case ".cs":
                    lang = "C#"; break;
                case ".java":
                    lang = "Java"; break;
                case ".py":
                    lang = "Python"; break;
                case ".rb":
                    lang = "Ruby"; break;
                case ".ini": case ".conf":
                    lang = "INI"; break;
                case ".bat": case ".cmd":
                    lang = "BAT"; break;
            }
            SetHighlighting(lang);
        }

        public void GoToLine(int line)
        {
            if (editor == null) return;
            editor.ActiveTextAreaControl.Caret.Line = line - 1;
            editor.ActiveTextAreaControl.Caret.Column = 0;
            editor.ActiveTextAreaControl.TextArea.ScrollToCaret();
        }

        public void SelectText(string text)
        {
            if (editor == null || string.IsNullOrEmpty(text)) return;
            int offset = editor.Text.IndexOf(text);
            if (offset < 0) return;

            var start = editor.Document.OffsetToPosition(offset);
            var end = editor.Document.OffsetToPosition(offset + text.Length);
            editor.ActiveTextAreaControl.SelectionManager.SetSelection(
                new DefaultSelection(editor.Document, start, end));
            editor.ActiveTextAreaControl.Caret.Position = end;
            editor.ActiveTextAreaControl.TextArea.ScrollToCaret();
        }

        public int FindNext(string text, int startOffset = 0)
        {
            if (editor == null || string.IsNullOrEmpty(text)) return -1;
            int idx = editor.Text.IndexOf(text, startOffset, StringComparison.OrdinalIgnoreCase);
            if (idx >= 0)
            {
                var start = editor.Document.OffsetToPosition(idx);
                var end = editor.Document.OffsetToPosition(idx + text.Length);
                editor.ActiveTextAreaControl.SelectionManager.SetSelection(
                    new DefaultSelection(editor.Document, start, end));
                editor.ActiveTextAreaControl.Caret.Position = end;
                editor.ActiveTextAreaControl.TextArea.ScrollToCaret();
            }
            return idx;
        }
    }
}
