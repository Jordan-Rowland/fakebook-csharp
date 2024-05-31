using FollowModel = fakebook.Models.Follow;
using UserModel = fakebook.Models.User;
using UserService = fakebook.Services.v1.User;
using db = fakebook.Models.ApplicationDbContext;
using fakebook.Models;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;


namespace fakebook.Services.v1;

public class Follow
{
    public static async Task<FollowModel?> FollowUser(
        db context, UserManager<UserModel> userManager, int FollowerId, int FollowedId)
    {
        if (FollowerId == FollowedId) return null;
        var user = await UserService.GetUser(userManager, FollowedId);
        var pending = user.Status == UserStatus.Private;
        var follow = new FollowModel()
        {
            FollowerId = FollowerId,
            FollowedId = user.Id,
            Pending = pending,
            CreatedAt = DateTime.Now,
        };
        context.Follows.Add(follow);
        await context.SaveChangesAsync();
        return follow;
    }

    public static async Task<bool> UnfollowUser(db context, int FollowerId, int FollowedId)
    {
        var follow = await context.Follows.FindAsync(FollowerId, FollowedId);
        if (follow == null)
        {
            throw new BadHttpRequestException(
                $"Follow Between Users Follower: {FollowerId} and Followed: {FollowedId} Not Found",
                StatusCodes.Status404NotFound);
        }
        context.Remove(follow);
        var result = await context.SaveChangesAsync();
        return result > 0;
    }

    public static async Task<List<UserModel>> GetFollowers(db context, int userId)
    {
        var follows = await GetFollowerIds(context, userId);
        return await context.Users.Where(u => follows.Contains(u.Id)).ToListAsync();
    }

    public static async Task<List<int>> GetFollowerIds(db context, int userId)
    {
        return await context.Follows
                            .Where(f => f.FollowedId == userId)
                            .Select(f => f.FollowerId)
                            .ToListAsync();
    }

    public static async Task<List<UserModel>> GetFollows(db context, int userId)
    {
        var follows = await GetFollowIds(context, userId);
        return await context.Users.Where(u => follows.Contains(u.Id)).ToListAsync();
    }

    public static async Task<List<int>> GetFollowIds(db context, int userId)
    {
        return await context.Follows
                            .Where(f => f.FollowerId == userId)
                            .Where(f => !f.Pending)
                            .Select(f => f.FollowedId)
                            .ToListAsync();
    }
}
