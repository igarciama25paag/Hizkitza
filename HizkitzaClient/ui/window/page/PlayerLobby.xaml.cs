using HizkitzaClient.ui.messagebox;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HizkitzaClient.ui.window.page
{
    public partial class PlayerLobby : Page
    {
        private readonly List<string> Itxurak = ["@", "G", "Q", "Ç", "C"];
        private readonly List<ComboBoxItem> Koloreak =
        [
            new ComboBoxItem() { Content = "Horia", Foreground = Brushes.Yellow },
            new ComboBoxItem() { Content = "Laranja", Foreground = Brushes.Orange },
            new ComboBoxItem() { Content = "Gorria", Foreground = Brushes.Red },
            new ComboBoxItem() { Content = "Magenta", Foreground = Brushes.Magenta },
            new ComboBoxItem() { Content = "Urdina", Foreground = Brushes.Turquoise },
            new ComboBoxItem() { Content = "Berdea", Foreground = Brushes.LimeGreen }
        ];
        private readonly List<string> Mapak = ["Bunker", "Mansioa", "Camping"];

        public PlayerLobby()
        {
            InitializeComponent();
            CommandDecoder.GamesEvent += Games;
            Client.MezuaBidali("ActivateGameUpdater");
            Client.LogSentEvent += LogSent;

            //Window.GetWindow(this).Closing += Window_Closing;

            foreach (var item in Mapak)
                mapa.Items.Add(item);

            foreach (var item in Koloreak)
                kolorea.Items.Add(item);

            foreach (var item in Itxurak)
                itxura.Items.Add(item);
        }

        private void Window_Closing(object? sender, CancelEventArgs e) => Close();

        private void Page_Unloaded(object sender, RoutedEventArgs e) => Close();

        private void Close()
        {
            if (Client.Alive)
                Client.MezuaBidali("DeactivateGameUpdater");
            CommandDecoder.GamesEvent -= Games;
            Client.LogSentEvent -= LogSent;
        }

        private void LogSent(object? sender, Client.LogSentEventArgs e)
        {
            Dispatcher?.Invoke(() => new HizkitzaInfoMessageBox($"{e.Mota} {e.Log}").ShowDialog());
        }

        private void Games(object? sender, CommandDecoder.GameEventArgs e)
        {
            Dispatcher?.Invoke(() =>
            {
                partidak.Items.Clear();
                foreach (var item in e.Games)
                    partidak.Items.Add(new ListBoxItem()
                    {
                        Content = item
                    });
            });
        }

        private void PartidaBerria_Click(object sender, RoutedEventArgs e)
        {
            if (izena.Text == null || izena.Text.Trim() == string.Empty)
                new HizkitzaInfoMessageBox("Partidak izen bat behar du").ShowDialog();
            else if (mapa.SelectedItem == null)
                new HizkitzaInfoMessageBox("Partidak mapa bat behar du").ShowDialog();
            else
                Client.MezuaBidali($"NewGame {izena.Text.Trim()} {mapa.Text.Trim()}");
        }
    }
}
