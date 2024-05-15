using Asp.Versioning;
using fakebook.Models;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using System.Diagnostics;
using System.Text.Json.Serialization;


public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllers().AddJsonOptions(x =>
            // Avoid circular references for Post -> Parent -> Replies
            x.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo { Title = "Fakebook", Version = "v1.0" });
        });
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(cfg =>
            {
                cfg.WithOrigins(builder.Configuration["AllowedOrigins"]!);
                cfg.AllowAnyHeader();
                cfg.AllowAnyMethod();
            });
            options.AddPolicy(name: "AnyOrigin", cfg =>
            {
                cfg.AllowAnyOrigin();
                cfg.AllowAnyHeader();
                cfg.AllowAnyMethod();
            });
            options.AddPolicy(name: "AnyOrigin_GetOnly", cfg =>
            {
                cfg.AllowAnyOrigin();
                cfg.AllowAnyHeader();
                cfg.WithMethods("GET");
            });
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = new ApiVersion(1, 0);
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
        
        //builder.Services.AddProblemDetails();
        
        var app = builder.Build();

        if (app.Configuration.GetValue<bool>("UseSwagger"))
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint(
                    $"/swagger/v1/swagger.json",
                    $"Fakebook v1");
            });
        }

        if (app.Configuration.GetValue<bool>("UseDeveloperExceptionPage"))
            app.UseDeveloperExceptionPage();
        else
            app.UseExceptionHandler(action =>
            {
                action.Run(async context =>
                {
                    var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

                    int statusCode = 404;
                    if (exceptionHandler != null && exceptionHandler.Error is BadHttpRequestException)
                        statusCode = (exceptionHandler?.Error as BadHttpRequestException)!.StatusCode;
                    context.Response.StatusCode = statusCode;

                    // TODO: logging, etc
                    ProblemDetails details = new()
                    {
                        Detail = exceptionHandler!.Error.Message,
                        Status = statusCode,
                    };
                    details.Extensions["traceId"] = Activity.Current?.Id ?? context.TraceIdentifier;

                    app.Logger.LogError(
                        exceptionHandler?.Error,
                        "An unhandled exception occurred."
                    );

                    await context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(details));
                });
            });

        app.UseHttpsRedirection();
        app.UseCors();
        app.UseAuthorization();

        #region old error handling
        //app.MapGet("/error",
        //    [ApiVersion("1.0")]
        //    [EnableCors("AnyOrigin")]
        //    [ResponseCache(NoStore = true)]
        //    (HttpContext context) =>
        //    {
        //        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        //        // TODO: logging, etc
        //        ProblemDetails details = new()
        //        {
        //            Detail = exceptionHandler!.Error.Message,
        //            Status = (exceptionHandler.Error as BadHttpRequestException)?.StatusCode,
        //        };
        //        details.Extensions["traceId"] =
        //            Activity.Current?.Id ?? context.TraceIdentifier;
        //        return Results.Problem(details);
        //    });

        //app.MapPost("/error",
        //    [ApiVersion("1.0")]
        //    [EnableCors("AnyOrigin")]
        //    [ResponseCache(NoStore = true)]
        //    (HttpContext context) =>
        //    {
        //        var exceptionHandler = context.Features.Get<IExceptionHandlerPathFeature>();

        //        // TODO: logging, etc
        //        ProblemDetails details = new()
        //        {
        //            Detail = exceptionHandler!.Error.Message,
        //            Status = (exceptionHandler.Error as BadHttpRequestException)?.StatusCode,
        //        };
        //        details.Extensions["traceId"] =
        //            Activity.Current?.Id ?? context.TraceIdentifier;
        //        return Results.Problem(details);
        //    });
        #endregion

        app.MapGet("/error/test",
            [ApiVersion("1.0")]
            [EnableCors("AnyOrigin_GetOnly")]
            [ResponseCache(NoStore = true)] () =>
                { throw new BadHttpRequestException("test", StatusCodes.Status412PreconditionFailed); });

        app.MapControllers();

        app.Run();

    }
}