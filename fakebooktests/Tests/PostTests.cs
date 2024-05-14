using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Json;
using Xunit.Abstractions;


namespace fakebooktests.Tests;

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
        Client = Factory.CreateClient();
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
    public async void CreatePost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        PostNewDTO postData = new() { Body = "Test post" };

        var response = await Client.PostAsJsonAsync("/v1/posts", postData);
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);  // Change and test 201 status
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Output.WriteLine($"{await response.Content.ReadAsStringAsync()}");
        Assert.Equal(1, data.Id);
        Assert.Equal(1, data.UserId);
        Assert.Equal("Test post", data.Body);
        Assert.Single(await Context.Posts.ToArrayAsync());
    }

    [Fact]
    public async void GetPost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder
            .AddUser().AddPost().AddPost()
            .AddUser().AddPost();

        var response = await Client.GetAsync($"/v1/posts/{builder.PostId}");
        
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal("Builder Post 3", data.Body);
        Assert.Equal(2, data.UserId);
    }

    [Fact]
    public async void GetNonexistentPost404Error()
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
    public async void GetDeletedPost404Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        var builderPost = builder.AddUser().GetBuilderPost();
        builderPost.Status = PostStatus.Deleted;
        builder.AddPost(builderPost);

        var response = await Client.GetAsync($"/v1/posts/{builderPost.Id}");

        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async void GetPosts()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder
            .AddUser().AddPost().AddPost()
            .AddUser().AddPost()
            .AddUser().AddPost();

        var response = await Client.GetAsync("/v1/posts");

        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestResponseDTO<PostResponseDTO[]>>())!.Data;
        Assert.Equal(4, data.Length);
        Assert.Equal(3, data[0].UserId);
    }

    [Fact]
    public async void UpdateDraftPost()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        var builderPost = builder.AddUser().GetBuilderPost();
        builderPost.Status = PostStatus.Draft;
        builder.AddPost(builderPost);

        PostNewDTO putData = new() { Body = "Updated" };
        var response = await Client.PutAsJsonAsync($"/v1/posts/{builder.PostId}", putData);

        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal(1, data.Id);
        Assert.Equal("Updated", data.Body);
    }

    [Fact]
    public async void UpdatePublishedPost422Error()
    {
        using var scope = Factory.Services.CreateScope();
        Context = GetScopedContext(scope);
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddPost();

        PostNewDTO putData = new() { Body = "Updated" };

        var response = await Client.PutAsJsonAsync($"/v1/posts/{builder.PostId}", putData);
        Assert.NotNull(response);
        Assert.Equal(422, (int)response.StatusCode);
        var responseString = await response.Content.ReadAsStringAsync();
        Assert.Contains("Cannot update a PUBLISHED post.", responseString);
    }

    [Fact]
    public async void DeletePost()
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