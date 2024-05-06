using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.EntityFrameworkCore;

namespace fakebook.Services.v1;

public static class Post
{
    public static async Task<Models.Post> CreatePost(ApplicationDbContext context, PostRequestDTO postData)
    {
        Models.Post post = new()
        {
            UserId = 2,  // Needs to be set via auth
            Body = postData.Body,
            ParentId = postData.ParentId,
            Status = postData.Status ?? 0,
            CreatedAt = DateTime.Now,
        };
        context.Posts.Add(post);
        await context.SaveChangesAsync();
        return post;
    }

    public static async Task<Models.Post[]> GetPosts(ApplicationDbContext context, PagingDTO? paging)
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

}
