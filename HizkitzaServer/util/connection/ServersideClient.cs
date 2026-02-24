using HizkitzaServer.util.connection;
using HizkitzaServer.util.data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using static HizkitzaServer.util.connection.Server;

namespace HizkitzaServer.util.connection
{
    public class ServersideClient
    {
        // Bezero Tcp objektuak
        private readonly TcpClient? client;
        private readonly NetworkStream? stream;
        private readonly StreamReader? reader;
        private readonly StreamWriter? writer;

        public readonly static object sendLock = new();

        // Erabiltzaile motak
        public enum ConnectionType
        {
            admin,
            user,
            download
        }

        // Erabiltzaile parametroak
        public Erabiltzailea? erabiltzailea;

        // Sartuta dagoen partida
        public Game? currentGame;

        // Bezeroa funtzionatzen dabilen
        public bool alive { get; private set; }

        // Bezero deskonektatu gertaera
        public event EventHandler<EventArgs>? DisconnectedEvent;

        // Mezu berria iritsi gertaera
        public event EventHandler<MessageArrivedEventArgs>? MessageArrivedEvent;
        public class MessageArrivedEventArgs : EventArgs
        {
            public required string Mezua { get; set; }
        }

        // Bezero bakoitzaren kudeatzailea sortu
        public ServersideClient(TcpClient bezero)
        {
            client = bezero;
            stream = bezero.GetStream();
            reader = new StreamReader(stream);
            writer = new StreamWriter(stream) { AutoFlush = true };
            Login();
        }

        // Login komandoa itxaron eta bezeroa autentikatu
        public async void Login()
        {
            await WaitMessage();
            if (erabiltzailea != null)
            {
                alive = true;
                Server.clients[erabiltzailea.Mota].Add(this);
                Server.NewLog($"Bezero berria {this}", LogType.INFO);
                DisconnectedEvent += Disconnect;
                Send($"Logged {erabiltzailea.Mota}");

                CreateConnectionChecker();
                CreateReceiverThread();
            }
        }

        // Bezeroa deskonektatzerakoan
        private void Disconnect(object? sender, EventArgs e)
        {
            if (erabiltzailea != null)
                Server.clients[erabiltzailea.Mota].Remove(this);
        }

        // Bezeroarekin konexioa dabilela segunduro egiaztatzen duen haria, deskonektatuta badago bezeroa itxi
        private void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (alive)
                {
                    if (client!.Client.Poll(0, SelectMode.SelectRead) && client.Client.Available == 0)
                        CloseClient(true);
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }

        // Bezeroaren mezuak entzun eta zerbitzariari pasatzen dizkion haria
        private void CreateReceiverThread()
        {
            new Thread(async () =>
            {
                while (alive) await WaitMessage();
            }).Start();
        }

        // Bezeroko mezu bat itxaron eta erroreren bat egon den itxaron.
        private async Task WaitMessage()
        {
            try
            {
                var mezua = reader?.ReadLine();
                if (mezua != null)
                {
                    MessageArrivedEvent?.Invoke(this, new()
                    {
                        Mezua = mezua
                    });
                    Server.NewLog($"{this} {mezua}", Server.LogType.INFO);
                    await CommandDecoder.ExecuteCommand(mezua, this);
                }
            }
            catch (Exception e)
            {
                Send("Denied " + e.Message);
                NewLog(e.Message, LogType.ERROR);
            }
        }

        // Bezeroari mezua bidali
        public void Send(string mezua)
        {
            try
            {
                lock (sendLock)
                {
                    writer?.WriteLine(mezua);
                }
            }
            catch { CloseClient(true); }
        }

        public void SendBytes(byte[] bytes, int count)
        {
            try
            {
                lock (sendLock)
                {
                    stream?.Write(bytes, 0, count);
                }
            }
            catch { CloseClient(true); }
        }

        // Bezeroa itxi eta bezero zerrendetatik kendu
        public void CloseClient(bool msg)
        {
            if (!alive) return;
            alive = false;
            client?.Close();
            stream?.Close();
            reader?.Close();
            writer?.Close();
            DisconnectedEvent?.Invoke(this, new());
            if (msg) Server.NewLog($"{this} bezeroa deskonektatu da", LogType.INFO);
        }

        public override string? ToString() => $"({erabiltzailea?.Izena ?? "anonymous"})";
    }
}
