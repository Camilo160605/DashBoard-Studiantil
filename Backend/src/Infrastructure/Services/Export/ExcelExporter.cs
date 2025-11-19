using ClosedXML.Excel;
using Domain.Entities;

namespace Infrastructure.Services.Export;

public class ExcelExporter : IExcelExporter
{
    public Task<byte[]> GenerateBoardExcelAsync(Board board, CancellationToken cancellationToken = default)
    {
        using var workbook = new XLWorkbook();
        var worksheet = workbook.AddWorksheet(board.Name[..Math.Min(board.Name.Length, 25)]);

        var currentRow = 1;
        worksheet.Cell(currentRow, 1).Value = board.Name;
        worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
        worksheet.Cell(currentRow, 1).Style.Font.FontSize = 18;
        worksheet.Cell(currentRow, 2).Value = DateTime.UtcNow;
        worksheet.Cell(currentRow, 2).Style.DateFormat.Format = "yyyy-MM-dd HH:mm";
        currentRow += 2;

        foreach (var column in board.Columns.OrderBy(c => c.Position))
        {
            worksheet.Cell(currentRow, 1).Value = column.Name;
            worksheet.Cell(currentRow, 1).Style.Font.Bold = true;
            worksheet.Range(currentRow, 1, currentRow, 3).Merge();
            currentRow++;

            worksheet.Cell(currentRow, 1).Value = "Título";
            worksheet.Cell(currentRow, 2).Value = "Descripción";
            worksheet.Cell(currentRow, 3).Value = "Asignado";
            worksheet.Range(currentRow, 1, currentRow, 3).Style.Font.Bold = true;
            currentRow++;

            var cards = board.Cards
                .Where(c => c.ColumnId == column.Id)
                .OrderBy(c => c.Position)
                .ToList();

            if (!cards.Any())
            {
                worksheet.Cell(currentRow, 1).Value = "(Sin tarjetas)";
                worksheet.Range(currentRow, 1, currentRow, 3).Merge();
                currentRow++;
                continue;
            }

            foreach (var card in cards)
            {
                worksheet.Cell(currentRow, 1).Value = card.Title;
                worksheet.Cell(currentRow, 2).Value = card.Description ?? string.Empty;
                worksheet.Cell(currentRow, 3).Value = card.Assignee?.Email ?? string.Empty;
                currentRow++;
            }

            currentRow++;
        }

        worksheet.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return Task.FromResult(stream.ToArray());
    }
}
