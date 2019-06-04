using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MediaFileManager.Desktop.Models
{
    public class WorkerResultParameter
    {
        public int PercentComplete { get; set; }
        public string BusyMessage { get; set; }
        public string FileName { get; set; }
        public bool IsPreview { get; set; }
    }
}
