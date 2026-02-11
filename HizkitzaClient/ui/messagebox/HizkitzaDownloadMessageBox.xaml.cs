using HizkitzaClient.util.connection;
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

namespace HizkitzaClient.ui.messagebox
{
    /// <summary>
    /// Interaction logic for HizkitzaDownloadMessageBox.xaml
    /// </summary>
    public partial class HizkitzaDownloadMessageBox : Window
    {
        public string Bytes { get; private set; } = "[0/0]";
        public string Percentage { get; private set; } = string.Empty;

        public HizkitzaDownloadMessageBox(string message)
        {
            InitializeComponent();
            DataContext = new MessageBoxViewModel(message);
        }

        public class MessageBoxViewModel(string message)
        {
            public string Message { get; set; } = message;
        }

        public static void ShowDialog(string msg)
        {
            new HizkitzaDownloadMessageBox(msg).ShowDialog();
        }
        // ||||| ||||| ||||| ||||| ||||| ||||| ||||| |||
        private void DownloadNewBytes(object sender, DownloadClient.DownloadNewBytesEventArgs e)
        {
            Bytes = $"[{e.TotalReceivedBytes}/{e.TotalBytes}]";
            var per = Math.Round((decimal)e.TotalBytes / e.TotalReceivedBytes);
        }

        private void DownloadEnded(object sender, DownloadClient.DownloadEndedEventArgs e)
        {
            Close();
        }
    }
}
