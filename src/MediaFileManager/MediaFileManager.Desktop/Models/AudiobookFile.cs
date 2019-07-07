namespace MediaFileManager.Desktop.Models
{
    public class AudiobookFile : BindableBase
    {
        private string _fileName;
        private string _title;
        private string _album;
        private string _artist;
        private string _filePath;

        public string FileName
        {
            get => _fileName;
            set => SetProperty(ref _fileName, value);
        }

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }

        public string Album
        {
            get => _album;
            set => SetProperty(ref _album, value);
        }

        public string Artist
        {
            get => _artist;
            set => SetProperty(ref _artist, value);
        }

        public string FilePath
        {
            get => _filePath;
            set => SetProperty(ref _filePath, value);
        }
    }
}
