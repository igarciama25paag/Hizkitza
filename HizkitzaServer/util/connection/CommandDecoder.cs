using HizkitzaClient.util.db;
using HizkitzaServer.util.db.data;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    private static Dictionary<string, ICommand> Commands = new()
    {
        ["login"] = new LoginCommand(),
        ["getlogcount"] = new GetLogCountCommand(),
        ["getlogs"] = new GetLogsCommand()
    };

    public class UnexistingCommandException(string message) : Exception(message);
    public class WrongCommandFormatException(string message) : Exception(message);
    public class DeniedException(string message) : Exception(message);

    public static async Task ExecuteCommand(string? command, ServersideClient client)
    {
        if (command != null)
        {
            var splitCommand = command.Split(" ");
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                await Commands[splitCommand[0]].Execute(args.ToArray(), client);
            }
            catch (KeyNotFoundException)
            {
                client.Send($"'{splitCommand[0]}' comandoa ez da existitzen");
            }
            catch (WrongCommandFormatException e)
            {
                client.Send($"Formatu okerra '{splitCommand[0]}' comandoarentzat: {e.Message}");
            }
        }
    }

    private interface ICommand
    {
        Task Execute(string[] args, ServersideClient client);
    }

    private class LoginCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
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
    }

    private class GetLogCountCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            client.Send($"logcount {Server.Logs.Count}");
        }
    }

    private class GetLogsCommand : ICommand
    {
        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                var n = int.Parse(args[0]);
                for (var i = n; i < Server.Logs.Count; i++)
                    client.Send($"newlog {Server.Logs[i]}");
            }
            catch (FormatException)
            {
                throw new WrongCommandFormatException(args[0]);
            }
        }
    }
}