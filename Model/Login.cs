namespace TuesberryAPIServer
{
    public class PKLoginRequest
    {
        public string? Email { get; set; }
        public string? Password { get; set; }
    }

    public class PKLoginResponse
    {
        public ErrorCode Result { get; set; }
        public string? Authtoken { get; set; }   
    }

    class DBUserInfo
    {
        public string? Email { get; set; }
        public string? HashedPassword { get; set; }
        public string? SaltValue { get; set; }
    }
}
