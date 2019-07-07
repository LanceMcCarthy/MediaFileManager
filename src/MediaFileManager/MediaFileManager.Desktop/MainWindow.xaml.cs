using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using MediaFileManager.Desktop.Views;

// ReSharper disable InconsistentNaming
namespace MediaFileManager.Desktop
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            FileTypeComboBox.ItemsSource = new List<string> {"Video", "Audio"};
        }

        private void FileTypeComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                if(FileTypeComboBox.SelectedItem.ToString() == "Video")
                {
                    SelectedViewBorder.Child = new VideoFilesView();
                }
                else
                {
                    SelectedViewBorder.Child = new AudioFilesView();
                }
            }
        }
    }
}
