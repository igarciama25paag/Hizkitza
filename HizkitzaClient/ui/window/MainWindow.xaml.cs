using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net.Security;
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

namespace HizkitzaClient.ui.window
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Username { get; set; } = Client.Izena ?? "null";
        public MainWindow()
        {
            DataContext = this;
            InitializeComponent();
            Closing += Window_Closing;
            Client.DisconnectedEvent += Disconnected;
            //Client.LogSentEvent += LogSent;
            //Client.MessageArrivedEvent += MessageArrived;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            Client.DisconnectedEvent -= Disconnected;
            Client.LogSentEvent -= LogSent;
            Client.MessageArrivedEvent -= MessageArrived;
            if (Client.Alive) Client.BezeroaItxi(null);
        }

        public void Disconnected(object? sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                var dialog = HizkitzaBooleanMessageBox.ShowDialog("Zerbitzariarekin konexioa galdu da. Aplikazioa itxi nahi al duzu?");
                if (dialog.DialogResult == false) new LoginWindow().Show();
                Close();
            });
        }

        public void LogSent(object? sender, Client.LogSentEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                HizkitzaInfoMessageBox.ShowDialog($"[{e.Mota}] {e.Log}");
            });
        }

        private void MessageArrived(object? sender, Client.MessageArrivedEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                HizkitzaInfoMessageBox.ShowDialog($"(MSG) {e.Mezua}");
            });
        }

        private void Itxi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = HizkitzaBooleanMessageBox.ShowDialog("Aplikazioa itxi nahi al duzu?");
            if (dialog.DialogResult == true) Close();
        }

        private void SaioaItxi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = HizkitzaBooleanMessageBox.ShowDialog("Saioa itxi nahi al duzu?");
            if (dialog.DialogResult == true)
            {
                new LoginWindow().Show();
                Close();
            }
        }
    }
}
