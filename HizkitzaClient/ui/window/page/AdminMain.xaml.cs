using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private readonly MainWindow FatherWindow;
        private readonly object logLock = new();

        public AdminMain(MainWindow father)
        {
            FatherWindow = father;
            CommandDecoder.ClearEvents();
            InitializeComponent();
            Unloaded += Page_Unloaded;
            RootClient();
            LogReciever();
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            if (Client.Alive)
                Client.MezuaBidali("DeactivateLogSender");
            CommandDecoder.NewLogEvent -= NewLog;
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

        private void LogReciever()
        {
            CommandDecoder.NewLogEvent += NewLog;
            Client.MezuaBidali("ActivateLogSender");
        }

        private void NewLog(object? sender, CommandDecoder.NewLogEventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                lock (logLock)
                {
                    var item = new ListBoxItem
                    {
                        Content = e.Log
                    };
                    LogList.Items.Add(item);
                    LogList.ScrollIntoView(item);
                }
            });
        }

        private void Bidali_Click(object sender, RoutedEventArgs e) => SendCommand();

        private void Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendCommand();
        }

        private void SendCommand()
        {
            var msg = command.Text;
            if (msg != null && msg == string.Empty)
                Client.MezuaBidali(command.Text);
            command.Text = null;
        }
    }
}
