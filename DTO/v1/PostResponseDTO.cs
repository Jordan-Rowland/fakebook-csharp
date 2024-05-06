namespace fakebook.DTO.v1;

public class PostResponseDTO : PostRequestDTO
{
    public int UserId { get; set; }
    //public Models.User User { get; set; }  // This should be UserDTO
    public DateTime CreatedAt { get; set; }

    public static PostResponseDTO Dump(Models.Post postModel)
    {
        return new()
        {
            Id = postModel.Id,
            Body = postModel.Body,
            ParentId = postModel.ParentId,
            Status = postModel.Status,
            CreatedAt = postModel.CreatedAt,
            UserId = postModel.UserId,
        };
    }
}
