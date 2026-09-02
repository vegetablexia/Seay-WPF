using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace Seay代码审计工具.Views
{
    public partial class WelcomeContent : UserControl
    {
        public event EventHandler<string> ProjectPathDropped;

        public WelcomeContent()
        {
            InitializeComponent();
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = ContainsDroppedFolder(e) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private void Grid_Drop(object sender, DragEventArgs e)
        {
            string folder = GetDroppedFolder(e);
            if (folder != null)
            {
                ProjectPathDropped?.Invoke(this, folder);
            }
            e.Handled = true;
        }

        private static bool ContainsDroppedFolder(DragEventArgs e)
        {
            return GetDroppedFolder(e) != null;
        }

        private static string GetDroppedFolder(DragEventArgs e)
        {
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return null;

            if (e.Data.GetData(DataFormats.FileDrop) is string[] paths)
            {
                foreach (string path in paths)
                {
                    if (Directory.Exists(path))
                        return path;
                }
            }
            return null;
        }
    }
}
