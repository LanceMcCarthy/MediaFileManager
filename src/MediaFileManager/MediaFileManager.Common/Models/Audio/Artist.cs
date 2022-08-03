namespace MediaFileManager.Common.Models.Audio;

public class Artist : BindableBase
{
    private string name;
    private List<Album> albums;

    public string Name
    {
        get => name;
        set => SetProperty(ref name, value);
    }

    public List<Album> Albums
    {
        get => albums;
        set => SetProperty(ref albums, value);
    }
}
