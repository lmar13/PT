using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Forms;
using System.IO;
using System.Diagnostics;
using System.ComponentModel;
using System.Globalization;
using PT_137131.ViewModel;

namespace PT_137131
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private FileExplorer fileExplorer;
        public MainWindow()
        {
            InitializeComponent();
            fileExplorer = new FileExplorer();
            DataContext = fileExplorer;
            fileExplorer.PropertyChanged += OnFileExplorerPropertyChanged;
            fileExplorer.OnOpenFileRequest += OnOpenFileRequest;
            fileExplorer.OnCreateFileRequest += OnCreateFileRequest;
            fileExplorer.OnDeleteFileRequest += OnDeleteFileRequest;
            fileExplorer.OnSelectFileRequest += OnSelectFileRequest;
        }

        private void OnOpenFileRequest(object? sender, FileInfoViewModel e)
        {
            var content = fileExplorer.GetFileContent(e);
            if (content is string text)
            {
                textBlock.Text = text;
            }
        }

        private void OnCreateFileRequest(object? sender, FileInfoViewModel e)
        {
            fileExplorer.CreateFile(e);
        }

        private void OnDeleteFileRequest(object? sender, FileInfoViewModel e)
        {
            fileExplorer.DeleteFile(e);
        }

        private void OnSelectFileRequest(object? sender, FileInfoViewModel e)
        {
            var content = fileExplorer.GetFileAttributes(e);
            if (content is string text)
            {
                fileAttText.Text = text;
            }
        }

        //private void OnOpenCommand(object sender, RoutedEventArgs e)
        //{
        //    using var dialog = new FolderBrowserDialog() { Description = "Select directory to open" };
        //    if (dialog.ShowDialog() == System.Windows.Forms.DialogResult.OK)
        //    {
        //        var path = dialog.SelectedPath;
        //        fileExplorer.OpenRoot(path);
        //        DataContext = fileExplorer;
        //    }
        //}

        private void OnCloseCommand(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void OnFileExplorerPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FileExplorer.Lang))
                CultureResources.ChangeCulture(CultureInfo.CurrentUICulture);
        }

        //private void OnFolderExpanded(object sender, RoutedEventArgs e)
        //{
        //    if (sender is not TreeViewItem item) return;
        //    CreateTreeView(item);
        //}

        //private void OnFileClicked(object sender, MouseButtonEventArgs e)
        //{
        //    if (sender is not TreeViewItem item) return;
        //    var path = (string)item.Tag;
        //    var fileExtension = new FileInfo(path).Extension;
        //    if (fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase)){
        //        using var textReader = File.OpenText(path);
        //        string text = textReader.ReadToEnd();

        //        textBlock.Text = text;
        //    }
        //    else
        //    {
        //        System.Windows.MessageBox.Show("Unsupported file format.");
        //    }
        //}

        //private void OnItemSelected(object sender, RoutedEventArgs e)
        //{
        //    Trace.WriteLine(sender);
        //    if (sender is not TreeViewItem item)
        //    {
        //        Trace.WriteLine("not a TreeViewItem");
        //        return;
        //    }
        //    var path = (string)item.Tag;
            

        //    FileAttributes fileAttribute = File.GetAttributes(path);
        //    fileAttText.Text = ConvertFileAttributesToRashString(fileAttribute);

        //    e.Handled = true;
        //}

        //private void OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        //{
        //    FileSystemInfoViewModel item = (FileSystemInfoViewModel)e.NewValue;
        //    if (item == null) return; 
        //    fileAttText.Text = ConvertFileAttributesToRashString(item.Model.Attributes);

        //    if (!File.Exists(item.Model.FullName)) return;

        //    var fileExtension = item.Model.Extension;
        //    if (fileExtension.Equals(".txt", StringComparison.OrdinalIgnoreCase))
        //    {
        //        using var textReader = File.OpenText(item.Model.FullName);
        //        string text = textReader.ReadToEnd();
        //        textBlock.Text = text;
        //    }
        //    else
        //    {
        //        System.Windows.MessageBox.Show("Unsupported file format.");
        //    }
        //}

        //private void CreateTreeView(TreeViewItem item)
        //{
        //    if (item.Items.Count == 1)
        //    {
        //        item.Items.Clear();
        //        try
        //        {
        //            var path = (string)item.Tag;

        //            string[] fileEntries = Directory.GetFiles(path);
        //            foreach (string file in fileEntries)
        //                item.Items.Add(ProcessItem(file, new FileInfo(file).Name));

        //            string[] subDirectories = Directory.GetDirectories(path);
        //            foreach (string subDirectory in subDirectories)
        //                item.Items.Add(ProcessItem(subDirectory, new DirectoryInfo(subDirectory).Name));
        //        }
        //        catch (Exception) { }
        //    }
        //}

        //private TreeViewItem ProcessItem(string path, string name)
        //{
        //    TreeViewItem newItem = new TreeViewItem();
        //    newItem.Header = name;
        //    newItem.Tag = path;
        //    if (Directory.Exists(path)) newItem.Items.Add(new TreeViewItem());

        //    AddListenersForItem(newItem, path);
        //    CreateContextMenuForItem(newItem);

        //    return newItem;
        //}

        //private void AddListenersForItem(TreeViewItem item, string path)
        //{
        //    item.Selected += new RoutedEventHandler(OnItemSelected);
        //    if (File.Exists(path)) item.MouseDoubleClick += new MouseButtonEventHandler(OnFileClicked);
        //    //else if (Directory.Exists(path)) item.Expanded += new RoutedEventHandler(OnFolderExpanded);
        //}

        //private void CreateContextMenuForItem(TreeViewItem item)
        //{
        //    ContextMenu menu = new ContextMenu();

        //    MenuItem create = new MenuItem() { Header = "Create", Tag = item };
        //    create.Click += new RoutedEventHandler(OnCreate);

        //    MenuItem delete = new MenuItem() { Header = "Delete", Tag = item };
        //    delete.Click += new RoutedEventHandler(OnDelete);

        //    menu.Items.Add(create);
        //    menu.Items.Add(delete);

        //    item.ContextMenu = menu;
        //}

        private void OnCreate(object sender, RoutedEventArgs e)
        {
            
            
            //if (sender is not MenuItem menuItem) return;

            //var item = (TreeViewItem)menuItem.Tag;
            //var path = (string)item.Tag;

            //if (!Directory.Exists(path)) return;

            //CreateDialog dialog = new CreateDialog();
            //dialog.ShowDialog();

            //if (dialog.Cancel) return;

            //try
            //{
            //    var newFileName = dialog.fileName.Text;
            //    var newPath = path + Path.DirectorySeparatorChar + newFileName;

            //    if (dialog.directoryType.IsChecked != null ? (bool)dialog.directoryType.IsChecked : false)
            //        if (!Directory.Exists(newPath)) Directory.CreateDirectory(newPath);
            //        else
            //        if (!File.Exists(newPath)) File.Create(newPath);

            //    SetFileAttributes(newPath, dialog);
            //    //item.Items.Add(ProcessItem(newPath, newFileName));
            //}
            //catch (Exception ex)
            //{
            //    System.Windows.MessageBox.Show(ex.Message);
            //}
        }

        //private void OnDelete(object sender, RoutedEventArgs e)
        //{
        //    if (sender is not MenuItem menuItem) return;
        //    var item = (TreeViewItem)menuItem.Tag;
        //    var parent = (TreeViewItem)item.Parent;
        //    var path = (string)item.Tag;


        //    if (File.Exists(path)) File.Delete(path);
        //    else if (Directory.Exists(path)) Directory.Delete(path, true);

        //    parent.Items.Remove(item);
        //}

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
