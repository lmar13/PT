using PT_137131.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace PT_137131.DialogWindow
{
    /// <summary>
    /// Logika interakcji dla klasy SortingDialog.xaml
    /// </summary>
    public partial class SortingDialog : Window
    {
        private SortingViewModel sorting;
        public SortingViewModel Sorting { get { return sorting; } }

        public SortingDialog(SortingViewModel sorting)
        {
            InitializeComponent();
            this.sorting = sorting;
            DataContext = this.sorting;
        }
    }
}
