using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HizkitzaClient.util.connection
{
    public static class Client
    {
        // Konexio datuak
        public static IPAddress ip { get; private set; } = IPAddress.Parse("127.0.0.1");
        public static int port { get; private set; } = 5000;

        // Bezero objektuak
        private static TcpClient? client;
        private static NetworkStream? stream;
        private static StreamReader? reader;
        private static StreamWriter? writer;

        // Bezeroa funtzionatzen ari den
        public static bool alive { get; private set; }

        // Erabiltzaile motak
        public enum ConnectionType
        {
            admin,
            user
        }

        // Bezeroaren izena eta mota
        public static string? izena;
        public static ConnectionType? mota;

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
        public static void Connect(IPAddress ip, int port, string izena, string pasahitza, bool register)
        {
            client = new();
            alive = true;
            Client.ip = ip;
            Client.port = port;
            try
            {
                client.Connect(Client.ip, Client.port);

                stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                if (register) Send($"Register {izena} {pasahitza}");
                else Send($"Login {izena} {pasahitza}");
                WaitMessage();
                if (mota != null)
                {
                    Client.izena = izena;
                    NewLog("Zerbitzarira konektatuta", LogType.INFO);
                    ConnectedEvent?.Invoke(null, new());

                    CreateConnectionChecker();
                    CreateReceiverThread();
                }
            }
            catch { CloseClient("Ezin izan da zerbitzaria atzitu"); }
        }

        // Zerbitzariarekin konexioa dabilela segunduro egiaztatzen duen haria, deskonektatuta badago bezeroa itxi
        private static void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (alive)
                {
                    if (client!.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                        CloseClient("Konexioa amaitu da");
                    Thread.Sleep(1000);
                }
            }).Start();
        }

        // Zerbitzariko mezu hartzailea sortu
        private static void CreateReceiverThread()
        {
            new Thread(() =>
            {
                while (alive) WaitMessage();
            }).Start();
        }

        // Zerbitzariko mezu bat itxaron eta erroreren bat egon den itxaron.
        private static void WaitMessage()
        {
            try
            {
                var mezua = reader?.ReadLine();
                if (mezua != null)
                {
                    MessageArrivedEvent?.Invoke(null, new()
                    {
                        Mezua = mezua
                    });
                    CommandDecoder.ExecuteCommand(mezua);
                }
            }
            catch (Exception e)
            {
                if (e.GetType() == typeof(IOException))
                    NewLog("Konexioa amaitu da", LogType.ERROR);
                else
                    NewLog(e.Message, LogType.ERROR);
            }
        }

        // Zerbitzariari mezua/komandoa bidali
        public static void Send(string mezua)
        {
            if (alive)
                try { writer?.WriteLine(mezua); }
                catch { CloseClient("Konexioa amaitu da"); }
        }

        // Bezeroa itxi
        public static void CloseClient(string? log)
        {
            if (!alive) return;
            alive = false;
            mota = null;
            izena = null;
            client?.Close();
            stream?.Close();
            reader?.Close();
            writer?.Close();
            DownloadClient.CloseClient();
            DisconnectedEvent?.Invoke(null, new());
            if (log != null) NewLog(log, LogType.ERROR);
        }

        public static void NewLog(string log, LogType mota)
        {
            LogSentEvent?.Invoke(null, new()
            {
                Log = log,
                Mota = mota
            });
        }

        public static void Download(string file, string arg)
        {
            DownloadClient.DownloadBytes(ip, port, file, arg);
        }
    }
}
