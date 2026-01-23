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

                Console.WriteLine,

                (log, good) =>
                {
                    Console.WriteLine(log);
                }
            );
            Server.Piztu();
        }
    } 
}