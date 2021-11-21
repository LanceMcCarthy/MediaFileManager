using System.Collections.Generic;

namespace MediaFileManager.Desktop.Models.Audio
{
    public class Album : BindableBase
    {
        private string name;
        private List<Song> songs;

        public string Name 
        { 
            get => name; 
            set => SetProperty(ref name, value); 
        }

        public List<Song> Songs 
        { 
            get => songs; 
            set => SetProperty(ref songs, value); 
        }
    }
}
