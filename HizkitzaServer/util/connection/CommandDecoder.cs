using HizkitzaServer.game.world.entity;
using HizkitzaServer.util.data;
using HizkitzaServer.util.db;
using HizkitzaServer.util.pdf;
using System.Collections.ObjectModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static HizkitzaServer.util.connection.Server;
using static HizkitzaServer.util.connection.ServersideClient;

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    // Komando zerrenda
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Register"] = new RegisterCommand(),
        ["Login"] = new LoginCommand(),
        ["LogSender"] = new LogSenderCommand(),
        ["GameUpdater"] = new GameUpdaterCommand(),
        ["NewGame"] = new NewGameCommand(),
        ["JoinGame"] = new GameCommands.JoinGameCommand(),
        ["LeaveGame"] = new GameCommands.LeaveGameCommand(),
        ["GameMessage"] = new GameCommands.GameMessageCommand(),
        ["Download"] = new DownloadCommand()
    };

    // Komando baimenak
    private readonly static Dictionary<string, Collection<ConnectionType?>> Perms = new()
    {
        ["Register"] = [null, ConnectionType.admin],
        ["Login"] = [null],
        ["LogSender"] = [ConnectionType.admin],
        ["GameUpdater"] = [ConnectionType.user],
        ["NewGame"] = [ConnectionType.admin, ConnectionType.user],
        ["JoinGame"] = [ConnectionType.user],
        ["LeaveGame"] = [ConnectionType.user],
        ["GameMessage"] = [ConnectionType.admin, ConnectionType.user],
        ["Download"] = [ConnectionType.download]
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

            // Komandoaren argumentuak lortu
            var args = splitCommand.ToList();
            args.RemoveAt(0);
            try
            {
                // Komandoa existitzen den egiaztatu
                var commandExe = Commands[commandName];

                // Komandoaren baimenak ikusi
                if (Perms.TryGetValue(commandName, out Collection<ConnectionType?>? value) &&
                    !value.Contains(client.erabiltzailea?.Mota))
                    throw new DeniedException("Baimenik gabe");

                // Komandoaren formatu egokia egiaztatu
                var splitFormat = commandExe.Format.Split(' ');
                if (!(splitFormat[^1] == "..." && args.Count >= splitFormat.Length - 1) &&
                    args.Count != splitFormat.Length - 1)
                    throw new WrongCommandFormatException($"'{commandName}' formatu okerra: {commandExe.Format}");

                // Komandoa exekutatu
                await commandExe.Execute([.. args], client);
            }
            catch (KeyNotFoundException)
            {
                throw new UnexistingCommandException($"'{commandName}' comandoa ez da existitzen");
            }
        }
    }


    // Komando interfaze orokorra
    private interface ICommand
    {
        // Komandoaren formatua
        string Format { get; }

        // Komando exekuzioa
        Task Execute(string[] args, ServersideClient client);
    }

    private class RegisterCommand : ICommand
    {
        public string Format => "Register <erabiltzailea> <pasahitza>";

        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                await HizkitzaDB.GetErabiltzailea(args[0]);
                throw new DeniedException($"'{args[0]}' erabiltzailea iada existitzen da");
            }
            catch (InvalidOperationException)
            {
                await HizkitzaDB.RegisterErabiltzailea(args[0], args[1]);
                await HizkitzaDB.NewErabiltzaileakStats(args[0]);
                Server.NewLog($"'{args[0]}' erabiltzaile berria sortu da", LogType.INFO);

                try
                {
                    if (client.erabiltzailea == null)
                        client.erabiltzailea = await HizkitzaDB.LoginErabiltzailea(args[0], args[1]);
                }
                catch (InvalidOperationException)
                {
                    throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
                }
            }
        }
    }


    // Saioa hasteko komandoa
    private class LoginCommand : ICommand
    {
        public string Format => "Login <erabiltzailea> <pasahitza>";

        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                // Bezeroaren saioa hasita ez dagoela egiaztatu
                if (client.erabiltzailea != null)
                    throw new DeniedException("Iada saioa hasita");

                // Deskargak egiteko saioa ez dela egiaztatu
                if (args[0] == "download" && args[1] == "download")
                    client.erabiltzailea = new(0, "download", "download", ConnectionType.download, "");
                else
                {
                    // Erabiltzailea dagoeneko saio batean dagoen egiaztatu
                    foreach (var list in Server.clients.Values)
                        if (list.Any(c => c.ToString() == args[0]))
                            throw new DeniedException($"'{args[0]}' saioa okupatuta");

                    // Kredentzialak egiaztatu eta erabiltzailea sortu
                    client.erabiltzailea = await HizkitzaDB.LoginErabiltzailea(args[0], args[1]);

                    /*if (args[0] == "admin" && args[1] == "admin")
                        client.erabiltzailea = new(0, "admin", "admin", ConnectionType.admin, "");
                    else if (args[0] == "user" && args[1] == "user")
                        client.erabiltzailea = new(0, "user", "user", ConnectionType.user, "");
                    else throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");*/
                }
            }
            catch (Exception e) when (e is IndexOutOfRangeException || e is InvalidOperationException)
            {
                throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
            }
        }

        
    }


    // Log bidalketa gaitzeko komandoa
    private class LogSenderCommand : ICommand
    {
        public string Format => "LogSender <bool>";

        // Gertaerari suskribatutako bezeroak
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            // Gertaerei suskribatu lehen aldiz exekutatzerakoan
            if (!subscribed)
            {
                Server.LogSentEvent += SendLog;
                subscribed = true;
            }

            try
            {
                if (bool.Parse(args[0]))
                {
                    if (clients.Contains(client))
                        throw new DeniedException("LogSender iada true");
                    client.DisconnectedEvent += ClientDisconnected;
                    clients.Add(client);
                }
                else
                {
                    if (!clients.Contains(client))
                        throw new DeniedException("LogSender iada false");
                    client.DisconnectedEvent -= ClientDisconnected;
                    clients.Remove(client);
                }
            } catch (FormatException)
            {
                throw new WrongCommandFormatException(Format);
            }
        }

        // Bezeroei log berriak bidali
        private void SendLog(object? sender, Server.LogSentEventArgs e)
        {
            foreach (var client in clients)
                client.Send($"Data Log [{DateTime.Now:t}] [{e.Mota}] {e.Log}");
        }

        // Bezeroa deskonektatzen denean desuskribatu
        private void ClientDisconnected(object? sender, EventArgs e)
        {
            var client = sender as ServersideClient;
            client.DisconnectedEvent -= ClientDisconnected;
            clients.Remove(client);
        }
    }


    // Partida bidalketa gaitzeko komandoa
    private class GameUpdaterCommand : ICommand
    {
        public string Format => "GameUpdater <bool>";

        // Gertaerari suskribatutako bezeroak
        private readonly List<ServersideClient> clients = [];
        private bool subscribed = false;

        public async Task Execute(string[] args, ServersideClient client)
        {
            // Gertaerei suskribatu lehen aldiz exekutatzerakoan
            if (!subscribed)
            {
                Server.GamesUpdateEvent += GamesUpdate;
                subscribed = true;
            }

            try
            {
                if (bool.Parse(args[0]))
                {
                    if (clients.Contains(client))
                        throw new DeniedException("GameUpdater iada true");
                    clients.Add(client);
                    client.DisconnectedEvent += ClientDisconnected;
                    client.Send($"Data Games {string.Join(" ", Server.partidak)}");
                }
                else
                {
                    if (!clients.Contains(client))
                        throw new DeniedException("GameUpdater iada false");
                    client.DisconnectedEvent -= ClientDisconnected;
                    clients.Remove(client);
                }
            }
            catch (FormatException)
            {
                throw new WrongCommandFormatException(Format);
            }
        }

        // Bezeroei partida zerrenda bidali
        private void GamesUpdate(object? sender, EventArgs e)
        {
            foreach (var client in clients)
                client.Send($"Data Games {string.Join(" ", Server.partidak)}");
        }

        // Bezeroa deskonektatzen denean desuskribatu
        private void ClientDisconnected(object? sender, EventArgs e)
        {
            var client = sender as ServersideClient;
            client.DisconnectedEvent -= ClientDisconnected;
            clients.Remove(client);
        }
    }


    // Partida berria sortzeko komandoa
    private class NewGameCommand : ICommand
    {
        public string Format => "NewGame <izena> <mapa>";
        public async Task Execute(string[] args, ServersideClient client)
        {
            if (args.Length > 2) throw new IndexOutOfRangeException();
            var newGame = new Game(args[0], args[1]);
            if (Server.partidak.Contains(newGame)) throw new DeniedException($"'{args[0]}' izena okupatuta");
            Server.NewGame(newGame);
        }
    }

    // Patida komandoak
    private class GameCommands
    {
        // Partida batean sartzeko komandoa
        public class JoinGameCommand : ICommand
        {
            public string Format => "JoinGame <izena> <itxura> <kolorea>";
            public async Task Execute(string[] args, ServersideClient client)
            {
                client.currentGame = Server.partidak.FirstOrDefault(p => p.Izena == args[0]) ?? throw new DeniedException($"'{args[0]}' izeneko partida ez da existitzen");
                client.itxura = char.Parse(args[1]);
                client.kolorea = args[2];
                client.Send($"InGame true {client.currentGame.Izena} {client.currentGame.Mapa}");
                client.currentGame.AddPlayer(client);
                client.DisconnectedEvent += Disconnect;
            }
        }

        // Partida uzteko komandoa
        public class LeaveGameCommand : ICommand
        {
            public string Format => "LeaveGame";
            public async Task Execute(string[] args, ServersideClient client)
            {
                var partida = client.currentGame ?? throw new DeniedException($"Ez zaude partida batean sartuta");
                partida.RemovePlayer(client);
                client.itxura = null;
                client.kolorea = null;
                client.currentGame = null;
                client.DisconnectedEvent -= Disconnect;
                client.Send($"InGame false null null");
            }
        }

        private static void Disconnect(object? sender, EventArgs e)
        {
            var client = sender as ServersideClient;
            client.currentGame?.RemovePlayer(client);
            client.itxura = null;
            client.kolorea = null;
            client.DisconnectedEvent -= Disconnect;
        }


        // Partida barruko mezuak bidaltzeko komandoa
        public class GameMessageCommand : ICommand
        {
            public string Format => "GameMessage ...";
            public async Task Execute(string[] args, ServersideClient client)
            {
                var partida = client.currentGame ?? throw new DeniedException($"Ez zaude partida batean sartuta");
                try
                {
                    foreach (var player in partida.GetPlayers())
                        player.Send($"Data Message {client.kolorea} {client.erabiltzailea!.Izena}_{client.itxura}: {string.Join(" ", args[..])}");
                }
                catch (FormatException)
                {
                    throw new WrongCommandFormatException(Format);
                }
            }
        }
    }


    // Fitxategi bat bidaltzeko komandoa
    private class DownloadCommand : ICommand
    {
        public string Format => "Download <file> <arg>";
        public async Task Execute(string[] args, ServersideClient client)
        {
            try
            {
                // Fitxategia sortu eta hartu
                var filePath = "";
                if (args[0] == "ErabiltzaileInforme")
                    filePath = await PDFGenerator.ErabiltzaileTxostena(args[1]);
                else if (args[0] == "PartidakInforme")
                    filePath = await PDFGenerator.PartidakTxostena();

                using FileStream fileStream = File.OpenRead(filePath);

                // Fitxategiaren tamaina bidali
                byte[] sizeBytes = BitConverter.GetBytes(fileStream.Length);
                client.SendBytes(sizeBytes, 8);

                // Utzik badago itzuli
                if (sizeBytes.Length == 0) return;

                // 4 KB-eko zatietan bidali
                byte[] buffer = new byte[4096];
                int bytesRead;
                while ((bytesRead = await fileStream.ReadAsync(buffer)) > 0)
                {
                    Thread.Sleep(250);
                    client.SendBytes(buffer, bytesRead);
                }
            }
            catch
            {
                Server.NewLog($"Download error: file not found", LogType.ERROR);
                client.SendBytes([0], 8);
            }
        }
    }
}