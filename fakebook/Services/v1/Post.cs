using Microsoft.EntityFrameworkCore;

using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;
using System.Diagnostics;


namespace fakebook.Services.v1;

public static class Post
{
    public static async Task<Models.Post> CreatePost(ApplicationDbContext context, PostNewDTO postData)
    {
        // Try and except block and throw a meaningful error!
        try
        {
            PostStatus status = PostStatus.Published;
            _ = Enum.TryParse(postData.Status, out status);
            Models.Post post = new()
            {
                UserId = 1,  // Needs to be set via auth
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

    public static async Task<Models.Post[]> GetPosts(
        ApplicationDbContext context, PagingDTO? paging = null)
    {
        var query = context.Posts.AsQueryable();
        query = query
            .Where(p => p.Status != PostStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt);

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

    public static async Task<Models.Post> GetPost(ApplicationDbContext context, int id)
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

    public static async Task<Models.Post> UpdatePost(
        ApplicationDbContext context, int id, PostUpdateDTO postData)
    {
        var post = await GetPost(context, id);
        PostStatus status = post.Status;
        if (status != PostStatus.Draft)
        {
            throw new BadHttpRequestException(
                $"Cannot update a PUBLISHED post.", StatusCodes.Status422UnprocessableEntity);
        }
        _ = Enum.TryParse(postData.Status ?? status.ToString("G"), out status);
        post.Body = postData.Body;
        post.Status = status;
        context.Posts.Update(post);
        await context.SaveChangesAsync();
        return post;
    }

    public static async Task<Models.Post> DeletePost(ApplicationDbContext context, int id)
    {
        var post = await GetPost(context, id);
        post.Status = PostStatus.Deleted;
        post.DeletedAt = DateTime.Now;
        context.Posts.Update(post);
        await context.SaveChangesAsync();
        return post;
    }
}
