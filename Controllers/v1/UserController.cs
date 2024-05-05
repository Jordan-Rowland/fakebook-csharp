using Asp.Versioning;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace fakebook.Controllers.v1;
[Route("v{version:apiVersion}/users")]
[ApiController]
[ApiVersion("1.0")]
public class UserController : ControllerBase
{
}
