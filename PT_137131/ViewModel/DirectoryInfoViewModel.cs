﻿using PT_137131.Enum;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PT_137131.ViewModel
{
    public class DirectoryInfoViewModel : FileSystemInfoViewModel
    {
        private FileSystemWatcher? watcher;
        private string? path;
        private string imageSource = "Resources/folder.png";
        private CancellationToken token;

        public DirectoryInfoViewModel(ViewModelBase owner): base(owner) 
        {
            Items.CollectionChanged += Items_CollectionChanged;
        }

        private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var item in args.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged += Item_PropertyChanged;
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var item in args.NewItems.Cast<FileSystemInfoViewModel>())
                    {
                        item.PropertyChanged -= Item_PropertyChanged;
                    }
                    break;
            }
        }

        private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "StatusMessage" && sender is FileSystemInfoViewModel viewModel)
            {
                this.StatusMessage = viewModel.StatusMessage;
            }

            if (args.PropertyName == "CurrentMaxThread" && sender is FileSystemInfoViewModel viewModelThread)
            {
                CurrentMaxThread = viewModelThread.CurrentMaxThread;
            }
        }

        public new FileSystemInfo? Model
        {
            get { return fileSystemInfo; }
            set
            {
                if (fileSystemInfo != value && value != null)
                {
                    SetProperties(value);

                    DirectoryInfo directoryInfo = new DirectoryInfo(value.FullName);
                    Size = directoryInfo.GetDirectories().Length + directoryInfo.GetFiles().Length;

                    NotifyPropertyChanged();
                }
            }
        }

        public bool IsInitialized
        {
            get
            {
                if (path == null) return false;
                if (path.Length == 0) return false;

                return true;
            }
        }

        public new string ImageSource { get => imageSource; private set { } }

        public DispatchedObservableCollection<FileSystemInfoViewModel> Items { get; private set; } = new DispatchedObservableCollection<FileSystemInfoViewModel>();
        public Exception? Exception { get; private set; }

        public void OnFileSystemChanged(object sender, FileSystemEventArgs e)
        {
            //Handling for multithread execution
            Application.Current.Dispatcher.Invoke(() => OnFileSystemChanged(e));
        }

        public bool Open(string path, CancellationToken token)
        {
            this.token = token;
            this.path = path;

            try
            {
                Items.Clear();
                ReadCatalogs(token);
                ReadFiles(token);
            }
            catch (OperationCanceledException exception)
            {
                StatusMessage = Strings.Cancelled_Operation;
                return false;
            }
            finally
            {
                InitlizeWatcher();
            }

            return true;
        }

        public void Sort(SortingViewModel sortingViewModel, CancellationToken token)
        {
            if (token.IsCancellationRequested) return;
            
            bool isEmpty = !IsInitialized;
            if (isEmpty) return;

            List<Task> tasks = new List<Task>();

            foreach(var item in Items)
            {
                Task task = null;

                if (item is DirectoryInfoViewModel directoryItem)
                {
                    task = Task.Factory.StartNew(() =>
                    {
                        if (token.IsCancellationRequested)
                        {
                            token.ThrowIfCancellationRequested();
                        }

                        var threadId = Thread.CurrentThread.ManagedThreadId;

                        if (CurrentMaxThread < threadId)
                        {
                            CurrentMaxThread = threadId;
                        }

                        Debug.WriteLine("Thread id: " + threadId);
                        Debug.WriteLine("Sorting directory: " + item?.Model?.Name);
                        directoryItem?.Sort(sortingViewModel, token);
                        Debug.WriteLine("Completed: " + directoryItem?.Model?.Name);
                    }, token, sortingViewModel.TaskCreationOptions, TaskScheduler.Default);

                    if (item?.Model?.FullName != null)
                    {
                        StatusMessage = Strings.Created_Sorting + " " + item.Model.FullName;
                    }
                }

                if (task != null) tasks.Add(task);
            }

            try
            {
                Task.WaitAll(tasks.ToArray());
            }
            catch (AggregateException ex)
            {
                StatusMessage = Strings.Cancelled_Operation;
                return;
            }
            catch(OperationCanceledException)
            {
                StatusMessage = Strings.Cancelled_Operation;
                return;
            }

            if (token.IsCancellationRequested) return;

            var orderableItems = Items.OrderBy(OrderByType);

            if (sortingViewModel.Direction == Direction.Ascending)
            {
                if (sortingViewModel.SortBy == SortBy.Alphabetical)
                {
                    orderableItems = orderableItems.ThenBy(item => item.Caption);
                }

                if (sortingViewModel.SortBy == SortBy.ModificationDate)
                {
                    orderableItems = orderableItems.ThenBy(item => item.LastWriteTime);
                }

                if (sortingViewModel.SortBy == SortBy.Extension)
                {
                    orderableItems = orderableItems.ThenBy(item => item.Extension);
                }

                if (sortingViewModel.SortBy == SortBy.Size)
                {
                    orderableItems = orderableItems.ThenBy(item => item.Size);
                }
            }

            if (sortingViewModel.Direction == Direction.Descending)
            {
                if (sortingViewModel.SortBy == SortBy.Alphabetical)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Caption);
                }

                if (sortingViewModel.SortBy == SortBy.ModificationDate)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.LastWriteTime);
                }

                if (sortingViewModel.SortBy == SortBy.Extension)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Extension);
                }

                if (sortingViewModel.SortBy == SortBy.Size)
                {
                    orderableItems = orderableItems.ThenByDescending(item => item.Size);
                }
            }

            var itemList = orderableItems.ToList();

            foreach (var item in itemList)
            {
                var newIndex = itemList.IndexOf(item);
                var oldIndex = Items.IndexOf(item);
                if (newIndex > -1)
                {
                    Items.Move(oldIndex, newIndex);
                }
            }
        }

        private int OrderByType(ViewModelBase item)
        {
            if (item is DirectoryInfoViewModel)
            {
                return 0;
            }
            return 1;
        }

        private void ReadCatalogs(CancellationToken token)
        {
            foreach (var dirName in Directory.GetDirectories(path))
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    var dirInfo = new DirectoryInfo(dirName);
                    DirectoryInfoViewModel itemViewModel = new DirectoryInfoViewModel(this);
                    itemViewModel.Model = dirInfo;
                    Items.Add(itemViewModel);

                    //recurrecny load
                    itemViewModel.Open(dirName, token);
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
        }

        private void ReadFiles(CancellationToken token)
        {
            if (path == null) return;
            foreach (var fileName in Directory.GetFiles(path))
            {
                try
                {
                    if (token.IsCancellationRequested)
                    {
                        token.ThrowIfCancellationRequested();
                    }

                    var fileInfo = new FileInfo(fileName);
                    FileInfoViewModel itemViewModel = new FileInfoViewModel(this);
                    itemViewModel.Model = fileInfo;
                    Items.Add(itemViewModel);
                }
                catch (OperationCanceledException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
        }

        private void InitlizeWatcher()
        {
            if (path == null) return;
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
            if (e.ChangeType == WatcherChangeTypes.Changed) return;
            if (path != null)
            {
                try
                {
                    Open(path, token);
                }
                catch (Exception ex)
                {
                    // do nothing
                }
            }
        }
    }
}
