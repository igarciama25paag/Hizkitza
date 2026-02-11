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

namespace HizkitzaServer.util.connection;

public static class CommandDecoder
{
    // Komando zerrenda
    private readonly static Dictionary<string, ICommand> Commands = new()
    {
        ["Login"] = new LoginCommand(),
        ["LogSender"] = new LogSenderCommand(),
        ["NewGame"] = new NewGameCommand(),
        ["GameUpdater"] = new GameUpdaterCommand(),
        ["Download"] = new DownloadCommand()
    };

    // Komando baimenak
    private readonly static Dictionary<string, Collection<ConnectionType>> Perms = new()
    {
        ["LogSender"] = [ConnectionType.admin],
        ["GameUpdater"] = [ConnectionType.admin, ConnectionType.user],
        ["NewGame"] = [ConnectionType.admin, ConnectionType.user],
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

                // Komandoaren formatu egokia egiaztatu
                var splitFormat = commandExe.Format.Split(' ');
                if (!(splitFormat[^1] == "..." && args.Count >= splitFormat.Length - 1) &&
                    args.Count != splitFormat.Length - 1)
                    throw new WrongCommandFormatException(commandExe.Format);

                // Komandoaren baimenak ikusi
                if (Perms.TryGetValue(commandName, out Collection<ConnectionType>? value) &&
                    (client.erabiltzailea == null || !value.Contains(client.erabiltzailea.Mota)))
                    throw new DeniedException("Baimenik gabe");

                // Komandoa exekutatu
                await commandExe.Execute([.. args], client);
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
        // Komandoaren formatua
        string Format { get; }

        // Komando exekuzioa
        Task Execute(string[] args, ServersideClient client);
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

                    // Pasahitza enkriptatu
                    //var pass = GetSHA256(args[1]);

                    // Kredentzialak egiaztatu eta erabiltzailea sortu
                    //client.erabiltzailea = await HizkitzaDB.LoginErabiltzailea(args[0], pass);
                    if (args[0] == "admin" && args[1] == "admin")
                        client.erabiltzailea = new(0, "admin", "admin", ConnectionType.admin, "");
                    else if (args[0] == "user" && args[1] == "user")
                        client.erabiltzailea = new(0, "user", "user", ConnectionType.user, "");
                    else throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
                }
            }
            catch (Exception e) when (e is IndexOutOfRangeException || e is InvalidOperationException)
            {
                throw new DeniedException($"Erabiltzaile edo pasahitz ezegokia");
            }
        }

        // Enkriptatu
        public static string GetSHA256(string str)
        {
            StringBuilder sb = new();
            byte[]? stream = SHA256.HashData(new ASCIIEncoding().GetBytes(str));
            for (int i = 0; i < stream.Length; i++) sb.AppendFormat("{0:x2}", stream[i]);
            return sb.ToString();
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
                Server.ClientDisconnectedEvent += ClientDisconnected;
                subscribed = true;
            }

            try
            {
                if (bool.Parse(args[0]))
                {
                    if (clients.Contains(client))
                        throw new DeniedException("LogSender iada true");
                    clients.Add(client);
                }
                else
                {
                    if (!clients.Contains(client))
                        throw new DeniedException("LogSender iada false");
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
        private void ClientDisconnected(object? sender, Server.ClientEventArgs e) => clients.Remove(e.Client);
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
                Server.ClientDisconnectedEvent += ClientDisconnected;
                subscribed = true;
            }

            try
            {
                if (bool.Parse(args[0]))
                {
                    if (clients.Contains(client))
                        throw new DeniedException("GameUpdater iada true");
                    clients.Add(client);
                    client.Send($"Data Games {string.Join(" ", Server.partidak)}");
                }
                else
                {
                    if (!clients.Contains(client))
                        throw new DeniedException("GameUpdater iada false");
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
        private void ClientDisconnected(object? sender, Server.ClientEventArgs e) => clients.Remove(e.Client);
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