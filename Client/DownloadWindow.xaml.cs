using Client.Properties;
using Client.Utils;
using Shared;
using Shared.Packets;
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
    /// Window die het bestand laat zien. De gebruiker kan deze dan downloaden.
    /// Renamen naar FilePreviewWindow
    /// </summary>
    public partial class DownloadWindow : Window
    {
        private Connection connection;
        private ProgressWindow progressWindow;
        public DownloadWindow(NetworkFile file)
        {
            InitializeComponent();
            this.File = file;
        }

        public NetworkFile File { get; private set; }

        private async void DownloadClick(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory("Downloads");
            connection = new Connection();
            await connection.Connect(Settings.Default.ServerIP, Settings.Default.ServerPort);
            await connection.SendPacket(new FileDownloadRequest(File.ID));
            download_Button.IsEnabled = false;

            progressWindow = new ProgressWindow();
            progressWindow.Owner = this;
            progressWindow.Show();
            try
            {
                await progressWindow.StartDownload(connection, File);
            }
            catch (Exception)
            {
                MessageBox.Show("Er ging iets fout.");
            }
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            //System.Drawing.Icon icon = FileIcon.GetJumboIcon('*' + File.FileFormat);
            //BitmapImage img = GetBitmapImage(icon);

            //fileIcon_Image.Source = img;
            fileIcon_Image.Source = GetBitmapImage(FileIcon.GetJumboIcon(File.FileFormat));
            fileName_Textblock.Text = "Name: " + File.Name;
            fileSize_Textblock.Text = "File Size: " + File.FileSize.ToString();
            fileFormat_Textblock.Text = "Format: " + File.FileFormat;
            creationDate_Textblock.Text = "Date: " + File.CreationDate.ToShortDateString();
            description_Textblock.Text = File.Description;
        }

        private static BitmapImage GetBitmapImage(System.Drawing.Icon icon)
        {
            MemoryStream stream = new MemoryStream();
            icon.ToBitmap().Save(stream, System.Drawing.Imaging.ImageFormat.Png);

            byte[] buffer = stream.GetBuffer();

            MemoryStream bufferStream = new MemoryStream(buffer);
            BitmapImage bitmap = new BitmapImage();
            bitmap.BeginInit();
            bitmap.StreamSource = bufferStream;
            bitmap.EndInit();

            return bitmap;
        }

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (fileIcon_Image.Source != null)
            {
                //FileIcon.DisposeIcon();
            }
        }
    }
}
