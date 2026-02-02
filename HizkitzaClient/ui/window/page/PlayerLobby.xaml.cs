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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HizkitzaClient.ui.window.page
{
    public partial class PlayerLobby : Page
    {
        private readonly MainWindow FatherWindow;

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

        public PlayerLobby(MainWindow father)
        {
            InitializeComponent();
            FatherWindow = father;
            RootClient();
            GameReciever();

            foreach (var item in Mapak)
                mapa.Items.Add(item);

            foreach (var item in Koloreak)
                kolorea.Items.Add(item);

            foreach (var item in Itxurak)
                itxura.Items.Add(item);

            new HizkitzaInfoMessageBox("Mezu berria, eta sinbolotxo bat dauka, ziutatzeko ulertzen duzula, hi babua haizela!").ShowDialog();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Client.Alive)
                Client.MezuaBidali("DeactivateGameUpdater");
            CommandDecoder.NewGameEvent -= NewGame;
            CommandDecoder.RemoveGameEvent -= RemoveGame;
        }

        private void RootClient()
        {
            Client.RootEvents(
                () => { },
                () => {
                    Dispatcher.Invoke(() => FatherWindow.Bota());
                },
                (mes) => { },
                (log, good) => { }
            );
        }

        private void GameReciever()
        {
            CommandDecoder.NewGameEvent += NewGame;
            CommandDecoder.RemoveGameEvent += RemoveGame;
            Client.MezuaBidali("ActivateGameUpdater");
        }

        private void NewGame(object? sender, CommandDecoder.GameEventArgs e)
        {
            partidak.Items.Add(e.Game.Izena);
        }

        private void RemoveGame(object? sender, CommandDecoder.GameEventArgs e)
        {
            partidak.Items.Remove(e.Game.Izena);
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
