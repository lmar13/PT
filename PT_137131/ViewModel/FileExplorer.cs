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
using System.Threading;
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
        private string statusMessage;
        private int currentMaxThread;
        private CancellationTokenSource source;

        public static readonly string[] TextFilesExtensions = new string[] { ".txt", ".ini", ".log" };
        public event EventHandler<FileInfoViewModel> OnOpenFileRequest;
        public event EventHandler<FileInfoViewModel> OnCreateFileRequest;
        public event EventHandler<FileInfoViewModel> OnDeleteFileRequest;
        public event EventHandler<FileInfoViewModel> OnSelectFileRequest;

        public ICommand OpenRootFolderCommand { get; private set; }
        public ICommand SortRootFolderCommand { get; private set; }
        public ICommand ExitCommand { get; private set; }
        public ICommand OpenFileCommand { get; private set; }
        public ICommand CreateFileCommand { get; private set; }
        public ICommand DeleteFileCommand { get; private set; }
        public ICommand SelecFileCommand { get; private set; }
        public ICommand OnCancelOperationCommand { get; private set; }

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

        public string StatusMessage
        {
            get { return statusMessage; }
            set
            {
                if (value != null && statusMessage != value)
                {
                    statusMessage = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int CurrentMaxThread
        {
            get { return currentMaxThread; }
            set
            {
                if (currentMaxThread != value)
                {
                    currentMaxThread = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public FileExplorer()
        {
            NotifyPropertyChanged(nameof(Lang));

            root = new DirectoryInfoViewModel(this);
            root.PropertyChanged += Root_PropertyChanged;
            NotifyPropertyChanged(nameof(Root));

            sorting = new SortingViewModel();
            sorting.SortBy = Enum.SortBy.Alphabetical;
            sorting.Direction = Enum.Direction.Ascending;
            NotifyPropertyChanged(nameof(Sorting));

            Sorting.PropertyChanged += OnSortingPropertyChangedAsync;

            OpenRootFolderCommand = new RelayCommand(OpenRootFolderExecuteAsync);
            SortRootFolderCommand = new RelayCommand(SortRootFolderExecute, CanExecuteSort);
            ExitCommand = new RelayCommand(ExitExecute);

            OpenFileCommand = new RelayCommand(OnOpenFileCommand, OpenFileExecute);

            CreateFileCommand = new RelayCommand(OnCreateFileCommand);
            DeleteFileCommand = new RelayCommand(OnDeleteFileCommand);
            SelecFileCommand = new RelayCommand(OnSelectFileCommand);

            OnCancelOperationCommand = new RelayCommand(OnCancelClicked);
        }

        private void OnCancelClicked(object obj)
        {
            if (source != null)
            {
                try
                {
                    source.Cancel();
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
        }

        private void Root_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "StatusMessage" && sender is FileSystemInfoViewModel viewModel)
            {
                this.StatusMessage = viewModel.StatusMessage;
            }
            
            if (args.PropertyName == "CurrentMaxThread" && sender is FileSystemInfoViewModel viewModelThread)
            {
                this.currentMaxThread = viewModelThread.CurrentMaxThread;
            }
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

        public string GetFileAttributes(FileInfoViewModel viewModel)
        {
            FileAttributes fileAttribute = File.GetAttributes(viewModel.Model.FullName);
            return ConvertFileAttributesToRashString(fileAttribute);
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

        private void OnSelectFileCommand(object obj)
        {
            if (obj is not FileInfoViewModel) return;
            FileInfoViewModel viewModel = (FileInfoViewModel)obj;
            OnSelectFileRequest.Invoke(this, viewModel);
        }

        private async void OnSortingPropertyChangedAsync(object? sender, PropertyChangedEventArgs e)
        {
            source = new CancellationTokenSource();

            await Task.Factory.StartNew(() =>
            {
                try
                {
                    Root.Sort(Sorting, source.Token);
                    Debug.WriteLine("Max thred id: " + CurrentMaxThread);
                    Debug.WriteLine("=================");
                    Root.CurrentMaxThread = 0;
                }
                catch(Exception)
                {
                    StatusMessage = Strings.Cancelled_Operation;
                }
                finally
                {
                    source.Dispose();
                    source = new CancellationTokenSource();
                }
            }, source.Token);
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

        private async void OpenRootFolderExecuteAsync(object parameter)
        {
            source = new CancellationTokenSource();

            var dlg = new FolderBrowserDialog() { Description = Strings.Directory_Description };
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    await Task.Factory.StartNew(() =>
                    {
                        StatusMessage = Strings.Loading;
                        var path = dlg.SelectedPath;
                        bool result = Root.Open(path, source.Token);
                        if (result) StatusMessage = Strings.Ready;
                    }, source.Token);
                }
                catch(OperationCanceledException)
                {
                    StatusMessage = Strings.Cancelled_Operation;
                }
                finally
                {
                    source.Dispose();
                    source = new CancellationTokenSource();
                }
            }
        }

        private void OpenRoot(string path, CancellationToken token)
        {
            //Root.Open(path, token);
        }

        private void SortRootFolderExecute(object parameter)
        {
            Window window = new SortingDialog(Sorting);
            window.Title = Strings.SortingDialog;
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

        private string ConvertFileAttributesToRashString(FileAttributes fileAttribute)
        {
            string rash = "";

            if (fileAttribute.HasFlag(FileAttributes.ReadOnly)) rash += "r";
            if (fileAttribute.HasFlag(FileAttributes.Archive)) rash += "a";
            if (fileAttribute.HasFlag(FileAttributes.System)) rash += "s";
            if (fileAttribute.HasFlag(FileAttributes.Hidden)) rash += "h";
            else rash += "-";

            return rash;
        }
    }
}
