namespace MediaFileManager.Common.Models;

public class WorkerProgress
{
    public int PercentComplete { get; set; }
    public string BusyMessage { get; set; }
    public string FileName { get; set; }
    public bool IsPreview { get; set; }
}
