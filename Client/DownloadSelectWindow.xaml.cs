using Client.Properties;
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
using WpfAnimatedGif;

namespace Client
{
    /// <summary>
    /// Interaction logic for DownloadSelectWindow.xaml
    /// </summary>
    public partial class DownloadSelectWindow : Window
    {
        DownloadWindow window;
        public DownloadSelectWindow() => InitializeComponent();

        private async void LoadClick(object sender, RoutedEventArgs e)
        {
            string input = searchBox_Textbox.Text;

            if (ContainsSpecialChars(input))
            {
                MessageBox.Show("Input can only contain numbers & alphanumeric characters");
                return;
            }

            // Server request
            ImageAnimationController controller = ImageBehavior.GetAnimationController(loadingGif_Image);
            controller.Play();

            Connection connection = new Connection();
            status_Textblock.Text = "Connecting...";
            await connection.Connect(Settings.Default.ServerIP, Settings.Default.ServerPort);
            status_Textblock.Text = "Connected";
            FileInfoRequest request = new FileInfoRequest(input);
            status_Textblock.Text = "Checking for file...";
            await connection.SendPacket(request);
            FileInfoResponse response = await connection.ReceivePacket() as FileInfoResponse;
            connection.Close();
            status_Textblock.Text = "Got response...";
            controller.Pause();
            if (response != null)
            {
                window = new DownloadWindow(response.File);
                window.download_Button.Click += DownloadButtonClick;
                window.Owner = this;
                window.Show();
            }
            else
            {
                
                status_Textblock.Text = "Invalid response!";
            }
        }

        private async void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            Connection connection = new Connection();
            await connection.Connect(Settings.Default.ServerIP, Settings.Default.ServerPort);
            
        }

        private static bool ContainsSpecialChars(string s) => s.Any(c => !Char.IsLetterOrDigit(c));

        private void WindowClosing(object sender, System.ComponentModel.CancelEventArgs e) => window?.Close();
    }
}
