using HizkitzaClient.util.connection;
using HizkitzaClient.util.game;
using System.Windows.Input;
using static HizkitzaClient.util.connection.Client;
using static HizkitzaClient.util.connection.CommandDecoder;

namespace HizkitzaClient.util.connection;

public static class CommandDecoder
{
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Logged"] = new LoggedCommand(),
        ["Denied"] = new DeniedCommand(),
        ["NewLog"] = new NewLogCommand(),
        ["NewGame"] = new NewGameCommand(),
        ["RemoveGame"] = new RemoveGameCommand()
    };

    public class UnexistingCommandException(string message) : Exception(message);
    public class WrongCommandFormatException(string message) : Exception(message);
    public class DeniedException(string message) : Exception(message);

    public static void ExecuteCommand(string? command)
    {
        if (command != null)
        {
            var splitCommand = command.Trim().Split(" ");
            var commandName = splitCommand[0];
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                Commands[commandName].Execute(args.ToArray());
            }
            catch (KeyNotFoundException)
            {
                var msg = $"'{commandName}' comandoa ez da existitzen";
                throw new UnexistingCommandException(msg);
            }
            catch (WrongCommandFormatException e)
            {
                var msg = $"Formatu okerra '{commandName}' comandoarentzat: {e.Message}";
                throw new UnexistingCommandException(msg);
            }
        }
    }

    public static void ClearEvents()
    {
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
                throw new WrongCommandFormatException("Logged <ConnectionType>");
            }
        }
    }


    // Komando ezeztatutako kom
    private class DeniedCommand : ICommand
    {
        public void Execute(string[] args)
        {
            throw new DeniedException(string.Join(" ", args));
        }
    }


    // Log berria iritsi den komandoa eta gertaera
    public static event EventHandler<NewLogEventArgs>? NewLogEvent;
    public class NewLogEventArgs : EventArgs
    {
        public required string Log { get; set; }
    }
    private class NewLogCommand : ICommand
    {
        public void Execute(string[] args)
        {
            NewLogEvent?.Invoke(null, new()
            {
                Log = string.Join(" ", args)
            });
        }
    }


    // Partida gehitzeko komandoa eta gertaera
    public static event EventHandler<GameEventArgs>? NewGameEvent;
    public class GameEventArgs : EventArgs
    {
        public required Game Game { get; set; }
    }
    private class NewGameCommand : ICommand
    {
        public void Execute(string[] args)
        {
            NewGameEvent?.Invoke(null, new()
            {
                Game = new(args[0], args[1])
            });
        }
    }


    // Partida ezabatzeko komandoa eta gertaera
    public static event EventHandler<GameEventArgs>? RemoveGameEvent;
    private class RemoveGameCommand : ICommand
    {
        public void Execute(string[] args)
        {
            RemoveGameEvent?.Invoke(null, new()
            {
                Game = new(args[0], args[1])
            });
        }
    }
}