using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaFileManager.Desktop.Models;
using TagLib;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop.Views
{
    public partial class AudioFilesView : UserControl
    {
        private readonly RadOpenFolderDialog openFolderDialog;
        private readonly ObservableCollection<OutputMessage> OutputMessages = new ObservableCollection<OutputMessage>();

        private readonly ObservableCollection<string> Albums = new ObservableCollection<string>();
        private readonly ObservableCollection<AudiobookFile> Audiobooks = new ObservableCollection<AudiobookFile>();

        public AudioFilesView()
        {
            InitializeComponent();

            openFolderDialog = new RadOpenFolderDialog { Owner = this, ExpandToCurrentDirectory = false };

            AlbumsListBox.ItemsSource = Albums;
            GridView.ItemsSource = Audiobooks;
            OutputListBox.ItemsSource = OutputMessages;

            WriteOutput($"Ready, open a folder to begin.", OutputMessageLevel.Success);
        }

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
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
            UpdateSelectedMP3Files();
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
            SetTitleTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
        }

        private void ArtistTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            SetTitleTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
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

            await Task.Run(() =>
            {
                try
                {
                    WriteOutput($"Updating MP3 tags...", OutputMessageLevel.Warning);

                    foreach (AudiobookFile item in GridView.SelectedItems)
                    {
                        var tFile = TagLib.File.Create(item.FilePath);

                        if (tFile != null)
                        {
                            tFile.Tag.Title = item.Title;
                            tFile.Tag.Album = item.Album;
                            tFile.Tag.AlbumArtists = new [] { item.Artist };
                            tFile.Tag.Composers = new[] { item.Artist };
                            

                            tFile.Save();
                        }

                        // Need to dispatch back to UI thread, variables to avoid access to modified closure problem
                        var currentIndex = GridView.SelectedItems.IndexOf(item);
                        var progressComplete = currentIndex / GridView.SelectedItems.Count * 100;
                        var progressText = $"Updating tags {progressComplete}% complete...";

                        this.Dispatcher.Invoke(() =>
                        {
                            busyIndicator.ProgressValue = progressComplete;
                            busyIndicator.BusyContent = $"Completed {progressText}...";
                        });
                    }

                    WriteOutput($"Renaming operation complete!", OutputMessageLevel.Success);
                }
                catch (Exception ex)
                {
                    WriteOutput(ex.Message, OutputMessageLevel.Error);
                }
            });

            UpdateSelectedMP3Files();

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;
        }

        private void UpdateSelectedMP3Files()
        {
            Audiobooks.Clear();

            foreach (string season in AlbumsListBox.SelectedItems)
            {
                var episodesResult = Directory.EnumerateFiles(season);

                foreach (var filePath in episodesResult)
                {
                    var song = new AudiobookFile
                    {
                        FilePath = filePath
                    };

                    var fileName = System.IO.Path.GetFileName(filePath);

                    if (!string.IsNullOrEmpty(fileName))
                    {
                        song.FileName = fileName;
                    }

                    var tFile = TagLib.File.Create(filePath);

                    if (tFile != null)
                    {
                        song.Title = tFile.Tag.Title;
                        song.Album = tFile.Tag.Album;
                        song.Artist = tFile.Tag.FirstAlbumArtist;
                    }
                    else
                    {
                        song.Title = "No Tag";
                        song.Album = "No Tag";
                        song.Artist = "No Tag";
                    }

                    Audiobooks.Add(song);
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
                if (removeLastItem && OutputMessages.Count > 0)
                {
                    OutputMessages.Remove(OutputMessages.LastOrDefault());
                }

                var message = new OutputMessage
                {
                    Message = text,
                    MessageColor = messageColor
                };

                OutputMessages.Add(message);
                OutputListBox.ScrollIntoView(message);
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    if (removeLastItem && OutputMessages.Count > 0)
                    {
                        OutputMessages.Remove(OutputMessages.LastOrDefault());
                    }

                    var message = new OutputMessage
                    {
                        Message = text,
                        MessageColor = messageColor
                    };

                    OutputMessages.Add(message);
                    OutputListBox.ScrollIntoView(message);
                });
            }
        }
    }
}
