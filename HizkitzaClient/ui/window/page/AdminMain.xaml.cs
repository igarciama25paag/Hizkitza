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
    public partial class AdminMain : Page
    {
        private readonly object logLock = new();
        public ObservableCollection<string> Logs { get; set; } = [];

        public AdminMain()
        {
            DataContext = this;
            InitializeComponent();
            Unloaded += Page_Unloaded;
            Loaded += Page_Loaded;
            CommandDecoder.DataEvent += Data;
            CommandDecoder.DeniedEvent += Denied;

            Client.MezuaBidali("LogSender true");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Window.GetWindow(this).Closing += Window_Closing;

        private void Page_Unloaded(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object? sender, CancelEventArgs e) => Close();

        private void Denied(object? sender, CommandDecoder.DeniedEventArgs e)
        {
            Dispatcher?.Invoke(() => HizkitzaInfoMessageBox.ShowDialog($"DENIED {e.Reason}"));
        }

        private void Data(object? sender, CommandDecoder.DataEventArgs e)
        {
            if (e.Mota == CommandDecoder.DataType.Log)
                Dispatcher.Invoke(() =>
                {
                    lock (logLock)
                    {
                        var newLog = "";
                        foreach (string str in e.Data)
                            newLog += str + " ";
                        Logs.Add(newLog.Trim());
                        logs.ScrollIntoView(logs.Items[^1]);
                    }
                });
        }

        private void Close()
        {
            if (Client.Alive)
                Client.MezuaBidali("LogSender false");
            CommandDecoder.DataEvent -= Data;
            CommandDecoder.DeniedEvent -= Denied;
        }

        private void Bidali_Click(object sender, RoutedEventArgs e) => SendCommand();

        private void Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendCommand();
        }

        private void SendCommand()
        {
            Dispatcher.Invoke(() =>
            {
                var msg = command.Text;
                if (msg != null && msg != string.Empty)
                    Client.MezuaBidali(command.Text);
                command.Text = null;
            });
        }
    }
}
