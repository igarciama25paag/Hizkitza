using HizkitzaClient.ui.messagebox;
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
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public string Usertag { get; private set; }
        public MainWindow(string username)
        {
            InitializeComponent();
            Usertag = username;
        }

        private void Itxi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new StyledMessageBox("Irten", "Irten nahi al duzu?", "Bai", "Ez");
            if (dialog.ShowDialog() == true)
            {
                Client.BezeroaItxi(null);
                Close();
            }
        }

        private void SaioaItxi_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new StyledMessageBox("Saioa itxi", "Saioa itxi nahi al duzu?", "Bai", "Ez");
            if (dialog.ShowDialog() == true)
            {
                new LoginWindow().Show();
                Client.BezeroaItxi(null);
                Close();
            }
        }

        public void Bota()
        {
            var dialog = new StyledMessageBox("Konexio errorea", "Zerbitzariarekin konexioa galdu da. Aplikazioa itxi nahi al duzu?", "Bai", "Ez");
            if (dialog.ShowDialog() == false) new LoginWindow().Show();
            if (Client.Alive) Client.BezeroaItxi(null);
            Close();
        }
    }
}
