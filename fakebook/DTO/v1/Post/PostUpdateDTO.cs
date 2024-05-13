using fakebook.Models;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1.Post;

public class PostUpdateDTO
{
    [StringLength(400)]
    [Required]
    public string Body { get; set; }
    [Description("Published | Draft")]
    public string? Status { get; set; }
}
