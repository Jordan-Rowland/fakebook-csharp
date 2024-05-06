using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using fakebook.DTO.v1;
using fakebook.Models;
using PostService = fakebook.Services.v1.Post;
using fakebook.Services.v1;
using Microsoft.Extensions.Hosting;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
    ApplicationDbContext context, ILogger<PostController> logger) : ControllerBase
{
    [HttpPost]
    [ResponseCache(NoStore = true)]
    public async Task<PostResponseDTO> Post(PostRequestDTO postData)
    {
        var post = await PostService.CreatePost(context, postData);
        // Return an object with [Data] instead of just the post object
        return PostResponseDTO.Dump(post); 
    }

    [HttpGet]
    public async Task<RestResponseDTO<PostResponseDTO[]>> GetPosts([FromQuery] PagingDTO paging)
    {
        var results = (await PostService.GetPosts(context, paging))
            .Select(PostResponseDTO.Dump).ToArray();
        return new RestResponseDTO<PostResponseDTO[]>  // Maybe drop this into a method
        {
            Data = results,
            PageIndex = paging.PageIndex,
            PageSize = paging.PageSize,
            RecordCount = results.Length,
            Q = paging.Q,
        };
    }

    [HttpGet("{id}")]
    public async Task<Models.Post> GetPost(int id)
    {
        // Make sure Post exists
        var query = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();

        return query;
    }

    // TODO: Fill this in
    [HttpPut("{id}")]
    [ResponseCache(NoStore = true)]
    public async Task<RestResponseDTO<Models.Post?>> Put(PostRequestDTO postData)
    {
        // Test accessing User from Post, post.User

        Models.Post post = new()
        {
            UserId = 2,  // Needs to be set via auth
            Body = postData.Body,
            ParentId = postData.ParentId,
            Status = postData.Status ?? 0,
            CreatedAt = DateTime.Now,  // Update the updatedAt variable, not created
        };

        context.Posts.Update(post);
        await context.SaveChangesAsync();

        // Figure out 
        return new RestResponseDTO<Models.Post?>
        {
            Data = post
        };
    }

    [HttpDelete("{id}")]
    public async Task<Models.Post> Delete(int id)
    {
        var query = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
        // Update DeletedAt and Status
        return query;
    }

}
