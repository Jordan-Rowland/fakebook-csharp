using fakebook.Models;

namespace fakebook.DTO.v1.Post;

public class PostUpdateDTO
{
    public string? Body { get; set; }
    public PostStatus? Status { get; set; }
}
