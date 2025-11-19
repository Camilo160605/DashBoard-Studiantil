using Api.Authorization;
using Api.DTOs.Columns;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/boards/{boardId:guid}/columns")]
[Authorize]
public class ColumnsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public ColumnsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(IEnumerable<ColumnDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetColumns(Guid boardId)
    {
        var columns = await _dbContext.Columns
            .AsNoTracking()
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Position)
            .Select(c => new ColumnDto
            {
                Id = c.Id,
                BoardId = c.BoardId,
                Name = c.Name,
                Position = c.Position
            })
            .ToListAsync();

        return Ok(columns);
    }

    [HttpPost]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(ColumnDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateColumn(Guid boardId, [FromBody] CreateColumnRequest request)
    {
        var lastPosition = await _dbContext.Columns
            .Where(c => c.BoardId == boardId)
            .MaxAsync(c => (decimal?)c.Position) ?? 0;

        var column = new Domain.Entities.Column
        {
            BoardId = boardId,
            Name = request.Name,
            Position = lastPosition + 1
        };

        await _dbContext.Columns.AddAsync(column);
        await _dbContext.SaveChangesAsync();

        var dto = new ColumnDto
        {
            Id = column.Id,
            BoardId = column.BoardId,
            Name = column.Name,
            Position = column.Position
        };

        return CreatedAtAction(nameof(GetColumns), new { boardId }, dto);
    }

    [HttpPut("{columnId:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateColumn(Guid boardId, Guid columnId, [FromBody] UpdateColumnRequest request)
    {
        var column = await _dbContext.Columns
            .FirstOrDefaultAsync(c => c.Id == columnId && c.BoardId == boardId);

        if (column is null)
        {
            return NotFound();
        }

        column.Name = request.Name;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{columnId:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteColumn(Guid boardId, Guid columnId)
    {
        var column = await _dbContext.Columns
            .FirstOrDefaultAsync(c => c.Id == columnId && c.BoardId == boardId);

        if (column is null)
        {
            return NotFound();
        }

        _dbContext.Columns.Remove(column);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("reorder")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> ReorderColumns(Guid boardId, [FromBody] IEnumerable<ReorderColumnRequest> request)
    {
        var columnIds = request.Select(r => r.ColumnId).ToHashSet();
        var columns = await _dbContext.Columns
            .Where(c => c.BoardId == boardId && columnIds.Contains(c.Id))
            .ToListAsync();

        foreach (var move in request)
        {
            var column = columns.FirstOrDefault(c => c.Id == move.ColumnId);
            if (column is not null)
            {
                column.Position = move.NewPosition;
            }
        }

        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
