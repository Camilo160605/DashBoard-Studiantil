using Domain.Entities;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Infrastructure.Services.Export;

public class PdfExporter : IPdfExporter
{
    public Task<byte[]> GenerateBoardPdfAsync(Board board, CancellationToken cancellationToken = default)
    {
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Margin(30);
                page.Size(PageSizes.A4);
                page.DefaultTextStyle(x => x.FontSize(12));
                page.Content().Column(column =>
                {
                    column.Spacing(15);
                    column.Item().AlignCenter().Text(board.Name).FontSize(26).SemiBold();
                    column.Item().AlignCenter().Text($"Generado: {DateTime.UtcNow:yyyy-MM-dd HH:mm} UTC");
                    column.Item().LineHorizontal(1).LineColor(Colors.Grey.Lighten2);

                    foreach (var boardColumn in board.Columns.OrderBy(c => c.Position))
                    {
                        column.Item().Text(boardColumn.Name).FontSize(18).SemiBold();
                        var cards = board.Cards
                            .Where(c => c.ColumnId == boardColumn.Id)
                            .OrderBy(c => c.Position)
                            .ToList();

                        if (!cards.Any())
                        {
                            column.Item().PaddingBottom(10).Text("(Sin tarjetas)").Italic().FontColor(Colors.Grey.Darken1);
                            continue;
                        }

                        column.Item().Table(table =>
                        {
                            table.ColumnsDefinition(columns =>
                            {
                                columns.RelativeColumn(3);
                                columns.RelativeColumn(4);
                                columns.RelativeColumn(2);
                            });

                            table.Header(header =>
                            {
                                header.Cell().Text("Título").SemiBold();
                                header.Cell().Text("Descripción").SemiBold();
                                header.Cell().Text("Asignado").SemiBold();
                            });

                            foreach (var card in cards)
                            {
                                table.Cell().Text(card.Title);
                                table.Cell().Text(card.Description ?? string.Empty);
                                table.Cell().Text(card.Assignee?.Email ?? string.Empty);
                            }
                        });
                    }
                });
            });
        });

        var pdf = document.GeneratePdf();
        return Task.FromResult(pdf);
    }
}
