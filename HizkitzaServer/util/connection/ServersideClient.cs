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

namespace HizkitzaServer.util.connection
{
    public class ServersideClient
    {
        private readonly TcpClient? Client;
        private readonly NetworkStream? Stream;
        private readonly StreamReader? Reader;
        private readonly StreamWriter? Writer;

        public readonly static object sendLock = new();

        public Erabiltzailea Erabiltzailea;
        public bool Alive { get; private set; }

        public ServersideClient(TcpClient bezero)
        {
            Client = bezero;
            Stream = bezero.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream) { AutoFlush = true };
            Login();
        }

        public async void Login()
        {
            try
            {
                await CommandDecoder.ExecuteCommand(Reader.ReadLine(), this);
                Alive = true;
                Server.LogBerria($"Bezero berria '{Erabiltzailea.Izena}'", true);
                Send("logged admin");

                CreateConnectionChecker();
                CreateReceiverThread();
            }
            catch (CommandDecoder.LoginDeniedException e)
            {
                Send("denied " + e.Message);
                CloseClient(null);
                throw new Exception(e.Message);
            }
        }

        private void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (Alive)
                {
                    if (Client.Client.Poll(0, SelectMode.SelectRead) && Client.Client.Available == 0)
                        CloseClient($"'{Erabiltzailea.Izena}' bezeroa deskonektatu da");
                    Thread.Sleep(1000);
                }
            }) { IsBackground = true }.Start();
        }

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
                catch { CloseClient($"'{Erabiltzailea.Izena}' bezeroa deskonektatu da"); }
            }).Start();
        }

        public void Send(string mezua)
        {
            try
            {
                lock (sendLock)
                {
                    Writer?.WriteLine(mezua);
                }
            }
            catch { CloseClient($"'{Erabiltzailea.Izena}' bezeroa deskonektatu da"); }
        }

        public void CloseClient(string? log)
        {
            Alive = false;
            Client?.Close();
            Stream?.Close();
            Reader?.Close();
            Writer?.Close();
            lock(Server.BezeroakLock)
            {
                Server.GetListByType(Erabiltzailea.Mota).Remove(this);
            }
            Server.ClientDisconnectedEvent?.Invoke(this);
            if (log != null) Server.LogBerria(log, false);
        }

        public override string? ToString() => Erabiltzailea.Izena;
    }
}
