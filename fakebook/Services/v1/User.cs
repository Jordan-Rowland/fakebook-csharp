using fakebook.DTO.v1;
using fakebook.DTO.v1.User;
using fakebook.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using db = fakebook.Models.ApplicationDbContext;
using UserModel = fakebook.Models.User;

namespace fakebook.Services.v1;

public class User
{
    public static async Task<UserModel> CreateUser(
        UserManager<UserModel> userManager, UserNewDTO postData)
    {
        DateTime Now = DateTime.Now;
        UserModel user = new()
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

    public static async Task<string> LoginUser(
        UserManager<UserModel> userManager, UserLoginDTO postData, IConfiguration configuration)
    {
        var user = await userManager.FindByNameAsync(postData.UserName!);
        if (user == null || !await userManager.CheckPasswordAsync(user, postData.Password!))
            throw new BadHttpRequestException(
                "Invalid login attempt.", StatusCodes.Status400BadRequest);

        return await GenerateToken(userManager, user, configuration);
    }

    public static async Task<string> GenerateToken(
        UserManager<UserModel> userManager, UserModel user, IConfiguration configuration)
    {
        var signingCredentials = new SigningCredentials(
            new SymmetricSecurityKey(
                System.Text.Encoding.UTF8.GetBytes(
                    configuration["JWT:SigningKey"])),
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim> {
            new("UserId", user.Id.ToString()), new("UserName", user.UserName!)};

        claims.AddRange(
            (await userManager.GetRolesAsync(user)).Select(r => new Claim("UserRole", r)));

        var jwtObject = new JwtSecurityToken(
            issuer: configuration["JWT:Issuer"],
            audience: configuration["JWT:Audience"],
            claims: claims,
            expires: DateTime.Now.AddSeconds(60 * 60 * 24),
            signingCredentials: signingCredentials);

        return new JwtSecurityTokenHandler().WriteToken(jwtObject);
    }

    public static async Task<UserModel> GetUser(UserManager<UserModel> userManager, int id)
    {
        var user = await userManager.FindByIdAsync(id.ToString());
        if (user == null || user.Status == UserStatus.Deleted)
        {
            throw new BadHttpRequestException(
                $"User with ID {id} Not Found", StatusCodes.Status404NotFound);
        }
        return user;
    }

    public  static async Task<IEnumerable<UserModel>> GetUsers(db context, PagingDTO paging)
    {
        // Need to account for Private users / Followed users
        var query = context.Users.AsQueryable();
        query = query
            .Where(p => p.Status != UserStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt);

        if (paging != null)
        {
            if (!string.IsNullOrEmpty(paging.Q))
                query = query.Where(u => u.UserName!.Contains(paging.Q));
            
            query = query
                .Skip(paging.PageIndex * paging.PageSize)
                .Take(paging.PageSize);
        }

        return await query.ToArrayAsync();
    }

    public  static async Task<UserModel> UpdateUser(
        UserManager<UserModel> userManager, int id, UserUpdateDTO userData)
    {
        var user = await GetUser(userManager, id);
        UserStatus status = user.Status;
        Enum.TryParse(userData.Status ?? status.ToString("G"), out status);

        if (userData.ExistingPassword != null && userData.NewPassword != null)
        {
            var passChangeResult = await userManager.ChangePasswordAsync(
                user, userData.ExistingPassword, userData.NewPassword);
            if (!passChangeResult.Succeeded)
                throw new BadHttpRequestException("Existing password not correct.");
        }

        user.Email = userData.Email;
        user.FirstName = userData.FirstName;
        user.LastName = userData.LastName;
        user.Location = userData.Location;
        user.Photo = userData.Photo;
        user.About = userData.About;
        user.Status = status;
        user.LastActive = DateTime.Now;
        
        await userManager.UpdateAsync(user);
        return user;
    }

    public  static async Task<UserModel> DeleteUser(
        UserManager<UserModel> userManager, int UserId)
    {
        var user = await GetUser(userManager, UserId);
        user.Status = UserStatus.Deleted;
        user.LastActive = DateTime.Now;
        await userManager.UpdateAsync(user);
        return user;
    }
}
