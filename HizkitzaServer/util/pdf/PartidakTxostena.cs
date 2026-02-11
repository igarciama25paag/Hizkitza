using HizkitzaServer.util.data;
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
    internal class PartidakTxostena(PartidaStats[] top10Partidak, string mapaFamatuena, PartidaStats partidaLuzeena, string dataAktiboena) : IDocument
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
                                .Text("Partidak Txostena")
                                .Black()
                                .FontSize(20)
                                .AlignCenter();

                            column.Item().Height(0.5f, Unit.Centimetre);

                            column.Item()
                                .Text($"Top 10 partidak")
                                .Black()
                                .FontSize(16);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).AlignMiddle().Padding(8).Text("Izena").Bold().AlignCenter();
                                    header.Cell().BorderBottom(1).AlignMiddle().Padding(8).Text("Jokalari Kop.").Bold().AlignCenter();
                                    header.Cell().BorderBottom(1).AlignMiddle().Padding(8).Text("Data").Bold().AlignCenter();
                                    header.Cell().BorderBottom(1).AlignMiddle().Padding(8).Text("Iraupena").Bold().AlignCenter();
                                    header.Cell().BorderBottom(1).AlignMiddle().Padding(8).Text("Mapa").Bold().AlignCenter();
                                });

                                foreach (var partida in top10Partidak)
                                {
                                    table.Cell().BorderBottom(0.5f).AlignMiddle().Padding(4).Text(partida.Izena).AlignCenter();
                                    table.Cell().BorderBottom(0.5f).AlignMiddle().Padding(4).Text(partida.Erabiltzaile_max.ToString()).AlignCenter();
                                    table.Cell().BorderBottom(0.5f).AlignMiddle().Padding(4).Text(partida.Sorkuntza_data).AlignCenter();
                                    table.Cell().BorderBottom(0.5f).AlignMiddle().Padding(4).Text(partida.Iraupena).AlignCenter();
                                    table.Cell().BorderBottom(0.5f).AlignMiddle().Padding(4).Text(partida.Mapa).AlignCenter();
                                }
                            });

                            column.Item().Height(1f, Unit.Centimetre);

                            column.Item()
                                .Text($"Beste datu batzuk")
                                .Black()
                                .FontSize(16);

                            column.Item().Height(0.5f, Unit.Centimetre);

                            AddListItem(1, "•", $"Mapa famatuena: {mapaFamatuena}");
                            AddListItem(1, "•", $"Partida luzeena: {partidaLuzeena.Izena} / {partidaLuzeena.Iraupena}");
                            AddListItem(1, "•", $"Data aktiboena: {dataAktiboena}");

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
