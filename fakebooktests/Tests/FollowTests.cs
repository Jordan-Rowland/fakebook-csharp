using Moq;
using Xunit.Abstractions;
using FollowService = fakebook.Services.v1.Follow;


namespace fakebooktests.Tests;
public class FollowTests(
    CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    : BaseTestClass(factory, output)
{
    [Fact]
    public async Task AddFollower()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser();
        var mockedUser = builder.GetBuilderUser();
        mockedUser.Id = builder.UserId - 1;
        UserManagerMock
            .Setup(m => m.FindByIdAsync((builder.UserId - 1).ToString()))
            .ReturnsAsync(mockedUser);

        var result = await FollowService.FollowUser(
            Context, UserManagerMock.Object, builder.UserId, builder.UserId - 1);

        Assert.NotNull(result);
        Assert.Equal(result.FollowerId, builder.UserId);
        Assert.Equal(result.FollowedId, builder.UserId - 1);
        var follows = Context.Follows
            .Where(f => f.FollowerId == builder.UserId)
            .Select(f => f.FollowedId)
            .ToList();
        Assert.Single(follows);
    }

    [Fact]
    public async Task AddFollowerToPrivateUser()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser();
        var mockedUser = builder.GetBuilderUser();
        mockedUser.Id = builder.UserId - 1;
        mockedUser.Status = fakebook.Models.UserStatus.Private;
        UserManagerMock
            .Setup(m => m.FindByIdAsync((builder.UserId - 1).ToString()))
            .ReturnsAsync(mockedUser);

        var result = await FollowService.FollowUser(
            Context, UserManagerMock.Object, builder.UserId, builder.UserId - 1);

        Assert.NotNull(result);
        Assert.Equal(result.FollowerId, builder.UserId);
        Assert.Equal(result.FollowedId, builder.UserId - 1);
        var follow = Context.Follows
            .Where(f => f.FollowerId == builder.UserId)
            .FirstOrDefault();
        Assert.True(follow!.Pending);
    }

    [Fact]
    public async Task Unfollow()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser().AddFollow();

        var result = await FollowService.UnfollowUser(
            Context, builder.UserId, builder.UserId - 1);

        Assert.True(result);
        var follows = Context.Follows
            .Where(f => f.FollowerId == builder.UserId)
            .Select(f => f.FollowedId)
            .ToList();
        Assert.Empty(follows);
    }

    [Fact]
    public async Task Getfollowers()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser().AddUser().AddUser();
        foreach (var id in builder.UserIds)
        {
            if (id == builder.UserId) continue;
            var follow = builder.GetBuilderFollow(id, builder.UserId);
            builder.AddFollow(follow);
        }

        var result = await FollowService.GetFollowers(Context, builder.UserId);

        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task Getfollowed()
    {
        TestBuilder builder = new(Context, Output);
        builder.AddUser().AddUser().AddUser().AddUser();
        foreach (var id in builder.UserIds)
        {
            if (id == builder.UserId) continue;
            var follow = builder.GetBuilderFollow(builder.UserId, id);
            builder.AddFollow(follow);
        }

        var result = await FollowService.GetFollows(Context, builder.UserId);

        Assert.NotEmpty(result);
        Assert.Equal(3, result.Count);
    }
}
