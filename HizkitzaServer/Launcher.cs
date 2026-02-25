using HizkitzaServer.util.connection;
using HizkitzaServer.util.db;
using HizkitzaServer.util.pdf;
using QuestPDF.Infrastructure;

namespace HizkitzaServer
{
    class Launcher
    {
        // Konfigurazio fitxategia
        private const string CONF_FILE = "conf.txt";

        // Zerbitzaria hasi
        public static void Main(string[] args)
        {
            QuestPDF.Settings.License = LicenseType.Community;
            try
            {
                GetConf();
                Server.LogSentEvent += (sender, args) =>
                {
                    Console.WriteLine($"[{DateTime.Now:t}] [{args.Mota}] {args.Log}");
                };
                Server.TurnOn();
            }
            catch (ConfigurationException e)
            {
                Console.WriteLine("Configurazio errorea: " + e.Message);
                Console.ReadLine();
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine("Fitxategi errorea: " + e.Message);
                CreateDefaultConfFile();
                Console.WriteLine($"\n{CONF_FILE} fitxategi berri bat sortu da konfigurazio lehenetsiekin");
                Console.ReadLine();
            }
            catch (Exception e)
            {
                Console.WriteLine("Launch errorea: " + e.Message);
                Console.ReadLine();
            }
        }

        // Konfigurazio salbuespena
        private class ConfigurationException(string message) : Exception(message);

        // Konfigurazio zerrenda
        private static readonly List<string> Configurations =
            [
            "HizkitzaPort",
            "PostgresHost",
            "PostgresPort",
            "PostgresUser",
            "PostgresPass"
            ];

        // Konfigurazioak lortu CONF_FILE fitxategitik
        private static void GetConf()
        {
            // Fitxategia lortu eta irakurri
            var confPath = Directory.GetCurrentDirectory() + "\\" + CONF_FILE;
            var lines = new List<string>();
            using (StreamReader reader = new(new FileStream(confPath, FileMode.Open, FileAccess.ReadWrite)))
            {
                var line = reader.ReadLine();
                while (line != null)
                {
                    lines.Add(line);
                    line = reader.ReadLine();
                }
            }

            // Konfigurazioak lortu
            foreach (var line in lines)
            {
                if (string.IsNullOrEmpty(line)) continue;
                var conf = line.Split('=');
                switch (conf[0])
                {
                    case "HizkitzaPort":
                        Server.PORT = int.Parse(conf[1]);
                        break;
                    case "PostgresHost":
                        HizkitzaDB.HOST = conf[1];
                        break;
                    case "PostgresPort":
                        HizkitzaDB.PORT = conf[1];
                        break;
                    case "PostgresUser":
                        HizkitzaDB.USERNAME = conf[1];
                        break;
                    case "PostgresPass":
                        HizkitzaDB.PASSWORD = conf[1];
                        break;
                    default:
                        throw new ConfigurationException($"{conf[0]} konfigurazioa ez da existitzen");
                }
                Configurations.Remove(conf[0]);
            }

            // Konfigurazioren bat falta bada salbuespena bota
            foreach (var conf in Configurations)
                Console.WriteLine($"Konfigurazio falta: {conf}");

            if (Configurations.Count > 0)
                throw new ConfigurationException("Konfigurazio falta");
        }

        // CONF_FILE fitxategi berri bat sortu konfigurazio lehenetsiekin
        private static void CreateDefaultConfFile()
        {
            var confPath = Directory.GetCurrentDirectory() + "\\" + CONF_FILE;
            File.Create(confPath).Close();

            using StreamWriter writer = new(new FileStream(confPath, FileMode.OpenOrCreate, FileAccess.ReadWrite));
            writer.WriteLine("HizkitzaPort=5000");
            writer.WriteLine("PostgresHost=localhost");
            writer.WriteLine("PostgresPort=5432");
            writer.WriteLine("PostgresUser=postgres");
            writer.WriteLine("PostgresPass=postgres");
        }
    } 
}