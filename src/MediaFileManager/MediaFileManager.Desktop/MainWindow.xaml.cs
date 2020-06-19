using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MediaFileManager.Desktop.Views;
using Telerik.Windows.Controls;

// ReSharper disable InconsistentNaming
namespace MediaFileManager.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            var rw = new RadWindow();

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

                this.Title = $"Media File Manager - {selectedItem}";

                Properties.Settings.Default.SelectedViewIndex = FileTypeComboBox.SelectedIndex;

                Microsoft.AppCenter.Analytics.Analytics.TrackEvent("View Selected", new Dictionary<string, string>
                {
                    { "FileType", selectedItem }
                });
            }
        }
    }
}
