using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using HizkitzaClient.util.game;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using static HizkitzaClient.util.connection.Client;
using static HizkitzaClient.util.connection.CommandDecoder;

namespace HizkitzaClient.util.connection;

public static class CommandDecoder
{
    // Komando zerrenda
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Logged"] = new LoggedCommand(),
        ["Denied"] = new DeniedCommand(),
        ["NewLog"] = new NewLogCommand(),
        ["Games"] = new GamesCommand()
    };

    // Komandoa ez dela existzen salbuespena
    public class UnexistingCommandException(string message) : Exception(message);

    // Komandoaren formatu okerra salbuespena
    public class WrongCommandFormatException(string message) : Exception(message);

    // Ukatua salbuespena
    public class DeniedException(string message) : Exception(message);

    // Komandoa prozesatu eta exekutatu
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
                Debug.WriteLine($"Command: '{commandName}' with args: '{string.Join(" ", args)}'");
            }
            catch (KeyNotFoundException)
            {
                var msg = $"'{commandName}' comandoa ez da existitzen";
                throw new UnexistingCommandException(msg);
            }
            catch (WrongCommandFormatException e)
            {
                var msg = $"Formatu okerra '{commandName}' comandoarentzat: {e.Message}";
                throw new WrongCommandFormatException(msg);
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
    
    // Komandoaren formatua ondo dagoela baieztatzeko
    private static void CheckCommandFormat(string[] args, string format)
    {
        if (args.Length != format.Split(' ').Length - 1) throw new WrongCommandFormatException(format);
    }

    // Saioa ondo hasita komandoa
    private class LoggedCommand : ICommand
    {
        public void Execute(string[] args)
        {
            // Komandoaren formatua egiaztatu
            CheckCommandFormat(args, "Logged <mota>");

            try { Client.Mota = (ConnectionType)Enum.Parse(typeof(ConnectionType), args[0]); }
            catch
            {
                throw new WrongCommandFormatException("Logged <ConnectionType>");
            }
        }
    }


    // Ukatuta komandoa eta gertaera
    public static event EventHandler<DeniedEventArgs>? DeniedEvent;
    public class DeniedEventArgs : EventArgs
    {
        public required string Reason { get; set; }
    }
    private class DeniedCommand : ICommand
    {
        public void Execute(string[] args)
        {
            DeniedEvent?.Invoke(null, new()
            {
                Reason = string.Join(" ", args)
            });
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
    public static event EventHandler<GameEventArgs>? GamesEvent;
    public class GameEventArgs : EventArgs
    {
        public required string[] Games { get; set; }
    }
    private class GamesCommand : ICommand
    {
        public void Execute(string[] args)
        {
            GamesEvent?.Invoke(null, new()
            {
                Games = args
            });
        }
    }
}