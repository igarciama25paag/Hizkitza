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
    /// <summary>
    /// Interaction logic for AdminMain.xaml
    /// </summary>
    public partial class AdminMain : Page
    {
        private readonly MainWindow FatherWindow;
        private readonly ObservableCollection<string> logList = [];
        private int lastLogN = 0;
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
            CommandDecoder.LogCountEvent += (n) =>
            {
                Dispatcher.Invoke(() => lastLogN = n);
            };
            Client.MezuaBidali("getlogcount");

            CommandDecoder.NewLogEvent += (log) =>
            {
                Dispatcher.Invoke(() =>
                {
                    lock (logLock)
                    { 
                        logList.Add(log);
                        LogList.ScrollIntoView(logList.Last());
                        lastLogN++;
                    }
                });
            };

            new Thread(() =>
            {
                Dispatcher.Invoke(() =>
                {
                    while (IsLoaded)
                    {
                        lock (logLock)
                        {
                            Client.MezuaBidali($"getlogs {lastLogN}");
                        }
                        Thread.Sleep(500);
                    }
                });
            }).Start();
        }
    }
}
