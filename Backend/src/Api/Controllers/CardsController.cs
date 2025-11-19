using Api.Authorization;
using Api.DTOs.Cards;
using Domain.Entities;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers;

[ApiController]
[Route("api/v1")]
[Authorize]
public class CardsController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;

    public CardsController(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    [HttpGet("boards/{boardId:guid}/cards")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(IEnumerable<CardDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCards(Guid boardId)
    {
        var cards = await _dbContext.Cards
            .AsNoTracking()
            .Where(c => c.BoardId == boardId)
            .OrderBy(c => c.Column != null ? c.Column.Position : 0)
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
                AssigneeEmail = c.Assignee != null ? c.Assignee.Email : null,
                DueDate = c.DueDate
            })
            .ToListAsync();

        return Ok(cards);
    }

    [HttpPost("boards/{boardId:guid}/cards")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status201Created)]
    public async Task<IActionResult> CreateCard(Guid boardId, [FromBody] CreateCardRequest request)
    {
        var column = await _dbContext.Columns
            .FirstOrDefaultAsync(c => c.Id == request.ColumnId && c.BoardId == boardId);

        if (column is null)
        {
            return BadRequest("La columna no pertenece al tablero indicado.");
        }

        var maxPosition = await _dbContext.Cards
            .Where(c => c.ColumnId == request.ColumnId)
            .MaxAsync(c => (decimal?)c.Position) ?? 0;

        var card = new Card
        {
            BoardId = boardId,
            ColumnId = request.ColumnId,
            Title = request.Title,
            Description = request.Description,
            Position = maxPosition + 1,
            AssigneeId = request.AssigneeId,
            DueDate = request.DueDate
        };

        await _dbContext.Cards.AddAsync(card);
        await _dbContext.SaveChangesAsync();

        var dto = await MapCardAsync(card.Id);
        return CreatedAtAction(nameof(GetCards), new { boardId }, dto);
    }

    [HttpPut("cards/{id:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCard(Guid id, [FromBody] UpdateCardRequest request)
    {
        var card = await _dbContext.Cards.FindAsync(id);
        if (card is null)
        {
            return NotFound();
        }

        card.Title = request.Title;
        card.Description = request.Description;
        card.AssigneeId = request.AssigneeId;
        card.DueDate = request.DueDate;
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("cards/{id:guid}")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCard(Guid id)
    {
        var card = await _dbContext.Cards.FindAsync(id);
        if (card is null)
        {
            return NotFound();
        }

        _dbContext.Cards.Remove(card);
        await _dbContext.SaveChangesAsync();
        return NoContent();
    }

    [HttpPatch("boards/{boardId:guid}/cards/move")]
    [Authorize(Policy = BoardPolicies.BoardOwner)]
    [ProducesResponseType(typeof(CardDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MoveCard(Guid boardId, [FromBody] MoveCardRequest request)
    {
        var card = await _dbContext.Cards.FirstOrDefaultAsync(c => c.Id == request.CardId && c.BoardId == boardId);
        if (card is null)
        {
            return NotFound();
        }

        var targetColumn = await _dbContext.Columns
            .FirstOrDefaultAsync(c => c.Id == request.ToColumnId && c.BoardId == boardId);

        if (targetColumn is null)
        {
            return BadRequest("La columna destino no pertenece al tablero.");
        }

        await using var transaction = await _dbContext.Database.BeginTransactionAsync();

        card.ColumnId = targetColumn.Id;
        card.Position = await CalculateNewPositionAsync(request, card);

        await _dbContext.SaveChangesAsync();
        await transaction.CommitAsync();

        var dto = await MapCardAsync(card.Id);
        return Ok(dto);
    }

    private async Task<decimal> CalculateNewPositionAsync(MoveCardRequest request, Card card)
    {
        decimal newPosition;
        if (request.PrevPos.HasValue && request.NextPos.HasValue)
        {
            var gap = request.NextPos.Value - request.PrevPos.Value;
            if (Math.Abs(gap) < 0.0001m)
            {
                await NormalizePositionsAsync(card.ColumnId);
                newPosition = request.PrevPos.Value + 1;
            }
            else
            {
                newPosition = (request.PrevPos.Value + request.NextPos.Value) / 2m;
            }
        }
        else if (request.PrevPos.HasValue)
        {
            newPosition = request.PrevPos.Value + 1;
        }
        else if (request.NextPos.HasValue)
        {
            newPosition = request.NextPos.Value - 1;
        }
        else
        {
            var maxPosition = await _dbContext.Cards
                .Where(c => c.ColumnId == request.ToColumnId)
                .MaxAsync(c => (decimal?)c.Position) ?? 0;
            newPosition = maxPosition + 1;
        }

        return newPosition;
    }

    private async Task NormalizePositionsAsync(Guid columnId)
    {
        var cards = await _dbContext.Cards
            .Where(c => c.ColumnId == columnId)
            .OrderBy(c => c.Position)
            .ToListAsync();

        var index = 1;
        foreach (var card in cards)
        {
            card.Position = index++;
        }
    }

    private async Task<CardDto> MapCardAsync(Guid cardId)
    {
        return await _dbContext.Cards
            .AsNoTracking()
            .Include(c => c.Assignee)
            .Where(c => c.Id == cardId)
            .Select(c => new CardDto
            {
                Id = c.Id,
                BoardId = c.BoardId,
                ColumnId = c.ColumnId,
                Title = c.Title,
                Description = c.Description,
                Position = c.Position,
                AssigneeId = c.AssigneeId,
                AssigneeEmail = c.Assignee != null ? c.Assignee.Email : null,
                DueDate = c.DueDate
            })
            .FirstAsync();
    }
}
