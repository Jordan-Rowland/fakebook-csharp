using fakebook.Models;
using Xunit.Abstractions;


namespace fakebooktests;
public class Seeding
{
    public static void IntializeTestDB(ApplicationDbContext db, ITestOutputHelper output)
    {
        db.Users.AddRange(GetUsers());
        db.SaveChanges();
        db.Posts.AddRange(GetPosts());
        db.SaveChanges();
    }

    private static List<User> GetUsers()
    {
        return [
            new() {
                //Id = 1,
                UserName = "TestUser1",
                PasswordHash = "sdfsdf",
                CreatedAt = DateTime.Now,
                LastActive = DateTime.Now,
                Status = 0,
            },
            new() {
                //Id = 2,
                UserName = "TestUse2",
                PasswordHash = "sdfsdf",
                CreatedAt = DateTime.Now,
                LastActive = DateTime.Now,
                Status = 0,
            },
            new() {
                //Id = 3,
                UserName = "TestUser3",
                PasswordHash = "sdfsdf",
                CreatedAt = DateTime.Now,
                LastActive = DateTime.Now,
                Status = 0,
            }
        ];
    }

        private static List<Post> GetPosts()
    {
        return [
            new() {
                Body = "TEST POST 1",
                UserId = 1,
                Status = 0,
                CreatedAt = DateTime.Now,
            },
            new() {
                Body = "TEST POST 2",
                UserId = 1,
                Status = 0,
                CreatedAt = DateTime.Now,
            },
            new() {
                Body = "TEST POST 3",
                UserId = 2,
                Status = 0,
                CreatedAt = DateTime.Now,
            },
            new() {
                Body = "TEST POST 4",
                UserId = 3,
                Status = 0,
                CreatedAt = DateTime.Now,
            }
        ];
    }

}
