namespace ShopBL;

public interface IAuthService
{
    bool Authenticate(string username, string password, out string role);
    bool Register(string username, string password, string role = "User");
    bool ResetPassword(string username, string newPassword);
    bool ChangePassword(string username, string oldPassword, string newPassword);
}