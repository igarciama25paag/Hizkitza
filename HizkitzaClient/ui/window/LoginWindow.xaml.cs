using HizkitzaClient.ui.window.page;
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

namespace HizkitzaClient.ui.window
{
    /// <summary>
    /// Interaction logic for LoginWindow.xaml
    /// </summary>
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();
            RootToWindow();
        }

        private void SaioaHasi_Click(object sender, RoutedEventArgs e) => SaioaHasi();

        private void SaioaHasi_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) SaioaHasi();
        }

        private void SaioBerria_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            Connection.Bezeroa.BezeroaItxi(null);
            Close();
        }

        private void SaioaHasi()
        {
            Connection.Bezeroa.Konektatu(serverip.Text, user.Text, pass.Password);
            if (Connection.Bezeroa.Mota == null)
                return;
            else
            {
                MainWindow mainWindow = new(user.Text);
                switch (Connection.Bezeroa.Mota)
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
            pass.Password = string.Empty;
        }

        private void RootToWindow()
        {
            Connection.Bezeroa.RootToWindow(
                () => {},
                () => {},
                (mes) => {},
                (log, good) => {
                    Dispatcher.Invoke(() => {
                        message.Text = log;
                    });
                }
            );
        }
    }
}
