using HizkitzaClient.util.connection;
using System.Windows.Input;
using static HizkitzaClient.util.connection.CommandDecoder;

namespace HizkitzaClient.util.connection;

public static class CommandDecoder
{
    private static Dictionary<string, ICommand> Commands = new()
    {
        ["logged"] = new LoggedCommand(),
        ["denied"] = new DeniedCommand(),
        ["logcount"] = new LogCountCommand(),
        ["newlog"] = new NewLogCommand()
    };

    public class UnexistingCommandException(string message) : Exception(message);
    public class WrongCommandFormatException(string message) : Exception(message);
    public class DeniedException(string message) : Exception(message);

    public static void ExecuteCommand(string command)
    {
        var splitCommand = command.Split(" ");
        var args = splitCommand.ToList();
        args.RemoveAt(0);
        try
        {
            Commands[splitCommand[0]].Execute(args.ToArray());
        }
        catch (KeyNotFoundException)
        {
            throw new UnexistingCommandException($"'{splitCommand[0]}' commandoa ez da existitzen");
        }
    }

    public static void ClearEvents()
    {
        LogCountEvent = null;
        NewLogEvent = null;
    }
    
    private interface ICommand
    {
        void Execute(string[] args);
    }

    private class LoggedCommand : ICommand
    {
        public void Execute(string[] args)
        {
            try { Client.Mota = (ConnectionType)Enum.Parse(typeof(ConnectionType), args[0]); }
            catch
            {
                throw new WrongCommandFormatException("Wrong 'logged' command format");
            }
        }
    }

    private class DeniedCommand : ICommand
    {
        public void Execute(string[] args)
        {
            throw new DeniedException(string.Join(" ", args));
        }
    }

    public delegate void ILogCount(int count);
    public static ILogCount? LogCountEvent;
    private class LogCountCommand : ICommand
    {
        public void Execute(string[] args)
        {
            LogCountEvent?.Invoke(int.Parse(args[0]));
        }
    }

    public delegate void INewLog(string log);
    public static INewLog? NewLogEvent;
    private class NewLogCommand : ICommand
    {
        public void Execute(string[] args)
        {
            NewLogEvent?.Invoke(string.Join(" ", args));
        }
    }
}