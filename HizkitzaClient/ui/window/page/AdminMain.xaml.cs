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
            RootClient();
            LogReciever();
        }

        private void RootClient()
        {
            Application.Current.MainWindow.Close();
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
            var logcountLock = new object();
            var lastLogN = 0;
            CommandDecoder.LogCountEvent = (n) =>
            {
                Dispatcher.Invoke(() => lastLogN = n);
            };
            Client.MezuaBidali("getlogcount");

            CommandDecoder.NewLogEvent = (log) =>
            {
                Dispatcher.Invoke(() =>
                {
                    lock (logLock)
                    {
                        var item = new ListBoxItem
                        {
                            Content = log
                        };
                        LogList.Items.Add(item);
                        LogList.ScrollIntoView(item);
                    }
                });
            };
            
            new Thread(() =>
            {
                Thread.Sleep(5000);
                var alive = true;
                while (alive)
                {
                    lock (logLock)
                    {
                        Client.MezuaBidali($"getlogs {lastLogN + LogList.Items.Count}");
                    }
                    Thread.Sleep(5000);
                    Dispatcher.Invoke(() => alive = IsLoaded);
                }
            }).Start();
        }

        private void Bidali_Click(object sender, RoutedEventArgs e) => SendCommand();

        private void Command_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SendCommand();
        }

        private void SendCommand()
        {
            Client.MezuaBidali(command.Text);
            command.Text = null;
        }
    }
}
