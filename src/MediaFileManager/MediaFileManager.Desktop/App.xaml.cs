using System.Windows;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            this.InitializeComponent();
        }
    }
}
