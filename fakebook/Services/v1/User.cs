using fakebook.DTO.v1.Post;
using fakebook.Models;
using Microsoft.AspNetCore.Identity;

namespace fakebook.Services.v1;

public class User
{
    public static async Task<Models.User> CreateUser(ApplicationDbContext context)
    {
        Models.User user = new()
        {
            Username = "Joroloro123",
            PasswordHash = "blablablah",
            CreatedAt = DateTime.Now,
            LastActive = DateTime.Now,
            Status = 0,
        };
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}
