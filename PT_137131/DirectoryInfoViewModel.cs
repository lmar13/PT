using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PT_137131
{
    public class DirectoryInfoViewModel: FileSystemInfoViewModel
    {
        private FileSystemWatcher watcher;
        private string path;
        private string imageSource = "Resources/folder.png";

        public string ImageSource { get => imageSource; private set { } }
        public ObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new ObservableCollection<FileSystemInfoViewModel>();
        public Exception Exception { get; private set; }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            //Handling for multithread execution
            Application.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
        }

        public bool Open(string path)
        {
            this.path = path;
            bool result = false;
            try
            {
                Items.Clear();
                ReadDirectories();
                ReadFiles();
                result = true;
            }
            catch (Exception ex)
            {
                Exception = ex;
            }

            if (result) InitlizeWatcher();

            return result;
        }

        private void ReadDirectories()
        {
            foreach (var dirName in Directory.GetDirectories(path))
            {
                var dirInfo = new DirectoryInfo(dirName);
                DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel();
                itemViewModel.Model = dirInfo;
                Items.Add(itemViewModel);

                //recurrecny load
                itemViewModel.Open(dirName);
            }
        }

        private void ReadFiles()
        {
            foreach (var fileName in Directory.GetFiles(path))
            {
                var fileInfo = new FileInfo(fileName);
                FileInfoViewModel itemViewModel = new FileInfoViewModel();
                itemViewModel.Model = fileInfo;
                Items.Add(itemViewModel);
            }
        }

        private void InitlizeWatcher()
        {
            watcher = new FileSystemWatcher(path);
            watcher.Created += OnFileSystemChanged;
            watcher.Renamed += OnFileSystemChanged;
            watcher.Deleted += OnFileSystemChanged;
            watcher.Changed += OnFileSystemChanged;

            watcher.Error += OnWatcherError;
            watcher.EnableRaisingEvents = true;

        }

        private void OnWatcherError(object sender, ErrorEventArgs e)
        {
            MessageBox.Show(e.ToString());
        }

        private void OnFileSystemChanged(FileSystemEventArgs e)
        {
            Open(path);
        }
    }
}
