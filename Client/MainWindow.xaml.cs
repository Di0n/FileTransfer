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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Client
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Window window;
        public MainWindow()
        {
            InitializeComponent();
        }

        private void UploadClick(object sender, MouseButtonEventArgs e)
        {
            //MessageBox.Show("Upload Window Template");
            window = new UploadSelectWindow();
            this.Visibility = Visibility.Hidden;
            window.Show();
            window.Closed += WindowClosed;
        }

        private void DownloadClick(object sender, MouseButtonEventArgs e)
        {
            // DownloadWindow window = new DownloadWindow();
            // window.Show();
            window = new DownloadSelectWindow();
            //this.Hide();
            this.Visibility = Visibility.Hidden;
            window.Show();
            window.Closed += WindowClosed;
        }

        private void WindowClosed(object sender, EventArgs e) => this.Visibility = Visibility.Visible;

        private void MouseEnterSelect(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Hand;

        private void MouseLeaveSelect(object sender, MouseEventArgs e) => Mouse.OverrideCursor = Cursors.Arrow;
    }
}
