using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Seay正则URL解码匹配测试工具;

namespace Seay代码审计工具.Views
{
    public class RegexMatchItem
    {
        public string Index { get; set; }
        public string Value { get; set; }
    }

    public partial class ToolsView : UserControl
    {
        private string noteFilePath;

        public ToolsView()
        {
            InitializeComponent();
            noteFilePath = Path.Combine(
                AppDomain.CurrentDomain.BaseDirectory, "config", "tempnote.txt");
        }

        public void SetPhpCode(string code)
        {
            txtPhpCode.Text = code;
            tabTools.SelectedIndex = 0;
        }

        private void btnRunPhp_Click(object sender, RoutedEventArgs e)
        {
            string code = txtPhpCode.Text;
            if (string.IsNullOrEmpty(code)) return;

            string binDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "bin");
            string tempFile = Path.Combine(binDir, "php.php");

            try
            {
                if (!Directory.Exists(binDir))
                    Directory.CreateDirectory(binDir);

                File.WriteAllText(tempFile, code, Encoding.Default);

                var psi = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    Arguments = "/c cd /d \"" + binDir + "\" && php.exe php.php",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    StandardOutputEncoding = Encoding.Default,
                    StandardErrorEncoding = Encoding.Default
                };

                var process = Process.Start(psi);
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                process.WaitForExit();

                txtPhpResult.Text = string.IsNullOrEmpty(error) ? output : output + "\n[ERROR]\n" + error;
            }
            catch (Exception ex)
            {
                txtPhpResult.Text = "执行失败: " + ex.Message;
            }
        }

        private void btnClearPhp_Click(object sender, RoutedEventArgs e)
        {
            txtPhpCode.Clear();
            txtPhpResult.Clear();
        }

        private void PhpResultsResizeThumb_DragDelta(object sender,
            System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = phpResultsRow.Height.Value - e.VerticalChange;
            phpResultsRow.Height = new GridLength(Math.Max(60, Math.Min(500, newHeight)));
        }

        private void txtRegexPattern_TextChanged(object sender, TextChangedEventArgs e)
        {
            RunRegexMatch();
        }

        private void txtRegexInput_TextChanged(object sender, TextChangedEventArgs e)
        {
            RunRegexMatch();
        }

        private void RegexOption_Changed(object sender, RoutedEventArgs e)
        {
            RunRegexMatch();
        }

        private void RunRegexMatch()
        {
            lvRegexResults.Items.Clear();

            string pattern = txtRegexPattern.Text;
            string input = txtRegexInput.Text;

            if (string.IsNullOrEmpty(pattern) || string.IsNullOrEmpty(input)) return;

            try
            {
                var options = RegexOptions.None;
                if (chkRegexIgnoreCase.IsChecked == true) options |= RegexOptions.IgnoreCase;
                if (chkRegexMultiline.IsChecked == true) options |= RegexOptions.Multiline;

                var matches = Regex.Matches(input, pattern, options);

                for (int i = 0; i < matches.Count; i++)
                {
                    if (matches[i].Success)
                    {
                        lvRegexResults.Items.Add(new RegexMatchItem
                        {
                            Index = (i + 1).ToString(),
                            Value = matches[i].Value
                        });
                    }
                }
            }
            catch (Exception) { }
        }

        private void cmbEncodeType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        private void btnEncode_Click(object sender, RoutedEventArgs e)
        {
            string input = txtEncodeInput.Text;
            if (string.IsNullOrEmpty(input)) return;

            string type = GetEncodeType();
            try
            {
                switch (type)
                {
                    case "URL":
                        txtEncodeOutput.Text = UrlInfo.func_UrlEncode(input);
                        break;
                    case "Base64":
                        txtEncodeOutput.Text = Base64Info.func_base64encode(input);
                        break;
                    case "Hex":
                        txtEncodeOutput.Text = HexInfo.func_HexEncode(input);
                        break;
                    case "MD5":
                        txtEncodeOutput.Text = md5encode.func_Md5EncryptCode(input);
                        break;
                    case "ASCII":
                        txtEncodeOutput.Text = ascii_info.func_asciiEncode(input, "");
                        break;
                    case "Unicode":
                        txtEncodeOutput.Text = UnicodeInfo.func_UnicodeEncode(input);
                        break;
                }
            }
            catch (Exception ex)
            {
                txtEncodeOutput.Text = "编码失败: " + ex.Message;
            }
        }

        private void btnDecode_Click(object sender, RoutedEventArgs e)
        {
            string input = txtEncodeInput.Text;
            if (string.IsNullOrEmpty(input)) return;

            string type = GetEncodeType();
            if (type == "MD5")
            {
                txtEncodeOutput.Text = "MD5 不可逆";
                return;
            }

            try
            {
                switch (type)
                {
                    case "URL":
                        txtEncodeOutput.Text = UrlInfo.func_UrlDencode(input);
                        break;
                    case "Base64":
                        txtEncodeOutput.Text = Base64Info.func_base64decode(input);
                        break;
                    case "Hex":
                        txtEncodeOutput.Text = HexInfo.func_HexDecode(input);
                        break;
                    case "ASCII":
                        txtEncodeOutput.Text = ascii_info.func_asciiDecode(input);
                        break;
                    case "Unicode":
                        txtEncodeOutput.Text = UnicodeInfo.func_UnicodeDecode(input);
                        break;
                }
            }
            catch (Exception ex)
            {
                txtEncodeOutput.Text = "解码失败: " + ex.Message;
            }
        }

        private string GetEncodeType()
        {
            if (cmbEncodeType.SelectedItem is ComboBoxItem item)
                return item.Content.ToString();
            return "URL";
        }

        private void btnSaveNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string dir = Path.GetDirectoryName(noteFilePath);
                if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                File.WriteAllText(noteFilePath, txtTempNote.Text, Encoding.UTF8);
                MessageBox.Show("保存成功", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("保存失败", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void btnLoadNote_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(noteFilePath))
                {
                    txtTempNote.Text = File.ReadAllText(noteFilePath, Encoding.UTF8);
                }
            }
            catch (Exception) { }
        }
    }
}
