using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using fakebook.DTO.v1;
using fakebook.Models;
using PostService = fakebook.Services.v1.Post;
using fakebook.DTO.v1.Post;
using System.Diagnostics;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
    ApplicationDbContext context, ILogger<PostController> logger) : ControllerBase
{
    [HttpPost]
    // https://code-maze.com/swagger-ui-asp-net-core-web-api/
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    [ProducesResponseType(400)]
    // Probably need 422 responses
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<PostResponseDTO>> Post(PostNewDTO postData)
    {
        // Where there was no user and I tried to make a post, I got a DB crash,
        // But it didn't throw a good error. Figure out why
        var post = await PostService.CreatePost(context, postData);
        return new RestDataDTO<PostResponseDTO> { Data = PostResponseDTO.Dump(post) };
    }

    [HttpGet]
    public async Task<RestDataDTO<PostResponseDTO[]>> GetPosts([FromQuery] PagingDTO paging)
    {
        var results = (await PostService.GetPosts(context, paging))
            .Select(PostResponseDTO.Dump).ToArray();
        return new RestResponseDTO<PostResponseDTO[]>
        {
            Data = results,
            PageIndex = paging.PageIndex,
            PageSize = paging.PageSize,
            RecordCount = results.Length,
            Q = paging.Q,
        };
    }

    [HttpGet("{id}")]
    public async Task<RestDataDTO<PostResponseDTO>> GetPost(int id)
    {
        var post = await PostService.GetPost(context, id);
        return new RestDataDTO<PostResponseDTO> { Data = PostResponseDTO.Dump(post) };
    }

    //// TODO: Fill this in
    //[ProducesResponseType(422)]
    //[HttpPut("{id}")]
    //[ResponseCache(NoStore = true)]
    //public async Task<RestResponseDTO<Post?>> Put(PostUpdateDTO postData)
    //{
    //    // Test accessing User from Post, post.User

    //    Post post = new()
    //    {
    //        UserId = 2,  // Needs to be set via auth
    //        Body = postData.Body,
    //        ParentId = postData.ParentId,
    //        Status = postData.Status ?? 0,
    //        CreatedAt = DateTime.Now,  // Update the updatedAt variable, not created
    //    };

    //    context.Posts.Update(post);
    //    await context.SaveChangesAsync();

    //    // Figure out 
    //    return new RestResponseDTO<Models.Post?>
    //    {
    //        Data = post
    //    };
    //}

    [HttpDelete("{id}")]
    public async Task<Models.Post> Delete(int id)
    {
        var query = await context.Posts.Where(p => p.Id == id).FirstOrDefaultAsync();
        // Update DeletedAt and Status
        return query;
    }

}
