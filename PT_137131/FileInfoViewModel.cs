using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PT_137131
{
    public class FileInfoViewModel : FileSystemInfoViewModel
    {
        private Dictionary<string, string> imageSource = new Dictionary<string, string>();

        public string ImageSource
        {
            get
            {
                string extension = base.Model.Extension;
                string source;

                imageSource.TryGetValue(extension, out source);
                if (source == null) return "Resources/file.png";

                return source;
            }
            private set { }
        }

        public FileInfoViewModel()
        {
            imageSource.Add(".txt", "Resources/txt.png");
            imageSource.Add(".pdf", "Resources/pdf.png");
        }
    }
}
