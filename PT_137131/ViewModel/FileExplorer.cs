using PT_137131.DialogWindow;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Input;

namespace PT_137131.ViewModel
{
    public class FileExplorer : ViewModelBase
    {
        private DirectoryInfoViewModel root;
        private SortingViewModel sorting;

        public static readonly string[] TextFilesExtensions = new string[] { ".txt", ".ini", ".log" };
        public event EventHandler<FileInfoViewModel> OnOpenFileRequest;
        public event EventHandler<FileInfoViewModel> OnCreateFileRequest;
        public event EventHandler<FileInfoViewModel> OnDeleteFileRequest;

        public ICommand OpenRootFolderCommand { get; private set; }
        public ICommand SortRootFolderCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand CreateFileCommand { get; private set; }
        public ICommand DeleteFileCommand { get; private set; }

        public DirectoryInfoViewModel Root
        {
            get => root;
            set
            {
                if (value != null) root = value;
                NotifyPropertyChanged();
            }
        }

        public SortingViewModel Sorting
        {
            get { return sorting; }
            set { if (value != null) sorting = value; NotifyPropertyChanged(); }
        }

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

        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));

            root = new DirectoryInfoViewModel(this);
            NotifyPropertyChanged(nameof(Root));

            sorting = new SortingViewModel();
            sorting.SortBy = Enum.SortBy.Alphabetical;
            sorting.Direction = Enum.Direction.Ascending;
            NotifyPropertyChanged(nameof(Sorting));

            Sorting.PropertyChanged += OnSortingPropertyChanged;

            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecute);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanExecuteSort);
            ExitCommand = new RelayCommand(ExitExecute);

            OpenFileCommand = new RelayCommand(OnOpenFileCommand, OpenFileExecute);

            CreateFileCommand = new RelayCommand(OnCreateFileCommand);
            DeleteFileCommand = new RelayCommand(OnDeleteFileCommand);
        }

        public string GetFileContent(FileInfoViewModel viewModel)
        {
            var extension = viewModel.Extension?.ToLower();
            if (TextFilesExtensions.Contains(extension))
            {
                return GetTextFileContent(viewModel);
            }
            return "";
        }

        private string GetTextFileContent(FileInfoViewModel viewModel)
        {
            string result = "";

            using(var textReader = File.OpenText(viewModel.Model.FullName))
            {
                result = textReader.ReadToEnd();
            }
            return result;
        }

        private void OnOpenFileCommand(object obj)
        {
            if (obj is not FileInfoViewModel) return;
            FileInfoViewModel viewModel = (FileInfoViewModel)obj;
            OnOpenFileRequest.Invoke(this, viewModel);
        }

        private bool OpenFileExecute(object parameter)
        {
            if (parameter is FileInfoViewModel viewModel)
            {
                var extension = viewModel.Extension?.ToLower();
                return TextFilesExtensions.Contains(extension);
            }
            return false;
        }

        private void OnCreateFileCommand(object obj)
        {
            Trace.WriteLine(obj.ToString());
            if (obj is not FileInfoViewModel) return;
            FileInfoViewModel viewModel = (FileInfoViewModel)obj;
            OnCreateFileRequest.Invoke(this, viewModel);
            
            
        }

        public void CreateFile(FileInfoViewModel viewModel)
        {
            CreateDialog dialog = new CreateDialog();
            dialog.ShowDialog();

            if (dialog.Cancel) return;

            var path = Path.GetFullPath(viewModel.Model.FullName);
            if (viewModel.Model is FileInfo) path = path.Replace("\\" + viewModel.Model.Name, "");

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

        private void OnDeleteFileCommand(object obj)
        {
            if (obj is not FileInfoViewModel) return;
            FileInfoViewModel viewModel = (FileInfoViewModel)obj;
            OnDeleteFileRequest.Invoke(this, viewModel);
        }

        public void DeleteFile(FileInfoViewModel viewModel)
        {
            if (viewModel.Model is FileInfo) File.Delete(viewModel.Model.FullName);
            else if (viewModel.Model is DirectoryInfo) Directory.Delete(viewModel.Model.FullName, true);

            NotifyPropertyChanged();
        }

        private void OnSortingPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            Root.Sort(Sorting);
        }

        private void ExitExecute(object parameter)
        {
            if (parameter == null) return;  

            if (parameter is not Window)
            {
                throw new ArgumentException("Not valid parameter passed into exit command");
            }

            Window window = (Window)parameter;
            window.Close();
        }

        private void OpenRootFolderExecute(object parameter)
        {
            var dlg = new FolderBrowserDialog() { Description = Strings.Directory_Description };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                var path = dlg.SelectedPath;
                OpenRoot(path);
            }

            Root.Sort(Sorting);
        }

        private void OpenRoot(string path)
        {
            Root.Open(path);
        }

        private void SortRootFolderExecute(object parameter)
        {
            Window window = new SortingDialog(Sorting);
            window.ShowDialog();
        }

        private bool CanExecuteSort(object parameter)
        {
            return root.IsInitialized;
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
