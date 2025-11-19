using Api.Authorization;
using Infrastructure.Data;
using Infrastructure.Services.Export;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/boards/{boardId:guid}/export")]
[Authorize]
public class ExportController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IExcelExporter _excelExporter;
    private readonly IPdfExporter _pdfExporter;

    public ExportController(
        ApplicationDbContext dbContext,
        IExcelExporter excelExporter,
        IPdfExporter pdfExporter)
    {
        _dbContext = dbContext;
        _excelExporter = excelExporter;
        _pdfExporter = pdfExporter;
    }

    [HttpGet("excel")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    public async Task<IActionResult> ExportExcel(Guid boardId)
    {
        var board = await LoadBoardAsync(boardId);
        if (board is null)
        {
            return NotFound();
        }

        var bytes = await _excelExporter.GenerateBoardExcelAsync(board);
        return File(bytes,
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            $"board-{boardId}.xlsx");
    }

    [HttpGet("pdf")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    public async Task<IActionResult> ExportPdf(Guid boardId)
    {
        var board = await LoadBoardAsync(boardId);
        if (board is null)
        {
            return NotFound();
        }

        var bytes = await _pdfExporter.GenerateBoardPdfAsync(board);
        return File(bytes, "application/pdf", $"board-{boardId}.pdf");
    }

    private async Task<Domain.Entities.Board?> LoadBoardAsync(Guid boardId)
    {
        return await _dbContext.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .Include(b => b.Cards)
                .ThenInclude(c => c.Assignee)
            .FirstOrDefaultAsync(b => b.Id == boardId);
    }
}
