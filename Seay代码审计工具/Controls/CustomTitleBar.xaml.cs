using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Seay代码审计工具.Controls
{
    public partial class CustomTitleBar : UserControl
    {
        public CustomTitleBar()
        {
            InitializeComponent();
            Loaded += (s, e) =>
            {
                var window = Window.GetWindow(this);
                if (window != null)
                {
                    window.StateChanged += (s2, e2) => UpdateMaxIcon(window);
                    UpdateMaxIcon(window);
                }
            };
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (e.ClickCount == 2)
            {
                ToggleMaximize(window);
            }
            else
            {
                if (window.WindowState == WindowState.Maximized)
                {
                    var point = PointToScreen(e.GetPosition(this));
                    window.WindowState = WindowState.Normal;
                    window.Left = point.X - window.Width / 2;
                    window.Top = point.Y - 16;
                }
                window.DragMove();
            }
        }

        private void btnMin_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.WindowState = WindowState.Minimized;
        }

        private void btnMax_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            ToggleMaximize(window);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            window.Close();
        }

        private void ToggleMaximize(Window window)
        {
            window.WindowState = window.WindowState == WindowState.Maximized
                ? WindowState.Normal
                : WindowState.Maximized;
            UpdateMaxIcon(window);
        }

        private void UpdateMaxIcon(Window window)
        {
            bool maximized = window.WindowState == WindowState.Maximized;
            maxIcon.Text = maximized ? "\uE923" : "\uE922";
            btnMax.ToolTip = maximized ? "还原" : "最大化";
        }

        public void UpdateTitle(string title)
        {
            txtTitle.Text = title;
        }
    }
}
