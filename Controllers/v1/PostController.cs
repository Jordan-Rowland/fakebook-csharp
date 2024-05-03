using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.EntityFrameworkCore;

namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
    ILogger<PostController> logger, ApplicationDbContext context) : ControllerBase
{

    [HttpGet]
    public async Task<RestDTO<Post[]>> Get(int pageIndex = 0, int pageSize = 10, string? q = null)
    {
        var query = context.Posts.AsQueryable();

        if (!string.IsNullOrEmpty(q))
            query = query.Where(p => p.Body.Contains(q));

        query = query
            .Where(p => p.Status != PostStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(pageIndex * pageSize)
            .Take(pageSize);

        // For some reason User isn't populating...
        var results = await query.ToArrayAsync();
        return new RestDTO<Post[]>
        {
            Data = results,
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = results.Length,
            Q = q,
        };
    }

    [HttpPost]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Post?>> Post(PostDTO postData)
    {
        Post post = new()
        {
            UserId = 2,  // Needs to be set via auth
            Body = postData.Body,
            ParentId = postData.ParentId,
            Status = postData.Status ?? 0,
            CreatedAt = DateTime.Now,
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        return new RestDTO<Post?>
        {
            Data = post
        };
    }


    // TODO: Fill this in
    [HttpPut]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Post?>> Put(PostDTO postData)
    {
        Post post = new()
        {
            UserId = 2,  // Needs to be set via auth
            Body = postData.Body,
            ParentId = postData.ParentId,
            Status = postData.Status ?? 0,
            CreatedAt = DateTime.Now,
        };

        context.Posts.Add(post);
        await context.SaveChangesAsync();

        return new RestDTO<Post?>
        {
            Data = post
        };
    }
}
