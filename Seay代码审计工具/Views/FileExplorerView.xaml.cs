using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Seay代码审计工具.Views
{
    public partial class FileExplorerView : UserControl
    {
        public event EventHandler<string> FileOpened;

        public FileExplorerView()
        {
            InitializeComponent();
        }

        public void LoadProject(string rootPath)
        {
            tvFiles.Items.Clear();
            try
            {
                var rootDir = new DirectoryInfo(rootPath);
                var rootItem = CreateTreeViewItem(rootDir.Name, rootDir.FullName, true);
                LoadDirectory(rootDir, rootItem);
                tvFiles.Items.Add(rootItem);
                rootItem.IsExpanded = true;
            }
            catch (Exception)
            {
            }
        }

        private void LoadDirectory(DirectoryInfo dir, TreeViewItem parentItem)
        {
            try
            {
                foreach (var subDir in dir.GetDirectories())
                {
                    var item = CreateTreeViewItem(subDir.Name, subDir.FullName, true);
                    parentItem.Items.Add(item);
                    item.Expanded += (s, e) => LoadDirectoryContents(subDir, item);
                }

                foreach (var file in dir.GetFiles())
                {
                    var item = CreateTreeViewItem(file.Name, file.FullName, false);
                    parentItem.Items.Add(item);
                }
            }
            catch (Exception)
            {
            }
        }

        private void LoadDirectoryContents(DirectoryInfo dir, TreeViewItem parentItem)
        {
            if (parentItem.Items.Count > 0 && parentItem.Items[0] is TreeViewItem placeholder && placeholder.Tag == null)
            {
                parentItem.Items.Clear();
                LoadDirectory(dir, parentItem);
            }
        }

        private TreeViewItem CreateTreeViewItem(string name, string fullPath, bool isDirectory)
        {
            var item = new TreeViewItem
            {
                Header = name,
                Tag = isDirectory ? null : fullPath
            };

            if (isDirectory)
            {
                item.Items.Add(new TreeViewItem());
            }

            return item;
        }

        private void tvFiles_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (tvFiles.SelectedItem is TreeViewItem item && item.Tag is string filePath)
            {
                if (File.Exists(filePath))
                {
                    FileOpened?.Invoke(this, filePath);
                }
            }
        }
    }
}
