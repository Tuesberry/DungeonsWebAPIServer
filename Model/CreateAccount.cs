namespace TuesberryAPIServer
{
    public class PkCreateAccountRequest
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    public class PkCreateAccountResponse
    {
        public ErrorCode Result { get; set; }
    }
}
