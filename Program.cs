using LearningWeb.Factories;
using LearningWeb.Services;
using LearningWeb.Repositorires;
using LearningWeb.Models;
using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie();

builder.Services.AddAuthorization();

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

var container = new UserServiceContainer();

container.Register(UserServiceFactory.Create);

app.MapGet("/", () => "トップページ");

app.MapGet("/user/{id}", (int id) =>
{
    var service = container.Get();
    return service.GetUserName(id);

});

app.MapPost("/login", (LoginRequest request) =>
{
    var service = container.Get();

    bool success = service.Login(
        request.Email,
        request.Password
    );
    if (success)
    {
        return Results.Ok("ログイン成功");
    }

    return Results.Unauthorized();
});

app.Run();
