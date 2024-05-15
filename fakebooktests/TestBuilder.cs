using Models = fakebook.Models;
using fakebook.Services.v1;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;


namespace fakebooktests;
internal class TestBuilder(Models.ApplicationDbContext db, ITestOutputHelper output)
{
    public int UserId { get; set; }
    public int PostId { get; set; }

    public TestBuilder AddUser(Models.User? _user = null)
    {
        var user = _user ?? GetBuilderUser();
        db.Add(user);
        db.SaveChanges();
        UserId = user.Id;
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
            Status = 0,
        };
    }

    public TestBuilder AddPost(Models.Post? _post = null)
    {
        var post = _post ?? GetBuilderPost();
        db.Add(post);
        db.SaveChanges();
        PostId = post.Id;
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
}
