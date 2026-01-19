using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaClient.util.connection
{
    public class Client
    {
        private const int PORT = 5000;
        private TcpClient? client;
        private NetworkStream? Stream;
        private StreamReader? Reader;
        private StreamWriter? Writer;

        public delegate void ILogSent(string log, bool good);
        private ILogSent? LogSentEvent;
        public delegate void IMessageArrived(string mezua);
        private IMessageArrived? MessageArrivedEvent;
        public delegate void IConnected();
        private IConnected? ConnectedEvent;
        public delegate void IDisconnected();
        private IDisconnected? DisconnectedEvent;

        public string? Izena;
        public ConnectionType? Mota;
        private bool alive = false;

        public void Konektatu(string ip, string izena, string pasahitza)
        {
            client = new();
            Izena = izena;
            alive = true;
            try
            {
                client.Connect(ip, PORT);

                Stream = client.GetStream();
                Reader = new StreamReader(Stream);
                Writer = new StreamWriter(Stream) { AutoFlush = true };

                MezuaBidali($"/connect {izena} {pasahitza}");
                var command = Reader.ReadLine().Split(" ");
                if (command[0] == "/connected")
                {
                    Mota = (ConnectionType)Enum.Parse(typeof(ConnectionType), command[1]);
                    LogBerria("Zerbitzarira konektatuta", true);
                    ConnectedEvent?.Invoke();

                    CreateConnectionChecker();
                    CreateReceiverThread();
                }
                else BezeroaItxi("Ezin izan da saioa hasi");
            }
            catch { BezeroaItxi("Ezin izan da zerbitzaria atzitu"); }
        }

        private void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (alive)
                {
                    if (client.Client.Poll(0, SelectMode.SelectRead))
                        BezeroaItxi("Konexioa amaitu da");
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private void CreateReceiverThread()
        {
            new Thread(() =>
            {
                try
                {
                    while (alive)
                    {
                        var mezua = Reader?.ReadLine();
                        if (mezua != null)
                            MessageArrivedEvent?.Invoke(mezua);
                    }
                }
                catch { BezeroaItxi("Konexioa amaitu da"); }
            })
            { IsBackground = true }.Start();
        }

        private void LogBerria(string log, bool good) => LogSentEvent?.Invoke(log, good);

        public void MezuaBidali(string mezua)
        {
            try { Writer?.WriteLine(mezua); }
            catch { BezeroaItxi("Konexioa amaitu da"); }
        }

        public void BezeroaItxi(string? log)
        {
            alive = false;
            Mota = null;
            client?.Close();
            Stream?.Close();
            Reader?.Close();
            Writer?.Close();
            DisconnectedEvent?.Invoke();
            if (log != null) LogBerria(log, false);
        }

        public void RootToWindow(IConnected connectedevent, IDisconnected discconnectedevent, IMessageArrived messagearrivedevent, ILogSent logsentevent)
        {
            ConnectedEvent = connectedevent;
            DisconnectedEvent = discconnectedevent;
            MessageArrivedEvent = messagearrivedevent;
            LogSentEvent = logsentevent;
        }
    }
}
