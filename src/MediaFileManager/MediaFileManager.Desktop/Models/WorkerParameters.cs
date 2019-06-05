using System.Collections.Generic;

namespace MediaFileManager.Desktop.Models
{
    public class WorkerParameters
    {
        public bool IsPreview { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumberStart { get; set; }
        public int EpisodeNumberEnd { get; set; }
        public int SelectedTextLength { get; set; }
        public List<string> SelectedEpisodes { get; set; }
    }
}
