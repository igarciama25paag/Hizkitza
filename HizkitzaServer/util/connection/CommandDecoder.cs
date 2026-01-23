using HizkitzaClient.util.db;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    private static Dictionary<string, ICommand> Commands = new()
    {
        ["login"] = new LoginCommand(),
        ["getlogcount"] = new GetLogCountCommand()
    };

    public class UnexistingCommandException(string message) : Exception(message);
    public class WrongCommandFormatException(string message) : Exception(message);
    public class LoginDeniedException(string message) : Exception(message);

    public static async Task ExecuteCommand(string command, ServersideClient client)
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
            throw new UnexistingCommandException($"'{splitCommand[0]}' commandoa ez da existitzen");
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
            if (Server.Admins.Any(admin => admin.Erabiltzailea.Izena == args[0])
                || Server.Users.Any(user => user.Erabiltzailea.Izena == args[0]))
                throw new LoginDeniedException("Login in use");

            try
            {
                client.Erabiltzailea = await HizkitzaDB.GetErabiltzailea(args[0], args[1]);
                Server.Admins.Add(client);
            }
            catch (InvalidOperationException)
            {
                throw new LoginDeniedException("Login denied");
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
            var n = int.Parse(args[0]);
            for (var i = n; n < Server.Logs.Count; i++)
                client.Send($"newlog {Server.Logs[i]}");
        }
    }
}