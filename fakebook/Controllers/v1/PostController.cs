using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using fakebook.Models;
using PostService = fakebook.Services.v1.Post;
using fakebook.DTO.v1.Post;
using fakebook.DTO.v1.User;
using fakebook.Services.v1;


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
    [ProducesResponseType(422)]  // Need to implement validation 
    [ProducesResponseType(400)]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<PostResponseDTO>> Post(PostNewDTO postData)
    {
        var post = await PostService.CreatePost(context, postData);
        return ControllerHelper.ReturnDataWithStatusCode(
            new RestDataDTO<PostResponseDTO>() { Data = PostResponseDTO.Dump(post) },
            statusCode: 201,
            HttpContext
        );
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [HttpGet]
    public async Task<RestDataDTO<PostResponseDTO[]>> GetPosts([FromQuery] PagingDTO paging)
    {
        logger.LogInformation("Get Method stated");
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

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [HttpGet("{id}")]
    public async Task<RestDataDTO<PostResponseDTO>> GetPost(int id)
    {
        var post = await PostService.GetPost(context, id);
        return new() { Data = PostResponseDTO.Dump(post) };
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    [HttpPut("{id}")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<PostResponseDTO>> Put(int id, PostUpdateDTO postData)
    {
        var post = await PostService.UpdatePost(context, id, postData);
        return new() { Data = PostResponseDTO.Dump(post) };
    }

    [ProducesResponseType(204)]
    [HttpDelete("{id}")]
    public async Task<RestDataDTO<DateTime>> Delete(int id)
    {
        var post = await PostService.DeletePost(context, id);
        return ControllerHelper.ReturnDataWithStatusCode(
            new RestDataDTO<DateTime>() { Data = (DateTime)post.DeletedAt! },
            statusCode: 204,
            HttpContext
        );
    }
}
