namespace MediaFileManager.Common.Models.AudioBook;

public class AudiobookFile : BindableBase
{
    private string fileName;
    private string title;
    private string album;
    private string artist;
    private string performer;
    private string filePath;

    public string FileName
    {
        get => fileName;
        set => SetProperty(ref fileName, value);
    }

    public string Title
    {
        get => title;
        set => SetProperty(ref title, value);
    }

    public string Album
    {
        get => album;
        set => SetProperty(ref album, value);
    }

    public string Artist
    {
        get => artist;
        set => SetProperty(ref artist, value);
    }

    public string Performer
    {
        get => performer;
        set => SetProperty(ref performer, value);
    }

    public string FilePath
    {
        get => filePath;
        set => SetProperty(ref filePath, value);
    }
}
