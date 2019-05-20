using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using MediaFileManager.Desktop.Models;
using Telerik.Windows.Controls;

namespace MediaFileManager.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly RadOpenFolderDialog openFolderDialog;
        // ReSharper disable once InconsistentNaming
        private readonly ObservableCollection<string> Seasons = new ObservableCollection<string>();
        // ReSharper disable once InconsistentNaming
        private readonly ObservableCollection<string> Episodes = new ObservableCollection<string>();
        // ReSharper disable once InconsistentNaming
        private readonly ObservableCollection<OutputMessage> OutputMessages = new ObservableCollection<OutputMessage>();

        public MainWindow()
        {
            InitializeComponent();

            SeasonsListBox.ItemsSource = Seasons;
            EpisodesListBox.ItemsSource = Episodes;
            OutputListBox.ItemsSource = OutputMessages;

            openFolderDialog = new RadOpenFolderDialog {Owner = this, ExpandToCurrentDirectory = false};

            WriteOutput($"Application ready, open a folder to begin.", OutputMessageLevel.Success);
        }

        #region Source operations

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

                busyIndicator.BusyContent = $"searching for seasons...";

                var seasonsResult = Directory.EnumerateDirectories(openFolderDialog.FileName).ToList();

                Seasons.Clear();

                foreach (var season in seasonsResult)
                {
                    Seasons.Add(season);

                    busyIndicator.BusyContent = $"added {season}";
                }

                if (Seasons.Count == 0)
                {
                    WriteOutput("No seasons detected, make sure there are subfolders with season number.", OutputMessageLevel.Warning);
                }
                else if (Seasons.Count == 1)
                {
                    WriteOutput($"Opened '{Path.GetFileName(openFolderDialog.FileName)}' ({Seasons.Count} season).", OutputMessageLevel.Success);
                }
                else
                {
                    WriteOutput($"Opened '{Path.GetFileName(openFolderDialog.FileName)}' ({Seasons.Count} seasons).", OutputMessageLevel.Success);
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

        private void SeasonsListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            Episodes.Clear();

            foreach (string season in SeasonsListBox.SelectedItems)
            {
                var episodesResult = Directory.EnumerateFiles(season);

                foreach (var filePath in episodesResult)
                {
                    Episodes.Add(filePath);
                }
            }

            if (SeasonsListBox.SelectedItems.Count == 0)
            {
                WriteOutput("Selections cleared.", OutputMessageLevel.Warning);
            }
            else if (SeasonsListBox.SelectedItems.Count == 1)
            {
                WriteOutput($"{Path.GetFileName(openFolderDialog.FileName)} selected ({Episodes.Count} episodes).", OutputMessageLevel.Informational);
            }
            else
            {
                WriteOutput($"{SeasonsListBox.SelectedItems.Count} seasons selected ({Episodes.Count} total episodes).", OutputMessageLevel.Informational);
            }
        }

        private void EpisodesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems == null)
                return;

            if (EpisodesListBox.SelectedItems.Count <= 0)
                return;

            var firstEpisodeFilePath = e.AddedItems.OfType<string>().FirstOrDefault();

            string curName = Path.GetFileName(firstEpisodeFilePath);

            if (!string.IsNullOrEmpty(curName))
            {
                EpisodeName_Renaming_TextBox.Text = curName;
                EpisodeName_Renumbering_TextBox.Text = curName;
            }
            else
            {
                EpisodeName_Renaming_TextBox.Text = string.Empty;
                EpisodeName_Renumbering_TextBox.Text = string.Empty;
            }
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            WriteOutput("Reset complete.", OutputMessageLevel.Success);
        }

        #endregion

        #region Renaming operation

        private void EpisodeNameTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OriginalTextBox_Renaming.Text = EpisodeName_Renaming_TextBox.SelectedText;
        }

        private void OriginalTextBox_Renaming_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateFileNameButton.IsEnabled = !string.IsNullOrEmpty(OriginalTextBox_Renaming?.Text);
        }

        private void UpdateFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busyIndicator.IsBusy = true;
                busyIndicator.BusyContent = "updating file names...";
                busyIndicator.IsIndeterminate = false;
                busyIndicator.ProgressValue = 0;

                if (string.IsNullOrEmpty(OriginalTextBox_Renaming.Text))
                {
                    WriteOutput($"No text selected, aborting file name update.", OutputMessageLevel.Error);
                    return;
                }
                else
                {
                    WriteOutput($"Renaming operation starting...", OutputMessageLevel.Warning);
                }

                for (int i = 1; i <= EpisodesListBox.SelectedItems.Count; i++)
                {
                    var episodeFilePath = EpisodesListBox.SelectedItems[i - 1] as string;

                    // Need to separate name and path in order to prefix the file name
                    string curDir = Path.GetDirectoryName(episodeFilePath);

                    if (string.IsNullOrEmpty(curDir))
                    {
                        WriteOutput($"Could not identify directory.", OutputMessageLevel.Error);
                        continue;
                    }

                    string curName = Path.GetFileName(episodeFilePath);

                    if (string.IsNullOrEmpty(curName))
                    {
                        WriteOutput($"Could not identify file name.", OutputMessageLevel.Error);
                        continue;
                    }

                    string newName = curName.Replace(OriginalTextBox_Renaming.Text, ReplacementTextBox.Text);

                    File.Move(episodeFilePath, Path.Combine(curDir, newName));

                    busyIndicator.ProgressValue = i / EpisodesListBox.SelectedItems.Count * 100;
                    busyIndicator.BusyContent = $"Renaming - '{OriginalTextBox_Renaming.Text}' to '{ReplacementTextBox.Text}'...";
                }

                WriteOutput($"Renaming complete.", OutputMessageLevel.Success, true);

                RefreshEpisodesList();
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }
            finally
            {
                busyIndicator.BusyContent = "";
                busyIndicator.IsBusy = false;
                busyIndicator.ProgressValue = 0;
            }
        }

        #endregion

        #region Renumbering operation

        private void EpisodeNameTextBox_Renumbering_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OriginalTextBox_Renumbering.Text = EpisodeName_Renumbering_TextBox.SelectedText;
        }

        private void OriginalTextBox_Renumbering_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(OriginalTextBox_Renumbering?.Text) || string.IsNullOrEmpty(SeasonNumberTextBox?.Text))
            {
                RenumberingButton.IsEnabled = false;
            }
            else
            {
                RenumberingButton.IsEnabled = true;
            }
        }

        private void SeasonNumberTextBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            // We also want to make sure user entered a season number
            if (string.IsNullOrEmpty(OriginalTextBox_Renumbering?.Text) || string.IsNullOrEmpty(SeasonNumberTextBox?.Text))
            {
                RenumberingButton.IsEnabled = false;
            }
            else
            {
                RenumberingButton.IsEnabled = true;
            }
        }

        private void RenumberingButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                busyIndicator.IsBusy = true;
                busyIndicator.BusyContent = "re-numbering and renaming files...";
                busyIndicator.IsIndeterminate = false;
                busyIndicator.ProgressValue = 0;

                if (string.IsNullOrEmpty(OriginalTextBox_Renumbering?.Text))
                {
                    WriteOutput($"You must selected text that will be replaced by the season and episode number.", OutputMessageLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(SeasonNumberTextBox?.Text))
                {
                    WriteOutput($"You must enter a valid two-digit number for the season.", OutputMessageLevel.Error);
                    return;
                }

                if (string.IsNullOrEmpty(EpisodeStartTextBox?.Text) || string.IsNullOrEmpty(EpisodeEndTextBox?.Text))
                {
                    WriteOutput($"You must enter a valid two-digit number for the first and last selected episodes.", OutputMessageLevel.Error);
                    return;
                }

                if (!int.TryParse(EpisodeStartTextBox?.Text, out int startingEpisodeNumber) || !int.TryParse(EpisodeEndTextBox?.Text, out int lastEpisodeNumber))
                {
                    WriteOutput($"You must enter a valid two-digit number for the first and last selected episodes.", OutputMessageLevel.Error);
                    return;
                }

                // Make sure the user has entered the correct number of episodes
                if (lastEpisodeNumber - startingEpisodeNumber + 1 != EpisodesListBox.SelectedItems.Count)
                {
                    WriteOutput($"The episode numbers do not match the total selected episodes, you need to have the same number of episode number as selected episodes.", OutputMessageLevel.Error);
                    return;
                }

                int currentEpisodeNumber = startingEpisodeNumber;

                WriteOutput("Begin re-numbering operation...", OutputMessageLevel.Warning);

                for (int i = 0; i < EpisodesListBox.SelectedItems.Count; i++)
                {
                    var episodeFilePath = EpisodesListBox.SelectedItems[i] as string;

                    string curDir = Path.GetDirectoryName(episodeFilePath);

                    if (string.IsNullOrEmpty(curDir))
                    {
                        WriteOutput($"Could not identify directory.", OutputMessageLevel.Error);
                        continue;
                    }

                    string curName = Path.GetFileName(episodeFilePath);

                    if (string.IsNullOrEmpty(curName))
                    {
                        WriteOutput($"Could not identify file name, skipping.", OutputMessageLevel.Error);
                        continue;
                    }

                    var selectedText = curName.Substring(0, OriginalTextBox_Renumbering.Text.Length);

                    var seasonFolderInfo = Directory.GetParent(episodeFilePath);
                    var showFolderInfo = seasonFolderInfo.Parent;
                    var showName = showFolderInfo.Name;

                    string newName = curName.Replace(selectedText, $"{showName} - S{SeasonNumberTextBox.Text}E{currentEpisodeNumber:00} - ");

                    File.Move(episodeFilePath, Path.Combine(curDir, newName));

                    busyIndicator.ProgressValue = i / EpisodesListBox.SelectedItems.Count * 100;
                    busyIndicator.BusyContent = $"Re-numbering: '{OriginalTextBox_Renumbering.Text}' to 'S{SeasonNumberTextBox.Text}E{i}:00'...";

                    currentEpisodeNumber++;
                }

                WriteOutput($"Numbered episodes complete.", OutputMessageLevel.Success, true);

                RefreshEpisodesList();
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }
            finally
            {
                busyIndicator.BusyContent = "";
                busyIndicator.IsBusy = false;
                busyIndicator.ProgressValue = 0;
            }
        }

        #endregion

        private void RefreshEpisodesList()
        {
            Episodes.Clear();

            foreach (string season in SeasonsListBox.SelectedItems)
            {
                var folderName = Path.GetFileName(season);

                if (string.IsNullOrEmpty(folderName))
                {
                    WriteOutput($"Could not identify directory.", OutputMessageLevel.Error);
                    return;
                }

                var episodesResult = Directory.EnumerateFiles(season);

                WriteOutput($"Searching {folderName} episodes...", OutputMessageLevel.Normal);

                foreach (var filePath in episodesResult)
                {
                    if(Path.HasExtension(filePath))
                    {
                        Episodes.Add(filePath);

                        WriteOutput($"Adding {filePath}...", OutputMessageLevel.Normal, true);
                    }
                }

                WriteOutput($"Refreshed {folderName} {Episodes.Count} episodes.", OutputMessageLevel.Normal, true);
            }
        }

        private void Reset()
        {
            EpisodeName_Renaming_TextBox.Text = string.Empty;
            EpisodeName_Renumbering_TextBox.Text = string.Empty;

            OriginalTextBox_Renaming.Text = string.Empty;
            OriginalTextBox_Renumbering.Text = string.Empty;

            ReplacementTextBox.Text = string.Empty;
            SeasonNumberTextBox.Text = string.Empty;

            Seasons.Clear();
            Episodes.Clear();
        }

        private void WriteOutput(string text, OutputMessageLevel level, bool removeLastItem = false)
        {
            Color messageColor = Colors.Gray;

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

    }
}
