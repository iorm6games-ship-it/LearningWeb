using LearningWeb.Factories;
using LearningWeb.Services;
using LearningWeb.Repositorires;
using LearningWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using System.Runtime.CompilerServices;

var builder = WebApplication.CreateBuilder(args);

var userRepository = new UserRepository();

builder.Services.AddSingleton(userRepository);
builder.Services.AddTransient<UserService>();

builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(
        options =>
        {
            options.ExpireTimeSpan = TimeSpan.FromMinutes(2);
            options.SlidingExpiration = true;

            options.Events.OnRedirectToLogin = context =>
            {
                context.Response.StatusCode =
                    StatusCodes.Status401Unauthorized;
                return Task.CompletedTask;
            };

            options.Events.OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode =
                    StatusCodes.Status403Forbidden;
                return Task.CompletedTask;
            };

            options.Events.OnValidatePrincipal = async context =>
            {
                
                string? idText = context.Principal?
                    .FindFirst(ClaimTypes.NameIdentifier)?.Value;
                
                if (!int.TryParse(idText, out int userId))
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );
                    return;
                }

                User? user = userRepository.GetUserById(userId);

                string? cookieRole = context.Principal?
                    .FindFirst(ClaimTypes.Role)?.Value;

                if (user == null || user.Role != cookieRole)
                {
                    context.RejectPrincipal();
                    await context.HttpContext.SignOutAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme
                    );
                }
            };
        }
    );
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
        policy
            .WithOrigins("http://localhost:5173")
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminPolicy", policy =>
    {
        policy.RequireRole("Admin");
    });
});

var app = builder.Build();
app.UseCors("ReactPolicy");
app.UseAuthentication();
app.UseAuthorization();

var container = new UserServiceContainer();

container.Register(() => new UserService(userRepository));

app.MapGet("/", () => "トップページ");
app.MapGet("/hello", () => "Hello from LearningWeb!");

app.MapGet("/user/{id:int}", (int id) =>
{
    var service = container.Get();
    return service.GetUserName(id);

});

app.MapPost("/login", async (
    LoginRequest request,
    HttpContext context,
    UserService service) =>
{
    Console.WriteLine("① /login に到達");
    LoginUser? loginUser = service.Login(
        request.Email,
        request.Password
    );
    Console.WriteLine($"Email: {loginUser?.Email}");

    if (loginUser == null)
    {
        return Results.Unauthorized();
    }
    
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, loginUser.Id.ToString()),
        new Claim(ClaimTypes.Name, loginUser.Email),
        new Claim(ClaimTypes.Email, loginUser.Email),
        new Claim(ClaimTypes.Role, loginUser.Role)
    };

    var identity = new ClaimsIdentity(
        claims,
        CookieAuthenticationDefaults.AuthenticationScheme
    );

    var principal = new ClaimsPrincipal(identity);

    await context.SignInAsync(
        CookieAuthenticationDefaults.AuthenticationScheme,
        principal
    );

    return Results.Ok("ログイン成功");
});

app.MapGet("/mypage", async (HttpContext context) =>
{
    
    Console.WriteLine(
        $"認証済: {context.User.Identity?.IsAuthenticated}"
    );
    Console.WriteLine(
        $"ユーザー名: {context.User.Identity?.Name}"
    );
    Console.WriteLine(
        $"Role: {context.User.FindFirst(ClaimTypes.Role)?.Value}"
    );
    var identity = context.User.Identity;
    var user = context.User;


    return Results.Ok(new
    {
        IsAuthenticated = identity?.IsAuthenticated,
        Name = identity?.Name,

        ServerUtcNow = DateTimeOffset.UtcNow,
        IsAdmin = user?.IsInRole("Admin")
    });
})
.RequireAuthorization();

app.MapPost("/logout", async (HttpContext context) =>
{
    await context.SignOutAsync(
        CookieAuthenticationDefaults.AuthenticationScheme
    );
    return Results.Ok("ログアウトしました");
})
.RequireAuthorization();

app.MapGet("/admin", () =>
{
    return Results.Ok("管理者ページ");
})
.RequireAuthorization("AdminPolicy");

app.Run();
