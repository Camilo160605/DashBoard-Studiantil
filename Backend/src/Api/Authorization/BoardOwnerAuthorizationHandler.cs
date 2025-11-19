using System.Security.Claims;
using Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace Api.Authorization;

public class BoardOwnerAuthorizationHandler : AuthorizationHandler<BoardOwnerRequirement>
{
    private readonly ApplicationDbContext _dbContext;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public BoardOwnerAuthorizationHandler(ApplicationDbContext dbContext, IHttpContextAccessor httpContextAccessor)
    {
        _dbContext = dbContext;
        _httpContextAccessor = httpContextAccessor;
    }

    protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, BoardOwnerRequirement requirement)
    {
        var userId = context.User.FindFirstValue("sub") ?? context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId))
        {
            context.Fail();
            return;
        }

        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext is null)
        {
            context.Fail();
            return;
        }

        var boardId = await ResolveBoardIdAsync(httpContext.Request.RouteValues, httpContext.RequestAborted);
        if (boardId is null)
        {
            context.Fail();
            return;
        }

        var ownsBoard = await _dbContext.Boards
            .AnyAsync(b => b.Id == boardId && b.OwnerId == userId, httpContext.RequestAborted);

        if (ownsBoard)
        {
            context.Succeed(requirement);
        }
        else
        {
            context.Fail();
        }
    }

    private async Task<Guid?> ResolveBoardIdAsync(RouteValueDictionary values, CancellationToken cancellationToken)
    {
        if (TryGetGuid(values, "boardId", out var boardId))
        {
            return boardId;
        }

        if (TryGetGuid(values, "id", out var id))
        {
            var controller = values.TryGetValue("controller", out var controllerValue)
                ? controllerValue?.ToString()
                : null;

            switch (controller)
            {
                case "Boards":
                    return id;
                case "Columns":
                    return await _dbContext.Columns
                        .Where(c => c.Id == id)
                        .Select(c => (Guid?)c.BoardId)
                        .FirstOrDefaultAsync(cancellationToken);
                case "Cards":
                    return await _dbContext.Cards
                        .Where(c => c.Id == id)
                        .Select(c => (Guid?)c.BoardId)
                        .FirstOrDefaultAsync(cancellationToken);
            }
        }

        if (TryGetGuid(values, "columnId", out var columnId))
        {
            return await _dbContext.Columns
                .Where(c => c.Id == columnId)
                .Select(c => (Guid?)c.BoardId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (TryGetGuid(values, "cardId", out var cardId))
        {
            return await _dbContext.Cards
                .Where(c => c.Id == cardId)
                .Select(c => (Guid?)c.BoardId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        return null;
    }

    private static bool TryGetGuid(RouteValueDictionary values, string key, out Guid result)
    {
        result = default;
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            return false;
        }

        return Guid.TryParse(raw.ToString(), out result);
    }
}
