using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MediaFileManager.Desktop.Helpers;
using MediaFileManager.Desktop.Views;

namespace MediaFileManager.Desktop.Windows
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            if (Properties.Settings.Default.PreferredTheme != "Fluent")
            {
                PersonalizationHelpers.UpdateTheme(Properties.Settings.Default.PreferredTheme);
            }

            InitializeComponent();

            ThemeComboBox.ItemsSource = PersonalizationHelpers.ThemeNames;
            ThemeComboBox.SelectedItem = Properties.Settings.Default.PreferredTheme;

            FileTypeComboBox.ItemsSource = new List<string> {"Videos", "Audiobooks", "Music"}; 
            FileTypeComboBox.SelectedIndex = Properties.Settings.Default.SelectedViewIndex;
        }

        private void FileTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                var selectedItem = FileTypeComboBox.SelectedItem.ToString();

                if (selectedItem == "Videos")
                {
                    SelectedViewBorder.Child = new VideoFilesView();
                }
                else if (selectedItem == "Audiobooks")
                {
                    SelectedViewBorder.Child = new AudiobookFilesView();
                }
                else if (selectedItem == "Music")
                {
                    SelectedViewBorder.Child = new MusicFilesView();
                }

                Title = $"Media File Manager - {selectedItem}";

                Properties.Settings.Default.SelectedViewIndex = FileTypeComboBox.SelectedIndex;

                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("View Selected", new Dictionary<string, string>
                {
                    { "FileType", selectedItem }
                });
            }
        }

        private void ThemeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                var selectedThemeName = ThemeComboBox.SelectedItem.ToString();

                PersonalizationHelpers.UpdateTheme(selectedThemeName);

                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("ThemeChanged", new Dictionary<string, string>
                {
                    { "Theme Name", selectedThemeName }
                });

                Properties.Settings.Default.PreferredTheme = selectedThemeName;
                Properties.Settings.Default.Save();
            } 
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            var about = new AboutWindow();
            about.Show();
        }
    }
}
