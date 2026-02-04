using HizkitzaServer.game;
using HizkitzaServer.util.db.data;
using System.Net;
using System.Net.Sockets;

namespace HizkitzaServer.util.connection
{
    public static class Server
    {
        // Portua
        private const int PORT = 5000;

        // TcpListener objektua
        private static TcpListener? listener;
        
        // Zerbitzaria funtzionatzen ari den
        public static bool Alive { get; private set; }

        public static readonly object bezeroakLock = new();

        // Bezero konektatu gertaera
        public static event EventHandler<ClientEventArgs>? ClientConnectedEvent;

        // Bezero deskonektatu gertaera
        public static event EventHandler<ClientEventArgs>? ClientDisconnectedEvent;
        public class ClientEventArgs : EventArgs
        {
            public required ServersideClient Client { get; set; }
        }

        // Log berria gertaera
        public static event EventHandler<LogSentEventArgs>? LogSentEvent;
        public class LogSentEventArgs : EventArgs
        {
            public required string Log { get; set; }
            public required LogType Mota { get; set; }
        }

        // Mezu berria iritsi gertaera
        public static event EventHandler<MessageArrivedEventArgs>? MessageArrivedEvent;
        public class MessageArrivedEventArgs : EventArgs
        {
            public required ServersideClient Client { get; set; }
            public required string Mezua { get; set; }
        }

        // Partidak eguneratu gertaera
        public static event EventHandler<EventArgs>? GamesUpdateEvent;

        // Log motak
        public enum LogType
        {
            INFO,
            WARN,
            ERROR
        }

        // Log-ak
        public static readonly List<string> Logs = [];

        // Partidak
        public static readonly List<Game> Partidak = [];

        // Bezeroak mota bakoitzaren arabera
        public static readonly Dictionary<ConnectionType, HashSet<ServersideClient>> Clients = new()
        {
            [ConnectionType.admin] = [],
            [ConnectionType.user] = []
        };

        // Zerbitzaria piztu eta bezeroak entzuten hasi
        public static void Piztu()
        {
            Alive = true;
            new Thread(() =>
            {
                try
                {
                    listener = new(IPAddress.Any, PORT);
                    listener.Start();
                    var host = Dns.GetHostEntry(Dns.GetHostName());
                    LogBerria($"ZERBITZARIA hasi da {host.AddressList[^1]}:{PORT}", LogType.INFO);

                    while (Alive) BezeroBerriaItxaron(listener);
                }
                catch
                {
                    LogBerria("Zerbitzari errorea", LogType.ERROR);
                    Itzali();
                }
            }).Start();
        }

        // Bezero berria itxaron eta bezeroen zerrendan gehitu
        private static void BezeroBerriaItxaron(TcpListener listener)
        {
            try
            {
                var bezeroBerria = new ServersideClient(listener.AcceptTcpClient());
                if (bezeroBerria.Erabiltzailea != null)
                    lock (bezeroakLock)
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

        // Zerbitzaria itzali eta bezero guztiak bota
        public static void Itzali()
        {
            Alive = false;
            lock (bezeroakLock)
            {
                listener?.Stop();
                foreach (var list in Clients.Values)
                    foreach (var bezero in list)
                        bezero.CloseClient(null);
            }
            LogBerria("ZERBITZARIA itzali da", LogType.INFO);
        }

        // Log berria erregistratu eta gertaera deitu
        public static void LogBerria(string log, LogType mota)
        {
            Logs.Add($"[{DateTime.Now:t}] [{mota}] {log}");
            LogSentEvent?.Invoke(null, new()
            {
                Log = log,
                Mota = mota
            });
        }

        // Mezu berria gertaera deitu eta CommnandDecoder-en bidez prozesatu
        public static async void MezuBerria(string mezua, ServersideClient bezero)
        {
            try
            {
                MessageArrivedEvent?.Invoke(null, new()
                {
                    Client = bezero,
                    Mezua = mezua
                });
                await CommandDecoder.ExecuteCommand(mezua, bezero);
            }
            catch (Exception e)
            {
                bezero.Send("Denied " + e.Message);
                LogBerria(e.Message, LogType.ERROR);
            }
        }

        // Deskonektatutako bezeroa bezeroen zerrendatik kendu eta gertaera deitu
        public static void ClientDisconnect(ServersideClient Client)
        {
            lock (bezeroakLock)
            {
                if (Client.Erabiltzailea != null)
                    Server.Clients[Client.Erabiltzailea.Mota].Remove(Client);
            }
            ClientDisconnectedEvent?.Invoke(null, new()
            {
                Client = Client
            });
        }

        // Partida berri bat gehitu eta gertaera deitu
        public static void NewGame(Game game)
        {
            Partidak.Add(game);
            GamesUpdateEvent?.Invoke(null, new());
        }

        // Partida bat kendu eta gertaera deitu
        public static void RemoveGame(Game game)
        {
            Partidak.Remove(game);
            GamesUpdateEvent?.Invoke(null, new());
        }
    }
}
