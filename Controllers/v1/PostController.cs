using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using System.ComponentModel.DataAnnotations;

namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
    ApplicationDbContext context, ILogger<PostController> logger) : ControllerBase
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
    public async Task<RestDTO<Post[]>> GetAll([FromQuery] PagingDTO paging)
    {
        var query = context.Posts.AsQueryable();

        if (!string.IsNullOrEmpty(paging.Q))
            query = query.Where(p => p.Body.Contains(paging.Q));

        query = query
            .Where(p => p.Status != PostStatus.Deleted)
            .OrderByDescending(p => p.CreatedAt)
            .Skip(paging.PageIndex * paging.PageSize)
            .Take(paging.PageSize);

        // For some reason User isn't populating...
        var results = await query.ToArrayAsync();
        return new RestDTO<Post[]>
        {
            Data = results,
            PageIndex = paging.PageIndex,
            PageSize = paging.PageSize,
            RecordCount = results.Length,
            Q = paging.Q,
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
    [HttpPut("{id}")]
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
