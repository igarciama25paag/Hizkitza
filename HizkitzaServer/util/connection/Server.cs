using HizkitzaServer.game;
using HizkitzaServer.util.db.data;
using System.Net;
using System.Net.Sockets;

namespace HizkitzaServer.util.connection
{
    public static class Server
    {
        private const int PORT = 5000;
        private static TcpListener? listener;
        public static bool Alive { get; private set; }

        public static readonly object BezeroakLock = new();

        public static event EventHandler<ClientConnectedEventArgs>? ClientConnectedEvent;
        public class ClientConnectedEventArgs : EventArgs
        {
            public required ServersideClient Client { get; set; }
        }
        public static event EventHandler<ClientDisconnectedEventArgs>? ClientDisconnectedEvent;
        public class ClientDisconnectedEventArgs : EventArgs
        {
            public required ServersideClient Client { get; set; }
        }
        public static event EventHandler<LogSentEventArgs>? LogSentEvent;
        public class LogSentEventArgs : EventArgs
        {
            public required string Log { get; set; }
            public required LogType Mota { get; set; }
        }
        public static event EventHandler<MessageSentEventArgs>? MessageSentEvent;
        public class MessageSentEventArgs : EventArgs
        {
            public required ServersideClient Client { get; set; }
            public required string Mezua { get; set; }
        }
        public static event EventHandler<GamesUpdateEventArgs>? GamesUpdateEvent;
        public class GamesUpdateEventArgs : EventArgs
        {
            public required Game Game { get; set; }
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

        public static readonly List<Game> Partidak = [];

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
                        ClientConnectedEvent?.Invoke(null, new()
                        {
                            Client = bezeroBerria
                        });
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
                MessageSentEvent?.Invoke(null, new()
                {
                    Client = bezero,
                    Mezua = mezua
                });
                await CommandDecoder.ExecuteCommand(mezua, bezero);
            }
            catch (Exception e)
            {
                LogBerria(e.Message, LogType.ERROR);
            }
        }

        public static void ClientDisconnect(ServersideClient Client)
        {
            lock (BezeroakLock)
            {
                if (Client.Erabiltzailea != null)
                    Server.Clients[Client.Erabiltzailea.Mota].Remove(Client);
            }
            ClientDisconnectedEvent?.Invoke(null, new()
            {
                Client = Client
            });
        }

        public static void NewGame(Game game)
        {
            Partidak.Add(game);
            GamesUpdateEvent?.Invoke(null, new()
            {
                Game = game
            });
        }

        public static void RemoveGame(Game game)
        {
            Partidak.Remove(game);
            GamesUpdateEvent?.Invoke(null, new()
            {
                Game = game
            });
        }
    }
}
