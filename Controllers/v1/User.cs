namespace fakebook.Controllers.v1;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; }
    public string PasswordHash { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Location { get; set; }
    public string? Photo { get; set; }
    public string? About { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime LastActive { get; set; }
    public UserStatus Status { get; set; }
}

public enum UserStatus { Public, Private, Deleted }
