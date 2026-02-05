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
        private readonly TcpClient? Client;
        private readonly NetworkStream? Stream;
        private readonly StreamReader? Reader;
        private readonly StreamWriter? Writer;

        public readonly static object sendLock = new();

        // Erabiltzaile parametroak
        public Erabiltzailea? Erabiltzailea;

        // Bezeroa funtzionatzen dabilen
        public bool Alive { get; private set; }

        // Bezero bakoitzaren kudeatzailea sortu
        public ServersideClient(TcpClient bezero)
        {
            Client = bezero;
            Stream = bezero.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream) { AutoFlush = true };
            Login();
        }

        // Login komandoa itxaron eta bezeroa autentikatu
        public async void Login()
        {
            var result = await MezuaItxaron();
            if (result == null && Erabiltzailea != null)
            {
                Alive = true;
                Server.Clients[Erabiltzailea.Mota].Add(this);
                Server.LogBerria($"Bezero berria {this}", LogType.INFO);
                Send($"Logged {Erabiltzailea.Mota}");

                CreateConnectionChecker();
                CreateReceiverThread();
            }
            else
            {
                Send($"Denied {result?.Message ?? "Erabiltzaile null"}");
                CloseClient(false);
            }
        }

        // Bezeroarekin konexioa dabilela segunduro egiaztatzen duen haria, deskonektatuta badago bezeroa itxi
        private void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (Alive)
                {
                    if (Client!.Client.Poll(0, SelectMode.SelectRead) && Client.Client.Available == 0)
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
                while (Alive) await MezuaItxaron();
            }).Start();
        }

        // Bezeroko mezu bat itxaron eta erroreren bat egon den itxaron.
        private async Task<Exception?> MezuaItxaron()
        {
            try
            {
                var mezua = Reader?.ReadLine();
                if (mezua != null) await Server.MezuBerria(mezua, this);
                return null;
            }
            catch (Exception e)
            {
                Send("Denied " + e.Message);
                LogBerria(e.Message, LogType.ERROR);
                return e;
            }
        }

        // Bezeroari mezua bidali
        public void Send(string mezua)
        {
            try
            {
                lock (sendLock)
                {
                    Writer?.WriteLine(mezua);
                }
            }
            catch { CloseClient(true); }
        }

        // Bezeroa itxi eta bezero zerrendetatik kendu
        public void CloseClient(bool msg)
        {
            Alive = false;
            Client?.Close();
            Stream?.Close();
            Reader?.Close();
            Writer?.Close();
            Server.ClientDisconnect(this);
            if (msg) Server.LogBerria($"{this} bezeroa deskonektatu da", LogType.INFO);
        }

        public override string? ToString() => $"({Erabiltzailea?.Izena ?? "anonymous"})";
    }
}
