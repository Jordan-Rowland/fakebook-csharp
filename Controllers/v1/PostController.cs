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
    public async Task<RestDTO<Post[]>> Get(int pageIndex = 0, int pageSize = 10)
    {
        var query = context.Posts
            .Where(p => p.Status != PostStatus.Deleted)
            .Skip(pageIndex * pageSize)
            .Take(pageSize);
        var results = await query.ToArrayAsync();
        return new RestDTO<Post[]>
        {
            Data = results,
            PageIndex = pageIndex,
            PageSize = pageSize,
            RecordCount = results.Length,
        };
    }

    [HttpPost]
    public Post Post(Post post)
    {
        return new Post();
    }
}
