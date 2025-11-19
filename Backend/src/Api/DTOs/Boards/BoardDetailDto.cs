using Api.DTOs.Cards;
using Api.DTOs.Columns;

namespace Api.DTOs.Boards;

public class BoardDetailDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public IReadOnlyCollection<ColumnDto> Columns { get; set; } = Array.Empty<ColumnDto>();
    public IReadOnlyCollection<CardDto> Cards { get; set; } = Array.Empty<CardDto>();
}
