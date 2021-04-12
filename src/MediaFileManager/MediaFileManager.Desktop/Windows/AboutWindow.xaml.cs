using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Crashes;
using Telerik.Windows.Controls;
using Windows.ApplicationModel;

namespace MediaFileManager.Desktop.Windows
{
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
            try
            {
                var uwpPackageVersion = $"{Package.Current.Id.Version.Major}.{Package.Current.Id.Version.Minor}.{Package.Current.Id.Version.Build}";

                Header = $"Media File Manager (v.{uwpPackageVersion}) - About";
            }
            catch (Exception ex)
            {
                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "AboutWindow_Loaded Exception", ex.Message }
                });
            }
            finally
            {

            }
        }
    }
}
