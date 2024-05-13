using fakebook.Models;

namespace fakebook.DTO.v1.User;

public class UserResponseDTO
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? Photo { get; set; }
    public string? About { get; set; }
    public UserStatus Status { get; set; }

    public static UserResponseDTO Dump(Models.User userModel)
    {
        return new()
        {
            Id = userModel.Id,
            Username = userModel.Username,
            Email = userModel.Email,
            FirstName = userModel.FirstName,
            LastName = userModel.LastName,
            Location = userModel.Location,
            Photo = userModel.Photo,
            About = userModel.About,
            Status = userModel.Status,
        };
    }
}
