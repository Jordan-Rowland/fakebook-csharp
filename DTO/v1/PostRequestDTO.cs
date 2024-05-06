using fakebook.Models;
using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1;

public class PostRequestDTO
{
    // See if I can use a constructor to validate the data

    public int? Id { get; set; }

    [StringLength(400)]
    [Required]
    public string? Body { get; set; }
    public int? ParentId { get; set; }
    public PostStatus? Status { get; set; }
}
