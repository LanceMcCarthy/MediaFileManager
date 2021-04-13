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
    public partial class HelpWindow : RadWindow
    {
        public HelpWindow()
        {
            InitializeComponent();
            Loaded += HelpWindow_Loaded;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
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
    }
}
