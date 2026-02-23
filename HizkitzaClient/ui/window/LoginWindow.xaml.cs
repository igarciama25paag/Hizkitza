using HizkitzaClient.ui.messagebox;
using HizkitzaClient.ui.window.page;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Net;
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
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            serverip.Text = $"{Client.ip}:{Client.port}";
            Closing += Window_Closing;
            Client.ConnectedEvent += Connected;
            Client.DisconnectedEvent += Disconnected;
            Client.LogSentEvent += LogSent;
        }

        private void Window_Closing(object? sender, CancelEventArgs e)
        {
            Client.ConnectedEvent -= Connected;
            Client.DisconnectedEvent -= Disconnected;
            Client.LogSentEvent -= LogSent;
        }

        // Konektatzen denean erabiltzaile leihoara eraman
        private void Connected(object? sender, EventArgs e)
        {
            MainWindow mainWindow = new();
            switch (Client.mota)
            {
                case ConnectionType.admin:
                    mainWindow.MainFrame.Navigate(new AdminMain());
                    break;
                case ConnectionType.user:
                    mainWindow.MainFrame.Navigate(new PlayerLobby());
                    break;
            }
            mainWindow.Show();
            Close();
        }

        private void Disconnected(object? sender, EventArgs e)
        {
            pass.Password = null;
        }

        private void LogSent(object? sender, Client.LogSentEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                message.Foreground = e.Mota switch
                {
                    Client.LogType.ERROR => Brushes.Pink,
                    Client.LogType.INFO => Brushes.Lime,
                    Client.LogType.WARN => Brushes.Orange,
                    _ => Brushes.White
                };
                message.Text = e.Log;
            });
        }

        private void SaioaHasi_Click(object sender, RoutedEventArgs e) => SaioaHasi(false);

        private void SaioaHasi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SaioaHasi(false);
        }

        private void SaioBerria_Click(object sender, RoutedEventArgs e)
        {
            if (HizkitzaArgMessageBox.ShowDialog("Sartu pasahitza berriro:") == pass.Password)
                SaioaHasi(true);
            else
                Client.NewLog("Pasahitza ez da zuzena", Client.LogType.ERROR);
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Client.CloseClient(null);
            Close();
        }

        // Saioa hasi
        private void SaioaHasi(bool register)
        {
            try
            {
                var helb = serverip.Text.Split(':');
                if (helb.Length != 2) throw new Exception();
                var ip = IPAddress.Parse(helb[0]);
                var port = int.Parse(helb[1]);
                Client.Connect(ip, port, user.Text, pass.Password, register);
            }
            catch
            {
                Client.NewLog("Helbide formatu ezegokia", Client.LogType.WARN);
            }
        }
    }
}
