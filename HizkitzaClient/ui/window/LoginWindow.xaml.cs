using HizkitzaClient.ui.window.page;
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
        }

        private void SaioaHasi(object sender, RoutedEventArgs e)
        {
            if(user.Text == "admin")
            {

            } else if (user.Text == "user")
            {
                MainWindow mainWindow = new();
                mainWindow.MainFrame.Navigate(new PlayerLobby());
                mainWindow.Show();
                Close();
            }
        }

        private void SaioBerria(object sender, RoutedEventArgs e)
        {

        }

        private void Exit(object sender, RoutedEventArgs e) => Close();

        private void SaioaHasi_KeyDown(object sender, KeyEventArgs e)
        {

        }
    }
}
