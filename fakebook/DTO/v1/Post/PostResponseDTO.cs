using fakebook.Models;

namespace fakebook.DTO.v1.Post;

public class PostResponseDTO : PostNewDTO
{
    public int Id { get; set; }
    public int UserId { get; set; }
    //public User.UserResponseDTO User { get; set; }  // This should be UserDTO
    public DateTime CreatedAt { get; set; }

    public static PostResponseDTO Dump(Models.Post postModel)
    {
        return new()
        {
            Id = postModel.Id,
            Body = postModel.Body,
            ParentId = postModel.ParentId,
            Status = postModel.Status.ToString("G"),
            CreatedAt = postModel.CreatedAt,
            UserId = postModel.UserId,
        };
    }
}
