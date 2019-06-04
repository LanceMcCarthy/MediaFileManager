using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFileManager.Desktop.Models
{
    public class WorkerParameter
    {
        public bool IsPreview { get; set; }
        public int SeasonNumber { get; set; }
        public int EpisodeNumberStart { get; set; }
        public int EpisodeNumberEnd { get; set; }
        public int SelectedTextLength { get; set; }

        public List<string> SelectedEpisodes { get; set; }

    }
}
