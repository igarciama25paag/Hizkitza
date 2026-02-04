using HizkitzaServer.util.connection;
using HizkitzaServer.util.db.data;
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
        private readonly TcpClient? Client;
        private readonly NetworkStream? Stream;
        private readonly StreamReader? Reader;
        private readonly StreamWriter? Writer;

        public readonly static object sendLock = new();

        public Erabiltzailea? Erabiltzailea;
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
            try
            {
                await CommandDecoder.ExecuteCommand(Reader!.ReadLine(), this);
                if (Erabiltzailea != null)
                {
                    Alive = true;
                    Server.Clients[Erabiltzailea.Mota].Add(this);
                    Server.LogBerria($"Bezero berria '{this}'", LogType.INFO);
                    Send($"Logged {Erabiltzailea.Mota}");

                    CreateConnectionChecker();
                    CreateReceiverThread();
                } else Send("Denied Erabiltzailea null");
            }
            catch (CommandDecoder.DeniedException e)
            {
                Send("Denied " + e.Message);
                CloseClient(null);
                Server.LogBerria($"{e.Message}", LogType.ERROR);
            }
            catch (CommandDecoder.WrongCommandFormatException e)
            {
                Send("Denied " + e.Message);
                CloseClient(null);
                Server.LogBerria($"{e.Message}", LogType.ERROR);
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
                        CloseClient($"'{this}' bezeroa deskonektatu da");
                    Thread.Sleep(1000);
                }
            })
            { IsBackground = true }.Start();
        }

        // Bezeroaren mezuak entzun eta zerbitzariari pasatzen dizkion haria
        private void CreateReceiverThread()
        {
            new Thread(() =>
            {
                try
                {
                    while (Alive)
                    {
                        var mezua = Reader?.ReadLine();
                        if (mezua != null) Server.MezuBerria(mezua, this);
                    }
                }
                catch { CloseClient($"'{this}' bezeroa deskonektatu da"); }
            }).Start();
        }

        // Bezeroari mezua bidali
        public void Send(string mezua)
        {
            try
            {
                lock (sendLock)
                {
                    Writer?.WriteLine(mezua);
                    //Console.WriteLine($"Sent to ({Erabiltzailea?.Izena}): {mezua}");
                }
            }
            catch { CloseClient($"'{this}' bezeroa deskonektatu da"); }
        }

        // Bezeroa itxi eta bezero zerrendetatik kendu
        public void CloseClient(string? log)
        {
            Alive = false;
            Client?.Close();
            Stream?.Close();
            Reader?.Close();
            Writer?.Close();
            Server.ClientDisconnect(this);
            if (log != null) Server.LogBerria(log, LogType.INFO);
        }

        public override string? ToString() => Erabiltzailea?.Izena ?? "null";
    }
}
