namespace Domain.Entities;

public class Column
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Position { get; set; }

    public Board? Board { get; set; }
    public ICollection<Card> Cards { get; set; } = new List<Card>();
}
