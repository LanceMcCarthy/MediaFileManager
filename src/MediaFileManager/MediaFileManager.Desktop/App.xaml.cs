using System.Windows;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            InitializeComponent();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // AppCenter
            Microsoft.AppCenter.AppCenter.Start(
                "d675d968-d698-4a38-832a-8b9db5d3764d",
                typeof(Microsoft.AppCenter.Analytics.Analytics), 
                typeof(Microsoft.AppCenter.Crashes.Crashes));
        }
    }
}
