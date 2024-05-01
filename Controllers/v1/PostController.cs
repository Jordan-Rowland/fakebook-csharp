using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using fakebook.Models;

namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController : ControllerBase
{
    [HttpGet]
    public RestDTO<Post[]> Get()
    {
        return new RestDTO<Post[]>
        {
            Data =
            [
                new Post()
                {
                    Id = 1,
                    UserId = 1,
                    Body = "Just testing the system",
                    Status = PostStatus.PUBLISED,
                    CreatedAt = DateTime.Now,
                },
                new Post()
                {
                    Id = 2,
                    UserId = 1,
                    Body = "Just testing the system 2",
                    Status = PostStatus.PUBLISED,
                    CreatedAt = DateTime.Now.AddMinutes(3),
                },
                new Post()
                {
                    Id = 3,
                    UserId = 2,
                    ParentId = 1,
                    Body = "Just testing the system 3",
                    Status = PostStatus.PUBLISED,
                    CreatedAt = DateTime.Now.AddMinutes(3),

                }
            ]
        };
    }

    [HttpPost]
    public Post Post(Post post)
    {
        return new Post();
    }
}
