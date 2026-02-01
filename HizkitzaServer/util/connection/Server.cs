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

        public delegate void IMessageSent(string mezua, ServersideClient bezero);
        public static IMessageSent? MessageSentEvent;
        public delegate void IClientConnected(ServersideClient bezero);
        public static IClientConnected? ClientConnectedEvent;
        public delegate void IClientDisconnected(ServersideClient bezero);
        public static IClientDisconnected? ClientDisconnectedEvent;

        public static event EventHandler<LogSentEventArgs>? LogSentEvent;
        public class LogSentEventArgs : EventArgs
        {
            public required string Log { get; set; }
            public required LogType Mota { get; set; }
        }

        public enum LogType
        {
            INFO,
            WARN,
            ERROR
        }

        public static readonly List<string> Logs = [];

        public static readonly Dictionary<ConnectionType, HashSet<ServersideClient>> Clients = new()
        {
            [ConnectionType.admin] = [],
            [ConnectionType.user] = []
        };

        public static readonly List<Game> Jokoak = [];

        public static void Piztu()
        {
            Alive = true;
            new Thread(() =>
            {
                try
                {
                    listener = new(IPAddress.Any, PORT);
                    listener.Start();
                    LogBerria($"ZERBITZARIA hasi da PORT:{PORT}", LogType.INFO);

                    while (Alive) BezeroBerriaItxaron(listener);
                }
                catch
                {
                    LogBerria("Zerbitzari errorea", LogType.ERROR);
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
                foreach (var list in Clients.Values)
                    foreach (var bezero in list)
                        bezero.CloseClient(null);
            }
            LogBerria("ZERBITZARIA itzali da", LogType.INFO);
        }

        private static void BezeroBerriaItxaron(TcpListener listener)
        {
            try
            {
                var bezeroBerria = new ServersideClient(listener.AcceptTcpClient());
                if (bezeroBerria.Erabiltzailea != null)
                    lock (BezeroakLock)
                    {
                        Clients[bezeroBerria.Erabiltzailea.Mota].Add(bezeroBerria);
                        ClientConnectedEvent?.Invoke(bezeroBerria);
                    }
            }
            catch (SocketException) { }
            catch (Exception e) { LogBerria("Unhandled Exception on BezeroBerriaItxaron: " + e.Message, LogType.ERROR); }
        }

        public static void LogBerria(string log, LogType mota)
        {
            Logs.Add($"[{DateTime.Now:t}] [{mota}] {log}");
            LogSentEvent?.Invoke(null, new()
            {
                Log = log,
                Mota = mota
            });
        }

        public static async void MezuBerria(string mezua, ServersideClient bezero)
        {
            try
            {
                MessageSentEvent?.Invoke(mezua, bezero);
                await CommandDecoder.ExecuteCommand(mezua, bezero);
            } catch (Exception e)
            {
                LogBerria(e.Message, LogType.ERROR);
            }
        }

        public static void RootEvents(IClientConnected? clientConnected, IClientDisconnected? clientDisconnected, IMessageSent? messageSent)
        {
            ClientConnectedEvent = clientConnected;
            ClientDisconnectedEvent = clientDisconnected;
            MessageSentEvent = messageSent;
        }
    }
}
