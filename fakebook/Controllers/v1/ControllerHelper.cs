using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;


namespace fakebook.Controllers.v1;
public class ControllerHelper
{
    public static T ReturnDataWithStatusCode<T>(T data, int statusCode, HttpContext httpContext)
    {
        httpContext.Response.StatusCode = statusCode;
        return data;
    }
}
