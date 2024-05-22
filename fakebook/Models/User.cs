using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace fakebook.Models;

public class User : IdentityUser<int>
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? Photo { get; set; }
    public string? About { get; set; }
    [Required]
    public DateTime CreatedAt { get; set; }
    [Required]
    public DateTime LastActive { get; set; }
    [Required]
    public UserStatus Status { get; set; }

    public ICollection<Post> Posts { get; set; }
}

public enum UserStatus { Public, Private, Deleted }
