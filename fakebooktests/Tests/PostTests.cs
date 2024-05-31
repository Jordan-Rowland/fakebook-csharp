using System.Net.Http.Json;
using Xunit.Abstractions;

using fakebook.DTO.v1;
using fakebook.DTO.v1.Post;
using fakebook.Models;
using PostService = fakebook.Services.v1.Post;
using PostModel = fakebook.Models.Post;
using UserModel = fakebook.Models.User;
using Microsoft.AspNetCore.Http;
using fakebook.Services.v1;
using Moq;


namespace fakebooktests.Tests;
public class PostTests(
    CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    : BaseTestClass(factory, output)
{
    [Fact]
    public async Task CreatePost()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser();

        PostNewDTO postData = new() { Body = "Test post" };
        var response = await PostService.CreatePost(Context, postData, builder.UserId);

        Assert.NotNull(response);
        Assert.IsType<PostModel>(response);
    }

    [Fact]
    public async Task GetPost()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddPost().AddPost();
        var builderUser = builder.GetBuilderUser();
        builderUser.Status = UserStatus.Private;
        builder.AddUser(builderUser).AddPost();

        var response = await Client.GetAsync($"/v1/posts/{builder.PostId}");
        
        Assert.NotNull(response);
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestDataDTO<PostResponseDTO>>())!.Data;
        Assert.Equal("Builder Post 3", data.Body);
        Assert.Equal(2, data.UserId);
    }

    [Fact]
    public async Task GetNonexistentPost404Error()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddPost();

        var response = await Client.GetAsync("/v1/posts/3");

        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetDeletedPost404Error()
    {
        TestBuilder builder = new(Context, Output);
        var builderPost = builder.AddUser().GetBuilderPost();
        builderPost.Status = PostStatus.Deleted;
        builder.AddPost(builderPost);

        var response = await Client.GetAsync($"/v1/posts/{builderPost.Id}");

        Assert.NotNull(response);
        Assert.Equal(404, (int)response.StatusCode);
    }

    [Fact]
    public async Task GetPosts()
    {
        TestBuilder builder = new(Context, Output);
        builder
            .AddUser().AddPost().AddPost()
            .AddUser().AddPost()
            .AddUser().AddPost();

        var response = await Client.GetAsync("/v1/posts");

        Assert.NotNull(response);
        Output.WriteLine(await response.Content.ReadAsStringAsync());
        Assert.Equal(200, (int)response.StatusCode);
        var data = (await response.Content.ReadFromJsonAsync<RestResponseDTO<PostResponseDTO[]>>())!.Data;
        Assert.Equal(4, data.Length);
        Assert.Equal(3, data[0].UserId);
    }

    [Fact]
    public async Task DontShowPrivatePostsToNonLoggedInUser()
    {
        TestBuilder builder = new(Context, Output);
        var builderPrivateUser = builder.GetBuilderUser();
        builderPrivateUser.Status = UserStatus.Private;
        builder.AddUser(builderPrivateUser).AddPost().AddPost();

        var response = await PostService.GetPosts(Context, null, null);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    [Fact]
    public async Task DontShowPrivatePostsToNonFollowingLoggedInUser()
    {
        TestBuilder builder = new(Context, Output);
        var builderPrivateUser = builder.GetBuilderUser();
        builderPrivateUser.Status = UserStatus.Private;
        builder.AddUser(builderPrivateUser).AddPost().AddPost();
        var builderUser = builder.GetBuilderUser();
        builder.AddUser(builderUser);
        UserManagerMock
            .Setup(m => m.FindByIdAsync(It.IsAny<string>()))
            .ReturnsAsync(builderUser);

        var response = await PostService.GetPosts(Context, UserManagerMock.Object, builder.UserId);

        Assert.NotNull(response);
        Assert.Empty(response);
    }

    //[Fact]
    //public async Task GetPostsWithNoFollowers()
    //{
    //    TestBuilder builder = new(Context, Output);
    //    var builderPrivateUser = builder.GetBuilderUser();
    //    builderPrivateUser.Status = UserStatus.Private;
    //    builder.AddUser(builderPrivateUser).AddPost().AddPost();

    //    var response = await PostService.GetPosts(Context, );

    //    Assert.NotNull(response);
    //    Assert.Equal(200, (int)response.StatusCode);
    //    //var data = (await response.Content.ReadFromJsonAsync<RestResponseDTO<PostResponseDTO[]>>())!.Data;
    //    Assert.Empty(data);
    //}

    [Fact]
    public async Task UpdateDraftPost()
    {
        TestBuilder builder = new(Context, Output);
        var builderPost = builder.AddUser().GetBuilderPost();
        builderPost.Status = PostStatus.Draft;
        builder.AddPost(builderPost);

        PostNewDTO putData = new() { Body = "Updated" };
        var response = await PostService.UpdatePost(Context, builder.PostId, putData, builder.UserId);

        Assert.NotNull(response);
        Assert.Equal(1, response.Id);
        Assert.Equal("Updated", response.Body);
    }

    [Fact]
    public async Task UpdatePublishedPost422Error()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddPost();

        PostNewDTO putData = new() { Body = "Updated" };
        var ex = await Assert.ThrowsAsync<BadHttpRequestException>(() =>
            PostService.UpdatePost(Context, builder.PostId, putData, builder.UserId));
        Assert.Equal(422, ex.StatusCode);
        Assert.Equal("Cannot update a PUBLISHED post.", ex.Message);
    }

    [Fact]
    public async Task DeletePost()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddPost();
        
        var response = await PostService.DeletePost(Context, builder.PostId, builder.UserId);

        Assert.NotNull(response);
        Assert.Equal(PostStatus.Deleted, response.Status);
        Assert.Equal(DateTime.Today.Date, response.DeletedAt!.Value.Date);
    }
}