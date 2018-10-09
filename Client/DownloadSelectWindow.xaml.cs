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
        public DownloadSelectWindow()
        {
            InitializeComponent();
        }

        private void LoadClick(object sender, RoutedEventArgs e)
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

            FileInfoRequest request = new FileInfoRequest(input);
        }

        private static bool ContainsSpecialChars(string s)
        {
            return s.Any(c => !Char.IsLetterOrDigit(c));
        }
    }
}
