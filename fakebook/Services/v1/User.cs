using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.DTO.v1.User;
using fakebook.Models;
using Microsoft.AspNetCore.Identity;

namespace fakebook.Services.v1;

public class User
{
    public static async Task<Models.User> CreateUser(
        ApplicationDbContext context, UserNewDTO postData)
    {
        Models.User user = new()
        {
            Username = postData.Username,
            PasswordHash = postData.Password,  // Hash the password
            CreatedAt = DateTime.Now,
            LastActive = DateTime.Now,
            Email = postData.Email,
            FirstName = postData.FirstName,
            LastName = postData.LastName,
            Location = postData.Location,
            Photo = postData.Photo,
            About = postData.About,
            Status = UserStatus.Public,
        };

        //{
        //    Username = "Joroloro123",
        //    PasswordHash = "blablablah",
        //    CreatedAt = DateTime.Now,
        //    LastActive = DateTime.Now,
        //    Status = 0,
        //};

        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }

    internal static async Task<Models.User> GetUser(ApplicationDbContext context, int id)
    {
        throw new NotImplementedException();
    }

    internal static async Task<IEnumerable<Models.User>> GetUsers(ApplicationDbContext context, PagingDTO paging)
    {
        throw new NotImplementedException();
    }

    internal static async Task<Models.User> UpdateUser(ApplicationDbContext context, int id, UserUpdateDTO userData)
    {
        throw new NotImplementedException();
    }

    internal static async Task DeleteUser(ApplicationDbContext context, int id)
    {
        throw new NotImplementedException();
    }
}
