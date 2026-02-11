using HizkitzaServer.util.data;
using HizkitzaServer.util.db;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HizkitzaServer.util.pdf
{
    internal class ErabiltzaileTxostena(Erabiltzailea erabiltzailea, ErabiltzaileStats erabiltzaileStats, PartidaStats partidaFamatuena) : IDocument
    {
        public void Compose(IDocumentContainer container)
        {
            container
                .Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.MarginHorizontal(3, Unit.Centimetre);
                    page.MarginVertical(2.25f, Unit.Centimetre);
                    page.PageColor(Colors.White);

                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("Erabiltzaile Txostena")
                                .Black()
                                .FontSize(20)
                                .AlignCenter();

                            column.Item().Height(0.5f, Unit.Centimetre);

                            column.Item().Text($"Izena: {erabiltzailea.Izena}");
                            column.Item().Text($"Sorkuntza data: {erabiltzailea.Sorkuntza_data}");

                            column.Item().Height(0.5f, Unit.Centimetre);

                            AddListItem(1, "•", $"Azken itxura: {erabiltzaileStats.Azken_itxura}");
                            AddListItem(1, "•", $"Azken kolorea: {erabiltzaileStats.Azken_kolorea}");
                            AddListItem(1, "•", $"Zenbat partidetan sartu: {erabiltzaileStats.Partida_sartu_n}");
                            AddListItem(1, "•", $"Denbora maximoa partidan: {erabiltzaileStats.Partida_t_max}");
                            AddListItem(1, "•", $"Azken sartutako partida: {erabiltzaileStats.Azken_sartu_partida}");

                            column.Item().Height(1f, Unit.Centimetre);

                            column.Item()
                                .Text("Partida Famatuena")
                                .Black()
                                .FontSize(16)
                                .AlignCenter();

                            column.Item().Height(0.5f, Unit.Centimetre);

                            column.Item().Text($"Izena: {partidaFamatuena.Izena}");
                            column.Item().Text($"Sorkuntza data: {partidaFamatuena.Sorkuntza_data}");

                            column.Item().Height(0.5f, Unit.Centimetre);

                            AddListItem(1, "•", $"Lortutako erabiltzaile maximoa: {partidaFamatuena.Erabiltzaile_max}");
                            AddListItem(1, "•", $"Iraupena: {partidaFamatuena.Iraupena}");
                            AddListItem(1, "•", $"Mapa: {partidaFamatuena.Mapa}");

                            void AddListItem(int nestingLevel, string bulletText, string text)
                            {
                                column.Item().Row(row =>
                                {
                                    row.ConstantItem(25 * nestingLevel);
                                    row.ConstantItem(25).Text(bulletText);
                                    row.RelativeItem().Text(text);
                                });
                            }
                        });
                });
        }
    }
}
