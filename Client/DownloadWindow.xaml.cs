using Client.Properties;
using Shared;
using Shared.Packets;
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
    /// Window die het bestand laat zien. De gebruiker kan deze dan downloaden.
    /// Renamen naar FilePreviewWindow
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private Connection connection;
        private DownloadProgressWindow progressWindow;
        public DownloadWindow(NetworkFile file)
        {
            InitializeComponent();
            this.File = file;
        }

        public NetworkFile File { get; private set; }

        private async void DownloadClick(object sender, RoutedEventArgs e)
        {
            connection = new Connection();
            await connection.Connect(Settings.Default.ServerIP, Settings.Default.ServerPort);
            await connection.SendPacket(new FileDownloadRequest(File.ID));
            download_Button.IsEnabled = false;

            progressWindow = new DownloadProgressWindow();
            progressWindow.Owner = this;
            progressWindow.Show();
            await progressWindow.StartDownload(connection, File);
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            fileName_Textblock.Text = "Name: " + File.Name;
            fileSize_Textblock.Text = "File Size: " + File.FileSize.ToString();
            fileFormat_Textblock.Text = "Format: " + File.FileFormat;
            creationDate_Textblock.Text = "Date: " + File.CreationDate.ToShortDateString();
            description_Textblock.Text = File.Description;
        }

    }
}
