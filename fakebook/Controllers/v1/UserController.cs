﻿using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;

using fakebook.DTO.v1;
using UserService = fakebook.Services.v1.User;
using fakebook.Models;
using fakebook.DTO.v1.User;
using Microsoft.AspNetCore.Identity;


namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/users")]
[ApiController]
[ApiVersion("1.0")]
public class UserController(
    ApplicationDbContext context,
    ILogger<UserController> logger,
    IConfiguration configuration,
    UserManager<User> userManager,
    SignInManager<User> SignInManager) : ControllerBase
{
    [HttpPost]
    // https://code-maze.com/swagger-ui-asp-net-core-web-api/
    [ProducesResponseType(201)]
    // Probably need 422 responses  --  add validation
    [ProducesResponseType(422)]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<UserResponseDTO>> Post(UserNewDTO postData)
    {
        var user = await UserService.CreateUser(postData, userManager);
        return ControllerHelper.ReturnDataWithStatusCode(
            new RestDataDTO<UserResponseDTO>() { Data = UserResponseDTO.Dump(user) },
            statusCode: 201,
            HttpContext
        );
    }

    [HttpPost("login")]
    [ProducesResponseType(200)]
    [ResponseCache(NoStore = true)]
    //{
	   // "username": "slimjob_dopamine",
	   // "password": "Qwerty1!"
    //}
    public async Task<RestDataDTO<string>> Login(UserLoginDTO postData) =>
        await UserService.LoginUser(userManager, postData, configuration);

    [ProducesResponseType(200)]
    [ProducesResponseType(404)]
    [HttpGet]
    public async Task<RestDataDTO<UserResponseDTO[]>> GetUsers([FromQuery] PagingDTO paging)
    {
        var results = (await UserService.GetUsers(context, paging))
            .Select(UserResponseDTO.Dump).ToArray();
        return new RestResponseDTO<UserResponseDTO[]>
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
    public async Task<RestDataDTO<UserResponseDTO>> GetUser(int id)
    {
        var user = await UserService.GetUser(context, id);
        return new() { Data = UserResponseDTO.Dump(user) };
    }

    [ProducesResponseType(200)]
    [ProducesResponseType(422)]
    [HttpPut("{id}")]
    [ResponseCache(NoStore = true)]
    public async Task<RestDataDTO<UserResponseDTO>> Put(int id, UserUpdateDTO userData)
    {
        var user = await UserService.UpdateUser(context, id, userData);
        return new() { Data = UserResponseDTO.Dump(user) };
    }

    [ProducesResponseType(204)]
    [HttpDelete("{id}")]
    public async Task<RestDataDTO<DateTime>> Delete(int id)
    {
        var user = await UserService.DeleteUser(context, id);
        return ControllerHelper.ReturnDataWithStatusCode(
            new RestDataDTO<DateTime>() { Data = user.LastActive! },
            statusCode: 204,
            HttpContext
        );
    }
}
