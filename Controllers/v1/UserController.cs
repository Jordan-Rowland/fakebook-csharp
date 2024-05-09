using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

using fakebook.DTO.v1.Post;
using fakebook.DTO.v1;
using UserService = fakebook.Services.v1.User;
using fakebook.Models;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/users")]
[ApiController]
[ApiVersion("1.0")]
public class UserController(
    ApplicationDbContext context, ILogger<PostController> logger) : ControllerBase
{

    [HttpPost]
    // https://code-maze.com/swagger-ui-asp-net-core-web-api/
    [ProducesResponseType(201)]
    [ProducesResponseType(422)]
    // Probably need 422 responses
    [ResponseCache(NoStore = true)]
    public async Task Post(PostNewDTO postData)
    {
        Debug.Write("GOT INTO THE USER THING");
        var user = await UserService.CreateUser(context);
        //return user;
    }

}
