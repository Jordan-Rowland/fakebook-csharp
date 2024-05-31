using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using fakebook.DTO.v1;
using fakebook.Models;
using UserModel = fakebook.Models.User;
using PostService = fakebook.Services.v1.Post;
using fakebook.DTO.v1.Post;
using Microsoft.AspNetCore.Authorization;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;
using static fakebook.Controllers.v1.ControllerHelper;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/posts")]
[ApiController]
[ApiVersion("1.0")]
public class PostController(
        ApplicationDbContext context,
        ILogger<PostController> logger,
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        UserManager<UserModel> userManager,
        SignInManager<UserModel> signInManager,
        IHttpContextAccessor httpContextAccessor)
    : CustomControllerBase<PostController>(
        context,
        logger,
        configuration,
        roleManager,
        userManager,
        signInManager,
        httpContextAccessor)
{
    [HttpPost]
    // https://code-maze.com/swagger-ui-asp-net-core-web-api/
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]  // Need to implement validation 
    [Authorize]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<PostResponseDTO>> Post(PostNewDTO postData)
    {
        var post = await PostService.CreatePost(Context, postData, UserId!.Value);
        return ReturnDataWithStatusCode(
            new RestDataDTO<PostResponseDTO>() { Data = PostResponseDTO.Dump(post) },
            statusCode: 201, HttpContext
        );
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [HttpGet]
    public async Task<RestDataDTO<PostResponseDTO[]>> GetPosts([FromQuery] PagingDTO paging)
    {
        // Need to be able to see if user can view private posts??
        var results = (await PostService.GetPosts(Context, userManager, UserId, paging))
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
        var post = await PostService.GetPost(Context, id);
        return new() { Data = PostResponseDTO.Dump(post) };
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    [Authorize]
    [HttpPut("{id}")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<PostResponseDTO>> Put(int id, PostUpdateDTO postData)
    {
        var post = await PostService.UpdatePost(Context, id, postData, UserId!.Value);
        return new() { Data = PostResponseDTO.Dump(post) };
    }

    [ProducesResponseType(204)]
    [Authorize]
    [HttpDelete("{id}")]
    public async Task<RestDataDTO<DateTime>> Delete(int id)
    {
        var post = await PostService.DeletePost(Context, id, UserId!.Value);
        return ReturnDataWithStatusCode(
            new RestDataDTO<DateTime>() { Data = (DateTime)post.DeletedAt! },
            statusCode: 204, HttpContext
        );
    }
}
