using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
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
        public ObservableCollection<string> Logs { get; } = [];

        public List<string> informeakList { get; } =
        [
            "ErabiltzaileInforme",
            "PartidakInforme"
        ];

        public AdminMain()
        {
            DataContext = this;
            InitializeComponent();
            Unloaded += Page_Unloaded;
            Loaded += Page_Loaded;
            CommandDecoder.DataEvent += Data;
            CommandDecoder.DeniedEvent += Denied;

            Client.Send("LogSender true");
        }

        private void Page_Loaded(object sender, RoutedEventArgs e) => Window.GetWindow(this).Closing += Window_Closing;

        private void Page_Unloaded(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object? sender, CancelEventArgs e) => Close();

        private void Denied(object? sender, CommandDecoder.DeniedEventArgs e)
        {
            Dispatcher?.Invoke(() => HizkitzaInfoMessageBox.ShowDialog($"DENIED {e.Reason}", false));
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
            if (Client.alive)
                Client.Send("LogSender false");
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
                    Client.Send(command.Text);
                command.Text = null;
            });
        }

        private void InformeaJaitsi_Click(object sender, RoutedEventArgs e)
        {
            var file = informeak.SelectedValue as string;
            var arg = "null";
            if (!string.IsNullOrEmpty(file))
            {
                if (file == "ErabiltzaileInforme")
                {
                    arg = HizkitzaArgMessageBox.ShowDialog("Sartu erabiltzaile bat:");
                    if (arg == null)
                    {
                        HizkitzaInfoMessageBox.ShowDialog("Ez da informerik jaitsi", false);
                        return;
                    }
                }

                DownloadClient.DownloadStartedEvent += DownloadStarted;
                DownloadClient.DownloadEndedEvent += DownloadEnded;
                Client.Download(file, arg);
            }
            else HizkitzaInfoMessageBox.ShowDialog("Ez da informerik aukeratu", false);
        }

        private void DownloadStarted(object? sender, DownloadClient.DownloadStartedEventArgs e)
        {
            jaitsi.IsEnabled = false;
            HizkitzaDownloadMessageBox.ShowDialog($"'{e.FileName}' fitxategia jaisten...");
        }

        private void DownloadEnded(object? sender, DownloadClient.DownloadEndedEventArgs e)
        {
            HizkitzaInfoMessageBox.ShowDialog(e.Reason, e.Successfully);
            if (e.Successfully)
            {
                Directory.CreateDirectory("downloads");
                File.WriteAllBytes("downloads\\" + e.File + ".pdf", DownloadClient.downloading[e.File]);
            }
            DownloadClient.downloading.Remove(e.File);
            DownloadClient.DownloadStartedEvent -= DownloadStarted;
            DownloadClient.DownloadEndedEvent -= DownloadEnded;
            jaitsi.IsEnabled = true;
        }
    }
}
