namespace MediaFileManager.Desktop.Models.Audio
{
    public class Song : BindableBase
    {
        private string name;
        private string filePath;

        public string Name
        {
            get => name;
            set => SetProperty(ref name, value);
        }

        public string FilePath 
        { 
            get => filePath; 
            set => SetProperty(ref filePath, value);
        }
    }
}
