using fakebook.Models;
using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1.Post;

public class PostUpdateDTO
{
    [StringLength(400)]
    [Required]
    public string Body { get; set; }
    public PostStatus? Status { get; set; }
}
