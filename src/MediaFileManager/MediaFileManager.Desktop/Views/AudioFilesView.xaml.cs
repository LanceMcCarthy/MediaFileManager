using Id3;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace MediaFileManager.Desktop.Views
{
    public partial class AudioFilesView : UserControl
    {
        public AudioFilesView()
        {
            InitializeComponent();
        }

        private void LoadFilesButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void UpdateTagsButton_Click(object sender, RoutedEventArgs e)
        {

        }

        private void ReadTags()
        {
            string[] musicFiles = Directory.GetFiles(@"C:\Music", "*.mp3");

            foreach (string musicFile in musicFiles)
            {
                using (var mp3 = new Mp3(musicFile))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                    Console.WriteLine("Title: {0}", tag.Title);
                    Console.WriteLine("Artist: {0}", tag.Artists);
                    Console.WriteLine("Album: {0}", tag.Album);
                }
            }
        }

        private IEnumerable<string> GetMusicFrom80s(IEnumerable<string> mp3FilePaths)
        {
            foreach (var mp3FilePath in mp3FilePaths)
            {
                using (var mp3 = new Mp3(mp3FilePath))
                {
                    Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                    if (tag.Year.Value.HasValue)
                    {
                        if (tag.Year >= 1980 && tag.Year < 1990)
                        {
                            yield return mp3FilePath;
                        }
                    }
                }
            }
        }

        private void SetCopyright(string mp3FilePath)
        {
            using (var mp3 = new Mp3(mp3FilePath, Mp3Permissions.ReadWrite))
            {
                Id3Tag tag = mp3.GetTag(Id3TagFamily.Version2X);

                if (!tag.Copyright.IsAssigned)
                {
                    int year = tag.Year.Value.GetValueOrDefault(2000);

                    string artists = tag.Artists.ToString();
                    tag.Copyright = $"{year} {artists}";
                    mp3.WriteTag(tag, WriteConflictAction.Replace);
                }
            }
        }
    }
}
