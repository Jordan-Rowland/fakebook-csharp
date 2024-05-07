using Microsoft.EntityFrameworkCore;

using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;


namespace fakebook.Services.v1;

public static class Post
{
    public static async Task<Models.Post> CreatePost(ApplicationDbContext context, PostNewDTO postData)
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

        // Task<ActionResult<RestDTO<Domain[]>>>

        var post = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
        if (post == null)
        {
            throw new BadHttpRequestException($"Post with ID {id} Not Found", StatusCodes.Status404NotFound);
        }
        return post;
    }

    public static async Task<Models.Post> UpdatePost(ApplicationDbContext context, int id, PostUpdateDTO postData)
    {
        var post = await GetPost(context, id);
        post.Body = postData.Body;
        post.Status = postData.Status ?? post.Status;
        context.Posts.Update(post);
        await context.SaveChangesAsync();
        return post;
    }
}
