using fakebook.Models;

namespace fakebook.DTO.v1;

public class PostDTO
{
    // See if I can use a constructor to validate the data

    public int? Id { get; set; }
    public string? Body { get; set; }
    public int? ParentId { get; set; }
    public PostStatus? Status { get; set; }
}
