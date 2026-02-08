using HizkitzaServer.util.connection;
using HizkitzaServer.util.db;

namespace HizkitzaServer
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            Server.MessageArrivedEvent += (sender, args) =>
            {
                Server.NewLog($"{args.Client} {args.Mezua}", Server.LogType.INFO);
            };
            Server.LogSentEvent += (sender, args) =>
            {
                Console.WriteLine($"[{DateTime.Now:t}] [{args.Mota}] {args.Log}");
            };
            Server.TurnOn();
            Tests();
        }

        private static async void Tests()
        {
            /*Console.WriteLine("Ane stats: " + (await HizkitzaDB.ErabiltzaileStats("Ane")).ToString());
            /*Console.WriteLine("Aneren partida famatuena: " + await HizkitzaDB.ErabiltzailePartidaFamatua("Ane"));
            Console.WriteLine("Data aktiboena: " + await HizkitzaDB.DataAktiboena());
            Console.WriteLine("Partida luzeena: " + await HizkitzaDB.PartidaLuzeena());
            Console.WriteLine("Mapa famatuena: " + await HizkitzaDB.MapaFamatuena());
            int n = 0;
            foreach (var item in await HizkitzaDB.Top10Partidak())
            {
                n++;
                Console.WriteLine($"Top {n}:" + item);
            }*/
        }
    } 
}