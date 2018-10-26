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
using System.Windows;

namespace Client
{
    /// <summary>
    /// Interaction logic for UploadCompleteWindow.xaml
    /// </summary>
    public partial class UploadCompleteWindow : Window
    {
        private string uploadText;

        public UploadCompleteWindow(string uploadText)
        {
            InitializeComponent();

            this.uploadText = uploadText;
            txtDownloadCode.Text = uploadText;
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(uploadText);
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
