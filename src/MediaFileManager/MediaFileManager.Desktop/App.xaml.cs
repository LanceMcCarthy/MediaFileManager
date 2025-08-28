using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Globalization;
using System.Windows;

namespace MediaFileManager.Desktop;

public partial class App : Application
{
    public App()
    {
        InitializeComponent();
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
    protected override async void OnStartup(StartupEventArgs e)
    {
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

        base.OnStartup(e);
    }
}