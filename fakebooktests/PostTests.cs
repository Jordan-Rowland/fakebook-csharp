using fakebook.Controllers.v1;
using fakebook.DTO.v1;
using fakebook.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Net.Http.Json;
using Xunit.Abstractions;

namespace fakebooktests;

public class PostTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private CustomWebApplicationFactory<Program> Factory { get; set; }
    private HttpClient Client { get; set; }
    private ITestOutputHelper Output { get; set; }
    private ApplicationDbContext? Context { get; set; }

    public PostTests(
        CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        Factory = factory;
        Client = Factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        Output = output;
    }

    private ApplicationDbContext GetScopedContext(IServiceScope scope)
    {
        var scopedServices = scope.ServiceProvider;
        Context = scopedServices.GetRequiredService<ApplicationDbContext>();
        Context.Database.EnsureCreated();
        //Seeding.IntializeTestDB(Context, Output);
        return Context;
    }

    [Fact]
    public async Task Test_GetPost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder
            .AddUser()
            .AddPost()
            .AddPost()
            .AddUser()
            .AddPost();

        var response = await Client.GetAsync("/v1/posts/3");
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<Post>>())!.Data;
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        Assert.Equal("Builder Post 3", data.Body);
        Assert.Equal(2, data.UserId);

        Context.Database.EnsureDeleted();
    }

    [Fact]
    public async Task Test_GetPosts()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder
            .AddUser()
            .AddPost()
            .AddPost()
            .AddUser()
            .AddPost()
            .AddUser()
            .AddPost();

        var response = await Client.GetAsync("/v1/posts");
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<Post[]>>())!.Data;
        Assert.Equal(200, (int)response.StatusCode);
        Assert.True(data.Length == 4);

        Context.Database.EnsureDeleted();
    }
}