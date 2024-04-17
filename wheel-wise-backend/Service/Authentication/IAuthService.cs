namespace wheel_wise.Service.Authentication;

public interface IAuthService
{
    Task<AuthResult> RegisterAsync(string email, string userName, string password, string role);
    Task<AuthResult> LoginAsync(string email, string password);
}