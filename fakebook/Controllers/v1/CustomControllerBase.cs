using fakebook.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

using UserModel = fakebook.Models.User;

namespace fakebook.Controllers.v1;
[ApiController]
public class CustomControllerBase<T> : ControllerBase
{
    public ApplicationDbContext Context { get; set; }
    public ILogger<T> Logger { get; set; }
    public IConfiguration Configuration { get;set; }
    public RoleManager<ApplicationRole> RoleManager { get; set; }
    public UserManager<UserModel> UserManager { get; set; }
    public SignInManager<UserModel> SignInManager { get; set; }
    public int? UserId { get; set; } = null;

    public CustomControllerBase(
        ApplicationDbContext context,
        ILogger<T> logger,
        IConfiguration configuration,
        RoleManager<ApplicationRole> roleManager,
        UserManager<User> userManager,
        SignInManager<User> signInManager,
        IHttpContextAccessor httpContextAccessor)
    {
        Context = context;
        Logger = logger;
        Configuration = configuration;
        RoleManager = roleManager;
        UserManager = userManager;
        SignInManager = signInManager;
        string userIdClaim = httpContextAccessor.HttpContext?.User?.FindFirst("UserId")?.Value;
        if (int.TryParse(userIdClaim, out int userId)) { UserId = userId; }
    }
}
