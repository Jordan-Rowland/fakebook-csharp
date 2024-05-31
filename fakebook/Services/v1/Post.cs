using Microsoft.EntityFrameworkCore;
using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;
using UserService = fakebook.Services.v1.User;
using UserModel = fakebook.Models.User;
using PostModel = fakebook.Models.Post;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;


namespace fakebook.Services.v1;

public static class Post
{
    public static async Task<PostModel> CreatePost(
        ApplicationDbContext context, PostNewDTO postData, int userId)
    {
        try
        {
            PostStatus status = PostStatus.Published;
            _ = Enum.TryParse(postData.Status, out status);
            PostModel post = new()
            {
                UserId = userId,
                Body = postData.Body,
                ParentId = postData.ParentId,
                Status = status,
                CreatedAt = DateTime.Now,
            };
            context.Posts.Add(post);
            await context.SaveChangesAsync();
            return post;
        }
        catch (Exception ex)
        {
            string message = ex switch
            {
                DbUpdateException => ex.InnerException?.Message!,
                _ => ex.Message
            };

            throw new BadHttpRequestException(message, StatusCodes.Status400BadRequest);
        }
    }

    public static async Task<PostModel[]> GetPosts(
        ApplicationDbContext context, UserManager<UserModel> userManager, int? userId, PagingDTO? paging = null)
    {
        UserModel? currUser = userId.HasValue ? await UserService.GetUser(userManager, userId.Value, context) : null;
        var query = context.Posts
            .Join(context.Users,
                post => post.UserId,
                user => user.Id,
                (post, user) => new { Post = post, User = user })
            .Where(joined => joined.Post.Status == PostStatus.Published)
            .Where(joined =>
                joined.User.Status == UserStatus.Public ||
                (joined.User.Status != UserStatus.Private &&
                    (currUser != null) &&
                    (currUser.Id == joined.User.Id
                        || currUser.FollowingIds!.Contains(joined.User.Id))))
            .OrderByDescending(joined => joined.Post.CreatedAt)
            .Select(joined => joined.Post)
            .AsQueryable();

        if (paging != null)
        {
            if (!string.IsNullOrEmpty(paging.Q))
                query = query.Where(p => p.Body.Contains(paging.Q));
            
            query = query
                .Skip(paging.PageIndex * paging.PageSize)
                .Take(paging.PageSize);
        }

        // For some reason User isn't populating...
        return await query.ToArrayAsync();
    }

    public static async Task<PostModel> GetPost(ApplicationDbContext context, int id)
    {
        var post = await context.Posts
            .Where(p => p.Id == id && p.Status != PostStatus.Deleted)
            .FirstOrDefaultAsync();
        if (post == null)
        {
            throw new BadHttpRequestException(
                $"Post with ID {id} Not Found", StatusCodes.Status404NotFound);
        }
        return post;
    }

    public static async Task<PostModel> UpdatePost(
        ApplicationDbContext context, int id, PostUpdateDTO postData, int userId)
    {
        var post = await GetPost(context, id);
        if (post.UserId != userId)
            throw new BadHttpRequestException("User ID does not match the current user.");

        PostStatus status = post.Status;
        if (status != PostStatus.Draft)
        {
            throw new BadHttpRequestException(
                $"Cannot update a PUBLISHED post.", StatusCodes.Status422UnprocessableEntity);
        }
        Enum.TryParse(postData.Status ?? status.ToString("G"), out status);
        post.Body = postData.Body;
        post.Status = status;
        context.Posts.Update(post);
        await context.SaveChangesAsync();
        return post;
    }

    public static async Task<PostModel> DeletePost(ApplicationDbContext context, int id, int userId)
    {
        var post = await GetPost(context, id);
        if (post.UserId != userId)
            throw new BadHttpRequestException("User ID does not match the current user.");
        post.Status = PostStatus.Deleted;
        post.DeletedAt = DateTime.Now;
        context.Posts.Update(post);
        await context.SaveChangesAsync();
        return post;
    }
}
