using LearningWeb.Factories;
using LearningWeb.Services;
using LearningWeb.Repositorires;
using LearningWeb.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var container = new UserServiceContainer();

container.Register(UserServiceFactory.Create);

app.MapGet("/", () => "トップページ");

app.MapGet("/user/{id:int}", (int id) =>
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
