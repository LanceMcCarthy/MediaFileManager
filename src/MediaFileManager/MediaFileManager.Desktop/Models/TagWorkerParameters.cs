using System.Collections.Generic;
using MediaFileManager.Desktop.Models.AudioBook;

namespace MediaFileManager.Desktop.Models
{
    public class TagWorkerParameters
    {
        public TagWorkerParameters(IList<AudiobookFile> audiobookFiles)
        {
            AudiobookFiles = audiobookFiles;
        }

        public IList<AudiobookFile> AudiobookFiles { get; }

        public bool? UpdateTitle { get; set; }

        public bool? UpdateArtistName { get; set; }

        public bool? UpdateAlbumName { get; set; }
    }
}
