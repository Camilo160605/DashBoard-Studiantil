using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Cards;

public class MoveCardRequest
{
    [Required]
    public Guid CardId { get; set; }

    [Required]
    public Guid ToColumnId { get; set; }

    public decimal? PrevPos { get; set; }
    public decimal? NextPos { get; set; }
}
