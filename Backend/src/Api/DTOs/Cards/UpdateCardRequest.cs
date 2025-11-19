using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Cards;

public class UpdateCardRequest
{
    [Required]
    [MaxLength(200)]
    public string Title { get; set; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; set; }

    public string? AssigneeId { get; set; }

    public DateTime? DueDate { get; set; }
}
