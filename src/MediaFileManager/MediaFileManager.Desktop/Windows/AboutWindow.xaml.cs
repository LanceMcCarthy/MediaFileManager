using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Windows;
using MarkdownSharp;
using Microsoft.AppCenter.Crashes;
using Telerik.Windows.Controls;
using Telerik.Windows.Documents.FormatProviders.Html;

namespace MediaFileManager.Desktop.Windows
{
    public partial class AboutWindow : RadWindow
    {
        public AboutWindow()
        {
            InitializeComponent();
            Loaded += AboutWindow_Loaded;
        }

        private async void AboutWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            try
            {
                this.Header = $"Media File Manager (v.{typeof(AboutWindow).Assembly.GetName().Version}) - Help & About";

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
                    { "AboutWindow_Loaded Exception", ex.Message }
                });
            }
            finally
            {
                BusyIndicator.IsBusy = false;
                BusyIndicator.Visibility = Visibility.Collapsed;
            }
        }
    }
}
