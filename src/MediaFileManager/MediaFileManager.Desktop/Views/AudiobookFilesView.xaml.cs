using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using MediaFileManager.Common.Models;
using MediaFileManager.Common.Models.AudioBook;
using MediaFileManager.Desktop.Helpers;
using Microsoft.AppCenter.Crashes;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop.Views;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
public partial class AudiobookFilesView
{
    private readonly BackgroundWorker backgroundWorker;
    private readonly RadOpenFolderDialog openFolderDialog;
    private readonly ObservableCollection<OutputMessage> statusMessages;
    private readonly ObservableCollection<string> audiobookTitles;
    private readonly ObservableCollection<AudiobookFile> audiobookFiles;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Irrelevant")]
    public AudiobookFilesView()
    {
        InitializeComponent();

        openFolderDialog = new RadOpenFolderDialog { Owner = this, ExpandToCurrentDirectory = false };

        StatusListBox.ItemsSource = statusMessages = new ObservableCollection<OutputMessage>();
        AudiobookTitlesListBox.ItemsSource = audiobookTitles = new ObservableCollection<string>();
        AudiobookFilesGridView.ItemsSource = audiobookFiles = new ObservableCollection<AudiobookFile>();

        backgroundWorker = new BackgroundWorker { WorkerReportsProgress = true };
        backgroundWorker.DoWork += BackgroundWorker_DoWork;
        backgroundWorker.ProgressChanged += BackgroundWorker_ProgressChanged;
        backgroundWorker.RunWorkerCompleted += BackgroundWorker_RunWorkerCompleted;

        WriteOutput("Ready, open an author folder to begin.", OutputMessageLevel.Success);

        Unloaded += AudiobookFilesView_Unloaded;
    }
    
    private void SelectAuthorFolderButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            WriteOutput("Opening folder picker...", OutputMessageLevel.Normal);

            LocalBusyIndicator.IsBusy = true;
            LocalBusyIndicator.Visibility = Visibility.Visible;
            LocalBusyIndicator.BusyContent = "opening folder...";
            LocalBusyIndicator.IsIndeterminate = true;

            if (!string.IsNullOrEmpty(Properties.Settings.Default.LastFolder))
            {
                // Need to bump up one level from the last folder location
                var topDirectoryInfo = Directory.GetParent(Properties.Settings.Default.LastFolder);

                openFolderDialog.InitialDirectory = topDirectoryInfo.FullName;

                WriteOutput("Starting at saved folder.", OutputMessageLevel.Normal);
            }
            else
            {
                WriteOutput("No saved folder, starting at root.", OutputMessageLevel.Warning);
            }

            openFolderDialog.ShowDialog();

            if (openFolderDialog.DialogResult != true)
            {
                WriteOutput("Canceled folder selection.", OutputMessageLevel.Normal);
                return;
            }
            else
            {
                Properties.Settings.Default.LastFolder = openFolderDialog.FileName;
                Properties.Settings.Default.Save();
            }

            Reset();

            LocalBusyIndicator.BusyContent = "searching for albums...";

            var folders = Directory.EnumerateDirectories(openFolderDialog.FileName).ToList();

            audiobookTitles.Clear();

            foreach (var folder in folders)
            {
                audiobookTitles.Add(folder);

                LocalBusyIndicator.BusyContent = $"added {folder}";
            }

            switch (audiobookTitles.Count)
            {
                case 0:
                    WriteOutput("No titles detected.", OutputMessageLevel.Error);
                    MessageBox.Show("The selected Author's folder should have subfolders, each subfolder should be named with the audiobook's title.", "No Titles Available.");
                    break;
                case 1:
                    WriteOutput($"Opened {Path.GetFileName(openFolderDialog.FileName)}' ({audiobookTitles.Count} title).", OutputMessageLevel.Success);
                    break;
                default:
                    WriteOutput($"Opened {Path.GetFileName(openFolderDialog.FileName)} ({audiobookTitles.Count} titles).", OutputMessageLevel.Success);
                    break;
            }

            Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Audiobook Folder Opened", new Dictionary<string, string>
                {
                    { "Audiobook Titles Loaded", $"{audiobookTitles.Count}" }
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
            LocalBusyIndicator.BusyContent = "";
            LocalBusyIndicator.IsBusy = false;
            LocalBusyIndicator.Visibility = Visibility.Collapsed;
            LocalBusyIndicator.IsIndeterminate = false;
        }
    }

    private void AudiobookTitlesListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
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

        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("User Reset", new Dictionary<string, string>
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
        if (SetAlbumNameCheckBox.IsChecked == true && string.IsNullOrEmpty(AlbumNameTextBox.Text))
        {
            WriteOutput("Album (book title) is empty.", OutputMessageLevel.Error);
            return;
        }

        if (SetArtistNameCheckBox.IsChecked == true && string.IsNullOrEmpty(ArtistTextBox.Text))
        {
            WriteOutput("Artist (author name) is empty.", OutputMessageLevel.Error);
            return;
        }

        if (AudiobookFilesGridView.SelectedItems.Count == 0)
        {
            WriteOutput("No files have been selected.", OutputMessageLevel.Error);
            return;
        }

        Microsoft.AppCenter.Analytics.Analytics.TrackEvent("Set Tags started", new Dictionary<string, string>
            {
                { "Set Book Title Enabled", $"{SetAlbumNameCheckBox.IsChecked}" },
                { "Set Author Name Enabled", $"{SetArtistNameCheckBox.IsChecked}" },
                { "Selected Audiobook files", $"{AudiobookFilesGridView.SelectedItems.Count}" },
                { "Authors", $"{audiobookTitles.Count}" }
            });

        LocalBusyIndicator.IsBusy = true;
        LocalBusyIndicator.Visibility = Visibility.Visible;
        LocalBusyIndicator.BusyContent = "updating tags...";
        LocalBusyIndicator.IsIndeterminate = false;
        LocalBusyIndicator.ProgressValue = 0;

        var selectedFiles = AudiobookFilesGridView.SelectedItems.Cast<AudiobookFile>().ToList();

        backgroundWorker.RunWorkerAsync(new TagWorkerParameters(selectedFiles)
        {
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

                    using (var tagLibFile = TagLib.File.Create(audiobookFile.FilePath))
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
#pragma warning disable CS0618 // Type or member is obsolete
                                tagLibFile.Tag.Artists = author; // Plex still uses Artists
#pragma warning restore CS0618 // Type or member is obsolete
                                tagLibFile.Tag.Performers = author;
                                tagLibFile.Tag.AlbumArtists = author;
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
        LocalBusyIndicator.ProgressValue = e.ProgressPercentage;
        LocalBusyIndicator.BusyContent = $"Updating tags, {e.ProgressPercentage}%...";

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

        LocalBusyIndicator.BusyContent = "";
        LocalBusyIndicator.IsBusy = false;
        LocalBusyIndicator.Visibility = Visibility.Collapsed;
        LocalBusyIndicator.ProgressValue = 0;
    }
    
    private void RefreshFileList()
    {
        audiobookFiles.Clear();

        foreach (string album in AudiobookTitlesListBox.SelectedItems)
        {
            var filePaths = Directory.EnumerateFiles(album);

            foreach (var filePath in filePaths)
            {
                if (Path.GetExtension(filePath)?.ToLower().Contains("mp3", StringComparison.InvariantCulture) == false)
                {
                    WriteOutput($"Skipping {Path.GetFileNameWithoutExtension(filePath)} (only MP3s allowed)...", OutputMessageLevel.Warning);
                    continue;
                }

                var audiobookFile = new AudiobookFile
                {
                    FilePath = filePath
                };

                var fileName = Path.GetFileName(filePath);

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
#pragma warning disable CS0618 // Type or member is obsolete
                        audiobookFile.Artist = tagLibFile.Tag.Artists?.FirstOrDefault();
#pragma warning restore CS0618 // Type or member is obsolete
                    }
                    catch (NullReferenceException ex)
                    {
                        Trace.TraceError("Tag.Artists was null. {0}", ex);
                    }

                    audiobookFile.Performer = tagLibFile.Tag.Performers?.FirstOrDefault();
                }

                tagLibFile?.Dispose();

                audiobookFiles.Add(audiobookFile);
            }
        }

        if (AudiobookTitlesListBox.SelectedItems.Count == 0)
        {
            WriteOutput("Selections cleared.", OutputMessageLevel.Warning);
        }
        else if (AudiobookTitlesListBox.SelectedItems.Count == 1)
        {
            WriteOutput($"{Path.GetFileName(openFolderDialog.FileName)} selected ({audiobookFiles.Count} files).", OutputMessageLevel.Informational);
        }
        else
        {
            WriteOutput($"{AudiobookTitlesListBox.SelectedItems.Count} selected ({audiobookFiles.Count} total files).", OutputMessageLevel.Informational);
        }
    }

    private void Reset()
    {
        AlbumNameTextBox.Text = string.Empty;
        ArtistTextBox.Text = string.Empty;

        SetAlbumNameCheckBox.IsChecked = true;
        SetTitleCheckBox.IsChecked = true;
        SetArtistNameCheckBox.IsChecked = true;

        audiobookTitles.Clear();
        audiobookFiles.Clear();
        statusMessages.Clear();
    }

    private void WriteOutput(string text, OutputMessageLevel level, bool removeLastItem = false)
    {
        var messageColor = level switch
        {
            OutputMessageLevel.Normal => Colors.Black,
            OutputMessageLevel.Informational => Colors.Gray,
            OutputMessageLevel.Success => Colors.Green,
            OutputMessageLevel.Warning => Colors.Goldenrod,
            OutputMessageLevel.Error => Colors.Red,
            _ => Colors.Gray
        };

        if (Dispatcher.CheckAccess())
        {
            if (removeLastItem && statusMessages.Count > 0)
            {
                statusMessages.Remove(statusMessages.LastOrDefault());
            }

            var message = new OutputMessage
            {
                Message = text,
                MessageColor = messageColor.ToSystemDrawingColor()
            };

            statusMessages.Add(message);

            StatusListBox.ScrollIntoView(message);
        }
        else
        {
            Dispatcher.Invoke(() =>
            {
                if (removeLastItem && statusMessages.Count > 0)
                {
                    statusMessages.Remove(statusMessages.LastOrDefault());
                }

                var message = new OutputMessage
                {
                    Message = text,
                    MessageColor = messageColor.ToSystemDrawingColor()
                };

                statusMessages.Add(message);

                StatusListBox.ScrollIntoView(message);
            });
        }
    }

    private void AudiobookFilesView_Unloaded(object sender, RoutedEventArgs e)
    {
        backgroundWorker.Dispose();
    }
}
