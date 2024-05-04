using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
    ILogger<PostController> logger, ApplicationDbContext context) : ControllerBase
{
    [HttpPost]
    [ResponseCache(NoStore = true)]
    public async Task<RestDTO<Post?>> Post(PostDTO postData)
    {
        // Needs validation and extract this to service method
        // Need separate DTOs for this and PUT methods
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

    [HttpGet]
    public async Task<RestDTO<Post[]>> GetAll(int pageIndex = 0, int pageSize = 10, string? q = null)
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

    [HttpGet("{id}")]
    public async Task<Post> GetOne(int id)
    {
        var query = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
        // Make sure Post exists

        return query;
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

        context.Posts.Update(post);
        await context.SaveChangesAsync();

        // Figure out 
        return new RestDTO<Post?>
        {
            Data = post
        };
    }

    [HttpDelete("{id}")]
    public async Task<Post> Delete(int id)
    {
        var query = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
        // Update DeletedAt and Status
        return query;
    }

}
