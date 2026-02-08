using HizkitzaClient.ui.messagebox;
using HizkitzaClient.util.connection;
using HizkitzaClient.util.game;
using System.Collections.ObjectModel;
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
        ["Data"] = new DataCommand()
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
            // Komandoa lortu
            var splitCommand = command.Trim().Split(" ");
            var commandName = splitCommand[0];

            // Komandoaren argumentuak lortu
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                // Komandoa existitzen den egiaztatu
                var commandExe = Commands[commandName];

                // Komandoaren formatu egokia egiaztatu
                var splitFormat = commandExe.Format.Split(' ');
                if (!(splitFormat[^1] == "..." && args.Count >= splitFormat.Length - 1) &&
                    args.Count != splitFormat.Length - 1)
                    throw new WrongCommandFormatException(commandExe.Format);

                // Komandoa exekutatu
                commandExe.Execute([.. args]);
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

    private interface ICommand
    {
        string Format { get; }
        void Execute(string[] args);
    }


    // Saioa ondo hasita komandoa
    private class LoggedCommand : ICommand
    {
        public string Format => "Logged <mota>";
        public void Execute(string[] args)
        {
            try { Client.mota = (ConnectionType)Enum.Parse(typeof(ConnectionType), args[0]); }
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
        public string Format => "Denied ...";
        public void Execute(string[] args)
        {
            DeniedEvent?.Invoke(null, new()
            {
                Reason = string.Join(" ", args)
            });
            throw new DeniedException(string.Join(" ", args));
        }
    }


    // Datu berriko gertaera
    public static event EventHandler<DataEventArgs>? DataEvent;
    public class DataEventArgs : EventArgs
    {
        public required DataType Mota { get; set; }
        public required string[] Data { get; set; }
    }

    // Datu motak
    public enum DataType
    {
        Log,
        Games
    }

    // Datu berriko komandoa
    private class DataCommand : ICommand
    {
        public string Format => "Data <mota> ...";
        public void Execute(string[] args)
        {
            try
            {
                DataType mota = (DataType)Enum.Parse(typeof(DataType), args[0]);
                var data = new string[args.Length - 1];
                for (int i = 0; i < data.Length; i++)
                    data[i] = args[i + 1];
                DataEvent?.Invoke(null, new()
                {
                    Mota = mota,
                    Data = data
                });
            }
            catch
            {
                throw new WrongCommandFormatException($"{args[0]} mota ez da existitzen");
            }
        }
    }
}