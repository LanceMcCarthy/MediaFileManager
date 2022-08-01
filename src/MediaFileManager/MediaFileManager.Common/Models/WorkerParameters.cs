namespace MediaFileManager.Common.Models;

public class WorkerParameters
{
    public WorkerParameters(List<string> selectedEpisodes)
    {
        SelectedEpisodes = selectedEpisodes;
    }

    public List<string> SelectedEpisodes { get; }

    public bool IsPreview { get; set; }

    public int SeasonNumber { get; set; }

    public int EpisodeNumberStart { get; set; }

    public int EpisodeNumberEnd { get; set; }

    public int SelectedTextLength { get; set; }
}
