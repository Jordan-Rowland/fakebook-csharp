using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1.User;

public class UserLoginDTO
{
    [Required]
    [MaxLength(255)]
    public string? UserName { get; set; }
    [Required]
    public string? Password { get; set; }
}
