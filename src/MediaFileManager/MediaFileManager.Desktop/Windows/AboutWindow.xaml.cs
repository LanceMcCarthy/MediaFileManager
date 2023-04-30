using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using Telerik.Windows.Controls;
using Windows.ApplicationModel;

namespace MediaFileManager.Desktop.Windows;

public partial class AboutWindow : RadWindow
{
    public AboutWindow()
    {
        InitializeComponent();
        Loaded += AboutWindow_Loaded;
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
    private void AboutWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
    {
        CopyrightTextBlock.Text =  $"Copyright 2017-{DateTime.Now.Year}, Lancelot Software";

        var version = "";

        try
        {
            version = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";

            
        }
        catch (Exception ex)
        {
            Crashes.TrackError(ex, new Dictionary<string, string>
            {
                { "AboutWindow_Loaded Exception", ex.Message }
            });

            version = $"Error: {ex.Message.Substring(0, 20)}";
        }
        finally
        {
            VersionTextBlock.Text = version;
        }
    }
}
