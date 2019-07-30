using System.Globalization;
using System.Windows;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Crashes;
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

        protected override async void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // AppCenter //AppCenterAnalyticsKey
            Microsoft.AppCenter.AppCenter.Start(
                MediaFileManager.Desktop.Properties.Settings.Default.AppCenterAnalyticsKey,
                typeof(Microsoft.AppCenter.Analytics.Analytics), 
                typeof(Microsoft.AppCenter.Crashes.Crashes));

            // Just use general region, not actual location.
            AppCenter.SetCountryCode(RegionInfo.CurrentRegion.TwoLetterISORegionName);

            // NOTE: The open source, beta version of this app will send crash data. Change this if you plan on using and redistributing.
            Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);

            // For example, the firts time the app launches after crashing, you can present with a prompt
            //var hasCrashed = await Crashes.HasCrashedInLastSessionAsync();

            //if (hasCrashed)
            //{
            //    var result = MessageBox.Show("The app crashed the last time you ran it. Would you like the app to automatically send errors to us?", 
            //        "Oh no! I crashed :(", 
            //        MessageBoxButton.YesNoCancel);

            //    if (result == MessageBoxResult.Yes)
            //    {
            //        Crashes.NotifyUserConfirmation(UserConfirmation.AlwaysSend);
            //    }
            //    else if (result == MessageBoxResult.No)
            //    {
            //        Crashes.NotifyUserConfirmation(UserConfirmation.Send);
            //    }
            //    else
            //    {
            //        Crashes.NotifyUserConfirmation(UserConfirmation.DontSend);
            //    }
            //}
        }
    }
}
