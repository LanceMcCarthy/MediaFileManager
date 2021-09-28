using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MediaFileManager.Desktop.UserControls;
using Telerik.Windows.Controls.SplashScreen;

namespace MediaFileManager.Desktop
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
        protected override async void OnStartup(StartupEventArgs e)
        {
            var dataContext = (SplashScreenDataContext)Telerik.Windows.Controls.RadSplashScreenManager.SplashScreenDataContext;
            dataContext.ImagePath = "/MediaFileManager.Desktop;component/Images/SplashScreenLogo.png";
            dataContext.ImageWidth = 600;
            dataContext.ImageHeight = 400;
            dataContext.ImageStretch = Stretch.UniformToFill;

            dataContext.Content = "Loading Media File Manager...";
            
            dataContext.IsIndeterminate = false;
            dataContext.MinValue = 0;
            dataContext.MaxValue = 100;

            Telerik.Windows.Controls.RadSplashScreenManager.Show<LoadingControl>();

            // AppCenter
            AppCenter.Start(MediaFileManager.Desktop.Properties.Settings.Default.AppCenterAnalyticsKey, 
                typeof(Analytics), 
                typeof(Crashes));

            // Just use general region, not actual location.
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);

            // Option 1 (good for beta releases) - Hard coding the choice for beta release of the app
            Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);

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

            // Temporarily pausing 5 seconds for the loading indicator, will be used for real data in the future
            for (var i = 0; i < 100; i++)
            {
                dataContext.ProgressValue = i;

                await Task.Delay(20);

                if (i < 40)
                {
                    dataContext.Footer = "Hi! Loading resources, this will be quick.";
                }
                else if (i < 80)
                {
                    dataContext.Footer = "Just a little bit more...";
                }
                else
                {
                    dataContext.Footer = "Finishing up...";
                }
            }

            Telerik.Windows.Controls.RadSplashScreenManager.Close();

            base.OnStartup(e);
        }
    }
}
