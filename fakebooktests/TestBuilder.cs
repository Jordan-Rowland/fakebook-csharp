using fakebook.Models;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using Xunit.Abstractions;


namespace fakebooktests;
internal class TestBuilder(ApplicationDbContext db, ITestOutputHelper output)
{
    //public ApplicationDbContext db { get; set; }
    public int UserId { get; set; }
    public int PostId { get; set; }

    public TestBuilder AddUser(params string[] args)
    {
        var user = new User()
        {
            Username = $"Builder User {UserId + 1}",
            PasswordHash = "XXXXX",
            CreatedAt = DateTime.Now,
            LastActive = DateTime.Now,
            Status = 0,
        };
        db.Add(user);
        db.SaveChanges();
        UserId = user.Id;
        return this;
    }

    public TestBuilder AddPost(params string[] args)
    {
        var post = new Post()
        {
            Body = $"Builder Post {PostId + 1}",
            UserId = UserId,
            CreatedAt = DateTime.Now,
            Status = 0,
        };
        db.Add(post);
        db.SaveChanges();
        PostId = post.Id;
        return this;
    }
}
