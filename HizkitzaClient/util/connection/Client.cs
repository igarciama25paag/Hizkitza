using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaClient.util.connection
{
    public static class Client
    {
        // Portua
        private const int PORT = 5000;

        // Bezero objektuak
        private static TcpClient? client;
        private static NetworkStream? Stream;
        private static StreamReader? Reader;
        private static StreamWriter? Writer;

        // Bezeroa funtzionatzen ari den
        public static bool Alive { get; private set; }

        // Bezeroaren izena eta mota
        public static string? Izena;
        public static ConnectionType? Mota;

        // Konektatuta gertara
        public static event EventHandler<EventArgs>? ConnectedEvent;

        // Deskonektatuta gertaera
        public static event EventHandler<EventArgs>? DisconnectedEvent;

        // Log berria gertaera
        public static event EventHandler<LogSentEventArgs>? LogSentEvent;
        public class LogSentEventArgs : EventArgs
        {
            public required string Log { get; set; }
            public required LogType Mota { get; set; }
        }

        // Mezu berria gertaera
        public static event EventHandler<MessageArrivedEventArgs>? MessageArrivedEvent;
        public class MessageArrivedEventArgs : EventArgs
        {
            public required string Mezua { get; set; }
        }

        // Log motak
        public enum LogType
        {
            INFO,
            WARN,
            ERROR
        }

        // Bezeroa zerbitzarira konektatu eta saioa hasten saiatu
        public static void Konektatu(string ip, string izena, string pasahitza)
        {
            client = new();
            Alive = true;
            try
            {
                client.Connect(ip, PORT);

                Stream = client.GetStream();
                Reader = new StreamReader(Stream);
                Writer = new StreamWriter(Stream) { AutoFlush = true };

                MezuaBidali($"Login {izena} {pasahitza}");
                try
                {
                    CommandDecoder.ExecuteCommand(Reader.ReadLine());
                    Izena = izena;
                    LogBerria("Zerbitzarira konektatuta", LogType.INFO);
                    ConnectedEvent?.Invoke(null, new());

                    CreateConnectionChecker();
                    CreateReceiverThread();
                }
                catch (CommandDecoder.DeniedException e)
                {
                    BezeroaItxi("Saioa ezeztatuta: " + e.Message);
                }
                catch (Exception e)
                {
                    BezeroaItxi("Konexio errorea: " + e.Message);
                }
            }
            catch { BezeroaItxi("Ezin izan da zerbitzaria atzitu"); }
        }

        // 
        private static void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (Alive)
                {
                    if (client!.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                        BezeroaItxi("Konexioa amaitu da");
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        private static void CreateReceiverThread()
        {
            new Thread(() =>
            {
                try
                {
                    while (Alive)
                    {
                        var mezua = Reader?.ReadLine();
                        if (mezua != null)
                        {
                            try
                            {
                                MessageArrivedEvent?.Invoke(null, new()
                                {
                                    Mezua = mezua
                                });
                                CommandDecoder.ExecuteCommand(mezua);
                            }
                            catch (Exception e)
                            {
                                LogBerria(e.Message, LogType.ERROR);
                            }
                        }
                    }
                }
                catch { BezeroaItxi("Konexioa amaitu da"); }
            }).Start();
        }

        private static void LogBerria(string log, LogType mota)
        {
            LogSentEvent?.Invoke(null, new()
            {
                Log = log,
                Mota = mota
            });
        }

        public static void MezuaBidali(string mezua)
        {
            try { Writer?.WriteLine(mezua); }
            catch { BezeroaItxi("Konexioa amaitu da"); }
        }

        public static void BezeroaItxi(string? log)
        {
            Alive = false;
            Mota = null;
            Izena = null;
            client?.Close();
            Stream?.Close();
            Reader?.Close();
            Writer?.Close();
            DisconnectedEvent?.Invoke(null, new());
            if (log != null) LogBerria(log, LogType.ERROR);
        }
    }
}
