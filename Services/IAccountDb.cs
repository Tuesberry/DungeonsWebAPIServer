namespace TuesberryAPIServer.Services
{
    public interface IAccountDb : IDisposable
    {
        public Task<ErrorCode> CreateAccount(string id, string pw);
        public Task<ErrorCode> VerifyAccount(string id, string pw);
    }
}
