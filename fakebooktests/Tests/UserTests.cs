using fakebook.DTO.v1;
using fakebook.Models;
using UserModel = fakebook.Models.User;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using System.Net.Http.Json;
using fakebook.DTO.v1.User;
using Microsoft.AspNetCore.Identity;
using Moq;
using UserService = fakebook.Services.v1.User;
using Microsoft.Extensions.Configuration;


namespace fakebooktests.Tests;
public class UserTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private CustomWebApplicationFactory<Program> Factory { get; set; }
    private HttpClient Client { get; set; }
    private ITestOutputHelper Output { get; set; }
    private ApplicationDbContext Context { get; set; }

    public UserTests(
        CustomWebApplicationFactory<Program> factory,
        ITestOutputHelper output)
    {
        Factory = factory;
        Client = Factory.CreateClient();
        Output = output;

        var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
    }

    private ApplicationDbContext GetScopedContext(IServiceScope scope)
    {
        var scopedServices = scope.ServiceProvider;
        Context = scopedServices.GetRequiredService<ApplicationDbContext>();
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
        return Context;
    }

    [Fact]
    public async Task CreateUser()
    {
        var userManagerMock = new Mock<UserManager<UserModel>>(
            Mock.Of<IUserStore<UserModel>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        userManagerMock
            .Setup(m => m.CreateAsync(
                It.IsAny<UserModel>(),
                It.IsAny<string>()))
            .Returns(Task.FromResult(IdentityResult.Success));

        UserNewDTO postData = new() { UserName = "testUser", Password = "testPass" };
        var res = await UserService.CreateUser(postData, userManagerMock.Object);
        
        Assert.NotNull(res);
        Assert.IsType<UserModel>(res);
    }

    [Fact]
    public async Task LoginUser()
    {
        TestBuilder builder = new(Context, Output);

        var userManagerMock = new Mock<UserManager<UserModel>>(
            Mock.Of<IUserStore<UserModel>>(), null, null, null, null, null, null, null, null);
        UserModel builderUser = builder.GetBuilderUser();
        userManagerMock
            .Setup(m => m.FindByNameAsync(It.IsAny<string>()))
            .ReturnsAsync(builderUser);
        userManagerMock
            .Setup(m => m.CheckPasswordAsync(builderUser, It.IsAny<string>()))
            .ReturnsAsync(true);
        IConfiguration configurationMock = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>() {
                {"JWT:SigningKey", "SigningKeySigningKeySigningKeySigningKey"} })
            .Build();

        UserLoginDTO postData = new() { UserName = builderUser.UserName, Password = "testPass" };
        var res = await UserService.LoginUser(
            userManagerMock.Object, postData, configurationMock);
        
        Assert.NotNull(res);
        Assert.IsType<RestDataDTO<string>>(res);
    }

    [Fact]
    public async Task GetUser()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        var response = await Client.GetAsync($"/v1/users/{builder.UserId}");
        
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        Assert.Equal($"Builder User 1", data.UserName);
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
        Assert.Equal(404, (int)response.StatusCode);
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
        builder.AddUser();
        
        UserUpdateDTO putData = new() { Email = "Updated@email.com" };
        var response = await Client.PutAsJsonAsync($"/v1/users/{builder.UserId}", putData);

        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        Assert.Equal(1, data.Id);
        Assert.Equal("Updated@email.com", data.Email);
    }

    [Fact]
    public async Task UpdateUser422Error()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        UserUpdateDTO putData = new()
        {
            ExistingPassword = "WrongP@55",
            NewPassword = "WrongP@55!2",
        };
        var response = await Client.PutAsJsonAsync($"/v1/users/{builder.UserId}", putData);

        Assert.NotNull(response);
        Assert.Equal(422, (int)response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains(
            "Existing password does not match. Password cannot be updated.",
            responseString
        );
    }

    [Fact]
    public async Task DeleteUser()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        var response = await Client.DeleteAsync($"/v1/users/{builder.UserId}");
        
        Assert.NotNull(response);
        Assert.Equal(204, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<DateTime>>())!.Data;
        Assert.Equal(DateTime.Today.Date, data.Date);
    }
}