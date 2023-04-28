namespace TuesberryAPIServer.ModelDb
{
    public class Account
    {
        public Int64 AccountId { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;
        public string SaltValue { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
    }

    public class AuthUser
    {
        public Int64 AccountId { get; set; } = 0;
        public string UserId { get; set; } = string.Empty;

        public string AuthToken { get; set; } = string.Empty;   
        public string State { get; set; } = string.Empty;   
    }

    public enum UserState
    {
        Default = 0,
        Login = 1,
        Playing = 2,
    }
}
