using fakebook.Models;
using Microsoft.AspNetCore.Authorization;
using UserModel = fakebook.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using fakebook.Constants;


namespace fakebook.Controllers.v1;
[Route("[controller]/[action]")]
[Authorize]
[ApiController]
public class SeedController(
    RoleManager<ApplicationRole> roleManager,
    UserManager<UserModel> userManager) : ControllerBase
{
    [HttpPost]
    [ResponseCache(NoStore = true)]
    public async Task<IActionResult> AuthData()
    {
        int rolesCreated = 0;
        int usersAddedToRoles = 0;

        if (!await roleManager.RoleExistsAsync(RoleNames.Administrator))
        {
            await roleManager.CreateAsync(new ApplicationRole(RoleNames.Administrator));
            rolesCreated++;
        }

        var testAdministrator = await userManager.FindByNameAsync("slimjob_dopamine");
        if (testAdministrator != null
            && !await userManager.IsInRoleAsync(testAdministrator, RoleNames.Administrator))
        {
            await userManager.AddToRoleAsync(testAdministrator, RoleNames.Administrator);
            usersAddedToRoles++;
        }
        return new JsonResult(new
        {
            RolesCreated = rolesCreated, UsersAddedToRoles = usersAddedToRoles
        });
    }
}
