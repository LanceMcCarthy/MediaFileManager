using System.Collections.Generic;

namespace MediaFileManager.Desktop.Models;

public class WorkerParameters(List<string> selectedEpisodes)
{
    public List<string> SelectedEpisodes { get; } = selectedEpisodes;

    public bool IsPreview { get; set; }

    public int SeasonNumber { get; set; }

    public int EpisodeNumberStart { get; set; }

    public int EpisodeNumberEnd { get; set; }

    public int SelectedTextLength { get; set; }
}
