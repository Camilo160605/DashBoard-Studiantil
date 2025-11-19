namespace Domain.Entities;

public class Board
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;

    public AppUser? Owner { get; set; }
    public ICollection<Column> Columns { get; set; } = new List<Column>();
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
