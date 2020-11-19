using System;
using System.Net.Http;
using System.Windows;
using MarkdownSharp;
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

            var ver = typeof(AboutWindow).Assembly.GetName().Version?.ToString();

            this.Header = $"Media File Manager (v.{ver}) - Help & About";
        }

        private async void AboutWindow_Loaded(object sender, System.Windows.RoutedEventArgs e)
        {
            using var client = new HttpClient();

            var markdownText = await client.GetStringAsync(new Uri("https://raw.githubusercontent.com/LanceMcCarthy/MediaFileManager/.github/other/help.md")).ConfigureAwait(true);

            var markdown = new Markdown();

            var convertedHtml = markdown.Transform(markdownText);

            var provider = new HtmlFormatProvider();

            var document = provider.Import(convertedHtml);

            RichTextBox.Document = document;

            BusyIndicator.IsBusy = false;
            BusyIndicator.Visibility = Visibility.Collapsed;
        }
    }
}
