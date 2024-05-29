using Models = fakebook.Models;
using Xunit.Abstractions;


namespace fakebooktests;
internal class TestBuilder(Models.ApplicationDbContext db, ITestOutputHelper output)
{
    public int UserId { get; set; }
    public int PostId { get; set; }

    public List<int> UserIds { get; set; } = [];
    public List<int> PostIds { get; set; } = [];

    public TestBuilder AddUser(Models.User? _user = null)
    {
        var user = _user ?? GetBuilderUser();
        // Need to add user here the same was I do when creating user in prod code -- maybe?
        db.Add(user);
        db.SaveChanges();
        UserId = user.Id;
        UserIds.Add(user.Id);
        return this;
    }

    public Models.User GetBuilderUser()
    {
        return new()
        {
            UserName = $"Builder User {UserId + 1}",
            PasswordHash = "asdfasdfqwerty",
            CreatedAt = DateTime.Now,
            LastActive = DateTime.Now,
            Status = Models.UserStatus.Public,
        };
    }

    public TestBuilder AddPost(Models.Post? _post = null)
    {
        var post = _post ?? GetBuilderPost();
        db.Add(post);
        db.SaveChanges();
        PostId = post.Id;
        PostIds.Add(post.Id);
        return this;
    }

    public Models.Post GetBuilderPost()
    {
        return new()
        {
            Body = $"Builder Post {PostId + 1}",
            UserId = UserId,
            CreatedAt = DateTime.Now,
            Status = Models.PostStatus.Published,
        };
    }

    public TestBuilder AddFollow(Models.Follow? _follow = null)
    {
        var follow = _follow ?? GetBuilderFollow();
        db.Add(follow);
        db.SaveChanges();
        return this;
    }

    public Models.Follow GetBuilderFollow(int? followerId = null, int? followedId = null)
    {
        return new()
        {
            FollowerId = followerId ?? UserId,
            FollowedId = followedId ?? UserId - 1,
            Pending = false,
            CreatedAt = DateTime.Now,
        };
    }
}
