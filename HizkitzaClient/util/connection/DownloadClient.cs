using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shapes;
using static HizkitzaClient.util.connection.Client;

namespace HizkitzaClient.util.connection
{
    public static class DownloadClient
    {
        // Bezero objektuak
        private static TcpClient? client;
        private static NetworkStream? stream;
        private static StreamReader? reader;
        private static StreamWriter? writer;

        public static event EventHandler<DownloadStartedEventArgs>? DownloadStartedEvent;
        public class DownloadStartedEventArgs : EventArgs
        {
            public required string FileName { get; set; }
        }
        public static event EventHandler<DownloadEndedEventArgs>? DownloadEndedEvent;
        public class DownloadEndedEventArgs : EventArgs
        {
            public required bool Successfully { get; set; }
            public required string Reason { get; set; }
        }

        public readonly static Dictionary<string, byte[]?> downloading = [];

        public class DownloadException(string message) : Exception(message);

        public static async void DownloadBytes(IPAddress ip, int port, string fileName)
        {
            try
            {
                client = new();
                client.Connect(ip, port);

                stream = client.GetStream();
                reader = new StreamReader(stream);
                writer = new StreamWriter(stream) { AutoFlush = true };

                Send("Login download download");
                var mezua = reader?.ReadLine();
                if (mezua != "Logged download")
                    throw new DownloadException("Ezin izan da saioa hasi");

                Send($"Download {fileName}");

                // Recibir tamaño
                byte[] sizeBytes = new byte[8];
                await stream.ReadAsync(sizeBytes.AsMemory(0, 8));
                long fileSize = BitConverter.ToInt64(sizeBytes, 0);

                if (fileSize == 0)
                    throw new DownloadException("Fitxategia ez da existitzen");

                // Redimensionar array
                downloading.Add(fileName, new byte[fileSize]);
                DownloadStartedEvent?.Invoke(null, new()
                {
                    FileName = fileName
                });

                // Recibir en chunks y montar en el array
                long totalReceived = 0;
                byte[] buffer = new byte[4096];

                while (totalReceived < fileSize)
                {
                    int bytesToRead = (int)Math.Min(buffer.Length, fileSize - totalReceived);
                    int bytesRead = await stream.ReadAsync(buffer.AsMemory(0, bytesToRead));

                    if (bytesRead == 0) break;

                    // Copiar chunk al array principal
                    Buffer.BlockCopy(buffer, 0, downloading[fileName]!, (int)totalReceived, bytesRead);
                    totalReceived += bytesRead;
                }

                DownloadEndedEvent?.Invoke(null, new()
                {
                    Successfully = true,
                    Reason = $"'{fileName}' fitxategia jaitsi da"
                });
            }
            catch (Exception e)
            {
                DownloadEndedEvent?.Invoke(null, new()
                {
                    Successfully = false,
                    Reason = e.Message
                });
            }
            finally
            {
                CloseClient();
            }
        }

        // Bezeroa itxi
        public static void CloseClient()
        {
            client?.Close();
            stream?.Close();
            reader?.Close();
            writer?.Close();
        }
    }
}
