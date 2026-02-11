using HizkitzaServer.game;
using System.Net;
using System.Net.Sockets;

namespace HizkitzaServer.util.connection
{
    public static class Server
    {
        // Portua
        public static int PORT = 5000;

        // TcpListener objektua
        private static TcpListener? listener;
        
        // Zerbitzaria funtzionatzen ari den
        public static bool alive { get; private set; }

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
        public static readonly List<string> logs = [];

        // Partidak
        public static readonly List<Game> partidak = [];

        // Bezeroak mota bakoitzaren arabera
        public static readonly Dictionary<ConnectionType, HashSet<ServersideClient>> clients = new()
        {
            [ConnectionType.admin] = [],
            [ConnectionType.user] = [],
            [ConnectionType.download] = []
        };

        // Zerbitzaria piztu eta bezeroak entzuten hasi
        public static void TurnOn()
        {
            alive = true;
            try
            {
                listener = new(IPAddress.Any, PORT);
                listener.Start();
                using (Socket socket = new(AddressFamily.InterNetwork, SocketType.Dgram, 0))
                {
                    socket.Connect("10.0.1.20", PORT);
                    IPEndPoint endPoint = socket.LocalEndPoint as IPEndPoint;
                    NewLog($"ZERBITZARIA hasi da {endPoint.Address}:{PORT}", LogType.INFO);
                }
                CreateClientWaiter();
            }
            catch (Exception e)
            {
                NewLog($"Zerbitzari errorea: {e.Message}", LogType.ERROR);
                TurnOff();
            }
        }

        // Bezeroak itxaroteko haria sortu
        private static void CreateClientWaiter()
        {
            new Thread(() =>
            {
                try
                {
                    while (alive) WaitNewClient(listener!);
                }
                catch
                {
                    NewLog("Bezeroak itxarotean errorea", LogType.ERROR);
                    TurnOff();
                }
            }).Start();
        }

        // Bezero berria itxaron eta bezeroen zerrendan gehitu
        private static void WaitNewClient(TcpListener listener)
        {
            try
            {
                var bezeroBerria = new ServersideClient(listener.AcceptTcpClient());
                if (bezeroBerria.erabiltzailea != null)
                    lock (bezeroakLock)
                    {
                        clients[bezeroBerria.erabiltzailea.Mota].Add(bezeroBerria);
                        ClientConnectedEvent?.Invoke(null, new()
                        {
                            Client = bezeroBerria
                        });
                    }
            }
            catch (SocketException) { }
        }

        // Zerbitzaria itzali eta bezero guztiak bota
        public static void TurnOff()
        {
            alive = false;
            lock (bezeroakLock)
            {
                listener?.Stop();
                foreach (var list in clients.Values)
                    foreach (var bezero in list)
                        bezero.CloseClient(false);
            }
            NewLog("ZERBITZARIA itzali da", LogType.INFO);
        }

        // Log berria erregistratu eta gertaera deitu
        public static void NewLog(string log, LogType mota)
        {
            logs.Add($"[{DateTime.Now:t}] [{mota}] {log}");
            LogSentEvent?.Invoke(null, new()
            {
                Log = log,
                Mota = mota
            });
        }

        // Mezu berria gertaera deitu eta CommnandDecoder-en bidez prozesatu
        public static async Task NewMessage(string mezua, ServersideClient bezero)
        {
            MessageArrivedEvent?.Invoke(null, new()
            {
                Client = bezero,
                Mezua = mezua
            });
            await CommandDecoder.ExecuteCommand(mezua, bezero);
        }

        // Deskonektatutako bezeroa bezeroen zerrendatik kendu eta gertaera deitu
        public static void ClientDisconnect(ServersideClient Client)
        {
            lock (bezeroakLock)
            {
                if (Client.erabiltzailea != null)
                    Server.clients[Client.erabiltzailea.Mota].Remove(Client);
            }
            ClientDisconnectedEvent?.Invoke(null, new()
            {
                Client = Client
            });
        }

        // Partida berri bat gehitu eta gertaera deitu
        public static void NewGame(Game game)
        {
            partidak.Add(game);
            GamesUpdateEvent?.Invoke(null, new());
        }

        // Partida bat kendu eta gertaera deitu
        public static void RemoveGame(Game game)
        {
            partidak.Remove(game);
            GamesUpdateEvent?.Invoke(null, new());
        }
    }
}
