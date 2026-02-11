using HizkitzaServer.util.db;
using QuestPDF.Fluent;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.pdf
{
    public static class PDFGenerator
    {
        // Erabiltzaile txostena sortu eta helbidea itzuli
        public static async Task<string> ErabiltzaileTxostena(string user)
        {
            var erabiltzailea = await HizkitzaDB.GetErabiltzailea(user);
            var erabiltzaileStats = await HizkitzaDB.ErabiltzaileStats(erabiltzailea.Izena);
            var partidaFamatuena = await HizkitzaDB.ErabiltzailePartidaFamatuena(erabiltzailea.Izena);

            Directory.CreateDirectory("txostenak");

            if (erabiltzailea != null && erabiltzaileStats != null && partidaFamatuena != null)
            {
                var path = $"txostenak\\{erabiltzailea.Izena}-{DateTime.Now.Ticks}.pdf";
                new ErabiltzaileTxostena(erabiltzailea, erabiltzaileStats, partidaFamatuena)
                    .GeneratePdf(path);
                return path;
            }
            return string.Empty;
        }

        // Partidak txostena sortu eta helbidea itzuli
        public static async Task<string> PartidakTxostena()
        {
            var top10Partidak = await HizkitzaDB.Top10Partidak();
            var mapaFamatuena = await HizkitzaDB.MapaFamatuena();
            var partidaLuzeena = await HizkitzaDB.PartidaLuzeena();
            var dataAktiboena = await HizkitzaDB.DataAktiboena();

            Directory.CreateDirectory("txostenak");

            if (top10Partidak != null && mapaFamatuena != null && partidaLuzeena != null && dataAktiboena != null)
            {
                var path = $"txostenak\\Partidak-{DateTime.Now.Ticks}.pdf";
                new PartidakTxostena([.. top10Partidak], mapaFamatuena, partidaLuzeena, dataAktiboena)
                    .GeneratePdf(path);
                return path;
            }
            return string.Empty;
        }
    }
}
