using Api.Authorization;
using Api.DTOs.Boards;
using Api.DTOs.Cards;
using Api.DTOs.Columns;
using Api.Extensions;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1/boards")]
[Authorize]
public class BoardsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public BoardsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<BoardSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBoards()
    {
        var userId = User.GetUserId();
        var boards = await _dbContext.Boards
            .AsNoTracking()
            .Where(b => b.OwnerId == userId)
            .Select(b => new BoardSummaryDto
            {
                Id = b.Id,
                Name = b.Name
            })
            .ToListAsync();

        return Ok(boards);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(BoardDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBoard(Guid id)
    {
        var board = await _dbContext.Boards
            .AsNoTracking()
            .Include(b => b.Columns)
            .Include(b => b.Cards)
                .ThenInclude(c => c.Assignee)
            .Include(b => b.Cards)
                .ThenInclude(c => c.Column)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (board is null)
        {
            return NotFound();
        }

        var response = new BoardDetailDto
        {
            Id = board.Id,
            Name = board.Name,
            Columns = board.Columns
                .OrderBy(c => c.Position)
                .Select(c => new ColumnDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    Name = c.Name,
                    Position = c.Position
                })
                .ToArray(),
            Cards = board.Cards
                .OrderBy(c => c.Column!.Position)
                .ThenBy(c => c.Position)
                .Select(c => new CardDto
                {
                    Id = c.Id,
                    BoardId = c.BoardId,
                    ColumnId = c.ColumnId,
                    Title = c.Title,
                    Description = c.Description,
                    Position = c.Position,
                    AssigneeId = c.AssigneeId,
                    AssigneeEmail = c.Assignee?.Email,
                    DueDate = c.DueDate
                })
                .ToArray()
        };

        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(BoardSummaryDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
    {
        var userId = User.GetUserId();
        var board = new Board
        {
            Name = request.Name,
            OwnerId = userId
        };

        await _dbContext.Boards.AddAsync(board);
        await _dbContext.SaveChangesAsync();

        var response = new BoardSummaryDto
        {
            Id = board.Id,
            Name = board.Name
        };

        return CreatedAtAction(nameof(GetBoard), new { id = board.Id }, response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBoard(Guid id, [FromBody] UpdateBoardRequest request)
    {
        var board = await _dbContext.Boards.FindAsync(id);
        if (board is null)
        {
            return NotFound();
        }

        board.Name = request.Name;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBoard(Guid id)
    {
        var board = await _dbContext.Boards.FindAsync(id);
        if (board is null)
        {
            return NotFound();
        }

        _dbContext.Boards.Remove(board);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }
}
