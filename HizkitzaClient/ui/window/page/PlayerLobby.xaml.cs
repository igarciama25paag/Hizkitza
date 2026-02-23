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
        private readonly object gamesLock = new();

        public List<ComboBoxItem> ItxurakList { get; } =
        [
            new ComboBoxItem() { Content = "@" },
            new ComboBoxItem() { Content = "Q" },
            new ComboBoxItem() { Content = "G" },
            new ComboBoxItem() { Content = "C" },
            new ComboBoxItem() { Content = "Ç" }
        ];
        public List<ComboBoxItem> KoloreakList { get; } =
        [
            new ComboBoxItem() { Content = "Horia", Foreground = Brushes.Yellow },
            new ComboBoxItem() { Content = "Laranja", Foreground = Brushes.Orange },
            new ComboBoxItem() { Content = "Gorria", Foreground = Brushes.Red },
            new ComboBoxItem() { Content = "Magenta", Foreground = Brushes.Magenta },
            new ComboBoxItem() { Content = "Urdina", Foreground = Brushes.Turquoise },
            new ComboBoxItem() { Content = "Berdea", Foreground = Brushes.LimeGreen }
        ];
        public List<string> MapakList { get; } = ["Bunker", "Mansioa", "Camping"];

        public PlayerLobby()
        {
            DataContext = this;
            InitializeComponent();
            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
            CommandDecoder.DataEvent += Data;
            CommandDecoder.DeniedEvent += Denied;

            Client.Send("GameUpdater true");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Window.GetWindow(this).Closing += Window_Closing;

        private void Page_Unloaded(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object? sender, CancelEventArgs e) => Close();

        private void Denied(object? sender, CommandDecoder.DeniedEventArgs e)
        {
            Dispatcher?.Invoke(() => HizkitzaInfoMessageBox.ShowDialog($"DENIED {e.Reason}"));
        }

        // Datu berriak ailatzerakoan partidak eguneratu
        private void Data(object? sender, CommandDecoder.DataEventArgs e)
        {
            if (e.Mota == CommandDecoder.DataType.Games)
                Dispatcher?.Invoke(() =>
                {
                    lock (gamesLock)
                    {
                        partidak.Items.Clear();
                        foreach (var item in e.Data)
                            partidak.Items.Add(new ListBoxItem()
                            {
                                Content = item
                            });
                    }
                });
        }

        // Partida baten sartzerakoan orria aldatu
        private void InGame(object? sender, CommandDecoder.InGameEventArgs e)
        {
            NavigationService.Navigate(new GamePage());
        }

        // Orritik ateratzerakoan desuskribatu
        private void Close()
        {
            Client.Send("GameUpdater false");
            CommandDecoder.DataEvent -= Data;
            CommandDecoder.DeniedEvent -= Denied;
        }

        // Partida berria sortu
        private void PartidaBerria_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(izena.Text))
                HizkitzaInfoMessageBox.ShowDialog("Partidak izen bat behar du");
            else if (mapa.SelectedItem == null)
                HizkitzaInfoMessageBox.ShowDialog("Partidak mapa bat behar du");
            else
                Client.Send($"NewGame {izena.Text.Trim()} {mapa.Text.Trim()}");
        }

        // Kolorea aukeratzerakoan itxura eta kolorearen koloreak aldatu
        private void Kolorea_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var col = ((ComboBoxItem)kolorea.SelectedItem).Foreground ?? Brushes.White;
            itxura.Foreground = col;
            kolorea.Foreground = col;
            foreach (var item in itxura.Items)
                ((ComboBoxItem)item).Foreground = col;
        }

        // Aukeratutako partidan sartu
        private void PartidanSartu_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(itxura.Text))
                HizkitzaInfoMessageBox.ShowDialog("Itxura bat aukeratu behar da");
            else if (string.IsNullOrEmpty(kolorea.Text))
                HizkitzaInfoMessageBox.ShowDialog("Kolore bat aukeratu behar da");
            else if (partidak.SelectedItem == null)
                HizkitzaInfoMessageBox.ShowDialog("Partida bat aukeratu behar da");
            else
                Client.Send($"JoinGame {partidak.SelectedItem} {itxura.Text} {kolorea.Text}");
        }
    }
}
