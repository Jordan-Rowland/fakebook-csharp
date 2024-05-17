using Azure;
using Microsoft.AspNetCore.Mvc;

namespace fakebook.Controllers.v1;
public class ControllerHelper
{
    public static T ReturnDataWithStatusCode<T>(T data, int statusCode, HttpContext httpContext)
    {
        httpContext.Response.StatusCode = statusCode;
        return data;
    }
}
