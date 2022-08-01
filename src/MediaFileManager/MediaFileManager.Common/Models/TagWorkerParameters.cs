using MediaFileManager.Common.Models.AudioBook;

namespace MediaFileManager.Common.Models
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
