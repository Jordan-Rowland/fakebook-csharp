using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.Extensions.DependencyInjection;
using Xunit.Abstractions;
using System.Net.Http.Json;
using fakebook.DTO.v1.User;
using Microsoft.EntityFrameworkCore;


namespace fakebooktests.Tests;
public class UserTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private CustomWebApplicationFactory<Program> Factory { get; set; }
    private HttpClient Client { get; set; }
    private ITestOutputHelper Output { get; set; }
    private ApplicationDbContext? Context { get; set; }

    public UserTests(
        CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        Factory = factory;
        Client = Factory.CreateClient();
        //Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        //{
        //    AllowAutoRedirect = false
        //});
        Output = output;
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
    public async void CreateUser()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        UserNewDTO postData = new()
        {
            Username = "testUser",
            Password = "testPass",
            Email = "testuser@email.com",
            FirstName = "Test",
            LastName = "User",
            Location = "Los Angeles, CA",
            Photo = "photo.png",
            About = "B.S. Software Engineering",
        };

        var response = await Client.PostAsJsonAsync("/v1/users", postData);
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        //Output.WriteLine((await response.Content.ReadAsStringAsync()).ToString());
        Assert.Equal(1, data.Id);
        Assert.Equal("testUser", data.Username);
        Assert.Single(await Context.Users.ToArrayAsync());
    }

    [Fact]
    public async void GetUser()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser();

        var response = await Client.GetAsync($"/v1/users/{builder.UserId}");
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        Output.WriteLine((await response.Content.ReadAsStringAsync()).ToString());
        Assert.Equal($"Builder User 1", data.Username);
    }

    [Fact]
    public async void GetNonexistentUser404Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        var response = await Client.GetAsync("/v1/users/2");
        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async void GetDeletedUser404Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        var builderUser = builder.GetBuilderUser();
        builderUser.Status = UserStatus.Deleted;
        builder.AddUser(builderUser);

        var response = await Client.GetAsync($"/v1/users/{builderUser.Id}");
        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async void GetUsers()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
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
    public async void UpdateUser()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser();
        
        UserUpdateDTO putData = new() { Email = "Updated@email.com" };
        var response = await Client.PutAsJsonAsync($"/v1/posts/{builder.UserId}", putData);
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<UserResponseDTO>>())!.Data;
        Assert.Equal(1, data.Id);
        Assert.Equal("Updated@email.com", data.Email);
    }

    [Fact]
    public async void UpdateUser422Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        //UserUpdateDTO putData = new() {  };
        Dictionary<string, string> putData = new() { { "username", "UpdatedTestUser" } };
        var response = await Client.PutAsJsonAsync($"/v1/users/{builder.UserId}", putData);
        Assert.NotNull(response);
        Assert.Equal(422, (int)response.StatusCode);
        //var responseString = await response.Content.ReadAsStringAsync();
        //Assert.Contains("Cannot update a PUBLISHED post.", responseString);
    }

    [Fact]
    public async void DeleteUser()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        var response = await Client.DeleteAsync($"/v1/users/{builder.UserId}");
        Assert.NotNull(response);
        Assert.Equal(204, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<DateTime>>())!.Data;
        Assert.Equal(DateTime.Today.Date, data.Date);
    }
}