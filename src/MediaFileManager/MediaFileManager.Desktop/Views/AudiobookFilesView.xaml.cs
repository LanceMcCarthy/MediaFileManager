using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaFileManager.Desktop.Models;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop.Views
{
    public partial class AudiobookFilesView : UserControl
    {
        private readonly RadOpenFolderDialog openFolderDialog;

        public readonly ObservableCollection<OutputMessage> StatusMessages = new ObservableCollection<OutputMessage>();
        public readonly ObservableCollection<string> Albums = new ObservableCollection<string>();
        public readonly ObservableCollection<AudiobookFile> Audiobooks = new ObservableCollection<AudiobookFile>();

        public AudiobookFilesView()
        {
            InitializeComponent();

            openFolderDialog = new RadOpenFolderDialog { Owner = this, ExpandToCurrentDirectory = false };

            AlbumsListBox.ItemsSource = Albums;
            GridView.ItemsSource = Audiobooks;
            OutputListBox.ItemsSource = StatusMessages;

            WriteOutput($"Ready, open a folder to begin.", OutputMessageLevel.Success);
        }

        private void SelectAuthorFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteOutput($"Opening folder picker...", OutputMessageLevel.Normal);

                busyIndicator.IsBusy = true;
                busyIndicator.BusyContent = "opening folder...";
                busyIndicator.IsIndeterminate = true;

                if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder))
                {
                    // Need to bump up one level from the last folder location
                    var topDirectoryInfo = Directory.GetParent(Properties.Settings.Default.LastFolder);

                    openFolderDialog.InitialDirectory = topDirectoryInfo.FullName;

                    WriteOutput($"Starting at saved folder.", OutputMessageLevel.Normal);
                }
                else
                {
                    WriteOutput($"No saved folder, starting at root.", OutputMessageLevel.Warning);
                }

                openFolderDialog.ShowDialog();

                if (openFolderDialog.DialogResult != true)
                {
                    WriteOutput($"Canceled folder selection.", OutputMessageLevel.Normal);
                    return;
                }
                else
                {
                    Properties.Settings.Default.LastFolder = openFolderDialog.FileName;
                    Properties.Settings.Default.Save();
                }

                Reset();

                busyIndicator.BusyContent = $"searching for albums...";

                var seasonsResult = Directory.EnumerateDirectories(openFolderDialog.FileName).ToList();

                Albums.Clear();

                foreach (var season in seasonsResult)
                {
                    Albums.Add(season);

                    busyIndicator.BusyContent = $"added {season}";
                }

                if (Albums.Count == 0)
                {
                    WriteOutput("No seasons detected, make sure there are subfolders with season number.", OutputMessageLevel.Warning);
                }
                else if (Albums.Count == 1)
                {
                    WriteOutput($"Opened '{System.IO.Path.GetFileName(openFolderDialog.FileName)}' ({Albums.Count} season).", OutputMessageLevel.Success);
                }
                else
                {
                    WriteOutput($"Opened '{System.IO.Path.GetFileName(openFolderDialog.FileName)}' ({Albums.Count} seasons).", OutputMessageLevel.Success);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }
            finally
            {
                busyIndicator.BusyContent = "";
                busyIndicator.IsBusy = false;
                busyIndicator.IsIndeterminate = false;
            }
        }

        private void AlbumsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshFileList();
        }

        private void GridView_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            if (GridView.SelectedItems.Count <= 0)
                return;

            var firstItem = e.AddedItems.OfType<AudiobookFile>().FirstOrDefault();

            if (firstItem == null)
                return;

            if (!string.IsNullOrEmpty(firstItem.Artist))
            {
                ArtistTextBox.Text = firstItem.Artist;
            }

            if (!string.IsNullOrEmpty(firstItem.Album))
            {
                AlbumNameTextBox.Text = firstItem.Album;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            WriteOutput("Reset complete.", OutputMessageLevel.Success);
        }

        private void AlbumNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
        }

        private void ArtistTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
        }

        private async void SetTagsButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(ArtistTextBox.Text) || string.IsNullOrEmpty(AlbumNameTextBox.Text))
            {
                WriteOutput($"One of the required fields is empty.", OutputMessageLevel.Error);
                return;
            }

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "updating tags...";
            busyIndicator.IsIndeterminate = false;
            busyIndicator.ProgressValue = 0;

            try
            {
                WriteOutput($"Updating MP3 tags...", OutputMessageLevel.Warning);

                foreach (AudiobookFile audiobookFile in GridView.SelectedItems)
                {
                    var tagLibFile = TagLib.File.Create(audiobookFile.FilePath);

                    if (tagLibFile != null)
                    {
                        // Plex uses the audiobook's title for the Album
                        if (SetAlbumNameCheckBox.IsChecked == true)
                        {
                            tagLibFile.Tag.Album = audiobookFile.Album;
                        }

                        // Using the filename for  titles in the 'album' helps keeps files in order
                        if (SetTitleCheckBox.IsChecked == true)
                        {
                            tagLibFile.Tag.Title = audiobookFile.FileName;
                        }

                        // Author and Artist fields (Plex uses Artist and Album)
                        if (SetArtistNameCheckBox.IsChecked == true)
                        {
                            var author = new[] {audiobookFile.Artist};
                            tagLibFile.Tag.Artists = author;
                            tagLibFile.Tag.AlbumArtists = author;
                            tagLibFile.Tag.Performers = author;
                            tagLibFile.Tag.Composers = author;
                        }

                        tagLibFile.Save();
                    }

                    // Need to dispatch back to UI thread, variables to avoid access to modified closure problem
                    var currentIndex = GridView.SelectedItems.IndexOf(audiobookFile);
                    var progressComplete = currentIndex / GridView.SelectedItems.Count * 100;
                    var progressText = $"Updating tags {progressComplete}% complete...";

                    WriteOutput(progressText, OutputMessageLevel.Informational, true);

                    busyIndicator.ProgressValue = progressComplete;
                    busyIndicator.BusyContent = $"Completed {progressText}...";
                }

                WriteOutput("Tags updated!", OutputMessageLevel.Success);
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }

            RefreshFileList();

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;
        }

        private void RefreshFileList()
        {
            Audiobooks.Clear();

            foreach (string album in AlbumsListBox.SelectedItems)
            {
                var filePaths = Directory.EnumerateFiles(album);

                foreach (var filePath in filePaths)
                {
                    var audiobookFile = new AudiobookFile
                    {
                        FilePath = filePath
                    };

                    var fileName = System.IO.Path.GetFileName(filePath);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        audiobookFile.FileName = fileName;
                    }

                    var tagLibFile = TagLib.File.Create(filePath);

                    if (tagLibFile != null)
                    {
                        audiobookFile.Title = tagLibFile.Tag.Title;
                        audiobookFile.Album = tagLibFile.Tag.Album;
                        audiobookFile.Artist = tagLibFile.Tag.Artists.FirstOrDefault();
                        audiobookFile.Performer = tagLibFile.Tag.Performers.FirstOrDefault();
                    }
                    else
                    {
                        audiobookFile.Title = "No Tag";
                        audiobookFile.Album = "No Tag";
                        audiobookFile.Artist = "No Tag";
                        audiobookFile.Performer = "No Tag";
                    }

                    Audiobooks.Add(audiobookFile);
                }
            }

            if (AlbumsListBox.SelectedItems.Count == 0)
            {
                WriteOutput("Selections cleared.", OutputMessageLevel.Warning);
            }
            else if (AlbumsListBox.SelectedItems.Count == 1)
            {
                WriteOutput($"{System.IO.Path.GetFileName(openFolderDialog.FileName)} selected ({Audiobooks.Count} files).", OutputMessageLevel.Informational);
            }
            else
            {
                WriteOutput($"{AlbumsListBox.SelectedItems.Count} albums selected ({Audiobooks.Count} total files).", OutputMessageLevel.Informational);
            }
        }

        private void Reset()
        {
            AlbumNameTextBox.Text = string.Empty;
            ArtistTextBox.Text = string.Empty;

            Albums.Clear();
            Audiobooks.Clear();
        }

        private void WriteOutput(string text, OutputMessageLevel level, bool removeLastItem = false)
        {
            var messageColor = Colors.Gray;

            switch (level)
            {
                case OutputMessageLevel.Normal:
                    messageColor = Colors.Black;
                    break;
                case OutputMessageLevel.Informational:
                    messageColor = Colors.Gray;
                    break;
                case OutputMessageLevel.Success:
                    messageColor = Colors.Green;
                    break;
                case OutputMessageLevel.Warning:
                    messageColor = Colors.Goldenrod;
                    break;
                case OutputMessageLevel.Error:
                    messageColor = Colors.Red;
                    break;
            }

            if (this.Dispatcher.CheckAccess())
            {
                if (removeLastItem && StatusMessages.Count > 0)
                {
                    StatusMessages.Remove(StatusMessages.LastOrDefault());
                }

                var message = new OutputMessage
                {
                    Message = text,
                    MessageColor = messageColor
                };

                StatusMessages.Add(message);
                OutputListBox.ScrollIntoView(message);
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (removeLastItem && StatusMessages.Count > 0)
                    {
                        StatusMessages.Remove(StatusMessages.LastOrDefault());
                    }

                    var message = new OutputMessage
                    {
                        Message = text,
                        MessageColor = messageColor
                    };

                    StatusMessages.Add(message);
                    OutputListBox.ScrollIntoView(message);
                });
            }
        }
    }
}
