using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Seay代码审计工具.Views
{
    public class AuditResultItem
    {
        public string Id { get; set; }
        public string BugType { get; set; }
        public string FilePath { get; set; }
        public string FullPath { get; set; }
        public string CodeDetail { get; set; }
        public string SearchKeyword { get; set; }
    }

    public partial class AuditView : UserControl
    {
        private Thread scanThread;
        private string rulePath;
        private string[] ruleArr;
        private List<string> bugResults = new List<string>();
        private List<string> errorInfoList = new List<string>();
        private List<string> urlList = new List<string>();
        private int resultCount = 0;
        // UI 线程在启动线程前捕获，扫描线程只读，避免跨线程访问 WPF 控件
        private bool scanIsAutoAudit;
        private string scanUrl = "";
        private string scanCookie = "";

        public AuditView()
        {
            InitializeComponent();
            rulePath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "config", "rule.bin");
        }

        private void AuditMode_Changed(object sender, RoutedEventArgs e)
        {
            if (spInfoLeakOptions == null) return;
            spInfoLeakOptions.Visibility =
                rbInfoLeak.IsChecked == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (scanThread != null && scanThread.ThreadState == ThreadState.Running)
            {
                MessageBox.Show("正在审计中，请先停止", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            string projectPath = F_Main.var_webpath;
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
            {
                MessageBox.Show("请先选择有效的项目目录", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            lvResults.Items.Clear();
            bugResults.Clear();
            errorInfoList.Clear();
            resultCount = 0;
            UpdateResultCount();
            spResults.Visibility = Visibility.Visible;

            bool isAutoAudit = rbAutoAudit.IsChecked == true;
            scanIsAutoAudit = isAutoAudit;

            if (isAutoAudit)
            {
                if (!File.Exists(rulePath))
                {
                    MessageBox.Show("请先添加扫描规则", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            else
            {
                if (string.IsNullOrEmpty(txtUrl.Text))
                {
                    MessageBox.Show("请输入访问程序URL地址", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                scanUrl = txtUrl.Text;
                scanCookie = txtCookie.Text;
                try
                {
                    using (var wc = new WebClient())
                    using (var stream = wc.OpenRead(txtUrl.Text))
                    { }
                }
                catch (Exception)
                {
                    MessageBox.Show("URL错误，请输入本地程序访问URL", "提示",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }

            ParameterizedThreadStart start = new ParameterizedThreadStart(StartScan);
            scanThread = new Thread(start);
            scanThread.IsBackground = true;
            scanThread.Start(projectPath);
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (scanThread != null && scanThread.ThreadState == ThreadState.Running)
            {
                try { scanThread.Abort(); } catch (Exception) { }
            }
            UpdateStatus("扫描已停止，发现 " + resultCount + " 个可疑漏洞");
        }

        private void StartScan(object pathObj)
        {
            string path = pathObj.ToString();
            toolshelper.fileNum = 0;
            int totalFiles = toolshelper.GetFileNum(path);

            Dispatcher.BeginInvoke(new Action(() =>
            {
                pgbScan.Maximum = totalFiles;
                pgbScan.Value = 0;
            }));

            var stopwatch = new System.Diagnostics.Stopwatch();
            stopwatch.Start();

            bool isAutoAudit = scanIsAutoAudit;

            if (isAutoAudit)
            {
                try
                {
                    ruleArr = File.ReadAllLines(rulePath, Encoding.GetEncoding("GBK"));
                }
                catch (Exception)
                {
                    ruleArr = new string[0];
                }
                ScanBug(path, totalFiles);
            }
            else
            {
                ErrorScan(path, totalFiles);
            }

            stopwatch.Stop();
            string time;
            if (stopwatch.Elapsed.TotalSeconds > 60)
                time = string.Format("{0:F}", stopwatch.Elapsed.TotalMinutes) + "分钟";
            else
                time = string.Format("{0:F}", stopwatch.Elapsed.TotalSeconds) + "秒";

            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateStatus("扫描完成，发现 " + resultCount + " 个可疑漏洞，花费时间 " + time);
            }));
        }

        private void ScanBug(string path, int totalFiles)
        {
            try
            {
                var rootFolder = new DirectoryInfo(path);

                foreach (var file in rootFolder.GetFiles("*.php"))
                {
                    CheckFile(file.FullName, totalFiles);
                }

                foreach (var subDir in rootFolder.GetDirectories())
                {
                    ScanBug(subDir.FullName, totalFiles);
                }
            }
            catch (Exception) { }
        }

        private void CheckFile(string filePath, int totalFiles)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (pgbScan.Value < pgbScan.Maximum)
                    pgbScan.Value += 1;
                UpdateStatus(filePath);
            }));

            try
            {
                string encoding = F_Main.var_fileencoding;
                if (string.IsNullOrEmpty(encoding)) encoding = "UTF-8";
                string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(encoding));

                if (string.IsNullOrEmpty(fileText) || ruleArr == null) return;

                string projectPath = F_Main.var_webpath ?? "";

                foreach (string rule in ruleArr)
                {
                    string[] parts = rule.Split('谶');
                    if (parts.Length < 3 || parts[0] != "1") continue;

                    string pattern = parts[1];
                    string bugType = parts[2];

                    try
                    {
                        var matches = Regex.Matches(fileText,
                            "\n.*" + pattern + ".*\n",
                            RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

                        for (int j = 0; j < matches.Count; j++)
                        {
                            if (!matches[j].Success) continue;

                            string dedup = rule + filePath + matches[j].Value.Trim();
                            if (bugResults.Contains(dedup)) continue;

                            bugResults.Add(dedup);

                            string relPath = filePath.Replace(projectPath, "").Replace("\\", "/");
                            string codeDetail = matches[j].Value.Trim();

                            Dispatcher.BeginInvoke(new Action(() =>
                            {
                                AddResult(bugType, relPath, filePath, codeDetail, pattern);
                            }));
                        }
                    }
                    catch (Exception) { }
                }
            }
            catch (Exception) { }
        }

        private void ErrorScan(string path, int totalFiles)
        {
            try
            {
                var rootFolder = new DirectoryInfo(path);

                foreach (var file in rootFolder.GetFiles("*.php"))
                {
                    LoadPhpError(file.FullName, totalFiles);
                }

                foreach (var subDir in rootFolder.GetDirectories())
                {
                    ErrorScan(subDir.FullName, totalFiles);
                }
            }
            catch (Exception) { }
        }

        private void LoadPhpError(string filePath, int totalFiles)
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (pgbScan.Value < pgbScan.Maximum)
                    pgbScan.Value += 1;
            }));

            GetUrlParams(filePath);

            string url = scanUrl;
            if (!url.EndsWith("/")) url += "/";
            string cookie = scanCookie;
            string projectPath = F_Main.var_webpath ?? "";

            foreach (string urlParam in urlList)
            {
                string errorInfo = "";
                try
                {
                    Dispatcher.BeginInvoke(new Action(() =>
                        UpdateStatus(url + urlParam.Replace("&", "&&"))));

                    using (var wc = new WebClient())
                    {
                        wc.Headers.Add("Cookie", cookie);
                        using (var stream = wc.OpenRead(url + urlParam))
                        using (var sr = new StreamReader(stream))
                        {
                            errorInfo = sr.ReadToEnd();
                        }
                    }
                }
                catch (Exception) { continue; }

                if (errorInfo.Contains("<b>Notice</b>: Use") ||
                    errorInfo.Contains("<b>Warning</b>:") ||
                    errorInfo.Contains("<b>Fatal error</b>:"))
                {
                    if (errorInfoList.Contains(filePath + errorInfo)) continue;
                    errorInfoList.Add(filePath + errorInfo);

                    string relPath = filePath.Replace(projectPath, "").Replace("\\", "/");
                    string detail = errorInfo.Length < 150 ? errorInfo : errorInfo.Substring(0, 150);

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AddResult("存在敏感信息泄露漏洞", url + urlParam, filePath, detail, "");
                    }));
                }
            }
        }

        private void GetUrlParams(string filePath)
        {
            urlList.Clear();
            string urlParams = "";
            string projectPath = F_Main.var_webpath ?? "";

            try
            {
                string encoding = F_Main.var_fileencoding;
                if (string.IsNullOrEmpty(encoding)) encoding = "UTF-8";
                string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(encoding));
                string relPath = filePath.Replace(projectPath, "").Replace("\\", "/");
                urlList.Add(relPath);

                if (!string.IsNullOrEmpty(fileText))
                {
                    var matches = Regex.Matches(fileText,
                        "\\$_(GET|REQUEST)\\[['\"]([a-zA-Z0-9_]{1,30})['\"]\\]",
                        RegexOptions.RightToLeft);

                    for (int j = 0; j < matches.Count; j++)
                    {
                        if (matches[j].Success)
                        {
                            string paramName = matches[j].Groups[2].Value;
                            if (urlParams.Contains(paramName + "[]=Seay")) continue;
                            urlParams += paramName + "[]=Seay&";
                        }
                    }
                    if (urlParams.Length > 0)
                        urlList.Add(relPath + "?" + urlParams);
                }
            }
            catch (Exception) { }
        }

        private void AddResult(string bugType, string filePath, string fullPath, string codeDetail, string keyword)
        {
            resultCount++;
            var item = new AuditResultItem
            {
                Id = resultCount.ToString(),
                BugType = bugType,
                FilePath = filePath,
                FullPath = fullPath,
                CodeDetail = codeDetail,
                SearchKeyword = keyword
            };
            lvResults.Items.Add(item);
            UpdateResultCount();
        }

        private void UpdateStatus(string text)
        {
            txtStatus.Text = text;
        }

        private void UpdateResultCount()
        {
            txtResultCount.Text = "共 " + resultCount + " 条结果";
            btnResultsToggle.Content = "结果 " + resultCount;
        }

        private void btnResultsToggle_Click(object sender, RoutedEventArgs e)
        {
            spResults.Visibility = spResults.Visibility == Visibility.Visible
                ? Visibility.Collapsed : Visibility.Visible;
        }

        private void ResultsResizeThumb_DragDelta(object sender,
            System.Windows.Controls.Primitives.DragDeltaEventArgs e)
        {
            double newHeight = spResults.Height + e.VerticalChange;
            spResults.Height = Math.Min(480, Math.Max(60, newHeight));
        }

        private void lvResults_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvResults.SelectedItem is AuditResultItem item)
            {
                if (File.Exists(item.FullPath))
                {
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.OpenFile(item.FullPath, item.CodeDetail);
                    }
                }
            }
        }

        private void btnReport_Click(object sender, RoutedEventArgs e)
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "HTML文件|*.html",
                FileName = "审计报告.html"
            };

            if (sfd.ShowDialog() != true) return;

            string templatePath = System.IO.Path.Combine(
                System.AppDomain.CurrentDomain.BaseDirectory, "config", "report.html");

            if (!File.Exists(templatePath))
            {
                MessageBox.Show("模板文件不存在: " + templatePath, "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                string reportContent = "";
                foreach (AuditResultItem item in lvResults.Items)
                {
                    string escaped = toolshelper.func_HtmlEntity(item.CodeDetail);
                    reportContent += "<tr><td width=\"5%\">" + item.Id +
                        "</td><td width=\"20%\">" + item.BugType +
                        "</td><td width=\"30%\">" + item.FilePath +
                        "</td><td width=\"45%\">" + escaped +
                        "</td></tr>\r\n";
                }

                string template = File.ReadAllText(templatePath, Encoding.GetEncoding("GBK"));
                string result = template
                    .Replace("$content$", reportContent)
                    .Replace("$count$", resultCount.ToString());

                File.WriteAllText(sfd.FileName, result, Encoding.GetEncoding("GBK"));
                MessageBox.Show("生成成功", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception)
            {
                MessageBox.Show("生成失败", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
