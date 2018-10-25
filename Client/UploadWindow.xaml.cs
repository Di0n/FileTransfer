using Client.Utils;
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
    /// Interaction logic for UploadWindow.xaml
    /// </summary>
    public partial class UploadWindow : Window
    {
        private Connection connection;
        private FileStream uploadFile;

        public UploadWindow(FileStream selectedFile)
        {
            this.uploadFile = selectedFile;
            this.connection = new Connection();
            InitializeComponent();

            string fullFileName = selectedFile.Name;
            string fileName = fullFileName.Split('\\')[fullFileName.Split('\\').Length -1];
            string fileType = "." + fileName.Split('.')[fileName.Split('.').Length -1];
            string fileSize = ExtensionMethods.ConvertFileSize(selectedFile.Length, ExtensionMethods.SizeUnit.kB);


            fileIcon_Image.Source = GetBitmapImage(FileIcon.GetJumboIcon(fileType));
            lblName.Content += fileName;
            lblSize.Content += fileSize;
            lblFormat.Content += fileType;
            lblCreationDate.Content += String.Format("{0}-{1}-{2}", DateTime.Now.Day, DateTime.Now.Month, DateTime.Now.Year);
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

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            // Actually upload the file
            connection = new Connection();
        }
    }
}
