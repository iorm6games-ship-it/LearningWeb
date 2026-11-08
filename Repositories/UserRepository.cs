namespace LearningWeb.Repositorires;
using LearningWeb.Models;
using Microsoft.AspNetCore.Identity;

public class UserRepository
{
    private readonly List<User> _users;
    public UserRepository()
    {
        var user = new User
        {
            Id = 1,
            UserName = "Taro",
            Email = "taro@example.com"
        };
        var hasher = new PasswordHasher<User>();

        user.PasswordHash = hasher.HashPassword(
            user,
            "Password123!"
        );
        _users = new List<User> { user };
    }

    public string GetUserName(int id)
    {
        return _users
            .FirstOrDefault(u => u.Id == id)?
            .UserName ?? "Unknown";
    }

    public User? GetUserByEmail(string email)
    {
        return _users.FirstOrDefault(
            u => u.Email == email
        );
    }
}