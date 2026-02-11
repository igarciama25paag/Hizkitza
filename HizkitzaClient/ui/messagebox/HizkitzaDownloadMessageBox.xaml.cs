using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Threading;

namespace HizkitzaClient.ui.messagebox
{
    public partial class HizkitzaDownloadMessageBox : Window
    {
        public string Message { get; private set; } = string.Empty;

        public HizkitzaDownloadMessageBox(string message)
        {
            InitializeComponent();
            DataContext = this;
            Message = message;
            Closing += Window_Closing;
            DownloadClient.DownloadNewBytesEvent += DownloadNewBytes;
            DownloadClient.DownloadEndedEvent += DownloadEnded;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            DownloadClient.DownloadNewBytesEvent -= DownloadNewBytes;
            DownloadClient.DownloadEndedEvent -= DownloadEnded;
        }

        public static void ShowDialog(string msg)
        {
            new HizkitzaDownloadMessageBox(msg).Show();
        }

        // Jaitsitako Byte-ak erakutsi
        private void DownloadNewBytes(object? sender, DownloadClient.DownloadNewBytesEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                downBytes.Text = $"[{e.TotalReceivedBytes}/{e.TotalBytes}]";
                float per = 0;
                if (e.TotalBytes > 0)
                    per = (float)e.TotalReceivedBytes / e.TotalBytes;
                progressBar.Text = new string('|', (int)Math.Round((decimal)(38f * per)));
            });
        }

        private void DownloadEnded(object? sender, DownloadClient.DownloadEndedEventArgs e)
        {
            Dispatcher?.Invoke(Close);
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DownloadClient.CloseClient();
            Close();
        }
    }
}
