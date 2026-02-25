using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    public partial class GamePage : Page
    {
        public ObservableCollection<ListBoxItem> playersList { get; } = [];
        private readonly object playersLock = new();
        public GamePage(string izena, string itxura, Brush kolorea)
        {
            DataContext = this;
            InitializeComponent();
            partidaIzena.Text = izena;
            jokalariItxura.Text = itxura;
            jokalariItxura.Foreground = kolorea;

            Loaded += Page_Loaded;
            Unloaded += Page_Unloaded;
            CommandDecoder.DataEvent += Data;
            CommandDecoder.DeniedEvent += Denied;
            CommandDecoder.InGameEvent += InGame;
        }
        private void Page_Loaded(object sender, RoutedEventArgs e) => Window.GetWindow(this).Closing += Window_Closing;

        private void Page_Unloaded(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object? sender, CancelEventArgs e) => Close();

        private void Denied(object? sender, CommandDecoder.DeniedEventArgs e)
        {
            Dispatcher?.Invoke(() => HizkitzaInfoMessageBox.ShowDialog($"DENIED {e.Reason}", false));
        }
        
        // Datu berriak ailatzerakoan zerrendak eguneratu
        private void Data(object? sender, CommandDecoder.DataEventArgs e)
        {
            if (e.Mota == CommandDecoder.DataType.Message)
                Dispatcher?.Invoke(() =>
                {
                    messages.Items.Add(new ListBoxItem()
                    {
                        Content = string.Join(" ", e.Data[1..]),
                        Foreground = new BrushConverter().ConvertFromString(e.Data[0]) as Brush
                    });
                });
            else if (e.Mota == CommandDecoder.DataType.Players)
            {
                Dispatcher?.Invoke(() =>
                {
                    lock (playersLock)
                    {
                        playersList.Clear();
                        foreach (var item in e.Data)
                            playersList.Add(new()
                            {
                                Content = item.Split(':')[0],
                                Foreground = new BrushConverter().ConvertFromString(item.Split(':')[1]) as Brush,
                            });
                    }
                });
            }
        }

        // Partidatik ateratzerakoan lobby-ra itzuli
        private void InGame(object? sender, CommandDecoder.InGameEventArgs e)
        {
            if (!e.Sartu) Dispatcher?.Invoke(() => NavigationService.Navigate(new PlayerLobby()));
        }

        // Orritik ateratzerakoan desuskribatu
        private void Close()
        {
            CommandDecoder.DataEvent -= Data;
            CommandDecoder.DeniedEvent -= Denied;
            CommandDecoder.InGameEvent -= InGame;
        }

        private void Bidali_Click(object sender, RoutedEventArgs e) => Send();

        private void Message_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter) Send();
        }

        // Mezua bidali
        private void Send()
        {
            if (!string.IsNullOrEmpty(message.Text))
            {
                Client.Send($"GameMessage {message.Text}");
                message.Text = string.Empty;
            }
        }

        // Partida utzi
        private void PartidaUtzi_Click(object sender, RoutedEventArgs e)
        {
            if (HizkitzaBooleanMessageBox.ShowDialog("Partidatik atera nahi al duzu?").DialogResult == true)
                Client.Send("LeaveGame");
        }
    }
}
