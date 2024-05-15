using fakebook.Models;
using System.ComponentModel.DataAnnotations;

namespace fakebook.DTO.v1.User;

public class UserNewDTO : UserUpdateDTO
{
    [Required]
    [MaxLength(32)]
    public required string UserName { get; set; }
    [Required]
    public required string Password { get; set; }
}
