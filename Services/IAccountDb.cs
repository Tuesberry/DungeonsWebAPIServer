namespace TuesberryAPIServer.Services
{
    public interface IAccountDb : IDisposable
    {
        public Task<ErrorCode> CreateAccount(string id, string pw);
        public Task<Tuple<ErrorCode, Int64>> VerifyAccount(string id, string pw);
    }
}
