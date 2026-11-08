using LearningWeb.Services;
using LearningWeb.Repositorires;

namespace LearningWeb.Factories;

public static class UserServiceFactory
{
    public static UserService Create()
    {
        UserRepository repository = new UserRepository();
        UserService service = new UserService(repository);
        return service;
    }
}