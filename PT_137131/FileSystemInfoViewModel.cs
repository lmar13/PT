using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Windows.Input;

namespace PT_137131
{
    public class FileSystemInfoViewModel: ViewModelBase
    {
        private FileSystemInfo fileSystemInfo;
        private DateTime lastWriteTime;
        private string caption;
        private string imageSource;
        private readonly ICommand createCommand;
        private readonly ICommand deleteCommand;

        public FileSystemInfoViewModel()
        {
            this.createCommand = new RelayCommand(this.CreateFile);
            this.deleteCommand = new RelayCommand(this.DeleteFile);
        }

        public FileSystemInfo Model
        {
            get { return fileSystemInfo; }
            set
            {
                if (fileSystemInfo != value)
                {
                    fileSystemInfo = value;
                    LastWriteTime = value.LastWriteTime;
                    Caption = value.Name;
                    NotifyPropertyChanged();
                }
            }
        }

        public string ImageSource
        {
            get { return imageSource; }
            set
            {
                if (imageSource != value)
                {
                    imageSource = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DateTime LastWriteTime
        {
            get { return lastWriteTime; }
            set
            {
                if (lastWriteTime != value)
                {
                    lastWriteTime = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string Caption
        {
            get { return caption; }
            set
            {
                if (caption != value)
                {
                    caption = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ICommand OnCreate
        {
            get
            {
                return this.createCommand;
            }
            private set { }
        }

        public ICommand OnDelete
        {
            get
            {
                return this.deleteCommand;
            }
            private set { }
        }

        private void CreateFile(object param)
        {
            CreateDialog dialog = new CreateDialog();
            dialog.ShowDialog();

            if (dialog.Cancel) return;

            var path = Path.GetFullPath(fileSystemInfo.FullName);
            if (fileSystemInfo is FileInfo) path = path.Replace("\\" + fileSystemInfo.Name, "");

            try
            {
                var newFileName = dialog.fileName.Text;
                var newPath = path + Path.DirectorySeparatorChar + newFileName;

                if (dialog.directoryType.IsChecked != null ? (bool)dialog.directoryType.IsChecked : false)
                {
                    if (!Directory.Exists(newPath))
                    {
                        Directory.CreateDirectory(newPath);
                    }
                        
                }    
                else
                {
                    if (!File.Exists(newPath))
                    {
                        File.Create(newPath);
                    }
                }
                    
                SetFileAttributes(newPath, dialog);
                NotifyPropertyChanged();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show(ex.Message);
            }
        }

        private void DeleteFile(object param)
        {
            if (fileSystemInfo is FileInfo) File.Delete(fileSystemInfo.FullName);
            else if (fileSystemInfo is DirectoryInfo) Directory.Delete(fileSystemInfo.FullName, true);
            
            NotifyPropertyChanged();
        }

        private void SetFileAttributes(string path, CreateDialog dialog)
        {
            if (dialog.readOnlyAtt.IsChecked != null ? (bool)dialog.readOnlyAtt.IsChecked : false)
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.ReadOnly);

            if (dialog.archiveAtt.IsChecked != null ? (bool)dialog.archiveAtt.IsChecked : false)
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Archive);

            if (dialog.hiddenAtt.IsChecked != null ? (bool)dialog.hiddenAtt.IsChecked : false)
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.Hidden);

            if (dialog.systemAtt.IsChecked != null ? (bool)dialog.systemAtt.IsChecked : false)
                File.SetAttributes(path, File.GetAttributes(path) | FileAttributes.System);
        }
    }
}
