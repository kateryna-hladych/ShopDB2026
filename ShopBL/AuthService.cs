using System;
using ShopDB.DAL.EF.Entities;
using ShopDB.DAL.EF.Repositories.Interfaces;

namespace ShopBL;

public class AuthService : IAuthService
{
    private const int MinUsernameLength = 3;
    private const int MaxUsernameLength = 20;
    private const int MinPasswordLength = 6;

    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users ?? throw new ArgumentNullException(nameof(users));
        _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
    }

    public bool Authenticate(string username, string password, out string role)
    {
        role = string.Empty;
        username = Normalize(username);

        if (username.Length == 0 || string.IsNullOrEmpty(password))
        {
            return false;
        }

        var user = _users.GetByUsername(username);
        if (user == null)
        {
            return false;
        }

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
        {
            return false;
        }

        role = user.Role;
        return true;
    }

    public bool Register(string username, string password, string role = "User")
    {
        
        username = Normalize(username);
        role = string.IsNullOrWhiteSpace(role) ? "User" : role.Trim();

        ValidateUsername(username);
        ValidatePassword(password);

        if (_users.GetByUsername(username) != null)
        {
            return false;
        }

        _users.Add(new User
        {
            Username = username,
            PasswordHash = _passwordHasher.HashPassword(password),
            Role = role
        });

        return true;
    }

    public bool ResetPassword(string username, string newPassword)
    {
        username = Normalize(username);
        ValidatePassword(newPassword);

        var user = _users.GetByUsername(username);
        if (user == null)
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        _users.Update(user);

        return true;
    }

    public bool ChangePassword(string username, string oldPassword, string newPassword)
    {
        username = Normalize(username);
        ValidatePassword(newPassword);

        var user = _users.GetByUsername(username);
        if (user == null)
        {
            return false;
        }

        if (!_passwordHasher.VerifyPassword(oldPassword ?? string.Empty, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = _passwordHasher.HashPassword(newPassword);
        _users.Update(user);

        return true;
    }

    

    private static string Normalize(string? value) => (value ?? string.Empty).Trim();

    private static void ValidateUsername(string username)
    {
        if (username.Length < MinUsernameLength || username.Length > MaxUsernameLength)
            throw new BusinessRuleException(
                $"Логін: від {MinUsernameLength} до {MaxUsernameLength} символів.");
    }

    private static void ValidatePassword(string? password)
    {
        if (string.IsNullOrEmpty(password) || password.Length < MinPasswordLength)
            throw new BusinessRuleException(
                $"Пароль: мінімум {MinPasswordLength} символів.");
    }
}