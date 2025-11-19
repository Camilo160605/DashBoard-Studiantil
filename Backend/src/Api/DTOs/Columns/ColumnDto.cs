namespace Api.DTOs.Columns;

public class ColumnDto
{
    public Guid Id { get; set; }
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Position { get; set; }
}
