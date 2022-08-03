using MarkdownSharp;
using Microsoft.AppCenter.Crashes;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Html;
using Windows.ApplicationModel;
using Windows.Management.Deployment;

namespace MediaFileManager.Desktop.Windows;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
public partial class HelpWindow : RadWindow
{
    public HelpWindow()
    {
        InitializeComponent();
        Loaded += HelpWindow_Loaded;
    }

    private async void HelpWindow_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            using var client = new HttpClient();
            var markdownText = await client.GetStringAsync(new Uri("https://raw.githubusercontent.com/LanceMcCarthy/MediaFileManager/main/.github/other/help.md")).ConfigureAwait(true);

            var markdown = new Markdown();
            var convertedHtml = markdown.Transform(markdownText);

            RichTextBox.Document = new HtmlFormatProvider().Import(convertedHtml);
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "HelpWindow_Loaded Exception", ex.Message }
                });
        }
        finally
        {
            BusyIndicator.IsBusy = false;
            BusyIndicator.Visibility = Visibility.Collapsed;
        }
    }

    private void RadButton_Click(object sender, RoutedEventArgs e)
    {
        // 1. check for update

        // todo - Having trouble with GetAwaiter, switched to synchronous GetResults instead
        PackageUpdateAvailabilityResult result = Package.Current.CheckUpdateAvailabilityAsync().GetResults();

        switch (result.Availability)
        {
            case PackageUpdateAvailability.Available:
            case PackageUpdateAvailability.Required:
                var popupResult = MessageBox.Show("an update is available, do you want to apply it now?");
                if (popupResult == MessageBoxResult.Yes)
                    InstallUpdate();
                break;
            case PackageUpdateAvailability.NoUpdates:
                MessageBox.Show("You are running the latest version.");
                break;
            case PackageUpdateAvailability.Unknown:
                MessageBox.Show("It is unknown if there is an update available.");
                break;
            default:
                break;
        }
    }

    private void InstallUpdate()
    {
        var pm = new PackageManager();

        // todo - Having trouble with GetAwaiter, switched to synchronous GetResults instead
        var updateRequestResult = pm.RequestAddPackageByAppInstallerFileAsync(
            new Uri("https://dvlup.blob.core.windows.net/general-app-files/Installers/MediaFileManager/index.html/PackageProject.appinstaller"),
            AddPackageByAppInstallerOptions.ForceTargetAppShutdown,
            pm.GetDefaultPackageVolume()).GetResults();

        if (updateRequestResult.ExtendedErrorCode != null)
        {
            var errorText = updateRequestResult.ErrorText;
        }
    }
}
