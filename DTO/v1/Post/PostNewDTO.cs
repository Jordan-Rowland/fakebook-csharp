using fakebook.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;

namespace fakebook.DTO.v1.Post;

public class PostNewDTO : PostUpdateDTO
{
    public int? Id { get; set; }

    [StringLength(400)]
    [Required]
    public int? ParentId { get; set; }
}
