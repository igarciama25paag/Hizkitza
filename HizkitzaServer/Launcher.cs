using HizkitzaServer.util.connection;

namespace HizkitzaServer
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            Server.MessageArrivedEvent += (sender, args) =>
            {
                Server.LogBerria($"{args.Client}: {args.Mezua}", Server.LogType.INFO);
            };
            Server.LogSentEvent += (sender, args) =>
            {
                Console.WriteLine($"[{DateTime.Now:t}] [{args.Mota}] {args.Log}");
            };
            Server.Piztu();
        }
    } 
}