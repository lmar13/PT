using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PT_137131
{
    public class FileExplorer : ViewModelBase
    {
        public string Lang
        {
            get { return CultureInfo.CurrentUICulture.TwoLetterISOLanguageName; }
            set
            {
                if (value != null)
                    if (CultureInfo.CurrentUICulture.TwoLetterISOLanguageName != value)
                    {
                        CultureInfo.CurrentUICulture = new CultureInfo(value);
                        NotifyPropertyChanged();
                    }
            }
        }

        private DirectoryInfoViewModel root;
        public DirectoryInfoViewModel Root
        {
            get => root;
            set
            {
                if (value != null) root = value;
            }
        }

        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));
            root = new DirectoryInfoViewModel();
        }

        public void OpenRoot(string path)
        {
            Root.Open(path);
        }
    }
}
