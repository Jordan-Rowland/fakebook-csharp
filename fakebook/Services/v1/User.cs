using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.DTO.v1.User;
using fakebook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace fakebook.Services.v1;

public class User
{
    public static async Task<Models.User> CreateUser(
        ApplicationDbContext context, UserNewDTO postData, UserManager<Models.User> userManager)
    {
        DateTime Now = DateTime.Now;
        Models.User user = new()
        {
            UserName = postData.UserName,
            CreatedAt = Now,
            LastActive = Now,
            Email = postData.Email,
            FirstName = postData.FirstName,
            LastName = postData.LastName,
            Location = postData.Location,
            Photo = postData.Photo,
            About = postData.About,
            Status = UserStatus.Public,
        };

        await userManager.CreateAsync(user, postData.Password);
        return user;
    }

    //public static async Task<Models.User> LoginUser(
    //    ApplicationDbContext context, UserNewDTO postData)
    //{
    //    DateTime Now = DateTime.Now;
    //}

    internal static async Task<Models.User> GetUser(ApplicationDbContext context, int id)
    {
        var user = await context.Users
            .Where(p => p.Id == id && p.Status != UserStatus.Deleted)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            throw new BadHttpRequestException(
                $"User with ID {id} Not Found", StatusCodes.Status404NotFound);
        }
        return user;
    }

    internal static async Task<IEnumerable<Models.User>> GetUsers(ApplicationDbContext context, PagingDTO paging)
    {
        // Need to account for Private users / Followed users
        var query = context.Users.AsQueryable();
        query = query
            .Where(p => p.Status != UserStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt);

        if (paging != null)
        {
            if (!string.IsNullOrEmpty(paging.Q))
                query = query.Where(p => p.UserName.Contains(paging.Q));
            query = query
                .Skip(paging.PageIndex * paging.PageSize)
                .Take(paging.PageSize);
        }

        return await query.ToArrayAsync();
    }

    internal static async Task<Models.User> UpdateUser(ApplicationDbContext context, int id, UserUpdateDTO userData)
    {
        var user = await GetUser(context, id);
        UserStatus status = user.Status;
        _ = Enum.TryParse(userData.Status ?? status.ToString("G"), out status);

        if (userData.NewPassword != null)
        {
            if (userData.ExistingPassword == user.PasswordHash)
            {
                user.PasswordHash = userData.NewPassword;
            }
            else
            {
                throw new BadHttpRequestException(
                    $"Existing password does not match. Password cannot be updated.",
                    StatusCodes.Status422UnprocessableEntity
                );
            }
        }

        user.Email = userData.Email;
        user.FirstName = userData.FirstName;
        user.LastName = userData.LastName;
        user.Location = userData.Location;
        user.Photo = userData.Photo;
        user.About = userData.About;
        user.Status = status;
        user.LastActive = DateTime.Now;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }

    internal static async Task<Models.User> DeleteUser(ApplicationDbContext context, int id)
    {
        var user = await GetUser(context, id);
        user.Status = UserStatus.Deleted;
        user.LastActive = DateTime.Now;
        context.Users.Update(user);
        await context.SaveChangesAsync();
        return user;
    }
}
