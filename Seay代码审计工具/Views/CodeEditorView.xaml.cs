using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Seay代码审计工具.Views
{
    public partial class CodeEditorView : UserControl
    {
        public string FilePath { get; private set; }
        private string fileText = "";
        private int searchIndex = 0;

        public event EventHandler<string> RequestOpenSearch;
        public event EventHandler<string> RequestOpenRunPhp;

        public CodeEditorView()
        {
            InitializeComponent();
            editorHost.FileSaved += (s, e) => SaveFile();
        }

        public string Text
        {
            get { return editorHost.Text; }
        }

        public string SelectedText
        {
            get { return editorHost.SelectedText; }
        }

        public void SelectCode(string text)
        {
            editorHost.SelectText(text);
        }

        public void LoadFile(string filePath)
        {
            FilePath = filePath;
            string encodingName = F_Main.var_fileencoding;
            if (string.IsNullOrEmpty(encodingName)) encodingName = "UTF-8";

            try
            {
                using (var sr = new StreamReader(filePath, Encoding.GetEncoding(encodingName)))
                {
                    fileText = sr.ReadToEnd();
                }
                editorHost.Text = fileText;
            }
            catch (Exception)
            {
                fileText = "";
            }

            string ext = Path.GetExtension(filePath).ToLower();
            editorHost.SetHighlightingByExtension(ext);
            AnalyzeSymbols();
        }

        private void AnalyzeSymbols()
        {
            lboxSymbols.Items.Clear();

            lboxSymbols.Items.Add(new ListBoxItem
            {
                Content = "-- 函数列表 --",
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Gray
            });

            try
            {
                if (!string.IsNullOrEmpty(fileText))
                {
                    var matches = Regex.Matches(fileText,
                        @"function\s{1,5}(\w{1,20})\s{0,5}\(",
                        RegexOptions.RightToLeft);

                    var seen = new HashSet<string>();
                    for (int j = 0; j < matches.Count; j++)
                    {
                        if (matches[j].Success)
                        {
                            string name = matches[j].Groups[1].Value;
                            if (seen.Add(name))
                                lboxSymbols.Items.Add(name);
                        }
                    }
                }
            }
            catch (Exception) { }

            lboxSymbols.Items.Add(new ListBoxItem { Content = "" });
            lboxSymbols.Items.Add(new ListBoxItem
            {
                Content = "-- 变量列表 --",
                FontWeight = FontWeights.Bold,
                Foreground = System.Windows.Media.Brushes.Gray
            });

            try
            {
                if (!string.IsNullOrEmpty(fileText))
                {
                    var matches = Regex.Matches(fileText,
                        "\\$\\w{1,20}((\\[[\"']|\\[)\\${0,1}[\\w\\[\\]\"']{0,30}){0,1}",
                        RegexOptions.RightToLeft);

                    var seen = new HashSet<string>();
                    for (int j = 0; j < matches.Count; j++)
                    {
                        if (matches[j].Success)
                        {
                            string val = matches[j].Value;
                            if (seen.Add(val))
                                lboxSymbols.Items.Add(val);
                        }
                    }
                }
            }
            catch (Exception) { }
        }

        private void lboxSymbols_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            lboxReferences.Items.Clear();

            if (lboxSymbols.SelectedItem == null) return;

            string selected;
            if (lboxSymbols.SelectedItem is ListBoxItem lbi)
            {
                string content = lbi.Content?.ToString() ?? "";
                if (content.StartsWith("--") || content == "") return;
                selected = content;
            }
            else
            {
                selected = lboxSymbols.SelectedItem.ToString();
            }

            try
            {
                string escaped = Regex.Escape(selected);
                var matches = Regex.Matches(fileText,
                    "\\n.*" + escaped + ".*\\n");

                for (int j = 0; j < matches.Count; j++)
                {
                    if (matches[j].Success)
                    {
                        lboxReferences.Items.Add(
                            (j + 1) + "\t" + matches[j].Value.Trim());
                    }
                }
            }
            catch (Exception) { }
        }

        public void SearchText(string text)
        {
            if (string.IsNullOrEmpty(text)) return;

            if (searchIndex == -1) searchIndex = 0;

            if (editorHost.SelectedText == text)
            {
                searchIndex = editorHost.Text.IndexOf(text, Math.Max(searchIndex, 0));
            }
            else
            {
                searchIndex = editorHost.Text.IndexOf(text);
            }

            if (searchIndex >= 0)
            {
                editorHost.SelectText(text);
                searchIndex += text.Length;
            }
        }

        private void lboxSymbols_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lboxSymbols.SelectedItem == null) return;
            string selected;
            if (lboxSymbols.SelectedItem is ListBoxItem lbi)
            {
                string content = lbi.Content?.ToString() ?? "";
                if (content.StartsWith("--") || content == "") return;
                selected = content;
            }
            else
            {
                selected = lboxSymbols.SelectedItem.ToString();
            }
            SearchText(selected);
        }

        private void lboxReferences_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lboxReferences.SelectedItem == null) return;
            string item = lboxReferences.SelectedItem.ToString();
            int tabIdx = item.IndexOf('\t');
            if (tabIdx >= 0 && tabIdx + 1 < item.Length)
            {
                string text = item.Substring(tabIdx + 1).Trim();
                editorHost.SelectText(text);
            }
        }

        private string EscapeForRegex(string input)
        {
            return input
                .Replace(@"\", @"\\")
                .Replace("$", "\\$")
                .Replace("[", "\\[")
                .Replace("]", "\\]")
                .Replace(".", "\\.")
                .Replace("*", "\\*")
                .Replace("!", "\\!")
                .Replace("+", "\\+")
                .Replace("|", "\\|")
                .Replace("?", "\\?")
                .Replace("^", "\\^")
                .Replace("}", "\\}")
                .Replace("{", "\\{");
        }

        private void Menu_TraceAll_Click(object sender, RoutedEventArgs e)
        {
            string selected = editorHost.SelectedText;
            if (string.IsNullOrEmpty(selected)) return;

            lboxReferences.Items.Clear();
            try
            {
                string escaped = EscapeForRegex(selected);
                var matches = Regex.Matches(fileText,
                    "\\n.*" + escaped + ".*\\n");

                for (int j = 0; j < matches.Count; j++)
                {
                    if (matches[j].Success)
                    {
                        lboxReferences.Items.Add(
                            (j + 1) + "\t" + matches[j].Value.Trim());
                    }
                }
            }
            catch (Exception) { }
        }

        private void Menu_LocateFunc_Click(object sender, RoutedEventArgs e)
        {
            string selected = editorHost.SelectedText;
            if (!string.IsNullOrEmpty(selected))
            {
                string keyword = selected.Contains("(")
                    ? "function " + selected
                    : "function " + selected + "(";
                RequestOpenSearch?.Invoke(this, keyword);
            }
        }

        private void Menu_GlobalSearch_Click(object sender, RoutedEventArgs e)
        {
            string selected = editorHost.SelectedText;
            if (!string.IsNullOrEmpty(selected))
            {
                RequestOpenSearch?.Invoke(this, selected);
            }
        }

        private void Menu_DebugSelected_Click(object sender, RoutedEventArgs e)
        {
            string selected = editorHost.SelectedText;
            if (!string.IsNullOrEmpty(selected))
            {
                RequestOpenRunPhp?.Invoke(this, selected);
            }
        }

        private void Menu_Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFile();
        }

        private void SaveFile()
        {
            if (string.IsNullOrEmpty(FilePath)) return;
            try
            {
                string encodingName = F_Main.var_fileencoding;
                if (encodingName == "UTF-8")
                {
                    File.WriteAllText(FilePath, editorHost.Text,
                        new UTF8Encoding(false));
                }
                else
                {
                    if (string.IsNullOrEmpty(encodingName))
                        encodingName = "UTF-8";
                    File.WriteAllText(FilePath, editorHost.Text,
                        Encoding.GetEncoding(encodingName));
                }
            }
            catch (Exception)
            {
                MessageBox.Show("保存失败", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void Menu_CopyPath_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(FilePath))
            {
                Clipboard.SetDataObject(FilePath.Replace("\\", "/"));
            }
        }
    }
}
