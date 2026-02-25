namespace Application.Clients;

public interface IAuthClient
{
    Task<bool> RegisterCredentialAsync(string email, string password);
    Task<bool> UpdateEmailAsync(string oldEmail, string newEmail);
    Task<bool> UpdatePasswordAsync(string email, string newPassword);
}
