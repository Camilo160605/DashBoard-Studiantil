using System.Security.Claims;
using Domain.Entities;

namespace Api.Services;

public interface IJwtTokenService
{
    (string Token, int ExpiresIn) GenerateToken(AppUser user, IEnumerable<Claim>? additionalClaims = null);
}
