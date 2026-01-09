using System.Net;
using System.Net.Sockets;

namespace HizkitzaServer
{
    public class Server
    {
        private const int PORT = 5000;
        private TcpListener? listener;
        public bool Alive { get; private set; }

        public readonly object BezeroakLock = new();

        public delegate void ILogSent(string log, bool good);
        public ILogSent? LogSentEvent;
        public delegate void IMessageSent(string mezua);
        public IMessageSent? MessageSentEvent;
        public delegate void IClientConnected(ServersideClient bezero);
        public IClientConnected? ClientConnectedEvent;
        public delegate void IClientDisconnected(ServersideClient bezero);
        public IClientDisconnected? ClientDisconnectedEvent;

        public readonly List<ServersideClient> Bezeroak = [];

        public void Piztu()
        {
            Alive = true;
            new Thread(() =>
            {
                try
                {
                    listener = new(IPAddress.Any, PORT);
                    listener.Start();
                    LogBerria($"ZERBITZARIA hasi da PORT:{PORT}", true);

                    while (Alive) BezeroBerriaItxaron(listener);
                }
                catch
                {
                    LogBerria("Zerbitzari errorea", false);
                    Itzali();
                }
            }).Start();
        }

        public void Itzali()
        {
            Alive = false;
            lock (BezeroakLock)
            {
                listener?.Stop();
                var bezReference = Bezeroak.ToList();
                foreach (var bezero in bezReference)
                    bezero.CloseClient(null);
            }
            LogBerria("ZERBITZARIA itzali da", false);
        }

        private void BezeroBerriaItxaron(TcpListener listener)
        {
            try
            {
                var bezeroBerria = new ServersideClient(this, listener.AcceptTcpClient());
                lock (BezeroakLock)
                {
                    Bezeroak.Add(bezeroBerria);
                    ClientConnectedEvent?.Invoke(bezeroBerria);
                }
            }
            catch (SocketException) { }
            catch { LogBerria("Bezero konexio errorea", false); }
        }

        public void LogBerria(string log, bool good)
        {
            LogSentEvent?.Invoke($"[{DateTime.Now.ToShortTimeString()}] {log}", good);
        }

        public void MezuBerria(string mezua)
        {
            MessageSentEvent?.Invoke($"[{DateTime.Now.ToShortTimeString()}] {mezua}");
        }

        public void SendEveryone(string mezua)
        {
            MezuBerria(mezua);
            lock (BezeroakLock)
            {
                foreach (var bezero in Bezeroak)
                    bezero.Send($"[{DateTime.Now.ToShortTimeString()}] {mezua}");
            }
        }
    }
}
