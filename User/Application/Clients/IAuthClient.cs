namespace Application.Clients;

public interface IAuthClient
{
    Task<bool> RegisterCredentialAsync(string email, string password);
}
