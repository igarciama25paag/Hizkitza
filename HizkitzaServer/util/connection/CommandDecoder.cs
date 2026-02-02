using HizkitzaClient.util.db;
using HizkitzaServer.util.db.data;
using System.Threading.Tasks;
using System.Windows.Input;
using static HizkitzaServer.util.connection.Server;

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Login"] = new LoginCommand(),
        ["ActivateLogSender"] = new ActivateLogSenderCommand(),
        ["DeactivateLogSender"] = new DeactivateLogSenderCommand(),
        ["NewGame"] = new NewGameCommand(),
        ["ActivateGameUpdater"] = new ActivateGameUpdaterCommand(),
        ["DeactivateGameUpdater"] = new DeactivateGameUpdaterCommand()
    };

    public class UnexistingCommandException(string message) : Exception(message);
    public class WrongCommandFormatException(string message) : Exception(message);
    public class DeniedException(string message) : Exception(message);

    public static async Task ExecuteCommand(string? command, ServersideClient client)
    {
        if (command != null)
        {
            var splitCommand = command.Trim().Split(" ");
            var commandName = splitCommand[0];
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                await Commands[commandName].Execute(args.ToArray(), client);
            }
            catch (KeyNotFoundException)
            {
                var msg = $"'{commandName}' comandoa ez da existitzen";
                client.Send($"Denied {msg}");
                throw new UnexistingCommandException(msg);
            }
            catch (WrongCommandFormatException e)
            {
                var msg = $"Formatu okerra '{commandName}' comandoarentzat: {e.Message}";
                client.Send($"Denied {msg}");
                throw new WrongCommandFormatException(msg);
            }
        }
    }

    private interface ICommand
    {
        Task Execute(string[] args, ServersideClient client);
    }


    // Saioa hasteko komandoa
    private class LoginCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                foreach (var list in Server.Clients.Values)
                    if (list.Any(c => c.ToString() == args[0]))
                        throw new DeniedException($"'{args[0]}' saioa okupatuta");

                try
                {
                    //client.Erabiltzailea = await HizkitzaDB.GetErabiltzailea(args[0], args[1]);
                    if (args[0] == "admin" && args[1] == "admin")
                        client.Erabiltzailea = new Erabiltzailea(0, "admin", "admin", ConnectionType.admin);
                    else if (args[0] == "user" && args[1] == "user")
                        client.Erabiltzailea = new Erabiltzailea(0, "user", "user", ConnectionType.user);
                    else throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
                }
                catch (InvalidOperationException)
                {
                    throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
                }
            }
            catch (IndexOutOfRangeException)
            {
                if (args.Length > 2)
                    throw new WrongCommandFormatException("Login <erabiltzailea> <pasahitza>");
                else
                    throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
            }
        }
    }


    // Log bidalketa gaitzeko komandoa
    private class ActivateLogSenderCommand : ICommand
    {
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            clients.Add(client);
            if (!subscribed)
            {
                Server.LogSentEvent += SendLog;
                Server.ClientDisconnectedEvent += ClientDisconnected;
                CommandDecoder.DeactivateLogSenderEvent += DeactivateLogs;
                subscribed = true;
            }
        }

        private void SendLog(object? sender, Server.LogSentEventArgs e)
        {
            foreach (var client in clients)
                client.Send($"NewLog [{DateTime.Now:t}] [{e.Mota}] {e.Log}");
        }

        private void DeactivateLogs(object? sender, CommandDecoder.DeactivateLogSenderEventArgs e)
        {
            clients.Remove(e.Client);
        }

        private void ClientDisconnected(object? sender, Server.ClientDisconnectedEventArgs e)
        {
            clients.Remove(e.Client);
        }
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
            try
            {
                if (args.Length > 2) throw new IndexOutOfRangeException();
                var newGame = new Game(args[0], args[1]);
                if (Server.Partidak.Contains(newGame)) throw new DeniedException($"'{args[0]}' izena okupatuta");
                Server.NewGame(newGame);
            }
            catch (IndexOutOfRangeException)
            {
                throw new WrongCommandFormatException("NewGame <izena> <mapa>");
            }
        }
    }


    // Partida bidalketa gaitzeko komandoa
    private class ActivateGameUpdaterCommand : ICommand
    {
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            clients.Add(client);
            if (!subscribed)
            {
                Server.GamesUpdateEvent += GamesUpdate;
                Server.ClientDisconnectedEvent += ClientDisconnected;
                CommandDecoder.DeactivateGameUpdaterEvent += DeactivateGames;
                subscribed = true;
            }
            string games = "";
            foreach (var item in Server.Partidak)
                games += item.ToString() + " ";
            client.Send($"Games {games}");
        }

        private void GamesUpdate(object? sender, EventArgs e)
        {
            string games = "";
            foreach (var item in Server.Partidak)
                games += item.ToString() + " ";
            foreach (var client in clients)
                client.Send($"Games {games.Trim()}");
        }

        private void DeactivateGames(object? sender, CommandDecoder.DeactivateGameUpdaterEventArgs e)
        {
            clients.Remove(e.Client);
        }

        private void ClientDisconnected(object? sender, Server.ClientDisconnectedEventArgs e)
        {
            clients.Remove(e.Client);
        }
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
            DeactivateLogSenderEvent?.Invoke(null, new()
            {
                Client = client
            });
        }
    }
}