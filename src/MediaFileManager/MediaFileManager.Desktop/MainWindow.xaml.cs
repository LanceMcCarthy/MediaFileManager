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
        private ObservableCollection<string> Seasons = new ObservableCollection<string>();
        private ObservableCollection<string> Episodes = new ObservableCollection<string>();
        private ObservableCollection<OutputMessage> OutputMessages = new ObservableCollection<OutputMessage>();
        readonly RadOpenFolderDialog openFolderDialog;

        public MainWindow()
        {
            InitializeComponent();

            SeasonsListBox.ItemsSource = Seasons;
            EpisodesListBox.ItemsSource = Episodes;
            OutputListBox.ItemsSource = OutputMessages;

            openFolderDialog = new RadOpenFolderDialog();
            openFolderDialog.Owner = this;
            openFolderDialog.ExpandToCurrentDirectory = false;

            WriteOutput($"Application ready, open a folder to begin.", OutputMessageLevel.Success);
        }


        #region Source operations

        private void SelectFolderButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                WriteOutput($"Opening folder picker...", OutputMessageLevel.Normal);

                if (!string.IsNullOrEmpty(Properties.Settings.Default["LastFolder"] as string))
                {
                    openFolderDialog.InitialDirectory = Properties.Settings.Default["LastFolder"] as string;

                    WriteOutput($"LastFolder setting loaded.", OutputMessageLevel.Normal);
                }
                else
                {
                    WriteOutput($"No LastFolder setting found, starting with root folder.", OutputMessageLevel.Warning);
                }

                openFolderDialog.ShowDialog();

                if (openFolderDialog.DialogResult != true)
                {
                    WriteOutput($"Canceled folder selection.", OutputMessageLevel.Normal);
                    return;
                }
                else
                {
                    Properties.Settings.Default["LastFolder"] = openFolderDialog.FileName;
                    Properties.Settings.Default.Save();
                }

                Reset();

                var seasonsResult = Directory.EnumerateDirectories(openFolderDialog.FileName);

                Seasons.Clear();

                foreach (var season in seasonsResult)
                {
                    Seasons.Add(season);
                }

                WriteOutput($"Discovered {seasonsResult?.Count()} seasons.", OutputMessageLevel.Success);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                WriteOutput(ex.Message, OutputMessageLevel.Error);
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

            WriteOutput($"{SeasonsListBox.SelectedItems.Count} seasons selected ({Episodes.Count} total episodes).", OutputMessageLevel.Normal);
        }

        private void EpisodesListBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (e.AddedItems != null)
            {
                if (EpisodesListBox.SelectedItems.Count > 0)
                {
                    var firstEpisodeFilePath = e.AddedItems.OfType<string>().FirstOrDefault();
                    string curName = Path.GetFileName(firstEpisodeFilePath);
                    EpisodeNameTextBox.Text = curName;
                }
            }
        }

        private void EpisodeNameTextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            OriginalTextBox_Renaming.Text = EpisodeNameTextBox.SelectedText;
            OriginalTextBox_Renumbering.Text = EpisodeNameTextBox.SelectedText;
        }

        private void ResetButton_Click(object sender, RoutedEventArgs e)
        {
            Reset();
            WriteOutput("Reset complete.", OutputMessageLevel.Success);
        }

        #endregion

        #region Renaming operation

        private void UpdateFileNameButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(OriginalTextBox_Renaming.Text))
            {
                WriteOutput($"No text selected, aborting file name update.", OutputMessageLevel.Error);
                return;
            }
            else
            {
                WriteOutput($"Renaming operation starting...", OutputMessageLevel.Warning);
            }

            foreach (string episodeFilePath in EpisodesListBox.SelectedItems)
            {
                // Need to separate name and path in order to prefix the file name
                string curDir = Path.GetDirectoryName(episodeFilePath);
                string curName = Path.GetFileName(episodeFilePath);

                string newName = curName.Replace(OriginalTextBox_Renaming.Text, ReplacementTextBox.Text);

                File.Move(episodeFilePath, Path.Combine(curDir, newName));

                WriteOutput($"Renaming - '{OriginalTextBox_Renaming.Text}' to '{ReplacementTextBox.Text}'...", OutputMessageLevel.Success, true);
            }

            WriteOutput($"Renaming complete.", OutputMessageLevel.Success, true);

            RefreshEpisodesList();
        }

        private void OriginalTextBox_Renaming_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            UpdateFileNameButton.IsEnabled = !string.IsNullOrEmpty(OriginalTextBox_Renaming?.Text);
        }

        #endregion

        #region Renumbering operation

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
            if (string.IsNullOrEmpty(SeasonNumberTextBox?.Text))
            {
                WriteOutput($"No text selected, aborting re-numbering operation.", OutputMessageLevel.Error);
                return;
            }

            var result = MessageBox.Show("You are about to renumber all of the selected episodes, using the order they're selected. Are you sure you want to continue?", "Warning!", MessageBoxButton.YesNo, MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
            {
                WriteOutput("Cancelled re-numbering operation.", OutputMessageLevel.Warning);
            }
            else
            {
                WriteOutput("Begin re-numering operation...", OutputMessageLevel.Warning);
            }

            for (int i = 1; i <= EpisodesListBox.SelectedItems.Count; i++)
            {
                var episodeFilePath = EpisodesListBox.SelectedItems[i - 1] as string;

                string curDir = Path.GetDirectoryName(episodeFilePath);
                string curName = Path.GetFileName(episodeFilePath);

                string newName = curName.Replace(OriginalTextBox_Renumbering.Text, $"S{SeasonNumberTextBox.Text}E{i.ToString("00")}");

                File.Move(episodeFilePath, Path.Combine(curDir, newName));

                WriteOutput($"Re-numbering: '{OriginalTextBox_Renumbering.Text}' to '{$"S{SeasonNumberTextBox.Text}E{i.ToString("00")}"}'...", OutputMessageLevel.Normal, true);
            }

            WriteOutput($"Numbered episodes complete.", OutputMessageLevel.Success, true);

            RefreshEpisodesList();
        }

        #endregion

        private void RefreshEpisodesList()
        {
            Episodes.Clear();

            foreach (string season in SeasonsListBox.SelectedItems)
            {
                var episodesResult = Directory.EnumerateFiles(season);

                WriteOutput($"Discovering episodes for {season}...", OutputMessageLevel.Normal);

                foreach (var filePath in episodesResult)
                {
                    Episodes.Add(filePath);
                    WriteOutput($"Adding {filePath}...", OutputMessageLevel.Normal, true);
                }

                WriteOutput($"Discovered {Episodes.Count} episodes.", OutputMessageLevel.Success, true);
            }
        }

        private void Reset()
        {
            EpisodeNameTextBox.Text = string.Empty;
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
