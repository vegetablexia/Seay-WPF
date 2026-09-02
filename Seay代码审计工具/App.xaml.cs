using System;
using System.Runtime.InteropServices;
using System.Windows;

namespace Seay代码审计工具
{
    public partial class App : Application
    {
        [DllImport("user32.dll")]
        private static extern int SetProcessDpiAwarenessContext(int value);

        protected override void OnStartup(StartupEventArgs e)
        {
            try
            {
                SetProcessDpiAwarenessContext(-4);
            }
            catch { }

            // WinForms 控件视觉样式：DarkMode_Explorer 滚动条主题依赖 comctl32 v6
            System.Windows.Forms.Application.EnableVisualStyles();

            base.OnStartup(e);
        }
    }
}
