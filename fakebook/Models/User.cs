using System.ComponentModel.DataAnnotations;

namespace fakebook.Models;

public class User
{
    [Key]
    [Required]
    public int Id { get; set; }
    [Required]
    [MaxLength(32)]
    public string Username { get; set; }
    [Required]
    public string PasswordHash { get; set; }
    public string? Email { get; set; }
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
