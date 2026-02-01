using HizkitzaServer.util.connection;

namespace HizkitzaServer
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            Server.RootEvents(

                (client) => 
                {
                    //Console.WriteLine($"[CLIENT CONNECTED] {client.Izena}");
                },

                (client) => 
                {
                    //Console.WriteLine($"[CLIENT DISCONNECTED] {client.Izena}");
                },

                (mezua, bezero) =>
                {
                    Console.WriteLine($"[{DateTime.Now:t}] [{bezero}] {mezua}");
                }                
            );
            Server.LogSentEvent += (sender, args) =>
            {
                Console.WriteLine($"[{DateTime.Now:t}] [{args.Mota}] {args.Log}");
            };
            Server.Piztu();
        }
    } 
}