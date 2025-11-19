using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Columns;

public class UpdateColumnRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
