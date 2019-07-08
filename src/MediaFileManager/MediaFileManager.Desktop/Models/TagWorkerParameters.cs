using System.Collections.Generic;

namespace MediaFileManager.Desktop.Models
{
    public class TagWorkerParameters
    {
        public IList<AudiobookFile> AudiobookFiles { get; set; }

        public bool? UpdateTitle { get; set; }

        public bool? UpdateArtistName { get; set; }

        public bool? UpdateAlbumName { get; set; }
    }
}
