using Asp.Versioning;
using fakebook.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

        builder.Services.AddIdentity<User, ApplicationRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 8;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>();

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme =
            options.DefaultChallengeScheme =
            options.DefaultForbidScheme =
            options.DefaultScheme =
            options.DefaultSignInScheme =
            options.DefaultSignOutScheme =
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes(
                        builder.Configuration["JWT:SigningKey"]!)
                )
            };
        });

        builder.Services.AddHttpContextAccessor();

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
        app.UseAuthentication();
        app.UseAuthorization();

        app.MapGet("/error/test",
            [ApiVersion("1.0")]
            [EnableCors("AnyOrigin_GetOnly")]
            [ResponseCache(NoStore = true)] () =>
                { throw new BadHttpRequestException("test", StatusCodes.Status412PreconditionFailed); });
        
        app.MapGet("/auth/test/1",
            [Authorize]
            [EnableCors("AnyOrigin")]
            [ResponseCache(NoStore = true)] () =>
            {
                return Results.Ok("You are authorized!");
            });

        app.MapControllers();

        app.Run();

    }
}