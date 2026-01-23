using System.Net;
using System.Net.Sockets;
using HizkitzaServer.game;

namespace HizkitzaServer.util.connection
{
    public static class Server
    {
        private const int PORT = 5000;
        private static TcpListener? listener;
        public static bool Alive { get; private set; }

        public static readonly object BezeroakLock = new();

        public delegate void ILogSent(string log, bool good);
        public static ILogSent? LogSentEvent;
        public delegate void IMessageSent(string mezua);
        public static IMessageSent? MessageSentEvent;
        public delegate void IClientConnected(ServersideClient bezero);
        public static IClientConnected? ClientConnectedEvent;
        public delegate void IClientDisconnected(ServersideClient bezero);
        public static IClientDisconnected? ClientDisconnectedEvent;

        public static readonly List<string> Logs = [];

        public static readonly HashSet<ServersideClient> Users = [];
        public static readonly HashSet<ServersideClient> Admins = [];
        public static readonly List<Game> Jokoak = [];
        
        public class ClientListNotFoundException(string message) : Exception(message);

        public static void Piztu()
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

        public static void Itzali()
        {
            Alive = false;
            lock (BezeroakLock)
            {
                listener?.Stop();
                var bezReference = Users.ToList();
                foreach (var bezero in bezReference)
                    bezero.CloseClient(null);
            }
            LogBerria("ZERBITZARIA itzali da", false);
        }

        private static void BezeroBerriaItxaron(TcpListener listener)
        {
            try
            {
                var bezeroBerria = new ServersideClient(listener.AcceptTcpClient());
                while (!bezeroBerria.Alive) Thread.Sleep(200);
                lock (BezeroakLock)
                {
                    GetListByType(bezeroBerria.Erabiltzailea.Mota).Add(bezeroBerria);
                    ClientConnectedEvent?.Invoke(bezeroBerria);
                }
            }
            catch (SocketException) { }
            catch (Exception e) { LogBerria("Unhandled Exception on BezeroBerriaItxaron: " + e.Message, false); }
        }

        public static void LogBerria(string log, bool good)
        {
            var status = good ? "INFO" : "ERROR";
            var newLog = $"[{DateTime.Now.ToShortTimeString()}] [{status}] {log}";
            Logs.Add(newLog);
            LogSentEvent?.Invoke($"{newLog}", good);
        }

        public static async void MezuBerria(string mezua, ServersideClient bezero)
        {
            await CommandDecoder.ExecuteCommand(mezua, bezero);
            MessageSentEvent?.Invoke($"[{DateTime.Now.ToShortTimeString()}] [{bezero.Erabiltzailea.Izena}] {mezua}");
        }

        public static HashSet<ServersideClient> GetListByType(ConnectionType? mota)
        {
            return mota switch
            {
                ConnectionType.admin => Admins,
                ConnectionType.user => Users,
                _ => throw new ClientListNotFoundException("Ez da listarik aurkitu hurrengo motarentzako: " + mota)
            };
        }
        
        public static void RootEvents(IClientConnected clientConnected, IClientDisconnected clientDisconnected, IMessageSent messageSent, ILogSent logSent)
        {
            ClientConnectedEvent = clientConnected;
            ClientDisconnectedEvent = clientDisconnected;
            MessageSentEvent = messageSent;
            LogSentEvent = logSent;
        }
    }
}
