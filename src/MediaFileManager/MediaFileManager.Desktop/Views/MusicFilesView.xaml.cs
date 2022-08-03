using MediaFileManager.Common.Models;
using MediaFileManager.Common.Models.Audio;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Media;
using MediaFileManager.Desktop.Helpers;

namespace MediaFileManager.Desktop.Views
{
    public partial class MusicFilesView : UserControl
    {
        private readonly ObservableCollection<Artist> sourceList;
        private readonly ObservableCollection<OutputMessage> statusMessages;
        private int totalSongs;

        public MusicFilesView()
        {
            InitializeComponent(); 
        }

        private void ScanSourceFolderButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            // TODO Validate inputs
            var sourceDirectory = SourceFolderTextBox.Text;
            
            GenerateSourceList(sourceDirectory);
        }

        private void StartProcessingButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            var targetDirectory = DestinationFolderTextBox.Text;

            RenameAndCopyFiles(targetDirectory);
        }

        private void ResetButton_Click(object sender, System.Windows.RoutedEventArgs e)
        {
            Reset();
        }

        private void GenerateSourceList(string directory)
        {
            WriteOutput("Checking for mp3 files in directory...", OutputMessageLevel.Normal);

            var mp3Files = Directory.EnumerateFiles(directory, "*.mp3").ToList();

            var total = mp3Files.Count;

            var artists = new List<Artist>();

            if (total < 1)
            {
                WriteOutput("There are no mp3 files present.", OutputMessageLevel.Error);
            }

            WriteOutput($"Found {total} mp3 files, continuing processing...", OutputMessageLevel.Normal);

            var totalArtists = 0;
            var totalAlbums = 0;

            foreach (var mp3File in mp3Files)
            {
                var tfile = TagLib.File.Create(mp3File);

                var song = tfile.Tag.Title;
                var album = tfile.Tag.Album;
                var artist = tfile.Tag.FirstPerformer;
                
                // check if artist exists,
                var matchingArtist = artists.FirstOrDefault(a => a.Name == artist);

                // If the artist does not exist, then we need to add all three data items and exit loop
                if (matchingArtist == null)
                {
                    artists.Add(new Artist
                    {
                        Name = artist,
                        Albums = new List<Album>
                {
                    new Album
                    {
                        Name = album,
                        Songs = new List<Song>
                        {
                            new Song
                            {
                                Name = song,
                                FilePath = mp3File
                            }
                        }
                    }
                }
                    });

                    WriteOutput($"ARTIST Discovered: Added {artist}, {album}, {song} to list.", OutputMessageLevel.Normal);

                    totalArtists++;
                    totalAlbums++;
                    totalSongs++;
                }
                else
                {
                    WriteOutput($"{artist} exists, checking for Albums...", OutputMessageLevel.Informational);

                    // If the artist exists, check to see if there a matching album
                    var matchingAlbum = matchingArtist.Albums.FirstOrDefault(a => a.Name == album);

                    // If the album doesn't exist, create a new one add the song and exit loop
                    if (matchingAlbum == null)
                    {
                        WriteOutput($"ALBUM Discovered: {album}, adding.", OutputMessageLevel.Normal);

                        matchingArtist.Albums.Add(new Album
                        {
                            Name = album,
                            Songs = new List<Song>
                    {
                        new Song
                        {
                            Name = song,
                            FilePath = mp3File
                        }
                    }
                        });

                        totalAlbums++;
                        totalSongs++;
                    }
                    else
                    {
                        WriteOutput($"{album} exists, checking songs...", OutputMessageLevel.Informational);

                        // if the album does exist, check if there is a matching song
                        var matchingSong = matchingAlbum.Songs.FirstOrDefault(a => a.Name == song);

                        // If the song does not exist, add it.
                        if (matchingSong == null)
                        {
                            WriteOutput($"SONG Discovered: {song}", OutputMessageLevel.Normal);

                            matchingAlbum.Songs.Add(new Song
                            {
                                Name = song,
                                FilePath = mp3File
                            });

                            totalSongs++;
                        }
                        else
                        {
                            WriteOutput($"{song} was already present in {album} for {artist}. Moving to next file.", OutputMessageLevel.Warning);
                        }
                    }
                }
            }

            WriteOutput($"Discovered: {totalArtists} artists, {totalAlbums} albums and {totalSongs} songs.", OutputMessageLevel.Success);
        }

        private void RenameAndCopyFiles(string baseDirectory)
        {
            int currentPass = 0;

            try
            {
                foreach (var artist in sourceList)
                {
                    var validArtistName = ReplaceInvalidChars(artist.Name);
                    var artistDirectory = Path.Join(baseDirectory, validArtistName);

                    if (!Directory.Exists(artistDirectory))
                    {
                        Directory.CreateDirectory(artistDirectory);

                        WriteOutput($"{artist.Name} - Created directory.", OutputMessageLevel.Normal);

                        foreach (var album in artist.Albums)
                        {
                            var validAlbumName = ReplaceInvalidChars(album.Name);
                            var albumDirectory = Path.Join(baseDirectory, validArtistName, validAlbumName);

                            if (!Directory.Exists(albumDirectory))
                            {
                                Directory.CreateDirectory(albumDirectory);

                                WriteOutput($"- Created {albumDirectory} subdirectory.", OutputMessageLevel.Informational);
                            }

                            foreach (var song in album.Songs)
                            {
                                currentPass += 1;

                                var percentComplete = (double)currentPass / totalSongs;

                                var validSongName = ReplaceInvalidChars(song.Name);

                                // Create a final file path using Artist/Album/Song format.
                                var destPath = Path.Join(baseDirectory, validArtistName, validAlbumName, $"{validSongName}.mp3");

                                if (!File.Exists(destPath))
                                {
                                    WriteOutput($" | {percentComplete:P1}% | Saving {validSongName} to {destPath}", OutputMessageLevel.Informational);

                                    File.Copy(song.FilePath, destPath);
                                }
                                else
                                {
                                    WriteOutput($"{validSongName} already existed, skipping....", OutputMessageLevel.Warning);
                                }
                            }
                        }
                    }

                    WriteOutput("Success, file copy complete.", OutputMessageLevel.Success);
                }
            }
            catch (Exception ex)
            {
                WriteOutput(ex.Message, OutputMessageLevel.Error);
            }
        }

        string ReplaceInvalidChars(string filename)
        {
            if (string.IsNullOrEmpty(filename))
            {
                filename = "MISSING";
                Console.WriteLine(@"----------------------------------------------------------------------------------");
                Console.WriteLine(@"|*** ALERT - input string was null, check artist/album/song for 'MISSING' name ***|");
                Console.WriteLine(@"----------------------------------------------------------------------------------");
            }

            return string.Join("_", filename.Split(Path.GetInvalidFileNameChars()));
        }

        private void Reset()
        {
            SourceFolderTextBox.Text = string.Empty;
            DestinationFolderTextBox.Text = string.Empty;

            sourceList.Clear();
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
    }
}
