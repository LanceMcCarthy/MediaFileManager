using MediaFileManager.Desktop.Models.AudioBook;
using System.Collections.Generic;

namespace MediaFileManager.Desktop.Models
{
    public class TagWorkerParameters(IList<AudiobookFile> audiobookFiles)
    {
        public IList<AudiobookFile> AudiobookFiles { get; } = audiobookFiles;

        public bool? UpdateTitle { get; set; }

        public bool? UpdateArtistName { get; set; }

        public bool? UpdateAlbumName { get; set; }
    }
}
