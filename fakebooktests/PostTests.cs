using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
        return Context;
    }

    [Fact]
    public async Task Test_CreatePost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser();

        Dictionary<string, string> postData = new()
        {
            {"body", "This is a test post"},
        };
        var response = await Client.PostAsJsonAsync("/v1/posts", postData);
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal(1, data.Id);
        Assert.Equal(1, data.UserId);
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

        var response = await Client.GetAsync($"/v1/posts/{builder.PostId}");
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal("Builder Post 3", data.Body);
        Assert.Equal(2, data.UserId);
    }

    [Fact]
    public async Task Test_GetNonexistentPost404Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser().AddPost();

        var response = await Client.GetAsync("/v1/posts/3");
        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task Test_GetDeletedPost404Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser();
        // Need to add this after building a user so UserId is set properly
        var builderPost = builder.GetBuilderPost();
        builderPost.Status = PostStatus.Deleted;
        builder.AddPost(builderPost);

        var response = await Client.GetAsync($"/v1/posts/{builderPost.Id}");
        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
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
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO[]>>())!.Data;
        Assert.Equal(4, data.Length);
        Assert.Equal(3, data[0].UserId);
    }

    [Fact]
    public async Task Test_UpdateDraftPost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser();
        // Need to add this after building a user so UserId is set properly
        var builderPost = builder.GetBuilderPost();
        builderPost.Status = PostStatus.Draft;
        builder.AddPost(builderPost);

        Dictionary<string, string> putData = new() { { "body", "Updated" } };
        var response = await Client.PutAsJsonAsync($"/v1/posts/{builder.PostId}", putData);
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal(1, data.Id);
        Assert.Equal("Updated", data.Body);
    }

    [Fact]
    public async Task Test_UpdatePublishedPost422Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser().AddPost();

        Dictionary<string, string> putData = new() { {"body", "Updated"} };
        var response = await Client.PutAsJsonAsync($"/v1/posts/{builder.PostId}", putData);
        Assert.NotNull(response);
        Assert.Equal(422, (int)response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot update a PUBLISHED post.", responseString);
    }

    [Fact]
    public async Task Test_DeletePost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);

        builder.AddUser().AddPost();

        var response = await Client.DeleteAsync($"/v1/posts/{builder.PostId}");
        Assert.NotNull(response);
        Assert.Equal(204, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<DateTime>>())!.Data;
        Assert.Equal(DateTime.Today.Date, data.Date);
    }
}