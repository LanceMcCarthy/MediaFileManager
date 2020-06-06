using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using MediaFileManager.Desktop.Models;
using Microsoft.AppCenter.Crashes;
using Telerik.Windows.Controls;
using Analytics = Microsoft.AppCenter.Analytics.Analytics;

namespace MediaFileManager.Desktop.Views
{
    public partial class AudiobookFilesView : UserControl, IDisposable
    {
        private readonly BackgroundWorker backgroundWorker;
        private readonly RadOpenFolderDialog openFolderDialog;
        
        public readonly ObservableCollection<OutputMessage> StatusMessages = new ObservableCollection<OutputMessage>();
        public readonly ObservableCollection<string> AudiobookTitles = new ObservableCollection<string>();
        public readonly ObservableCollection<AudiobookFile> AudiobookFiles = new ObservableCollection<AudiobookFile>();

        public AudiobookFilesView()
        {
            InitializeComponent();

            openFolderDialog = new RadOpenFolderDialog { Owner = this, ExpandToCurrentDirectory = false };

            StatusListBox.ItemsSource = StatusMessages;
            AudiobookTitlesListBox.ItemsSource = AudiobookTitles;
            AudiobookFilesGridView.ItemsSource = AudiobookFiles;

            backgroundWorker = new BackgroundWorker();
            backgroundWorker.WorkerReportsProgress = true;
            backgroundWorker.DoWork += BackgroundWorker_DoWork;
            backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
            backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

            WriteOutput($"Ready, open an author folder to begin.", OutputMessageLevel.Success);
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

                var folders = Directory.EnumerateDirectories(openFolderDialog.FileName).ToList();

                AudiobookTitles.Clear();

                foreach (var folder in folders)
                {
                    AudiobookTitles.Add(folder);

                    busyIndicator.BusyContent = $"added {folder}";
                }

                if (AudiobookTitles.Count == 0)
                {
                    WriteOutput("No titles detected.", OutputMessageLevel.Error);

                    MessageBox.Show("The selected Author's folder should have subfolders, each subfolder should be named with the audiobook's title.", "No Titles Available.");
                }
                else if (AudiobookTitles.Count == 1)
                {
                    WriteOutput($"Opened {Path.GetFileName(openFolderDialog.FileName)}' ({AudiobookTitles.Count} title).", OutputMessageLevel.Success);
                }
                else
                {
                    WriteOutput($"Opened {Path.GetFileName(openFolderDialog.FileName)} ({AudiobookTitles.Count} titles).", OutputMessageLevel.Success);
                }

                Analytics.TrackEvent("Audiobook Folder Opened", new Dictionary<string, string>
                {
                    { "Audiobook Titles Loaded", AudiobookTitles.Count.ToString() }
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);

                WriteOutput(ex.Message, OutputMessageLevel.Error);

                Crashes.TrackError(ex, new Dictionary<string, string>
                {
                    { "Folder Open", "Audiobook Author" }
                });
            }
            finally
            {
                busyIndicator.BusyContent = "";
                busyIndicator.IsBusy = false;
                busyIndicator.IsIndeterminate = false;
            }
        }

        private void AudiobookTitlesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            RefreshFileList();
        }

        private void AudiobookFilesGridView_OnSelectionChanged(object sender, SelectionChangeEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            if (AudiobookFilesGridView.SelectedItems.Count <= 0)
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

            WriteOutput("Reset complete! Open a folder to continue.", OutputMessageLevel.Success);

            Analytics.TrackEvent("User Reset", new Dictionary<string, string>
            {
                { "View Type" , "Audiobooks" }
            });
        }

        private void AlbumNameTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
        }

        private void ArtistTextBox_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateTagsButton.IsEnabled = !string.IsNullOrEmpty((sender as RadWatermarkTextBox)?.Text);
        }

        private void SetTagsButton_Click(object sender, RoutedEventArgs e)
        {
            if (SetAlbumNameCheckBox.IsChecked.Value && string.IsNullOrEmpty(AlbumNameTextBox.Text))
            {
                WriteOutput($"Album (book title) is empty.", OutputMessageLevel.Error);
                return;
            }

            if (SetArtistNameCheckBox.IsChecked.Value && string.IsNullOrEmpty(ArtistTextBox.Text))
            {
                WriteOutput($"Artist (author name) is empty.", OutputMessageLevel.Error);
                return;
            }

            if (AudiobookFilesGridView.SelectedItems.Count == 0)
            {
                WriteOutput($"No files have been selected.", OutputMessageLevel.Error);
                return;
            }

            Analytics.TrackEvent("Set Tags started", new Dictionary<string, string>
            {
                { "Set Book Title Enabled", SetAlbumNameCheckBox.IsChecked.Value.ToString() },
                { "Set Author Name Enabled", SetArtistNameCheckBox.IsChecked.Value.ToString() },
                { "Selected Audiobook files", AudiobookFilesGridView.SelectedItems.Count.ToString() },
                { "Authors", AudiobookTitles.Count.ToString() },
            });

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "updating tags...";
            busyIndicator.IsIndeterminate = false;
            busyIndicator.ProgressValue = 0;

            backgroundWorker.RunWorkerAsync(new TagWorkerParameters
            {
                AudiobookFiles = AudiobookFilesGridView.SelectedItems.Cast<AudiobookFile>().ToList(),
                UpdateAlbumName = SetAlbumNameCheckBox.IsChecked,
                UpdateTitle = SetTitleCheckBox.IsChecked,
                UpdateArtistName = SetArtistNameCheckBox.IsChecked
            });
        }

        private void BackgroundWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is TagWorkerParameters tagParams)
            {
                try
                {
                    for (int i = 0; i < tagParams.AudiobookFiles.Count; i++)
                    {
                        var audiobookFile = tagParams.AudiobookFiles[i];

                        using(var tagLibFile = TagLib.File.Create(audiobookFile.FilePath))
                        {
                            if (tagLibFile != null)
                            {
                                // Plex uses the audiobook's title for the Album
                                if (tagParams.UpdateAlbumName == true)
                                {
                                    tagLibFile.Tag.Album = audiobookFile.Album;
                                }

                                // Using the filename for titles in the 'album' helps keeps files in order
                                if (tagParams.UpdateTitle == true)
                                {
                                    tagLibFile.Tag.Title = Path.GetFileNameWithoutExtension(audiobookFile.FilePath);
                                }

                                // Author and Artist fields (Plex uses Artist and Album)
                                if (tagParams.UpdateArtistName == true)
                                {
                                    var author = new[] { audiobookFile.Artist };
                                    tagLibFile.Tag.Artists = author;
                                    tagLibFile.Tag.AlbumArtists = author;
                                    tagLibFile.Tag.Performers = author;
                                    tagLibFile.Tag.Composers = author;
                                }

                                tagLibFile.Save();

                                // Report progress
                                backgroundWorker.ReportProgress(i / tagParams.AudiobookFiles.Count * 100);
                            }
                        }

                        e.Result = new WorkerResult
                        {
                            FinalMessage = $"Complete, updated {tagParams.AudiobookFiles.Count} files.",
                        };
                    }
                }
                catch (Exception ex)
                {
                    WriteOutput(ex.Message, OutputMessageLevel.Error);
                }
            }
        }

        private void BackgroundWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            busyIndicator.ProgressValue = e.ProgressPercentage;
            busyIndicator.BusyContent = $"Updating tags, {e.ProgressPercentage}%...";

            // Also write to output, replacing the last line written.
            WriteOutput($"Updating tags, {e.ProgressPercentage}%...", OutputMessageLevel.Informational, true);
        }

        private void BackgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result == null)
            {
                MessageBox.Show("Something went wrong starting the background worker, try again.", "Worker parameter was null", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            if (e.Result is WorkerResult resultParameter)
            {
                WriteOutput(resultParameter.FinalMessage, OutputMessageLevel.Success);

                RefreshFileList();
            }

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;
        }

        private void RefreshFileList()
        {
            AudiobookFiles.Clear();

            foreach (string album in AudiobookTitlesListBox.SelectedItems)
            {
                var filePaths = Directory.EnumerateFiles(album);

                foreach (var filePath in filePaths)
                {
                    if (Path.GetExtension(filePath)?.ToLower().Contains("mp3") == false)
                    {
                        WriteOutput($"Skipping {Path.GetFileNameWithoutExtension(filePath)} (only MP3s allowed)...", OutputMessageLevel.Warning);
                        continue;
                    }

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

                    if (tagLibFile?.Tag != null)
                    {
                        audiobookFile.Title = tagLibFile.Tag.Title;
                        audiobookFile.Album = tagLibFile.Tag.Album;

                        try
                        {
                            audiobookFile.Artist = tagLibFile.Tag.Artists?.FirstOrDefault();
                        }
                        catch { }
                        
                        audiobookFile.Performer = tagLibFile.Tag.Performers?.FirstOrDefault();
                    }

                    AudiobookFiles.Add(audiobookFile);
                }
            }

            if (AudiobookTitlesListBox.SelectedItems.Count == 0)
            {
                WriteOutput("Selections cleared.", OutputMessageLevel.Warning);
            }
            else if (AudiobookTitlesListBox.SelectedItems.Count == 1)
            {
                WriteOutput($"{Path.GetFileName(openFolderDialog.FileName)} selected ({AudiobookFiles.Count} files).", OutputMessageLevel.Informational);
            }
            else
            {
                WriteOutput($"{AudiobookTitlesListBox.SelectedItems.Count} selected ({AudiobookFiles.Count} total files).", OutputMessageLevel.Informational);
            }
        }

        private void Reset()
        {
            AlbumNameTextBox.Text = string.Empty;
            ArtistTextBox.Text = string.Empty;

            SetAlbumNameCheckBox.IsChecked = true;
            SetTitleCheckBox.IsChecked = true;
            SetArtistNameCheckBox.IsChecked = true;

            AudiobookTitles.Clear();
            AudiobookFiles.Clear();
            StatusMessages.Clear();
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
                StatusListBox.ScrollIntoView(message);
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
                    StatusListBox.ScrollIntoView(message);
                });
            }
        }

        public void Dispose()
        {
            backgroundWorker.Dispose();
        }
    }
}
