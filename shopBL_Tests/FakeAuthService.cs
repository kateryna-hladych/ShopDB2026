using ShopBL;

namespace shopBL_Tests
{
    
    public class FakeAuthService : IAuthService
    {
        public bool AuthenticateResult { get; set; }
        public string RoleToReturn { get; set; } = "User";
        public bool RegisterResult { get; set; } = true;
        public bool ResetResult { get; set; } = true;
        public bool ChangeResult { get; set; } = true;
        public Exception? ExceptionToThrow { get; set; }

        public int AuthenticateCalls { get; private set; }
        public int RegisterCalls { get; private set; }
        public int ResetCalls { get; private set; }

        public string? LastUsername { get; private set; }
        public string? LastPassword { get; private set; }

        public bool Authenticate(string username, string password, out string role)
        {
            AuthenticateCalls++;
            LastUsername = username;
            LastPassword = password;

            if (ExceptionToThrow != null) throw ExceptionToThrow;

            role = AuthenticateResult ? RoleToReturn : string.Empty;
            return AuthenticateResult;
        }

        public bool Register(string username, string password, string role = "User")
        {
            RegisterCalls++;
            LastUsername = username;
            LastPassword = password;

            if (ExceptionToThrow != null) throw ExceptionToThrow;

            return RegisterResult;
        }

        public bool ResetPassword(string username, string newPassword)
        {
            ResetCalls++;
            LastUsername = username;
            LastPassword = newPassword;

            if (ExceptionToThrow != null) throw ExceptionToThrow;

            return ResetResult;
        }

        public bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            LastUsername = username;
            LastPassword = newPassword;

            if (ExceptionToThrow != null) throw ExceptionToThrow;

            return ChangeResult;
        }
    }
}