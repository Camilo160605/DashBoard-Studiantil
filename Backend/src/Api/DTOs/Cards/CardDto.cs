namespace Api.DTOs.Cards;

public class CardDto
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public Guid ColumnId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Position { get; set; }
    public string? AssigneeId { get; set; }
    public string? AssigneeEmail { get; set; }
    public DateTime? DueDate { get; set; }
}
