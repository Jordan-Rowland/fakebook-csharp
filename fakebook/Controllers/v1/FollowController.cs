using Asp.Versioning;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using fakebook.Models;
using UserModel = fakebook.Models.User;
using FollowService = fakebook.Services.v1.Follow;
using static fakebook.Controllers.v1.ControllerHelper;
using Microsoft.AspNetCore.Authorization;
using fakebook.DTO.v1;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/follows")]
[ApiController]
[ApiVersion("1.0")]
public class FollowController(
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
    [ProducesResponseType(201)]
    [Authorize]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<DateTime>> FollowUser(FollowDTO postData)
    {
        var follow = await FollowService.FollowUser(Context, UserManager, UserId!.Value, postData.FollowedId);
        return ReturnDataWithStatusCode(
            new RestDataDTO<DateTime>() { Data = follow.CreatedAt },
            StatusCodes.Status201Created,
            HttpContext
        );
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(201)]
    [Authorize]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<Dictionary<string, bool>>> UnfollowUser(int id)
    {
        var result = await FollowService.UnfollowUser(Context, UserId!.Value, id);
        return ReturnDataWithStatusCode(
            new RestDataDTO<Dictionary<string, bool>>() {
                Data = new Dictionary<string, bool>() { { "success", result } }},
            StatusCodes.Status204NoContent,
            HttpContext
        );
    }

    [HttpGet("getFollowers/{id}")]
    [ProducesResponseType(201)]
    [Authorize]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<List<UserModel>>> GetFollowers(int id)
    {
        var followers = await FollowService.GetFollowers(Context, id);
        return new RestDataDTO<List<UserModel>>() { Data = followers };
    }

    [HttpGet("getFollowed/{id}")]
    [ProducesResponseType(201)]
    [Authorize]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<List<UserModel>>> GetFollows(int id)
    {
        var follows = await FollowService.GetFollows(Context, id);
        return new RestDataDTO<List<UserModel>>() { Data = follows };
    }
}

public class FollowDTO
{
    public int FollowedId { get; set; }
}
