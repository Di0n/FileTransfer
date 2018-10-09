using Shared;
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

namespace Client
{
    /// <summary>
    /// Interaction logic for DownloadProgressWindow.xaml
    /// </summary>
    public partial class DownloadProgressWindow : Window
    {
        private NetworkFile file;
        public DownloadProgressWindow()
        {
            InitializeComponent();
        }

        internal Task StartDownload(Connection connection, NetworkFile file)
        {
            this.file = file;
            connection.FileTransferProgressChanged += FileTransferProgressChanged;
            return connection.ReceiveFileAsync(file.Name);
        }

        private void FileTransferProgressChanged(object sender, ProgressEventArgs args)
        {
            Console.WriteLine($"{args.BytesTransferred}/{args.TotalBytes}\nSpeed: {args.TransferSpeed}\n");
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
