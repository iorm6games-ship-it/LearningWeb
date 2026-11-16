using LearningWeb.Repositorires;
using Microsoft.AspNetCore.Identity;
using LearningWeb.Models;

namespace LearningWeb.Services;

public class UserService
{
    private UserRepository _repository;
    private readonly PasswordHasher<User> _hasher = new();
    public UserService(UserRepository repository)
    {
        _repository = repository;
    }

    public string GetUserName(int id)
    {
        return _repository.GetUserName(id);
    }

    public bool VerifyPassword(
        User user,
        string inputPassword)
    {

        var result = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            inputPassword);
        
        return result != PasswordVerificationResult.Failed;
    }

    public LoginUser? Login(string email, string password)
    {
        User? user = _repository.GetUserByEmail(email);

        if (user == null) return null;

        if (!VerifyPassword(user, password)) return null;

        return new LoginUser
        {
            Id = user.Id,
            Email = user.Email,
            Role = user.Role
        };
    }
    
}