namespace TuesberryAPIServer.Services
{
    public interface IAccountDb : IDisposable
    {
        public Task<Tuple<ErrorCode, Int64>> CreateAccount(string id, string pw);
        public Task<Tuple<ErrorCode, Int64>> VerifyAccount(string id, string pw);
    }
}
