using System;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace Seay代码审计工具.Views
{
    public class SearchResultItem
    {
        public string Id { get; set; }
        public string FilePath { get; set; }
        public string FullPath { get; set; }
        public string MatchContent { get; set; }
    }

    public partial class SearchView : UserControl
    {
        private Thread searchThread;
        private string keyword;
        private bool isRegex;
        private bool isCaseInsensitive;
        private int resultCount = 0;

        public string Keyword
        {
            get { return txtKeyword.Text; }
            set { txtKeyword.Text = value; }
        }

        public SearchView()
        {
            InitializeComponent();
        }

        public void StartSearch()
        {
            if (searchThread != null && searchThread.ThreadState == ThreadState.Running)
            {
                MessageBox.Show("正在搜索中，请先停止", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            if (string.IsNullOrEmpty(txtKeyword.Text))
            {
                MessageBox.Show("请输入要查找的内容", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            lvResults.Items.Clear();
            resultCount = 0;
            UpdateResultCount();

            keyword = txtKeyword.Text;
            isRegex = chkRegex.IsChecked == true;
            isCaseInsensitive = chkCaseInsensitive.IsChecked == true;

            if (isCaseInsensitive && !isRegex)
            {
                keyword = keyword.ToLower();
            }

            string projectPath = F_Main.var_webpath;
            if (string.IsNullOrEmpty(projectPath) || !Directory.Exists(projectPath))
            {
                MessageBox.Show("请先选择有效的项目目录", "提示",
                    MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            ParameterizedThreadStart start = new ParameterizedThreadStart(StartSearchThread);
            searchThread = new Thread(start);
            searchThread.Start(projectPath);
        }

        private void StartSearchThread(object pathObj)
        {
            string path = pathObj.ToString();
            Dispatcher.BeginInvoke(new Action(() => UpdateStatus("正在搜索...")));
            ScanDirectory(path);
            Dispatcher.BeginInvoke(new Action(() =>
            {
                UpdateStatus("搜索完成，发现 " + resultCount + " 处");
            }));
        }

        private void ScanDirectory(string path)
        {
            try
            {
                var rootFolder = new DirectoryInfo(path);

                foreach (var file in rootFolder.GetFiles("*.php"))
                {
                    if (isRegex)
                        SearchWithRegex(file.FullName);
                    else
                        SearchWithString(file.FullName);
                }

                foreach (var subDir in rootFolder.GetDirectories())
                {
                    ScanDirectory(subDir.FullName);
                }
            }
            catch (Exception) { }
        }

        private void SearchWithRegex(string filePath)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateStatus(filePath)));

            try
            {
                string encoding = F_Main.var_fileencoding;
                if (string.IsNullOrEmpty(encoding)) encoding = "UTF-8";
                string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(encoding));

                if (string.IsNullOrEmpty(fileText)) return;

                var options = RegexOptions.RightToLeft;
                if (isCaseInsensitive) options |= RegexOptions.IgnoreCase;

                var matches = Regex.Matches(fileText, "\n.*" + keyword + ".*\n", options);

                string projectPath = F_Main.var_webpath ?? "";

                for (int j = 0; j < matches.Count; j++)
                {
                    if (!matches[j].Success) continue;

                    string relPath = filePath.Replace(projectPath, "").Replace("\\", "/");
                    string matchContent = matches[j].Value.Trim();

                    Dispatcher.BeginInvoke(new Action(() =>
                    {
                        AddResult(relPath, filePath, matchContent);
                    }));
                }
            }
            catch (Exception) { }
        }

        private void SearchWithString(string filePath)
        {
            Dispatcher.BeginInvoke(new Action(() => UpdateStatus(filePath)));

            try
            {
                string encoding = F_Main.var_fileencoding;
                if (string.IsNullOrEmpty(encoding)) encoding = "UTF-8";
                string fileText = File.ReadAllText(filePath, Encoding.GetEncoding(encoding));

                if (!fileText.Contains(keyword)) return;

                string[] lines = File.ReadAllLines(filePath, Encoding.GetEncoding(encoding));
                string projectPath = F_Main.var_webpath ?? "";
                string relPath = filePath.Replace(projectPath, "").Replace("\\", "/");

                foreach (string line in lines)
                {
                    bool matched;
                    if (!isCaseInsensitive)
                        matched = line.Contains(keyword);
                    else
                        matched = line.ToLower().Contains(keyword);

                    if (matched)
                    {
                        string matchContent = line.Trim();
                        Dispatcher.BeginInvoke(new Action(() =>
                        {
                            AddResult(relPath, filePath, matchContent);
                        }));
                    }
                }
            }
            catch (Exception) { }
        }

        private void AddResult(string filePath, string fullPath, string matchContent)
        {
            resultCount++;
            var item = new SearchResultItem
            {
                Id = resultCount.ToString(),
                FilePath = filePath,
                FullPath = fullPath,
                MatchContent = matchContent
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
        }

        private void btnSearch_Click(object sender, RoutedEventArgs e)
        {
            StartSearch();
        }

        private void btnStop_Click(object sender, RoutedEventArgs e)
        {
            if (searchThread != null && searchThread.ThreadState == ThreadState.Running)
            {
                try { searchThread.Abort(); } catch (Exception) { }
            }
            UpdateStatus("搜索已停止，发现 " + resultCount + " 处");
        }

        private void txtKeyword_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                StartSearch();
            }
        }

        private void lvResults_DoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (lvResults.SelectedItem is SearchResultItem item)
            {
                if (File.Exists(item.FullPath))
                {
                    var mainWindow = Window.GetWindow(this) as MainWindow;
                    if (mainWindow != null)
                    {
                        mainWindow.OpenFile(item.FullPath, item.MatchContent);
                    }
                }
            }
        }
    }
}
