using Microsoft.AspNetCore.Identity;

namespace Domain.Entities;

public class AppUser : IdentityUser
{
    public ICollection<Board> Boards { get; set; } = new List<Board>();
    public ICollection<Card> AssignedCards { get; set; } = new List<Card>();
}
