using System.ComponentModel.DataAnnotations;

namespace Api.DTOs.Columns;

public class ReorderColumnRequest
{
    [Required]
    public Guid ColumnId { get; set; }

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal NewPosition { get; set; }
}
