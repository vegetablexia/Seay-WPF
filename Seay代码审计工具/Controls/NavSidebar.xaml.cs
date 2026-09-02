using System;
using System.Windows;
using System.Windows.Controls;

namespace Seay代码审计工具.Controls
{
    public partial class NavSidebar : UserControl
    {
        public event EventHandler<string> NavigationChanged;

        public NavSidebar()
        {
            InitializeComponent();
        }

        private void NavButton_Checked(object sender, RoutedEventArgs e)
        {
            if (sender is RadioButton rb && rb.Tag is string tag)
            {
                NavigationChanged?.Invoke(this, tag);
            }
        }
    }
}
