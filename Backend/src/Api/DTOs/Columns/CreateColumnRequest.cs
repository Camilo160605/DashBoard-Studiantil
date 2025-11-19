using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Columns;

public class CreateColumnRequest
{
    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;
}
