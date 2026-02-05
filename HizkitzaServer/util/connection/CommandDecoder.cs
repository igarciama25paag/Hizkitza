using HizkitzaServer.util.data;
using HizkitzaServer.util.db;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static HizkitzaServer.util.connection.Server;

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    // Komando zerrenda
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Login"] = new LoginCommand(),
        ["ActivateLogSender"] = new ActivateLogSenderCommand(),
        ["DeactivateLogSender"] = new DeactivateLogSenderCommand(),
        ["NewGame"] = new NewGameCommand(),
        ["ActivateGameUpdater"] = new ActivateGameUpdaterCommand(),
        ["DeactivateGameUpdater"] = new DeactivateGameUpdaterCommand()
    };

    // Komando baimenak
    private readonly static Dictionary<string, Collection<ConnectionType>> Perms = new()
    {
        ["ActivateLogSender"] = [ConnectionType.admin],
        ["DeactivateLogSender"] = [ConnectionType.admin],
        ["NewGame"] = [ConnectionType.admin, ConnectionType.user],
        ["ActivateGameUpdater"] = [ConnectionType.admin, ConnectionType.user],
        ["DeactivateGameUpdater"] = [ConnectionType.admin, ConnectionType.user]
    };

    // Komandoa ez dela existzen salbuespena
    public class UnexistingCommandException(string message) : Exception(message);

    // Komandoaren formatu okerra salbuespena
    public class WrongCommandFormatException(string message) : Exception(message);

    // Ukatua salbuespena
    public class DeniedException(string message) : Exception(message);

    // Komandoa prozesatu eta exekutatu
    public static async Task ExecuteCommand(string? command, ServersideClient client)
    {
        if (command != null)
        {
            // Komandoa lortu
            var splitCommand = command.Trim().Split(" ");
            var commandName = splitCommand[0];

            // Komandoaren baimenak ikusi
            if (Perms.TryGetValue(commandName, out Collection<ConnectionType>? value) &&
                (client.Erabiltzailea == null || !value.Contains(client.Erabiltzailea.Mota)))
                throw new DeniedException("Baimenik gabe");

            // Komandoaren argumentuak lortu
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                // Komandoa exekutatu
                await Commands[commandName].Execute([.. args], client);
            }
            catch (KeyNotFoundException)
            {
                throw new UnexistingCommandException($"'{commandName}' comandoa ez da existitzen");
            }
            catch (WrongCommandFormatException e)
            {
                throw new WrongCommandFormatException($"'{commandName}' formatu okerra: {e.Message}");
            }
        }
    }

    // Komando interfaze orokorra
    private interface ICommand
    {
        Task Execute(string[] args, ServersideClient client);
    }

    // Komandoaren formatua ondo dagoela egiaztatzeko
    private static void CheckCommandFormat(string[] args, string format)
    {
        if (args.Length != format.Split(' ').Length - 1) throw new WrongCommandFormatException(format);
    }

    // Saioa hasteko komandoa
    private class LoginCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                // Komandoaren formatua egiaztatu
                CheckCommandFormat(args, "Login <erabiltzailea> <pasahitza>");

                // Erabiltzailea dagoeneko saio batean dagoen egiaztatu
                foreach (var list in Server.Clients.Values)
                    if (list.Any(c => c.ToString() == args[0]))
                        throw new DeniedException($"'{args[0]}' saioa okupatuta");

                // Kredentzialak egiaztatu eta erabiltzailea sortu
                //client.Erabiltzailea = await HizkitzaDB.GetErabiltzailea(args[0], args[1]);
                if (args[0] == "admin" && args[1] == "admin")
                    client.Erabiltzailea = new Erabiltzailea(0, "admin", "admin", ConnectionType.admin);
                else if (args[0] == "user" && args[1] == "user")
                    client.Erabiltzailea = new Erabiltzailea(0, "user", "user", ConnectionType.user);
                else throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
            }
            catch (Exception e) when (e is IndexOutOfRangeException || e is InvalidOperationException)
            {
                throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
            }
        }
    }


    // Log bidalketa gaitzeko komandoa
    private class ActivateLogSenderCommand : ICommand
    {
        // Gertaerari suskribatutako bezeroak
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "ActivateLogSender");

            clients.Add(client);

            // Gertaerei suskribatu lehen aldiz exekutatzerakoan
            if (!subscribed)
            {
                Server.LogSentEvent += SendLog;
                Server.ClientDisconnectedEvent += ClientDisconnected;
                CommandDecoder.DeactivateLogSenderEvent += DeactivateLogs;
                subscribed = true;
            }
        }

        // Bezeroei log berriak bidali
        private void SendLog(object? sender, Server.LogSentEventArgs e)
        {
            foreach (var client in clients)
                client.Send($"NewLog [{DateTime.Now:t}] [{e.Mota}] {e.Log}");
        }

        // Bezeroa log bidalketa desgaitzen denean desuskribatu
        private void DeactivateLogs(object? sender, CommandDecoder.DeactivateLogSenderEventArgs e) => clients.Remove(e.Client);

        // Bezeroa deskonektatzen denean desuskribatu
        private void ClientDisconnected(object? sender, Server.ClientEventArgs e) => clients.Remove(e.Client);
    }


    // Log bidalketa gelditzeko komandoa eta gertaera
    public static event EventHandler<DeactivateLogSenderEventArgs>? DeactivateLogSenderEvent;
    public class DeactivateLogSenderEventArgs : EventArgs
    {
        public required ServersideClient Client { get; set; }
    }
    private class DeactivateLogSenderCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "DeactivateLogSender");

            DeactivateLogSenderEvent?.Invoke(null, new()
            {
                Client = client
            });
        }
    }


    // Partida berria sortzeko komandoa
    private class NewGameCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "NewGame <izena> <mapa>");

            if (args.Length > 2) throw new IndexOutOfRangeException();
            var newGame = new Game(args[0], args[1]);
            if (Server.Partidak.Contains(newGame)) throw new DeniedException($"'{args[0]}' izena okupatuta");
            Server.NewGame(newGame);
        }
    }


    // Partida bidalketa gaitzeko komandoa
    private class ActivateGameUpdaterCommand : ICommand
    {
        // Gertaerari suskribatutako bezeroak
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "ActivateGameUpdater");

            clients.Add(client);

            // Gertaerei suskribatu lehen aldiz exekutatzerakoan
            if (!subscribed)
            {
                Server.GamesUpdateEvent += GamesUpdate;
                Server.ClientDisconnectedEvent += ClientDisconnected;
                CommandDecoder.DeactivateGameUpdaterEvent += DeactivateGames;
                subscribed = true;
            }
            client.Send($"Games {string.Join(" ", Server.Partidak)}");
        }

        // Bezeroei partida zerrenda bidali
        private void GamesUpdate(object? sender, EventArgs e)
        {
            foreach (var client in clients)
                client.Send($"Games {string.Join(" ", Server.Partidak)}");
        }

        // Bezeroa partida eguneraketa desgaitzen denean desuskribatu
        private void DeactivateGames(object? sender, CommandDecoder.DeactivateGameUpdaterEventArgs e) => clients.Remove(e.Client);

        // Bezeroa deskonektatzen denean desuskribatu
        private void ClientDisconnected(object? sender, Server.ClientEventArgs e) => clients.Remove(e.Client);
    }


    // Partida bidalketa gelditzeko komandoa eta gertaera
    public static event EventHandler<DeactivateGameUpdaterEventArgs>? DeactivateGameUpdaterEvent;
    public class DeactivateGameUpdaterEventArgs : EventArgs
    {
        public required ServersideClient Client { get; set; }
    }
    private class DeactivateGameUpdaterCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "DeactivateGameUpdater");

            DeactivateLogSenderEvent?.Invoke(null, new()
            {
                Client = client
            });
        }
    }
}