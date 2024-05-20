using fakebook.DTO.v1;
using fakebook.DTO.v1.User;
using fakebook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace fakebook.Services.v1;

public class User
{
    public static async Task<Models.User> CreateUser(
        UserNewDTO postData, UserManager<Models.User> userManager)
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
        var res = await userManager.CreateAsync(user, postData.Password);
        if (!res.Succeeded)
            throw new BadHttpRequestException("An error occured and the user was not created.");
        return user;
    }

    public static async Task<RestDataDTO<string>> LoginUser(
        UserManager<Models.User> userManager, UserLoginDTO postData, IConfiguration configuration)
    {
        var user = await userManager.FindByNameAsync(postData.UserName!);

        if (user == null || !await userManager.CheckPasswordAsync(user, postData.Password!))
            throw new BadHttpRequestException("Invalid login attempt.", StatusCodes.Status400BadRequest);

        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    configuration["JWT:SigningKey"])),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> { new("UserId", user.Id.ToString()) };

        var jwtObject = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddSeconds(3600),
            signingCredentials: signingCredentials);

        var jwtString = new JwtSecurityTokenHandler().WriteToken(jwtObject);

        return new() { Data = jwtString };
    }

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
                query = query.Where(u => u.UserName.Contains(paging.Q));
            
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
        Enum.TryParse(userData.Status ?? status.ToString("G"), out status);

        if (userData.NewPassword != null)
        {
            if (userData.ExistingPassword == user.PasswordHash)
            {
                // This needs to be updated...
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

        // ... Probably needs a new method here
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
