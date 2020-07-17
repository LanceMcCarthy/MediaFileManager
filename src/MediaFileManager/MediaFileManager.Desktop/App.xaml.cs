using System.Globalization;
using System.Threading;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.SplashScreen;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
using System.Diagnostics.CodeAnalysis;
using Analytics =  Microsoft.AppCenter.Analytics.Analytics;
using Settings = MediaFileManager.Desktop.Properties.Settings;

[assembly: SuppressMessage("Design", "CA1051:Do not declare visible instance fields", Justification = "<Pending>", Scope = "member")]
[assembly: SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "<Pending>", Scope = "member")]
[assembly: SuppressMessage("Design", "CA1063:Implement IDisposable Correctly", Justification = "<Pending>", Scope = "type")]
namespace MediaFileManager.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            StyleManager.ApplicationTheme = new FluentTheme();
            InitializeComponent();
        }

        protected override async void OnStartup(StartupEventArgs e)
        {
            var dataContext = (SplashScreenDataContext)RadSplashScreenManager.SplashScreenDataContext;
            dataContext.ImagePath = "/MediaFileManager.Desktop;component/Images/Splashscreen Logo 100.png";
            dataContext.Content = "Loading Media File Manager...";
            
            dataContext.IsIndeterminate = false;
            dataContext.MinValue = 0;
            dataContext.MaxValue = 100;

            RadSplashScreenManager.Show();

            // AppCenter
            AppCenter.Start(Settings.Default.AppCenterAnalyticsKey, typeof(Analytics), typeof(Crashes));

            // Just use general region, not actual location.
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);

            // Option 1 (good for beta releases) - Hard coding the choice for beta release of the app
            // Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);

            // Option 2 (good for production releases) - Wait until first time it crashes, then ask user what they want to do.
            var hasCrashed = await Crashes.HasCrashedInLastSessionAsync().ConfigureAwait(true);

            if (hasCrashed)
            {
                var result = MessageBox.Show("The app crashed the last time you ran it. Would you like the app to automatically send errors to us?",
                    "Oh no! I crashed 😪", MessageBoxButton.YesNoCancel);

                switch (result)
                {
                    case MessageBoxResult.Yes:
                        Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
                        break;
                    case MessageBoxResult.No:
                        Crashes.NotifyUserConfirmation(UserConfirmation.Send);
                        break;
                    default:
                        Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
                        break;
                }
            }

            // Temporary faking the loading indicator
            for (var i = 0; i < 100; i++)
            {
                dataContext.ProgressValue = i;
                Thread.Sleep(30);

                dataContext.Footer = i < 50 ? "Hi! Loading resources, this will be quick." : "Just a little bit more...";
            }

            RadSplashScreenManager.Close();

            base.OnStartup(e);
        }
    }
}
