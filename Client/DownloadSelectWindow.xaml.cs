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
using WpfAnimatedGif;
using System.ComponentModel;

namespace Client
{
    /// <summary>
    /// Interaction logic for DownloadSelectWindow.xaml
    /// </summary>
    public partial class DownloadSelectWindow : Window
    {
        readonly List<Window> windows;
        public DownloadSelectWindow()
        {
            InitializeComponent();
            windows = new List<Window>();
        }

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
            await Task.Delay(1000);
            status_Textblock.Text = "Got response...";
            controller.Pause();

            if (response != null && response.Exists)
            {
                DownloadWindow window = new DownloadWindow(response.File);
                window.Owner = this;
                window.Show();
                window.Closing += DownloadWindowClosing;

                windows.Add(window);
            }
            else
                status_Textblock.Text = "Invalid response!";
        }

        private void DownloadWindowClosing(object sender, CancelEventArgs e)
        {
            Window w = sender as Window;
            if (w != null)
                lock (windows)
                    windows.Remove(w);
        }

        private async void DownloadButtonClick(object sender, RoutedEventArgs e)
        {
            Connection connection = new Connection();
            await connection.Connect(Settings.Default.ServerIP, Settings.Default.ServerPort);

            DownloadWindow window = sender as DownloadWindow;
            NetworkFile file = window.File;
            
            // Moet uiteindelijk in een apart download window worden gemaakt
            await connection.SendPacket(new FileDownloadRequest(file.ID));
            connection.FileTransferProgressChanged += (o, ev) =>
            {

            };
            await connection.ReceiveFileAsync(file.Name, file.FileSize);
        }


       
        private static bool ContainsSpecialChars(string s) => s.Any(c => !Char.IsLetterOrDigit(c));

        private void WindowClosing(object sender, CancelEventArgs e)
        {
           /* lock (windows)
            {
                windows.ForEach(w => w?.Close());
                windows.Clear();
            }*/
        }
    }
}
