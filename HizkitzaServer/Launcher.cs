using HizkitzaServer.util.connection;

namespace HizkitzaServer
{
    class Launcher
    {
        public static void Main(string[] args)
        {
            Server server = new()
            {
                MessageSentEvent = (message) => 
                {
                    Console.WriteLine($"[MESSAGE] {message}");
                },

                LogSentEvent = (log, good) => 
                {
                    string status = good ? "INFO" : "ERROR";
                    Console.WriteLine($"[{status}] {log}");
                },

                ClientConnectedEvent = (client) => 
                {
                    //Console.WriteLine($"[CLIENT CONNECTED] {client.Izena}");
                },

                ClientDisconnectedEvent = (client) => 
                {
                    //Console.WriteLine($"[CLIENT DISCONNECTED] {client.Izena}");
                }
            };
            server.Piztu();
        }
    } 
}