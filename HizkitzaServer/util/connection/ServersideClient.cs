using HizkitzaServer.util.connection;
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
        private readonly Server Server;
        private readonly TcpClient Client;
        private readonly NetworkStream Stream;
        private readonly StreamReader Reader;
        private readonly StreamWriter Writer;

        public readonly string Izena;
        public ConnectionType? Mota;
        public bool Alive { get; private set; }

        public ServersideClient(Server zerbitzari, TcpClient bezero)
        {
            Server = zerbitzari;
            Client = bezero;
            Stream = bezero.GetStream();
            Reader = new StreamReader(Stream);
            Writer = new StreamWriter(Stream) { AutoFlush = true };
            var command = Reader.ReadLine().Split(" ");

            if (command[0] == "/connect" && command[1] == "admin" && command[2] == "admin")
            {
                Izena = "admin";
                Mota = ConnectionType.admin;
                Alive = true;
                Server.LogBerria($"Bezero berria '{Izena}'", true);
                Send("/connected admin");

                CreateConnectionChecker();
                CreateReceiverThread();
            } else
            {
                Writer.WriteLine("/denied");
                CloseClient($"Saio bat ezeztatu da '{command[1]}':'{command[2]}'");
            }
        }

        private void CreateConnectionChecker()
        {
            new Thread(() =>
            {
                while (Alive)
                {
                    if (Client.Client.Poll(0, SelectMode.SelectRead))
                        CloseClient($"'{Izena}' bezeroa deskonektatu da");
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
                        if (mezua != null)
                            Server.SendEveryone($"{Izena}: {mezua}");
                    }
                }
                catch { CloseClient($"'{Izena}' bezeroa deskonektatu da"); }
            }).Start();
        }

        public void Send(string mezua)
        {
            try { Writer?.WriteLine(mezua); }
            catch { CloseClient($"'{Izena}' bezeroa deskonektatu da"); }
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
                if (Mota != null)
                    Server.GetListByType(Mota).Remove(this);
            }
            Server.ClientDisconnectedEvent?.Invoke(this);
            if (log != null) Server.LogBerria(log, false);
        }

        public override string? ToString() => Izena;
    }
}
