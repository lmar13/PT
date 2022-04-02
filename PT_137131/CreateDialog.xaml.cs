using System.Windows;

namespace PT_137131
{
    /// <summary>
    /// Logika interakcji dla klasy CreateDialog.xaml
    /// </summary>
    public partial class CreateDialog : Window
    {
        public bool Cancel { get; set; }

        public CreateDialog()
        {
            InitializeComponent();
        }

        private void OnCancelBtnClick(object sender, RoutedEventArgs e)
        {
            Cancel = true;
            this.Close();
        }

        private void OnSuccessBtnClick(object sender, RoutedEventArgs e)
        {
            Cancel = false;
            this.Close();
        }
    }
}
