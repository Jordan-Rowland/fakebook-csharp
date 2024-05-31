using Xunit.Abstractions;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Identity;
using Moq;
using Microsoft.Extensions.Configuration;

using fakebook.DTO.v1;
using fakebook.DTO.v1.User;
using fakebook.Models;
using UserModel = fakebook.Models.User;
using UserService = fakebook.Services.v1.User;
using Microsoft.AspNetCore.Http;


namespace fakebooktests.Tests;
public class UserTests(
    CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    : BaseTestClass(factory, output)
{
    [Fact]
    public async Task CreateUser()
    {
        UserManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<UserModel>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult(IdentityResult.Success));

        UserNewDTO postData = new() { UserName = "testUser", Password = "testPass" };
        var res = await UserService.CreateUser(UserManagerMock.Object, postData);
        
        Assert.NotNull(res);
        Assert.IsType<UserModel>(res);
    }

    [Fact]
    public async Task LoginUser()
    {
        TestBuilder builder = new(Context, Output);
        var builderUser = builder.GetBuilderUser();
        builder.AddUser(builderUser);
        UserManagerMock
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(builderUser);
        UserManagerMock
            .Setup(m => m.CheckPasswordAsync(builderUser, It.IsAny<string>()))
            .ReturnsAsync(true);
        UserManagerMock
            .Setup(m => m.GetRolesAsync(It.IsAny<UserModel>()))
            .ReturnsAsync(["Administrator"]);
        IConfiguration configurationMock = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() {
                {"JWT:SigningKey", "SigningKeySigningKeySigningKeySigningKey"} })
            .Build();

        UserLoginDTO postData = new() { UserName = builderUser.UserName, Password = "testPass" };
        var res = await UserService.LoginUser(
            UserManagerMock.Object, postData, configurationMock);
        
        Assert.NotNull(res);
        Assert.IsType<string>(res);
    }

    [Fact]
    public async Task GetUserWithFollowedIds()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser().AddFollow();

        var response = await Client.GetAsync($"/v1/users/2");

        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        Assert.Equal($"Builder User 2", data.UserName);
        Assert.Equal([1], data.FollowingIds);
    }

    [Fact]
    public async Task GetNonexistentUser404Error()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        var response = await Client.GetAsync("/v1/users/2");

        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedUser404Error()
    {
        TestBuilder builder = new(Context, Output);
        var builderUser = builder.GetBuilderUser();
        builderUser.Status = UserStatus.Deleted;
        builder.AddUser(builderUser);

        var response = await Client.GetAsync($"/v1/users/{builderUser.Id}");

        Assert.NotNull(response);
        Output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(404, (int)response.StatusCode);
        Assert.Contains(
            $"User with ID {builderUser.Id} Not Found",
            await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetUsers()
    {
        TestBuilder builder = new(Context, Output);
        builder
            .AddUser().AddUser().AddUser().AddUser();

        var response = await Client.GetAsync("/v1/users");

        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO[]>>())!.Data;
        Assert.Equal(4, data.Length);
    }

    [Fact]
    public async Task UpdateUser()
    {
        TestBuilder builder = new(Context, Output);
        var testUser = builder.GetBuilderUser();
        builder.AddUser(testUser);
        UserManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(testUser)!);
        UserManagerMock
            .Setup(m => m.UpdateAsync(It.IsAny<UserModel>()))
            .Returns(Task.FromResult(IdentityResult.Success));

        UserUpdateDTO putData = new() { Email = "Updated@email.com" };
        var response = await UserService.UpdateUser(UserManagerMock.Object, builder.UserId, putData);

        Assert.NotNull(response);
        Assert.Equal(1, response.Id);
        Assert.Equal("Updated@email.com", response.Email);
    }

    public class IdentityResultMock : IdentityResult
    {
        public IdentityResultMock(bool succeeded = false) => Succeeded = succeeded;
    }

    [Fact]
    public async Task UpdatePasswordFail()
    {
        TestBuilder builder = new(Context, Output);
        UserModel testUser = builder.GetBuilderUser();
        UserManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(testUser));
        IdentityResultMock identityResultMock = new();
        UserManagerMock
            .Setup(m => m.ChangePasswordAsync(
                It.IsAny<UserModel>(),
                It.IsAny<string>(),
                It.IsAny<string>()
            )).ReturnsAsync(identityResultMock);

        UserUpdateDTO putData = new()
        {
            ExistingPassword = "WrongP@55",
            NewPassword = "WrongP@55!2",
        };
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(
            () => UserService.UpdateUser(UserManagerMock.Object, builder.UserId, putData));
        Assert.Equal(400, ex.StatusCode);
        Assert.Equal("Existing password not correct.", ex.Message);
    }

    [Fact]
    public async Task DeleteUser()
    {
        TestBuilder builder = new(Context, Output);
        UserModel testUser = builder.GetBuilderUser();
        UserManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .Returns(Task.FromResult(testUser));

        var response = await UserService.DeleteUser(UserManagerMock.Object, builder.UserId);

        Assert.NotNull(response);
        Assert.Equal(UserStatus.Deleted, response.Status);
        Assert.Equal(DateTime.Today.Date, response.LastActive.Date);
    }
}
