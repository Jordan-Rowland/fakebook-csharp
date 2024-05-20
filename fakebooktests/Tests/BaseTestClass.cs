using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit.Abstractions;

using fakebook.Models;
using UserModel = fakebook.Models.User;


namespace fakebooktests.Tests;
public class BaseTestClass : IClassFixture<CustomWebApplicationFactory<Program>>
{
    public HttpClient Client { get; set; }
    public ITestOutputHelper Output { get; set; }
    public ApplicationDbContext Context { get; set; }
    public Mock<UserManager<UserModel>> UserManagerMock { get; set; }


    public BaseTestClass(
        CustomWebApplicationFactory<Program> factory, ITestOutputHelper output)
    {
        Client = factory.CreateClient();
        Output = output;
        UserManagerMock = new Mock<UserManager<UserModel>>(
            Mock.Of<IUserStore<UserModel>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);
        Context = factory
            .Services
            .CreateScope()
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        Context.Database.EnsureDeleted();
        Context.Database.EnsureCreated();
    }
}
