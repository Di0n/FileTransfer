using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
    public partial class UploadSelectWindow : Window
    {
        private FileStream selectedFile;

        public UploadSelectWindow()
        {
            InitializeComponent();
        }

        private void btnSelectFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Title = "Select a file";

            if(fileDialog.ShowDialog() ?? true)
            {
                selectedFile = fileDialog.OpenFile() as FileStream;

                int maxFileNameLength = 30;
                string fullFileName = selectedFile.Name;
                string shortenedFileName = (fullFileName.Length > maxFileNameLength - 1) 
                    ? "..." + fullFileName.Substring(fullFileName.Length - maxFileNameLength, maxFileNameLength) 
                    : fullFileName;

                lblFileName.Content = shortenedFileName;
            }
        }

        private void btnUpload_Click(object sender, RoutedEventArgs e)
        {
            if(selectedFile == null)
            {
                // Get file from URL input
                MessageBox.Show("No file selected!", "FileTransfer", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                //MessageBox.Show("no file selected");
                return;
            }

            new UploadWindow(selectedFile).Show();
            // Upload selected file

        }
    }
}
