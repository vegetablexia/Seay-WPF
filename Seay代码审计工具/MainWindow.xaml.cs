using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Seay代码审计工具
{
    public partial class MainWindow : Window
    {
        private TabControl tabControl;
        private Dictionary<string, TabItem> openFiles = new Dictionary<string, TabItem>();
        private Views.WelcomeContent welcomeContent;
        private double sidePanelWidth = 250;

        // 侧栏收起时列宽必须归零：固定宽度的列不会随内容 Collapse 自动收缩
        private void SetSidePanelVisible(bool visible)
        {
            if (visible)
            {
                sidePanelColumn.Width = new GridLength(sidePanelWidth);
                sidePanelBorder.Visibility = Visibility.Visible;
                sideSplitter.Visibility = Visibility.Visible;
            }
            else
            {
                if (sidePanelColumn.Width.Value > 0)
                    sidePanelWidth = sidePanelColumn.Width.Value;
                sidePanelColumn.Width = new GridLength(0);
                sidePanelBorder.Visibility = Visibility.Collapsed;
                sideSplitter.Visibility = Visibility.Collapsed;
            }
        }

        public MainWindow()
        {
            InitializeComponent();
            ShowWelcomePanel();
        }

        [System.Runtime.InteropServices.DllImport("dwmapi.dll")]
        private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int value, int size);

        // Win11: 窗口级深色标志，让 Win32 滚动条（编辑器）渲染为深色
        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            var handle = new System.Windows.Interop.WindowInteropHelper(this).Handle;
            int on = 1;
            if (DwmSetWindowAttribute(handle, 20, ref on, 4) != 0)
                DwmSetWindowAttribute(handle, 19, ref on, 4);
        }

        private void NavSidebar_NavigationChanged(object sender, string panel)
        {
            switch (panel)
            {
                case "Files":
                    ShowFileExplorer();
                    break;
                case "Search":
                    ShowSearchPanel();
                    break;
                case "Tools":
                    ShowToolsPanel();
                    break;
            }
        }

        private void ShowWelcomePanel()
        {
            auditPanel.Visibility = Visibility.Collapsed;
            SetSidePanelVisible(true);
            sidePanelHost.Children.Clear();
            mainContentHost.Children.Clear();

            var welcome = new Views.WelcomeView();
            welcome.OpenProjectRequested += Welcome_OpenProjectRequested;
            welcome.ProjectPathDropped += (s, path) => OpenProjectFolder(path);
            sidePanelHost.Children.Add(welcome);

            welcomeContent = new Views.WelcomeContent();
            welcomeContent.ProjectPathDropped += (s, path) => OpenProjectFolder(path);
            mainContentHost.Children.Add(welcomeContent);
        }

        private void ShowFileExplorer()
        {
            if (string.IsNullOrEmpty(F_Main.var_webpath))
            {
                ShowWelcomePanel();
                return;
            }
            LoadFileTree(F_Main.var_webpath);
        }

        private void ShowSearchPanel()
        {
            auditPanel.Visibility = Visibility.Collapsed;
            SetSidePanelVisible(false);
            sidePanelHost.Children.Clear();
            mainContentHost.Children.Clear();
            var searchView = new Views.SearchView();
            mainContentHost.Children.Add(searchView);
        }

        private void ShowToolsPanel()
        {
            auditPanel.Visibility = Visibility.Collapsed;
            SetSidePanelVisible(false);
            sidePanelHost.Children.Clear();
            mainContentHost.Children.Clear();
            var toolsView = new Views.ToolsView();
            mainContentHost.Children.Add(toolsView);
        }

        private void Welcome_OpenProjectRequested(object sender, EventArgs e)
        {
            var dialog = new System.Windows.Forms.FolderBrowserDialog();
            dialog.Description = "选择项目目录";
            if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                OpenProjectFolder(dialog.SelectedPath);
            }
        }

        private void OpenProjectFolder(string path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path)) return;

            txtProjectPath.Text = path;
            F_Main.var_webpath = path;
            LoadFileTree(path);
            SetStatus("已打开项目: " + path);
        }

        private void LoadFileTree(string rootPath)
        {
            auditPanel.Visibility = Visibility.Visible;
            SetSidePanelVisible(true);
            sidePanelHost.Children.Clear();
            var fileExplorer = new Views.FileExplorerView();
            fileExplorer.LoadProject(rootPath);
            fileExplorer.FileOpened += FileExplorer_FileOpened;
            sidePanelHost.Children.Add(fileExplorer);
            RestoreMainContent();
        }

        // 从搜索/工具箱等面板切回文件浏览时，恢复主内容区（编辑器标签或欢迎页）
        private void RestoreMainContent()
        {
            if (tabControl != null && tabControl.Items.Count > 0)
            {
                if (!mainContentHost.Children.Contains(tabControl))
                {
                    mainContentHost.Children.Clear();
                    mainContentHost.Children.Add(tabControl);
                }
                return;
            }

            if (welcomeContent != null && !mainContentHost.Children.Contains(welcomeContent))
            {
                mainContentHost.Children.Clear();
                mainContentHost.Children.Add(welcomeContent);
            }
        }

        private void FileExplorer_FileOpened(object sender, string filePath)
        {
            OpenFile(filePath);
        }

        public void OpenFile(string filePath, string selectText = null)
        {
            if (openFiles.ContainsKey(filePath))
            {
                tabControl.SelectedItem = openFiles[filePath];
                if (!string.IsNullOrEmpty(selectText))
                    ((Views.CodeEditorView)openFiles[filePath].Content).SelectCode(selectText);
                return;
            }

            EnsureTabControl();

            var editorView = new Views.CodeEditorView();
            editorView.LoadFile(filePath);
            if (!string.IsNullOrEmpty(selectText))
                editorView.SelectCode(selectText);
            editorView.RequestOpenSearch += (s, keyword) => OpenSearchTab(keyword);
            editorView.RequestOpenRunPhp += (s, code) => OpenRunPhpTab(code);

            var tab = new TabItem();
            tab.Content = editorView;

            string fileName = Path.GetFileName(filePath);
            var header = new StackPanel { Orientation = Orientation.Horizontal };
            header.Children.Add(new TextBlock
            {
                Text = fileName,
                VerticalAlignment = VerticalAlignment.Center
            });

            var closeBtn = new Button
            {
                Content = "×",
                Width = 16,
                Height = 16,
                Margin = new Thickness(6, 0, 0, 0),
                Style = (Style)FindResource("TabCloseButtonStyle")
            };
            closeBtn.Click += (s, e) => CloseTab(tab, filePath);
            header.Children.Add(closeBtn);

            tab.Header = header;
            tab.ToolTip = filePath;

            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;
            openFiles[filePath] = tab;

            SetStatus(filePath);
        }

        private void CloseTab(TabItem tab, string filePath)
        {
            tabControl.Items.Remove(tab);
            openFiles.Remove(filePath);

            if (tabControl.Items.Count == 0)
            {
                mainContentHost.Children.Remove(tabControl);
                if (welcomeContent != null)
                    mainContentHost.Children.Add(welcomeContent);
                else
                    ShowWelcomePanel();
            }
        }

        private void EnsureTabControl()
        {
            if (tabControl == null)
            {
                tabControl = new TabControl();
                tabControl.Style = (Style)FindResource("DarkTabControlStyle");
                tabControl.ItemContainerStyle = (Style)FindResource("DarkTabItemStyle");
            }

            if (!mainContentHost.Children.Contains(tabControl))
            {
                mainContentHost.Children.Clear();
                mainContentHost.Children.Add(tabControl);
            }
        }

        public void OpenSearchTab(string keyword)
        {
            EnsureTabControl();
            var searchView = new Views.SearchView();
            if (!string.IsNullOrEmpty(keyword))
                searchView.Keyword = keyword;

            var tab = new TabItem
            {
                Header = "全局搜索",
                Content = searchView
            };
            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;

            if (!string.IsNullOrEmpty(keyword))
                searchView.StartSearch();
        }

        public void OpenRunPhpTab(string code)
        {
            EnsureTabControl();
            var toolsView = new Views.ToolsView();
            toolsView.SetPhpCode(code);

            var tab = new TabItem
            {
                Header = "PHP调试",
                Content = toolsView
            };
            tabControl.Items.Add(tab);
            tabControl.SelectedItem = tab;
        }

        public void SetStatus(string message)
        {
            txtStatus.Text = message;
        }

        public void SetEncoding(string encoding)
        {
            txtEncoding.Text = encoding;
        }
    }
}
