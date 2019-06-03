using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using MediaFileManager.Desktop.Models;
using Telerik.Windows.Controls;

// ReSharper disable InconsistentNaming
namespace MediaFileManager.Desktop
{
    public partial class MainWindow : Window
    {
        private readonly RadOpenFolderDialog openFolderDialog;
        private readonly ObservableCollection<string> Seasons = new ObservableCollection<string>();
        private readonly ObservableCollection<string> Episodes = new ObservableCollection<string>();
        private readonly ObservableCollection<OutputMessage> OutputMessages = new ObservableCollection<OutputMessage>();

        private readonly ObservableCollection<string> ResultPreview = new ObservableCollection<string>();

        public MainWindow()
        {
            InitializeComponent();

            SeasonsListBox.ItemsSource = Seasons;
            EpisodesListBox.ItemsSource = Episodes;
            PreviewListBox.ItemsSource = ResultPreview;
            OutputListBox.ItemsSource = OutputMessages;

            openFolderDialog = new RadOpenFolderDialog {Owner = this, ExpandToCurrentDirectory = false};

            WriteOutput($"Ready, open a folder to begin.", OutputMessageLevel.Success);
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

        private async void UpdateFileNameButton_Click(object sender, RoutedEventArgs e)
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

            // variables for background thread access
            var selectedItems = EpisodesListBox.SelectedItems.Cast<string>().ToList();
            var selectedText = OriginalTextBox_Renaming.Text;
            var replacementText = ReplacementTextBox.Text;

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "updating file names...";
            busyIndicator.IsIndeterminate = false;
            busyIndicator.ProgressValue = 0;

            await Task.Run(() =>
            {
                try
                {
                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        var episodeFilePath = selectedItems[i];

                        // Need to separate name and path in order to prefix the file name
                        string curDir = Path.GetDirectoryName(episodeFilePath);

                        if (string.IsNullOrEmpty(curDir))
                        {
                            WriteOutput($"Could not find directory, skipping.", OutputMessageLevel.Error);
                            continue;
                        }

                        string curName = Path.GetFileName(episodeFilePath);

                        if (string.IsNullOrEmpty(curName))
                        {
                            WriteOutput($"Could not find file, skipping.", OutputMessageLevel.Error);
                            continue;
                        }

                        // Replace the selected text with the new text (support empty replacement to remove text)
                        string newName = curName.Replace(selectedText, replacementText);

                        // Rename the file
                        File.Move(episodeFilePath, Path.Combine(curDir, newName));

                        // Need to dispatch back to UI thread, variables to avoid access to modified closure problem
                        var progressComplete = i / selectedItems.Count * 100;
                        var progressText = $"Renaming - '{selectedText}' to '{replacementText}'...";

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

            RefreshEpisodesList();

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;
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

        private async void RenumberingButton_Click(object sender, RoutedEventArgs e)
        {
            await RenumberPreviewAsync();

            //await RenumberAsync();
        }

        private async Task RenumberPreviewAsync()
        {
            // NOTE: Try BGWorker instead of task 
            //https://www.wpf-tutorial.com/misc/multi-threading-with-the-backgroundworker/
            BackgroundWorker worker = new BackgroundWorker();
            worker.WorkerReportsProgress = true;
            worker.DoWork += RenumberWorker_DoWork;
            worker.ProgressChanged += RenumberWorker_ProgressChanged;
            worker.RunWorkerCompleted += RenumberWorker_RunWorkerCompleted;

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "re-numbering and renaming files...";
            busyIndicator.IsIndeterminate = false;
            busyIndicator.ProgressValue = 0;

            worker.RunWorkerAsync();
        }

        private void RenumberWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            (sender as BackgroundWorker).DoWork -= RenumberWorker_DoWork;
            (sender as BackgroundWorker).ProgressChanged -= RenumberWorker_ProgressChanged;
            (sender as BackgroundWorker).RunWorkerCompleted -= RenumberWorker_RunWorkerCompleted;

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;

            ResultPreviewPane.IsSelected = true;
        }

        private void RenumberWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            busyIndicator.ProgressValue = e.ProgressPercentage;
            busyIndicator.BusyContent = $"Completed {e.ProgressPercentage:P2}...";

            if(e.UserState is string newName)
            {
                ResultPreview.Add(newName);
            }
        }

        private void RenumberWorker_DoWork(object sender, DoWorkEventArgs e)
        {
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
                WriteOutput($"You must enter a first and last episode number.", OutputMessageLevel.Error);
                return;
            }

            if (!int.TryParse(EpisodeStartTextBox?.Text, out int startingEpisodeNumber) || !int.TryParse(EpisodeEndTextBox?.Text, out int lastEpisodeNumber))
            {
                WriteOutput($"You must use a valid two-digit value for the start and end episode number.", OutputMessageLevel.Error);
                return;
            }

            // Make sure the user has entered the correct number of episodes
            if (lastEpisodeNumber - startingEpisodeNumber + 1 != EpisodesListBox.SelectedItems.Count)
            {
                WriteOutput($"The episode numbers do not match the total selected episodes, you need to have the same number of episode number as selected episodes.", OutputMessageLevel.Error);
                return;
            }

            // Variables for background thread access
            var selectedItems = EpisodesListBox.SelectedItems.Cast<string>().ToList();
            var selectedTextLength = OriginalTextBox_Renumbering.Text.Length;
            var seasonNumberText = SeasonNumberTextBox.Text;

            try
            {
                int currentEpisodeNumber = startingEpisodeNumber;

                WriteOutput("Begin re-numbering operation...", OutputMessageLevel.Warning);

                for (int i = 0; i < selectedItems.Count; i++)
                {
                    var episodeFilePath = selectedItems[i];

                    string curDir = Path.GetDirectoryName(episodeFilePath);

                    if (string.IsNullOrEmpty(curDir))
                    {
                        WriteOutput($"Could not find directory, skipping.", OutputMessageLevel.Error);
                        continue;
                    }

                    string curName = Path.GetFileName(episodeFilePath);

                    if (string.IsNullOrEmpty(curName))
                    {
                        WriteOutput($"Could not find file, skipping.", OutputMessageLevel.Error);
                        continue;
                    }

                    // Using substring and Length so that user doesn't need an exact natch (e.g. episode number will be different for each selected episode, thus only one selection will get renamed... the exact match)
                    var selectedText = curName.Substring(0, selectedTextLength);

                    // Get the show name to workaround not being able to select exact text
                    var showName = Directory.GetParent(episodeFilePath)?.Parent?.Name;

                    // Prefix the name name with the Show, then the season, then the episode number
                    string newName = curName.Replace(selectedText, $"{showName} - S{seasonNumberText}E{currentEpisodeNumber:00} -");

                    // Need to dispatch back to UI thread, variables to avoid access to modified closure problem
                    var progressComplete = i / selectedItems.Count * 100;
                    var progressText = $"Completed: S{seasonNumberText}E{currentEpisodeNumber:00}...";

                    e.Result = newName;
                    (sender as BackgroundWorker).ReportProgress(progressComplete);

                    // Increment the episode number
                    currentEpisodeNumber++;
                }

                (sender as BackgroundWorker).ReportProgress(100);

                WriteOutput($"Re-numbering operation complete!", OutputMessageLevel.Success);
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }
        }

        private async Task RenumberAsync()
        {
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
                WriteOutput($"You must enter a first and last episode number.", OutputMessageLevel.Error);
                return;
            }

            if (!int.TryParse(EpisodeStartTextBox?.Text, out int startingEpisodeNumber) || !int.TryParse(EpisodeEndTextBox?.Text, out int lastEpisodeNumber))
            {
                WriteOutput($"You must use a valid two-digit value for the start and end episode number.", OutputMessageLevel.Error);
                return;
            }

            // Make sure the user has entered the correct number of episodes
            if (lastEpisodeNumber - startingEpisodeNumber + 1 != EpisodesListBox.SelectedItems.Count)
            {
                WriteOutput($"The episode numbers do not match the total selected episodes, you need to have the same number of episode number as selected episodes.", OutputMessageLevel.Error);
                return;
            }

            // Variables for background thread access
            var selectedItems = EpisodesListBox.SelectedItems.Cast<string>().ToList();
            var selectedTextLength = OriginalTextBox_Renumbering.Text.Length;
            var seasonNumberText = SeasonNumberTextBox.Text;

            busyIndicator.IsBusy = true;
            busyIndicator.BusyContent = "re-numbering and renaming files...";
            busyIndicator.IsIndeterminate = false;
            busyIndicator.ProgressValue = 0;

            await Task.Run(() =>
            {
                try
                {
                    int currentEpisodeNumber = startingEpisodeNumber;

                    WriteOutput("Begin re-numbering operation...", OutputMessageLevel.Warning);

                    for (int i = 0; i < selectedItems.Count; i++)
                    {
                        var episodeFilePath = selectedItems[i];

                        string curDir = Path.GetDirectoryName(episodeFilePath);

                        if (string.IsNullOrEmpty(curDir))
                        {
                            WriteOutput($"Could not find directory, skipping.", OutputMessageLevel.Error);
                            continue;
                        }

                        string curName = Path.GetFileName(episodeFilePath);

                        if (string.IsNullOrEmpty(curName))
                        {
                            WriteOutput($"Could not find file, skipping.", OutputMessageLevel.Error);
                            continue;
                        }

                        // Using substring and Length so that user doesn't need an exact natch (e.g. episode number will be different for each selected episode, thus only one selection will get renamed... the exact match)
                        var selectedText = curName.Substring(0, selectedTextLength);

                        // Get the show name to workaround not being able to select exact text
                        var showName = Directory.GetParent(episodeFilePath)?.Parent?.Name;

                        // Prefix the name name with the Show, then the season, then the episode number
                        string newName = curName.Replace(selectedText, $"{showName} - S{seasonNumberText}E{currentEpisodeNumber:00} -");

                        // Rename the file
                        File.Move(episodeFilePath, Path.Combine(curDir, newName));

                        // Need to dispatch back to UI thread, variables to avoid access to modified closure problem
                        var progressComplete = i / selectedItems.Count * 100;
                        var progressText = $"Completed: S{seasonNumberText}E{currentEpisodeNumber:00}...";

                        this.Dispatcher.Invoke(() =>
                        {
                            busyIndicator.ProgressValue = progressComplete;
                            busyIndicator.BusyContent = $"Completed {progressText}...";
                        });

                        // Increment the episode number
                        currentEpisodeNumber++;
                    }

                    WriteOutput($"Re-numbering operation complete!", OutputMessageLevel.Success);
                }
                catch (Exception ex)
                {
                    WriteOutput(ex.Message, OutputMessageLevel.Error);
                }
            });

            RefreshEpisodesList();

            busyIndicator.BusyContent = "";
            busyIndicator.IsBusy = false;
            busyIndicator.ProgressValue = 0;
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
