using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PT_137131.ViewModel
{
    public class FileInfoViewModel : FileSystemInfoViewModel
    {
        private Dictionary<string, string> imageSource = new Dictionary<string, string>();
        public ICommand OpenFileCommand { get; private set; }
        public ICommand CreateFileCommand { get; private set; }
        public ICommand DeleteFileCommand { get; private set; }
        public ICommand SelectFileCommand { get; private set; }

        public new string ImageSource
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

        public FileInfoViewModel(ViewModelBase owner): base(owner)
        {
            imageSource.Add(".txt", "Resources/txt.png");
            imageSource.Add(".pdf", "Resources/pdf.png");

            OpenFileCommand = new RelayCommand(OnOpenFileCommand, CanExecuteOnOpenFileCommand);
            CreateFileCommand = new RelayCommand(OnCreateFileCommand);
            DeleteFileCommand = new RelayCommand(OnDeleteFileCommand);
            SelectFileCommand = new RelayCommand(OnSelectFileCommand);
        }

        private bool CanExecuteOnOpenFileCommand(object obj)
        {
            return OwnerExplorer.OpenFileCommand.CanExecute(obj);
        }

        private void OnOpenFileCommand(object obj)
        {
            OwnerExplorer.OpenFileCommand.Execute(obj);
        }

        private void OnCreateFileCommand(object obj)
        {
            OwnerExplorer.CreateFileCommand.Execute(obj);
        }

        private void OnDeleteFileCommand(object obj)
        {
            OwnerExplorer.DeleteFileCommand.Execute(obj);
        }

        private void OnSelectFileCommand(object obj)
        {
            OwnerExplorer.SelecFileCommand.Execute(obj);
        }
    }
}
