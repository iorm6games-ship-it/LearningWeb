using LearningWeb.Services;

namespace LearningWeb.Factories;
public class UserServiceContainer
{
    private Func<UserService>? _factory;

    public void Register(Func<UserService> factory)
    {
        _factory = factory;
    }

    public UserService Get()
    {
        
        if (_factory == null)
        {
            throw new InvalidOperationException(
                "UserService が登録されていません。"
            );
        }

        return _factory();
    }
}