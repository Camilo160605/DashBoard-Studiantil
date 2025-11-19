using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Boards;

public class CreateBoardRequest
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = string.Empty;
}
