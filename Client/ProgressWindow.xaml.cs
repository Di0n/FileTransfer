using Client.Properties;
using Shared;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for ProgressWindow.xaml
    /// </summary>
    public partial class ProgressWindow : Window
    {
        private NetworkFile file;
        public ProgressWindow()
        {
            InitializeComponent();
        }

        internal Task StartDownload(Connection connection, NetworkFile file)
        {
            this.file = file;
            connection.FileTransferProgressChanged += FileTransferProgressChanged;
            progressBar_Pb.Maximum = file.FileSize;
            return Task.Run(async () =>
            {
                await connection.ReceiveFileAsync(Settings.Default.DownloadPath + file.Name, file.FileSize);
            });
        }

        internal Task StartUpload(Connection connection, FileStream file)
        {
            connection.FileTransferProgressChanged += FileTransferProgressChanged;
            progressBar_Pb.Maximum = file.Length;
            return Task.Run(async () =>
            {
                await connection.SendFileAsync(file.Name);
            });
        }

        private async void FileTransferProgressChanged(object sender, ProgressEventArgs args)
        {
            Console.WriteLine($"{args.BytesTransferred}/{args.TotalBytes}\nSpeed: {args.TransferSpeed}\n");
            //progress_Textblock.Text = $"{ args.BytesTransferred}/{ args.TotalBytes}";


            await progressBar_Pb.Dispatcher.BeginInvoke((Action)(() =>
            {
                progressBar_Pb.Value = args.BytesTransferred;
            }));

            await progress_Textblock.Dispatcher.BeginInvoke((Action)(() =>
            {
                progress_Textblock.Text = $"{ args.BytesTransferred}/{ args.TotalBytes}";
            }));
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {

        }
    }
}
